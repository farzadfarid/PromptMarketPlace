function openCreate() {
    document.getElementById('modalTitle').textContent = 'سرویس‌دهنده جدید';
    document.getElementById('formId').value = '0';
    document.querySelector('[name="Form.Name"]').value = '';
    document.querySelector('[name="Form.BaseUrl"]').value = '';
    document.querySelector('[name="Form.Description"]').value = '';
    document.querySelector('[name="Form.ApiKey"]').value = '';
    document.getElementById('apiKeyHint').classList.add('d-none');
}

function openEdit(id, name, baseUrl, description, hasApiKey) {
    document.getElementById('modalTitle').textContent = 'ویرایش سرویس‌دهنده';
    document.getElementById('formId').value = id;
    document.querySelector('[name="Form.Name"]').value = name;
    document.querySelector('[name="Form.BaseUrl"]').value = baseUrl;
    document.querySelector('[name="Form.Description"]').value = description;
    document.querySelector('[name="Form.ApiKey"]').value = '';

    if (hasApiKey) {
        document.getElementById('apiKeyHint').classList.remove('d-none');
    } else {
        document.getElementById('apiKeyHint').classList.add('d-none');
    }

    var modal = new bootstrap.Modal(document.getElementById('providerModal'));
    modal.show();
}
