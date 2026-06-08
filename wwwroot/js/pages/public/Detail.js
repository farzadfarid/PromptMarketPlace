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
});
