/* ─── Audio upload ─────────────────────────────────────────── */
function handleAudioUpload(input, fieldName) {
    var preview     = document.getElementById('uploadPreview_'     + fieldName);
    var placeholder = document.getElementById('uploadPlaceholder_' + fieldName);
    var nameEl      = document.getElementById('uploadFileName_'    + fieldName);
    var area        = document.getElementById('uploadArea_'        + fieldName);
    if (!input.files || !input.files[0]) return;
    var file = input.files[0];
    if (file.size > 150 * 1024 * 1024) {
        if (window.showAlert) showAlert('حجم فایل نباید بیشتر از ۱۵۰ مگابایت باشد.', 'warning');
        else alert('حجم فایل نباید بیشتر از ۱۵۰ مگابایت باشد.');
        input.value = '';
        return;
    }
    if (nameEl)      nameEl.textContent = file.name;
    if (preview)     preview.classList.remove('d-none');
    if (placeholder) placeholder.classList.add('d-none');
    if (area)        area.style.borderColor = 'rgba(249,115,22,.5)';
}

/* ─── File upload ──────────────────────────────────────────── */
function handleFileUpload(input, fieldName) {
    var preview     = document.getElementById('uploadPreview_'  + fieldName);
    var placeholder = document.getElementById('uploadPlaceholder_' + fieldName);
    var img         = document.getElementById('previewImg_'     + fieldName);
    var nameEl      = document.getElementById('uploadFileName_' + fieldName);
    var area        = document.getElementById('uploadArea_'     + fieldName);
    if (!input.files || !input.files[0]) return;
    var file = input.files[0];
    if (file.size > 10 * 1024 * 1024) {
        if (window.showAlert) showAlert('حجم فایل نباید بیشتر از ۱۰ مگابایت باشد.', 'warning');
        else alert('حجم فایل نباید بیشتر از ۱۰ مگابایت باشد.');
        input.value = '';
        return;
    }
    var reader = new FileReader();
    reader.onload = function (e) {
        if (img)         img.src            = e.target.result;
        if (nameEl)      nameEl.textContent = file.name;
        if (preview)     preview.classList.remove('d-none');
        if (placeholder) placeholder.classList.add('d-none');
        if (area)        area.style.borderColor = 'rgba(249,115,22,.5)';
        try { sessionStorage.setItem('inputPreview_' + fieldName, e.target.result); } catch (ex) {}
    };
    reader.readAsDataURL(file);
}

/* ─── Skeleton loading ─────────────────────────────────────── */
var SKELETON_MAP = {
    text:  { id: 'skelText',  label: 'در حال تولید متن...' },
    code:  { id: 'skelCode',  label: 'در حال تولید کد...' },
    form:  { id: 'skelText',  label: 'در حال پردازش...' },
    image: { id: 'skelImage', label: 'در حال تولید تصویر...' },
    video: { id: 'skelVideo', label: 'در حال تولید ویدیو...' },
    audio: { id: 'skelAudio', label: 'در حال تولید صدا...' },
};

function showSkeleton(outputType) {
    var skeleton = document.getElementById('outputSkeleton');
    var label    = document.getElementById('skelLabel');
    if (!skeleton) return;

    // Hide all type-specific skeletons first
    ['skelText', 'skelImage', 'skelVideo', 'skelAudio', 'skelCode'].forEach(function (id) {
        var el = document.getElementById(id);
        if (el) el.classList.add('d-none');
    });

    var cfg = SKELETON_MAP[outputType] || SKELETON_MAP['text'];
    var typeEl = document.getElementById(cfg.id);
    if (typeEl)  typeEl.classList.remove('d-none');
    if (label)   label.textContent = cfg.label;
    skeleton.classList.remove('d-none');

    // Scroll to skeleton
    skeleton.scrollIntoView({ behavior: 'smooth', block: 'start' });
}

/* ─── DOMContentLoaded ─────────────────────────────────────── */
document.addEventListener('DOMContentLoaded', function () {

    /* ── Star rating ─────────────────────────────────────────── */
    var starContainer = document.getElementById('starRating');
    if (starContainer) {
        var starLabels  = Array.from(starContainer.querySelectorAll('.star-lbl'));
        var ratingInput = document.getElementById('ratingValue');
        var starError   = document.getElementById('starError');
        var selectedVal = 0;

        function paintStars(upTo) {
            starLabels.forEach(function (lbl) {
                lbl.querySelector('i').style.color = parseInt(lbl.dataset.val) <= upTo
                    ? '#f59e0b'
                    : '#2a3a5a';
            });
        }

        starLabels.forEach(function (lbl) {
            lbl.addEventListener('mouseenter', function () { paintStars(parseInt(this.dataset.val)); });
            lbl.addEventListener('click', function () {
                selectedVal = parseInt(this.dataset.val);
                if (ratingInput) ratingInput.value = selectedVal;
                if (starError)   starError.style.display = 'none';
                paintStars(selectedVal);
            });
        });
        starContainer.addEventListener('mouseleave', function () { paintStars(selectedVal); });

        var reviewForm = starContainer.closest('form');
        if (reviewForm) {
            reviewForm.addEventListener('submit', function (e) {
                if (selectedVal < 1) {
                    e.preventDefault();
                    if (starError) starError.style.display = 'block';
                    starContainer.scrollIntoView({ behavior: 'smooth', block: 'center' });
                }
            });
        }
    }

    /* ── Scroll to output if already present ─────────────────── */
    var output = document.querySelector('.output-result');
    if (output) {
        setTimeout(function () {
            output.scrollIntoView({ behavior: 'smooth', block: 'start' });
        }, 120);
    }

    /* ── Input preview strip (after image execution) ─────────── */
    var inputPreviewContainer = document.getElementById('inputPreviewContainer');
    if (output && inputPreviewContainer) {
        var form = document.getElementById('runForm');
        var outputType = form ? (form.dataset.outputType || '').toLowerCase() : '';
        if (outputType === 'image' || outputType === 'video') {
            var previews = [];
            document.querySelectorAll('.upload-drop-area').forEach(function (area) {
                var fieldName = area.dataset.fieldname;
                if (!fieldName) return;
                var src = null;
                try { src = sessionStorage.getItem('inputPreview_' + fieldName); } catch (ex) {}
                if (src) previews.push(src);
            });
            if (previews.length > 0) {
                var label = document.createElement('span');
                label.className = 'input-preview-label';
                label.innerHTML = '<i class="fas fa-image me-1"></i>ورودی شما';
                inputPreviewContainer.appendChild(label);
                previews.forEach(function (src) {
                    var img = document.createElement('img');
                    img.src       = src;
                    img.className = 'input-preview-thumb';
                    img.alt       = 'ورودی';
                    img.addEventListener('click', function () {
                        var titleEl = document.getElementById('showcaseModalTitle');
                        var bodyEl  = document.getElementById('showcaseModalBody');
                        if (titleEl) titleEl.textContent = 'تصویر ورودی شما';
                        if (bodyEl) {
                            bodyEl.innerHTML = '';
                            var fullImg = document.createElement('img');
                            fullImg.src       = src;
                            fullImg.className = 'img-fluid rounded';
                            bodyEl.appendChild(fullImg);
                        }
                        document.getElementById('showcaseModalTrigger').click();
                    });
                    inputPreviewContainer.appendChild(img);
                });
                inputPreviewContainer.classList.remove('d-none');
            }
        }
    }

    /* ── Run form: skeleton + button state ───────────────────── */
    var runForm = document.getElementById('runForm');
    if (runForm) {
        runForm.addEventListener('submit', function (e) {

            // 1. Validate required FileUpload / AudioUpload fields
            var valid = true;
            document.querySelectorAll('.upload-drop-area[data-required="true"]').forEach(function (area) {
                var fieldName = area.dataset.fieldname;
                var input     = document.getElementById('file_' + fieldName);
                var errEl     = document.getElementById('uploadError_' + fieldName);
                if (!input || !input.files || input.files.length === 0) {
                    valid = false;
                    area.style.borderColor = '#ef4444';
                    if (errEl) errEl.classList.remove('d-none');
                    area.scrollIntoView({ behavior: 'smooth', block: 'center' });
                } else {
                    area.style.borderColor = '';
                    if (errEl) errEl.classList.add('d-none');
                }
            });
            if (!valid) { e.preventDefault(); return; }

            // 2. Determine output type
            var outputType = (runForm.dataset.outputType || 'text').toLowerCase();

            // 3. Show skeleton
            showSkeleton(outputType);

            // 4. Disable button with loading state
            var btn = document.getElementById('runBtn');
            if (btn) {
                btn.disabled = true;
                btn.innerHTML = '<span class="spinner-border spinner-border-sm me-2" style="width:1rem;height:1rem;"></span>در حال اجرا...';
            }
        });
    }

    /* ── Showcase modal ──────────────────────────────────────── */
    document.querySelectorAll('.showcase-item').forEach(function (el) {
        el.addEventListener('click', function () {
            var caption = this.dataset.caption || '';
            var text    = this.dataset.text    || '';
            var url     = this.dataset.imgurl  || '';
            var type    = (this.dataset.type   || '').toLowerCase();
            var titleEl = document.getElementById('showcaseModalTitle');
            var bodyEl  = document.getElementById('showcaseModalBody');

            if (titleEl) titleEl.textContent = caption || 'نمونه خروجی';
            if (!bodyEl) return;
            bodyEl.innerHTML = '';

            if (type === 'video' && url) {
                var vid = document.createElement('video');
                vid.controls   = true;
                vid.className  = 'w-100 rounded';
                vid.style.maxHeight = '500px';
                var src = document.createElement('source');
                src.src = url;
                vid.appendChild(src);
                bodyEl.appendChild(vid);
            } else if (type === 'audio' && url) {
                if (caption) {
                    var cap = document.createElement('p');
                    cap.className   = 'small mb-2';
                    cap.style.color = '#8897b0';
                    cap.textContent = caption;
                    bodyEl.appendChild(cap);
                }
                var aud = document.createElement('audio');
                aud.controls  = true;
                aud.className = 'w-100';
                var src2 = document.createElement('source');
                src2.src = url;
                aud.appendChild(src2);
                bodyEl.appendChild(aud);
            } else if (url && (type === 'image' || (!text && url))) {
                var img = document.createElement('img');
                img.src       = url;
                img.className = 'img-fluid rounded';
                bodyEl.appendChild(img);
            } else {
                var pre = document.createElement('pre');
                pre.style.whiteSpace  = 'pre-wrap';
                pre.style.fontFamily  = 'inherit';
                pre.style.fontSize    = '.9rem';
                pre.style.lineHeight  = '1.7';
                pre.style.color       = '#c5d0e0';
                pre.textContent       = text;
                bodyEl.appendChild(pre);
            }
        });
    });
});

/* ─── ImageSelect card click ──────────────────────────────── */
document.addEventListener('click', function(e) {
    var card = e.target.closest('.img-select-card');
    if (!card) return;
    var radio = card.querySelector('input[type=radio]');
    if (!radio) return;
    var name = radio.name;
    document.querySelectorAll('input[name="' + name + '"]').forEach(function(r) {
        r.closest('.img-select-card').classList.remove('selected');
    });
    radio.checked = true;
    card.classList.add('selected');
});

/* ─── Copy open prompt ────────────────────────────────────── */
document.addEventListener('DOMContentLoaded', function () {
    var btn = document.getElementById('copyPromptBtn');
    if (!btn) return;
    btn.addEventListener('click', function () {
        var text = document.getElementById('publicPromptText').textContent;
        navigator.clipboard.writeText(text).then(function () {
            var orig = btn.innerHTML;
            btn.innerHTML = '<i class="fas fa-check me-1"></i>کپی شد';
            setTimeout(function () { btn.innerHTML = orig; }, 2000);
        });
    });
});

/* ─── Shooting stars canvas animation ────────────────────── */
(function(){
    var c = document.getElementById('stars-canvas');
    if (!c) return;
    var cx = c.getContext('2d');

    function resize(){ c.width = innerWidth; c.height = innerHeight; }
    resize();
    window.addEventListener('resize', resize);

    var COLS = ['249,115,22','251,146,60','255,255,255','255,255,255'];
    var pool = [];

    function mkStar(stagger) {
        var fromTop = Math.random() > 0.35;
        var ang = 0.72 + (Math.random() - 0.5) * 0.45;
        var spd = Math.random() * 1.1 + 0.35;
        return {
            x: fromTop ? Math.random() * c.width * 1.1 : -15,
            y: fromTop ? -15 : Math.random() * c.height * 0.55,
            vx: Math.cos(ang) * spd,
            vy: Math.sin(ang) * spd,
            len: Math.random() * 100 + 45,
            w:   Math.random() * 1.3 + 0.25,
            col: COLS[Math.floor(Math.random() * COLS.length)],
            maxOp: Math.random() * 0.5 + 0.3,
            op: 0, age: 0,
            delay: stagger ? Math.floor(Math.random() * 260) : Math.floor(Math.random() * 80) + 20
        };
    }

    for (var i = 0; i < 14; i++) pool.push(mkStar(true));

    function frame(){
        cx.clearRect(0, 0, c.width, c.height);
        for (var i = 0; i < pool.length; i++) {
            var s = pool[i];
            if (s.delay > 0) { s.delay--; continue; }
            s.age++;
            s.op = Math.min(s.maxOp, s.age * 0.033 * s.maxOp);

            var edgeFade = Math.min(1, Math.min(c.width - s.x, c.height - s.y) / 110);
            var op = s.op * Math.max(0, edgeFade);

            if (op > 0.008) {
                var spd = Math.sqrt(s.vx*s.vx + s.vy*s.vy);
                var ux = s.vx/spd, uy = s.vy/spd;
                var tx = s.x - ux*s.len, ty = s.y - uy*s.len;

                var g = cx.createLinearGradient(tx, ty, s.x, s.y);
                g.addColorStop(0,   'rgba('+s.col+',0)');
                g.addColorStop(0.6, 'rgba('+s.col+','+(op*0.35)+')');
                g.addColorStop(1,   'rgba('+s.col+','+op+')');
                cx.beginPath(); cx.moveTo(tx,ty); cx.lineTo(s.x,s.y);
                cx.strokeStyle = g; cx.lineWidth = s.w; cx.lineCap = 'round'; cx.stroke();

                var r = s.w * 2.8;
                var glow = cx.createRadialGradient(s.x,s.y,0,s.x,s.y,r);
                glow.addColorStop(0,'rgba('+s.col+','+op+')');
                glow.addColorStop(1,'rgba('+s.col+',0)');
                cx.beginPath(); cx.arc(s.x,s.y,r,0,6.2832);
                cx.fillStyle = glow; cx.fill();
            }

            s.x += s.vx; s.y += s.vy;
            if (s.x > c.width+130 || s.y > c.height+130) pool[i] = mkStar(false);
        }
        requestAnimationFrame(frame);
    }
    requestAnimationFrame(frame);
})();
