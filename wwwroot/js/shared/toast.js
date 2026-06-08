document.addEventListener('DOMContentLoaded', function () {
    document.querySelectorAll('.toast.show').forEach(function (el) {
        setTimeout(function () {
            bootstrap.Toast.getOrCreateInstance(el).hide();
        }, 4000);
    });

    // Hide validation-summary containers when there are no errors
    document.querySelectorAll('.validation-summary-valid').forEach(function (ul) {
        var alert = ul.closest('.alert');
        if (alert) alert.style.display = 'none';
    });
});
