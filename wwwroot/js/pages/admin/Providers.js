function openCreate() {
    document.getElementById('modalTitle').textContent = 'سرویس‌دهنده جدید';
    document.getElementById('formId').value = '0';
    document.querySelector('[name="Form.Name"]').value = '';
    document.querySelector('[name="Form.BaseUrl"]').value = '';
    document.querySelector('[name="Form.Description"]').value = '';
    document.querySelector('[name="Form.ApiKey"]').value = '';
    document.getElementById('apiKeyHint').classList.add('d-none');
    resetModalTestResult();
}

function openEdit(id, name, baseUrl, description, hasApiKey) {
    document.getElementById('modalTitle').textContent = 'ویرایش سرویس‌دهنده';
    document.getElementById('formId').value = id;
    document.querySelector('[name="Form.Name"]').value = name;
    document.querySelector('[name="Form.BaseUrl"]').value = baseUrl;
    document.querySelector('[name="Form.Description"]').value = description;
    document.querySelector('[name="Form.ApiKey"]').value = '';
    document.getElementById('apiKeyHint').classList[hasApiKey ? 'remove' : 'add']('d-none');
    resetModalTestResult();

    new bootstrap.Modal(document.getElementById('providerModal')).show();
}

function resetModalTestResult() {
    const el = document.getElementById('modalTestResult');
    el.classList.add('d-none');
    el.className = 'alert py-2 mb-0 d-none';
}

// Test button inside the modal
async function testConnectionFromModal() {
    const baseUrl = document.querySelector('[name="Form.BaseUrl"]').value.trim();
    const apiKey = document.querySelector('[name="Form.ApiKey"]').value.trim();
    const providerId = parseInt(document.getElementById('formId').value) || null;

    const btn = document.getElementById('modalTestBtn');
    btn.disabled = true;
    btn.innerHTML = '<span class="spinner-border spinner-border-sm me-1"></span> در حال تست...';

    const result = await doTestConnection(baseUrl, apiKey || null, providerId > 0 ? providerId : null);

    btn.disabled = false;
    btn.innerHTML = '<i class="fas fa-plug me-1"></i> تست اتصال';

    const el = document.getElementById('modalTestResult');
    const icon = document.getElementById('modalTestIcon');
    const msg = document.getElementById('modalTestMessage');

    el.className = `alert py-2 mb-0 ${result.success ? 'alert-success' : 'alert-danger'}`;
    icon.className = `fas me-1 ${result.success ? 'fa-check-circle' : 'fa-times-circle'}`;
    msg.textContent = result.message;
    el.classList.remove('d-none');
}

// Test button in the table row
async function testConnectionByRow(btn, providerId, baseUrl) {
    const originalHtml = btn.innerHTML;
    btn.disabled = true;
    btn.innerHTML = '<span class="spinner-border spinner-border-sm"></span>';

    const result = await doTestConnection(baseUrl, null, providerId);

    btn.disabled = false;
    if (result.success) {
        btn.innerHTML = '<i class="fas fa-check"></i>';
        btn.classList.replace('btn-outline-info', 'btn-outline-success');
        btn.title = result.message;
    } else {
        btn.innerHTML = '<i class="fas fa-times"></i>';
        btn.classList.replace('btn-outline-info', 'btn-outline-danger');
        btn.title = result.message;
    }

    setTimeout(() => {
        btn.innerHTML = originalHtml;
        btn.classList.remove('btn-outline-success', 'btn-outline-danger');
        btn.classList.add('btn-outline-info');
        btn.title = 'تست اتصال';
    }, 4000);
}

async function doTestConnection(baseUrl, apiKey, providerId) {
    const token = document.querySelector('input[name="__RequestVerificationToken"]')?.value ?? '';
    try {
        const resp = await fetch('?handler=TestConnection', {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json',
                'RequestVerificationToken': token
            },
            body: JSON.stringify({ baseUrl, apiKey, providerId })
        });
        if (!resp.ok) return { success: false, message: `خطای سرور: ${resp.status}` };
        return await resp.json();
    } catch {
        return { success: false, message: 'خطا در ارسال درخواست.' };
    }
}
