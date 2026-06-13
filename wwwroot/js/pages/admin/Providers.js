function openCreate() {
    document.getElementById('modalTitle').textContent = 'سرویس‌دهنده جدید';
    document.getElementById('formId').value = '0';
    document.querySelector('[name="Form.Name"]').value = '';
    document.querySelector('[name="Form.BaseUrl"]').value = '';
    document.querySelector('[name="Form.Description"]').value = '';
    document.getElementById('formApiKeyInput').value = '';
    document.getElementById('formProviderType').value = '0';
    document.getElementById('apiKeyHint').classList.add('d-none');
    document.getElementById('showApiKeyBtn').classList.add('d-none');
    resetModalTestResult();
}

function openEdit(id, name, baseUrl, description, hasApiKey, providerType) {
    document.getElementById('modalTitle').textContent = 'ویرایش سرویس‌دهنده';
    document.getElementById('formId').value = id;
    document.querySelector('[name="Form.Name"]').value = name;
    document.querySelector('[name="Form.BaseUrl"]').value = baseUrl;
    document.querySelector('[name="Form.Description"]').value = description;
    document.getElementById('formApiKeyInput').value = '';
    document.getElementById('formProviderType').value = providerType ?? 0;
    document.getElementById('apiKeyHint').classList[hasApiKey ? 'remove' : 'add']('d-none');
    var showBtn = document.getElementById('showApiKeyBtn');
    showBtn.classList[hasApiKey ? 'remove' : 'add']('d-none');
    showBtn.innerHTML = '<i class="fas fa-eye"></i>';
    resetModalTestResult();
    new bootstrap.Modal(document.getElementById('providerModal')).show();
}

async function showApiKey() {
    var id = document.getElementById('formId').value;
    if (!id || id === '0') return;
    var btn = document.getElementById('showApiKeyBtn');
    var input = document.getElementById('formApiKeyInput');
    if (input.value) { input.value = ''; btn.innerHTML = '<i class="fas fa-eye"></i>'; return; }
    btn.innerHTML = '<span class="spinner-border spinner-border-sm"></span>';
    try {
        var resp = await fetch('?handler=DecryptedKey&id=' + id);
        var data = await resp.json();
        if (data.success) { input.value = data.key; btn.innerHTML = '<i class="fas fa-eye-slash"></i>'; }
        else { btn.innerHTML = '<i class="fas fa-eye"></i>'; alert('دریافت API Key ناموفق بود.'); }
    } catch { btn.innerHTML = '<i class="fas fa-eye"></i>'; }
}

function resetModalTestResult() {
    const el = document.getElementById('modalTestResult');
    el.className = 'alert py-2 mb-0 d-none';
}

// ─── Capability Toggle ────────────────────────────────────────────────────────

async function toggleCapability(btn) {
    const providerId = parseInt(btn.dataset.providerId);
    const capability = btn.dataset.capability;
    const isCurrentlyActive = btn.dataset.active === 'true';
    const newActive = !isCurrentlyActive;

    btn.disabled = true;
    const originalHtml = btn.innerHTML;
    btn.innerHTML = '<span class="spinner-border spinner-border-sm"></span>';

    const token = document.querySelector('input[name="__RequestVerificationToken"]')?.value ?? '';
    try {
        const resp = await fetch('?handler=SetCapability', {
            method: 'POST',
            headers: { 'Content-Type': 'application/json', 'RequestVerificationToken': token },
            body: JSON.stringify({ providerId, capability, isActive: newActive })
        });
        const data = await resp.json();

        if (!data.success) {
            btn.innerHTML = originalHtml;
            btn.disabled = false;
            alert(data.message || 'خطا در تغییر وضعیت');
            return;
        }

        // آپدیت UI: همه دکمه‌های همین capability را غیرفعال کن، سپس این یکی را فعال
        document.querySelectorAll(`.capability-toggle[data-capability="${capability}"]`).forEach(b => {
            const isThis = parseInt(b.dataset.providerId) === providerId;
            const shouldBeActive = isThis && newActive;
            b.dataset.active = shouldBeActive ? 'true' : 'false';
            b.className = `btn btn-sm capability-toggle ${shouldBeActive ? 'btn-success' : 'btn-outline-secondary'}`;
            b.innerHTML = `<i class="fas ${shouldBeActive ? 'fa-check-circle' : 'fa-circle'}"></i>`;
            b.title = shouldBeActive ? 'فعال — کلیک برای غیرفعال' : 'کلیک برای فعال کردن';
            b.disabled = false;
        });

        // آپدیت کارت‌های خلاصه در بالا
        updateSummaryCard(capability);

    } catch {
        btn.innerHTML = originalHtml;
        btn.disabled = false;
        alert('خطا در ارتباط با سرور');
    }
}

function updateSummaryCard(capability) {
    // پیدا کردن provider فعال برای این capability
    const activeBtns = document.querySelectorAll(
        `.capability-toggle[data-capability="${capability}"][data-active="true"]`
    );
    const capIndex = { Text: 0, Image: 1, Video: 2, Audio: 3 }[capability];
    const summaryCards = document.querySelectorAll('.col-6.col-md-3 .fw-semibold');
    if (summaryCards[capIndex] == null) return;

    if (activeBtns.length > 0) {
        const row = activeBtns[0].closest('tr');
        const name = row?.querySelector('td .fw-semibold')?.textContent ?? '';
        summaryCards[capIndex].textContent = name;
        summaryCards[capIndex].className = 'fw-semibold text-success small';
    } else {
        summaryCards[capIndex].textContent = 'تنظیم نشده';
        summaryCards[capIndex].className = 'fw-semibold text-warning small';
    }
}

// ─── Test Connection ──────────────────────────────────────────────────────────

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
    document.getElementById('modalTestIcon').className =
        `fas me-1 ${result.success ? 'fa-check-circle' : 'fa-times-circle'}`;
    document.getElementById('modalTestMessage').textContent = result.message;
    el.className = `alert py-2 mb-0 ${result.success ? 'alert-success' : 'alert-danger'}`;
}

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
            headers: { 'Content-Type': 'application/json', 'RequestVerificationToken': token },
            body: JSON.stringify({ baseUrl, apiKey, providerId })
        });
        if (!resp.ok) return { success: false, message: `خطای سرور: ${resp.status}` };
        return await resp.json();
    } catch {
        return { success: false, message: 'خطا در ارسال درخواست.' };
    }
}

// ─── Check Balance ────────────────────────────────────────────────────────────

async function checkBalance(btn, providerId) {
    const loading = document.getElementById('balanceLoading');
    const result  = document.getElementById('balanceResult');
    const error   = document.getElementById('balanceError');

    loading.classList.remove('d-none');
    result.classList.add('d-none');
    error.classList.add('d-none');

    new bootstrap.Modal(document.getElementById('balanceModal')).show();

    const token = document.querySelector('input[name="__RequestVerificationToken"]')?.value ?? '';
    try {
        const resp = await fetch('?handler=CheckBalance', {
            method: 'POST',
            headers: { 'Content-Type': 'application/json', 'RequestVerificationToken': token },
            body: JSON.stringify({ providerId })
        });
        const data = await resp.json();
        loading.classList.add('d-none');

        if (data.success) {
            document.getElementById('balanceValue').textContent = data.value;
            document.getElementById('balanceLabel').textContent = data.label;
            result.classList.remove('d-none');
        } else {
            error.textContent = data.message;
            error.classList.remove('d-none');
        }
    } catch {
        loading.classList.add('d-none');
        error.textContent = 'خطا در دریافت اطلاعات.';
        error.classList.remove('d-none');
    }
}
