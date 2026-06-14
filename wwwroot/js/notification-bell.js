/* ── Notification Bell + Toast System ───────────────────────── */
(function () {
    var POLL_MS      = 30000;
    var RECENT_URL   = '/api/notifications?handler=recent';
    var COUNT_URL    = '/api/notifications?handler=unread';
    var MARK_URL     = '/api/notifications?handler=markAllRead';
    var TOAST_MS     = 6000;
    var MAX_TOASTS   = 3;

    var wrap     = document.getElementById('notifBellWrap');
    if (!wrap) return;

    var btn      = document.getElementById('notifBellBtn');
    var badge    = document.getElementById('notifBadge');
    var dropdown = document.getElementById('notifDropdown');
    var listEl   = document.getElementById('notifList');
    var markBtn  = document.getElementById('notifMarkAll');
    var isOpen   = false;
    var lastCount       = 0;
    var initialized     = false;
    var lastSeenMaxId   = 0;

    /* ── Category config ────────────────────────── */
    var CAT = {
        app_review:  { icon: 'fa-clipboard-check', label: 'بررسی ابزار' },
        app_status:  { icon: 'fa-check-circle',    label: 'وضعیت ابزار' },
        open_prompt: { icon: 'fa-lock-open',        label: 'پرامپت باز'  },
        review:      { icon: 'fa-star',             label: 'نظر کاربر'   },
        withdrawal:  { icon: 'fa-money-bill-wave',  label: 'برداشت'       },
        general:     { icon: 'fa-bell',             label: 'اعلان'        }
    };

    /* ── Persian helpers ────────────────────────── */
    function toPersian(n) {
        return String(n).replace(/\d/g, function(d){ return '۰۱۲۳۴۵۶۷۸۹'[d]; });
    }

    function relTime(isoStr) {
        var d = new Date(isoStr);
        var diff = Math.floor((Date.now() - d.getTime()) / 1000);
        if (diff < 60)    return 'همین الان';
        if (diff < 3600)  return toPersian(Math.floor(diff / 60))   + ' دقیقه پیش';
        if (diff < 86400) return toPersian(Math.floor(diff / 3600)) + ' ساعت پیش';
        if (diff < 604800)return toPersian(Math.floor(diff / 86400))+ ' روز پیش';
        return d.toLocaleDateString('fa-IR');
    }

    function escHtml(s) {
        return String(s)
            .replace(/&/g,'&amp;').replace(/</g,'&lt;')
            .replace(/>/g,'&gt;').replace(/"/g,'&quot;');
    }

    /* ── Badge ──────────────────────────────────── */
    function setBadge(n) {
        if (n > 0) {
            badge.textContent = n > 99 ? '۹۹+' : toPersian(n);
            badge.classList.remove('hidden');
            btn.classList.add('has-unread');
            if (n > lastCount) badge.classList.add('pulse');
            else badge.classList.remove('pulse');
        } else {
            badge.classList.add('hidden');
            badge.classList.remove('pulse');
            btn.classList.remove('has-unread');
        }
        lastCount = n;
    }

    /* ── Toast container (lazy) ─────────────────── */
    function getToastContainer() {
        var c = document.getElementById('notifToastContainer');
        if (!c) {
            c = document.createElement('div');
            c.id = 'notifToastContainer';
            c.className = 'notif-toast-container';
            document.body.appendChild(c);
        }
        return c;
    }

    /* ── Show one toast ─────────────────────────── */
    function showToast(n) {
        var container = getToastContainer();
        if (container.children.length >= MAX_TOASTS) return;

        var cat = CAT[n.category] || CAT.general;

        var toast = document.createElement('div');
        toast.className = 'notif-toast';
        toast.setAttribute('data-cat', n.category || 'general');

        toast.innerHTML =
            '<div class="notif-toast__accent"></div>' +
            '<div class="notif-toast__body">' +
                '<div class="notif-toast__icon"><i class="fas ' + cat.icon + '"></i></div>' +
                '<div class="notif-toast__text">' +
                    '<div class="notif-toast__label">' + escHtml(cat.label) + '</div>' +
                    '<div class="notif-toast__title">' + escHtml(n.title) + '</div>' +
                    (n.message ? '<div class="notif-toast__msg">' + escHtml(n.message) + '</div>' : '') +
                '</div>' +
                '<button class="notif-toast__close" type="button" title="بستن"><i class="fas fa-times"></i></button>' +
            '</div>' +
            (n.link
                ? '<div class="notif-toast__footer"><span class="notif-toast__action"><i class="fas fa-arrow-left"></i>مشاهده جزئیات</span></div>'
                : '') +
            '<div class="notif-toast__progress"><div class="notif-toast__progress-inner" id="tp_' + Date.now() + '_bar"></div></div>';

        container.appendChild(toast);

        /* Navigate on body click */
        if (n.link) {
            toast.addEventListener('click', function(e) {
                if (!e.target.closest('.notif-toast__close')) location.href = n.link;
            });
        }

        /* Close button */
        var closeBtn = toast.querySelector('.notif-toast__close');
        closeBtn.addEventListener('click', function(e) {
            e.stopPropagation();
            dismissToast(toast, clearTimer);
        });

        /* Slide in */
        requestAnimationFrame(function() {
            requestAnimationFrame(function() {
                toast.classList.add('show');
            });
        });

        /* Progress bar shrink */
        var bar = toast.querySelector('.notif-toast__progress-inner');
        bar.style.transition = 'width ' + TOAST_MS + 'ms linear';
        requestAnimationFrame(function() {
            requestAnimationFrame(function() { bar.style.width = '0%'; });
        });

        /* Auto dismiss */
        var timer = setTimeout(function() { dismissToast(toast, null); }, TOAST_MS);
        function clearTimer() { clearTimeout(timer); }

        /* Pause on hover */
        toast.addEventListener('mouseenter', function() { clearTimeout(timer); bar.style.transition = 'none'; });
        toast.addEventListener('mouseleave', function() {
            bar.style.transition = 'width 1500ms linear';
            bar.style.width = '0%';
            timer = setTimeout(function() { dismissToast(toast, null); }, 1500);
        });
    }

    function dismissToast(toast, clearFn) {
        if (clearFn) clearFn();
        toast.classList.remove('show');
        toast.classList.add('hide');
        setTimeout(function() {
            if (toast.parentNode) toast.parentNode.removeChild(toast);
        }, 380);
    }

    /* ── Polling ────────────────────────────────── */
    function pollCount() {
        fetch(COUNT_URL, { credentials: 'same-origin' })
            .then(function(r) { return r.ok ? r.json() : null; })
            .then(function(d) {
                if (!d) return;
                var newCount = d.count;

                if (!initialized) {
                    /* First call — just set baseline, no toasts */
                    setBadge(newCount);
                    if (newCount > 0) {
                        fetch(RECENT_URL, { credentials: 'same-origin' })
                            .then(function(r){ return r.ok ? r.json() : []; })
                            .then(function(items) {
                                if (items.length) lastSeenMaxId = Math.max.apply(null, items.map(function(i){ return i.id||0; }));
                            }).catch(function(){});
                    }
                    initialized = true;
                    return;
                }

                if (newCount > lastCount) {
                    /* New notifications — fetch & toast */
                    setBadge(newCount);
                    fetch(RECENT_URL, { credentials: 'same-origin' })
                        .then(function(r){ return r.ok ? r.json() : []; })
                        .then(function(items) {
                            var fresh = items.filter(function(i){ return (i.id||0) > lastSeenMaxId; });
                            if (fresh.length) lastSeenMaxId = Math.max.apply(null, fresh.map(function(i){ return i.id||0; }));
                            fresh.slice(0, MAX_TOASTS).forEach(function(n, idx) {
                                setTimeout(function(){ showToast(n); }, idx * 200);
                            });
                        }).catch(function(){});
                } else {
                    setBadge(newCount);
                }
            }).catch(function(){});
    }

    /* ── Dropdown helpers ───────────────────────── */
    function buildList(items) {
        if (!items || !items.length) {
            listEl.innerHTML = '<div class="notif-empty"><i class="fas fa-bell-slash"></i>اعلانی وجود ندارد</div>';
            return;
        }
        listEl.innerHTML = items.map(function(n) {
            var cat  = CAT[n.category] || CAT.general;
            var href = n.link ? ' onclick="location.href=\'' + n.link.replace(/'/g,"\\'") + '\'"' : '';
            return '<div class="notif-item ' + (n.isRead ? '' : 'unread') + '" data-cat="' + escHtml(n.category) + '"' + href + '>' +
                '<div class="notif-icon"><i class="fas ' + cat.icon + '"></i></div>' +
                '<div class="notif-item__body">' +
                    '<div class="notif-item__title">' + escHtml(n.title) + '</div>' +
                    (n.message ? '<div class="notif-item__msg">' + escHtml(n.message) + '</div>' : '') +
                    '<div class="notif-item__time">' + relTime(n.createdAt) + '</div>' +
                '</div></div>';
        }).join('');
    }

    function openDropdown() {
        isOpen = true;
        dropdown.classList.add('open');
        listEl.innerHTML = '<div class="notif-empty"><i class="fas fa-spinner fa-spin"></i></div>';
        fetch(RECENT_URL, { credentials: 'same-origin' })
            .then(function(r){ return r.ok ? r.json() : []; })
            .then(function(items) { buildList(items); markAllRead(); })
            .catch(function(){ buildList([]); });
    }

    function closeDropdown() {
        isOpen = false;
        dropdown.classList.remove('open');
    }

    function markAllRead() {
        var token = document.querySelector('input[name="__RequestVerificationToken"]');
        fetch(MARK_URL, {
            method: 'POST',
            credentials: 'same-origin',
            headers: {
                'RequestVerificationToken': token ? token.value : '',
                'Content-Type': 'application/json'
            }
        }).then(function(){ setBadge(0); }).catch(function(){});
    }

    /* ── Events ─────────────────────────────────── */
    btn.addEventListener('click', function(e) {
        e.stopPropagation();
        isOpen ? closeDropdown() : openDropdown();
    });

    markBtn.addEventListener('click', function(e) {
        e.stopPropagation();
        markAllRead();
        listEl.querySelectorAll('.notif-item.unread').forEach(function(el){
            el.classList.remove('unread');
        });
    });

    document.addEventListener('click', function(e) {
        if (isOpen && !wrap.contains(e.target)) closeDropdown();
    });

    /* ── Init ───────────────────────────────────── */
    pollCount();
    setInterval(pollCount, POLL_MS);
})();
