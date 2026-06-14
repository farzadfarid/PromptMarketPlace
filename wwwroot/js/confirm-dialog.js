(function () {
    'use strict';

    /* ─── Alert Toast ──────────────────────────────────────────────── */
    const ALERT_TYPES = {
        success: { icon: 'fa-check-circle',        accent: '#22c55e', bg: '#f0fdf4', text: '#166534' },
        warning: { icon: 'fa-exclamation-triangle', accent: '#f59e0b', bg: '#fffbeb', text: '#92400e' },
        error:   { icon: 'fa-times-circle',         accent: '#ef4444', bg: '#fef2f2', text: '#991b1b' },
        info:    { icon: 'fa-info-circle',           accent: '#3b82f6', bg: '#eff6ff', text: '#1e40af' },
    };

    let toastContainer = null;

    function getToastContainer() {
        if (!toastContainer) {
            toastContainer = document.createElement('div');
            toastContainer.id = 'appAlertContainer';
            toastContainer.style.cssText =
                'position:fixed;bottom:1.5rem;left:1.5rem;z-index:9999;display:flex;flex-direction:column-reverse;gap:.65rem;pointer-events:none;';
            document.body.appendChild(toastContainer);
        }
        return toastContainer;
    }

    function showAlert(message, type) {
        type = type || 'info';
        const cfg = ALERT_TYPES[type] || ALERT_TYPES.info;
        const container = getToastContainer();

        const el = document.createElement('div');
        el.style.cssText =
            'background:' + cfg.bg + ';border:1px solid ' + cfg.accent + '30;border-left:4px solid ' + cfg.accent + ';' +
            'border-radius:12px;padding:.85rem 1.1rem;display:flex;align-items:flex-start;gap:.75rem;' +
            'box-shadow:0 8px 24px rgba(0,0,0,.12);pointer-events:all;max-width:340px;' +
            'opacity:0;transform:translateY(16px) scale(.97);transition:opacity .22s ease,transform .22s cubic-bezier(.34,1.56,.64,1);';

        el.innerHTML =
            '<i class="fas ' + cfg.icon + '" style="color:' + cfg.accent + ';font-size:1.1rem;margin-top:.05rem;flex-shrink:0;"></i>' +
            '<span style="color:' + cfg.text + ';font-size:.875rem;line-height:1.5;flex:1;">' + message + '</span>' +
            '<button style="background:none;border:none;cursor:pointer;color:' + cfg.text + ';opacity:.5;padding:0;margin-top:.05rem;font-size:.8rem;line-height:1;" aria-label="بستن">' +
            '<i class="fas fa-times"></i></button>';

        container.appendChild(el);
        requestAnimationFrame(function () {
            el.style.opacity = '1';
            el.style.transform = 'translateY(0) scale(1)';
        });

        var closeBtn = el.querySelector('button');
        function dismiss() {
            el.style.opacity = '0';
            el.style.transform = 'translateY(8px) scale(.97)';
            setTimeout(function () { el.remove(); }, 230);
        }
        closeBtn.addEventListener('click', dismiss);
        var timer = setTimeout(dismiss, 4500);
        el.addEventListener('mouseenter', function () { clearTimeout(timer); });
        el.addEventListener('mouseleave', function () { timer = setTimeout(dismiss, 2000); });
    }

    window.showAlert = showAlert;

    /* ─── Confirm Modal ────────────────────────────────────────────── */
    let modalEl = null;
    let bsModal  = null;
    let pendingCb = null;

    const TYPES = {
        danger:  { icon: 'fa-trash-alt',           color: '#ef4444', bg: '#fef2f2', btnClass: 'btn-danger',  title: 'تأیید حذف'     },
        warning: { icon: 'fa-exclamation-triangle', color: '#f59e0b', bg: '#fffbeb', btnClass: 'btn-warning', title: 'تأیید عملیات' },
        info:    { icon: 'fa-question-circle',      color: '#3b82f6', bg: '#eff6ff', btnClass: 'btn-primary', title: 'تأیید'         },
    };

    function buildModal() {
        const el = document.createElement('div');
        el.id = 'appConfirmModal';
        el.className = 'modal fade';
        el.tabIndex = -1;
        el.setAttribute('data-bs-backdrop', 'static');
        el.innerHTML = `
<div class="modal-dialog modal-dialog-centered" style="max-width:380px">
  <div class="modal-content border-0 shadow-xl confirm-card">
    <div class="confirm-header">
      <div id="confirmCircle" class="confirm-circle">
        <i id="confirmIcon" class="fas fa-trash-alt"></i>
      </div>
      <h6 id="confirmTitle" class="confirm-title"></h6>
    </div>
    <div class="confirm-body">
      <p id="confirmMessage" class="confirm-msg"></p>
    </div>
    <div class="confirm-footer">
      <button type="button" class="btn confirm-cancel" id="confirmCancelBtn">
        <i class="fas fa-times me-1"></i>انصراف
      </button>
      <button type="button" class="btn confirm-ok fw-semibold" id="confirmOkBtn">
        <i class="fas fa-check me-1"></i>تأیید
      </button>
    </div>
  </div>
</div>`;
        document.body.appendChild(el);
        document.getElementById('confirmCancelBtn').addEventListener('click', () => bsModal.hide());
        document.getElementById('confirmOkBtn').addEventListener('click', () => {
            bsModal.hide();
            if (pendingCb) pendingCb();
        });
        return el;
    }

    function showConfirm(message, callback, type) {
        if (!modalEl) {
            modalEl = buildModal();
            bsModal  = new bootstrap.Modal(modalEl);
        }

        const cfg = TYPES[type] || TYPES.danger;

        const circle = document.getElementById('confirmCircle');
        circle.style.background = cfg.bg;
        circle.style.color      = cfg.color;
        circle.style.boxShadow  = `0 0 0 6px ${cfg.color}1a`;

        document.getElementById('confirmIcon').className    = 'fas ' + cfg.icon;
        document.getElementById('confirmTitle').textContent = cfg.title;
        document.getElementById('confirmMessage').textContent = message;

        const okBtn = document.getElementById('confirmOkBtn');
        okBtn.className = `btn confirm-ok fw-semibold ${cfg.btnClass}`;

        pendingCb = callback;
        bsModal.show();
    }

    // Intercept buttons with data-confirm (capture phase so it fires before form submit)
    document.addEventListener('click', function (e) {
        const btn = e.target.closest('button[data-confirm]');
        if (!btn) return;
        e.preventDefault();
        e.stopImmediatePropagation();
        const msg  = btn.getAttribute('data-confirm');
        const type = btn.getAttribute('data-confirm-type') || 'danger';
        showConfirm(msg, () => {
            btn.removeAttribute('data-confirm');
            btn.click();
        }, type);
    }, true);

    // Intercept forms with data-confirm
    document.addEventListener('submit', function (e) {
        const form = e.target;
        if (!form.hasAttribute('data-confirm')) return;
        e.preventDefault();
        e.stopImmediatePropagation();
        const msg  = form.getAttribute('data-confirm');
        const type = form.getAttribute('data-confirm-type') || 'danger';
        showConfirm(msg, () => {
            form.removeAttribute('data-confirm');
            form.submit();
        }, type);
    }, true);

    window.showConfirm = showConfirm;
})();
