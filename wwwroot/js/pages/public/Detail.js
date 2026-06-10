document.addEventListener('DOMContentLoaded', function () {
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
            var imgUrl = this.dataset.imgurl || '';
            var titleEl = document.getElementById('showcaseModalTitle');
            var bodyEl = document.getElementById('showcaseModalBody');
            if (titleEl) titleEl.textContent = caption || 'نمونه خروجی';
            if (!bodyEl) return;
            if (imgUrl) {
                var img = document.createElement('img');
                img.src = imgUrl;
                img.className = 'img-fluid rounded';
                bodyEl.innerHTML = '';
                bodyEl.appendChild(img);
            } else {
                var pre = document.createElement('pre');
                pre.style.whiteSpace = 'pre-wrap';
                pre.style.fontFamily = 'inherit';
                pre.style.fontSize = '.9rem';
                pre.style.lineHeight = '1.7';
                pre.textContent = text;
                bodyEl.innerHTML = '';
                bodyEl.appendChild(pre);
            }
        });
    });
});
