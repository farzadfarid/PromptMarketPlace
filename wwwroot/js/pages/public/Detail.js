function handleFileUpload(input, fieldName) {
    var preview = document.getElementById('uploadPreview_' + fieldName);
    var placeholder = document.getElementById('uploadPlaceholder_' + fieldName);
    var img = document.getElementById('previewImg_' + fieldName);
    var nameEl = document.getElementById('uploadFileName_' + fieldName);
    var area = document.getElementById('uploadArea_' + fieldName);
    if (!input.files || !input.files[0]) return;
    var file = input.files[0];
    if (file.size > 10 * 1024 * 1024) {
        alert('حجم فایل نباید بیشتر از ۱۰ مگابایت باشد.');
        input.value = '';
        return;
    }
    var reader = new FileReader();
    reader.onload = function (e) {
        if (img) img.src = e.target.result;
        if (nameEl) nameEl.textContent = file.name;
        if (preview) preview.classList.remove('d-none');
        if (placeholder) placeholder.classList.add('d-none');
        if (area) area.style.background = 'rgba(249,115,22,.04)';
    };
    reader.readAsDataURL(file);
}

document.addEventListener('DOMContentLoaded', function () {
    // Star rating
    var starContainer = document.getElementById('starRating');
    if (starContainer) {
        var starLabels = Array.from(starContainer.querySelectorAll('.star-lbl'));
        var ratingInput = document.getElementById('ratingValue');
        var starError = document.getElementById('starError');
        var selectedVal = 0;

        function paintStars(upTo) {
            starLabels.forEach(function (lbl) {
                lbl.querySelector('i').style.color = parseInt(lbl.dataset.val) <= upTo ? '#f97316' : '#ddd';
            });
        }

        starLabels.forEach(function (lbl) {
            lbl.addEventListener('mouseenter', function () {
                paintStars(parseInt(this.dataset.val));
            });
            lbl.addEventListener('click', function () {
                selectedVal = parseInt(this.dataset.val);
                if (ratingInput) ratingInput.value = selectedVal;
                if (starError) starError.style.display = 'none';
                paintStars(selectedVal);
            });
        });

        starContainer.addEventListener('mouseleave', function () {
            paintStars(selectedVal);
        });

        // validate before submit
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

    // Scroll to output result after execution
    var output = document.querySelector('.output-result');
    if (output) {
        output.scrollIntoView({ behavior: 'smooth', block: 'start' });
    }

    // Disable submit button while loading
    var form = document.querySelector('form[action*="Run"]');
    if (form) {
        form.addEventListener('submit', function () {
            var btn = form.querySelector('button[type="submit"]');
            if (btn) {
                btn.disabled = true;
                btn.innerHTML = '<span class="spinner-border spinner-border-sm me-2"></span>در حال اجرا...';
            }
        });
    }

    // Showcase modal
    document.querySelectorAll('.showcase-item').forEach(function (el) {
        el.addEventListener('click', function () {
            var caption = this.dataset.caption || '';
            var text = this.dataset.text || '';
            var url = this.dataset.imgurl || '';
            var type = (this.dataset.type || '').toLowerCase();
            var titleEl = document.getElementById('showcaseModalTitle');
            var bodyEl = document.getElementById('showcaseModalBody');
            if (titleEl) titleEl.textContent = caption || 'نمونه خروجی';
            if (!bodyEl) return;
            bodyEl.innerHTML = '';

            if (type === 'video' && url) {
                var vid = document.createElement('video');
                vid.controls = true;
                vid.className = 'w-100 rounded';
                vid.style.maxHeight = '500px';
                var src = document.createElement('source');
                src.src = url;
                vid.appendChild(src);
                bodyEl.appendChild(vid);
            } else if (type === 'audio' && url) {
                var aud = document.createElement('audio');
                aud.controls = true;
                aud.className = 'w-100 mt-3';
                var src2 = document.createElement('source');
                src2.src = url;
                aud.appendChild(src2);
                if (caption) {
                    var cap = document.createElement('p');
                    cap.className = 'text-muted small mt-2 mb-0';
                    cap.textContent = caption;
                    bodyEl.appendChild(cap);
                }
                bodyEl.appendChild(aud);
            } else if (url && (type === 'image' || (!text && url))) {
                var img = document.createElement('img');
                img.src = url;
                img.className = 'img-fluid rounded';
                bodyEl.appendChild(img);
            } else {
                var pre = document.createElement('pre');
                pre.style.whiteSpace = 'pre-wrap';
                pre.style.fontFamily = 'inherit';
                pre.style.fontSize = '.9rem';
                pre.style.lineHeight = '1.7';
                pre.textContent = text;
                bodyEl.appendChild(pre);
            }
        });
    });
});
