// Pricing guide per known provider (matched by display name)
const PROVIDER_PRICING = [
    {
        match: /openrouter/i,
        text: 'در سایت OpenRouter، برو به صفحه <a href="https://openrouter.ai/models" target="_blank" rel="noopener">openrouter.ai/models</a>، روی نام مدل کلیک کن — قیمت input/output token نمایش داده می‌شود.'
    },
    {
        match: /chatqt/i,
        text: 'در <a href="https://console.chatqt.com" target="_blank" rel="noopener">console.chatqt.com</a> وارد شو، از بخش Models/Pricing قیمت هر مدل را می‌توانی ببینی.'
    },
    {
        match: /avalai|آوال/i,
        text: 'در داشبورد <a href="https://avalai.ir" target="_blank" rel="noopener">avalai.ir</a>، بخش مدل‌ها یا pricing را باز کن تا قیمت هر مدل نمایش داده شود.'
    },
    {
        match: /openai/i,
        text: 'در سایت OpenAI برو به <a href="https://platform.openai.com/docs/pricing" target="_blank" rel="noopener">platform.openai.com/docs/pricing</a> — قیمت‌ها به صورت per 1M token نوشته شده‌اند.'
    },
    {
        match: /anthropic|claude/i,
        text: 'در سایت Anthropic برو به <a href="https://www.anthropic.com/pricing" target="_blank" rel="noopener">anthropic.com/pricing</a> — قیمت‌ها به صورت per 1M token نوشته شده‌اند.'
    },
];

// ─── Tab switching ────────────────────────────────────────────────────────────

function switchTab(tab) {
    const isAuto = tab === 'auto';
    document.getElementById('tab-auto').classList.toggle('d-none', !isAuto);
    document.getElementById('tab-manual').classList.toggle('d-none', isAuto);
    document.getElementById('tab-auto-btn').classList.toggle('active', isAuto);
    document.getElementById('tab-manual-btn').classList.toggle('active', !isAuto);
    document.getElementById('importSaveBtn').classList.toggle('d-none', !isAuto);
    document.getElementById('manualSaveBtn').classList.toggle('d-none', isAuto);
    if (!isAuto) updatePricingGuide();
}

// ─── Pricing guide ────────────────────────────────────────────────────────────

function updatePricingGuide() {
    const select = document.getElementById('formProviderId');
    const selectedText = select.options[select.selectedIndex]?.text ?? '';
    const guideText = document.getElementById('pricingGuideText');

    if (!selectedText || select.value === '') {
        guideText.innerHTML = 'ابتدا یک سرویس‌دهنده انتخاب کنید تا راهنمای قیمت‌گذاری نمایش داده شود.';
        return;
    }

    const match = PROVIDER_PRICING.find(p => p.match.test(selectedText));
    guideText.innerHTML = match
        ? match.text
        : `به صفحه مدل‌ها یا pricing در سایت «${selectedText}» مراجعه کن.`;
}

// ─── Modal: ایجاد جدید ─────────────────────────────────────────────────────

function openCreate() {
    setModalTitle('مدل جدید', 'روش افزودن را انتخاب کنید');

    // نمایش تب‌ها
    document.getElementById('modalTabs').classList.remove('d-none');

    // پاک کردن فرم دستی
    resetManualForm();

    // Reset auto tab state
    document.getElementById('importProviderId').value = '';
    document.getElementById('importLoading').classList.add('d-none');
    document.getElementById('importError').classList.add('d-none');
    document.getElementById('importContent').classList.add('d-none');
    document.querySelectorAll('.import-cap-check').forEach(cb => cb.checked = false);

    // فعال کردن تب خودکار
    switchTab('auto');
}

// ─── Modal: ویرایش ────────────────────────────────────────────────────────

function openEdit(id, providerId, name, modelId, description, capabilitiesJson,
                  costToken, costImage, costVideo, maxTokens, isDefault, sortOrder) {

    setModalTitle('ویرایش مدل', name);

    // مخفی کردن تب‌ها در ویرایش
    document.getElementById('modalTabs').classList.add('d-none');

    // فعال کردن فقط تب دستی
    document.getElementById('tab-auto').classList.add('d-none');
    document.getElementById('tab-manual').classList.remove('d-none');
    document.getElementById('importSaveBtn').classList.add('d-none');
    document.getElementById('manualSaveBtn').classList.remove('d-none');

    // پر کردن فرم
    document.getElementById('formId').value = id;
    document.getElementById('formProviderId').value = providerId;
    document.getElementById('formName').value = name;
    document.getElementById('formModelId').value = modelId;
    document.getElementById('formDescription').value = description;
    document.getElementById('formSortOrder').value = sortOrder;
    document.getElementById('formCostToken').value = costToken !== 'null' ? costToken : '';
    document.getElementById('formCostImage').value = costImage !== 'null' ? costImage : '';
    document.getElementById('formCostVideo').value = costVideo !== 'null' ? costVideo : '';
    document.getElementById('formMaxTokens').value = maxTokens !== 'null' ? maxTokens : '';
    document.getElementById('formIsDefault').checked = isDefault;

    let caps = [];
    try { caps = JSON.parse(capabilitiesJson); } catch (e) {}
    document.querySelectorAll('.capability-check').forEach(cb => {
        cb.checked = caps.includes(cb.value);
    });

    updatePricingGuide();

    new bootstrap.Modal(document.getElementById('modelModal')).show();
}

// ─── Helper ───────────────────────────────────────────────────────────────────

function setModalTitle(title, subtitle) {
    document.getElementById('modalTitle').textContent = title;
    document.getElementById('modalSubtitle').textContent = subtitle;
}

function resetManualForm() {
    document.getElementById('formId').value = '0';
    document.getElementById('formProviderId').value = '';
    document.getElementById('formName').value = '';
    document.getElementById('formModelId').value = '';
    document.getElementById('formDescription').value = '';
    document.getElementById('formSortOrder').value = '0';
    document.getElementById('formCostToken').value = '';
    document.getElementById('formCostImage').value = '';
    document.getElementById('formCostVideo').value = '';
    document.getElementById('formMaxTokens').value = '';
    document.getElementById('formIsDefault').checked = false;
    document.querySelectorAll('.capability-check').forEach(cb => cb.checked = false);
}

// ─── Import از API ────────────────────────────────────────────────────────────

let currentImportProviderId = null;

async function loadProviderModels() {
    const providerSel = document.getElementById('importProviderId');
    const providerId = parseInt(providerSel.value);
    if (!providerId) {
        showImportError('ابتدا یک سرویس‌دهنده انتخاب کنید.');
        return;
    }

    currentImportProviderId = providerId;

    document.getElementById('importLoading').classList.remove('d-none');
    document.getElementById('importError').classList.add('d-none');
    document.getElementById('importContent').classList.add('d-none');

    const token = document.querySelector('input[name="__RequestVerificationToken"]')?.value ?? '';
    try {
        const resp = await fetch('?handler=FetchProviderModels', {
            method: 'POST',
            headers: { 'Content-Type': 'application/json', 'RequestVerificationToken': token },
            body: JSON.stringify({ providerId })
        });
        const data = await resp.json();
        document.getElementById('importLoading').classList.add('d-none');

        if (!data.success) {
            showImportError(data.message);
            return;
        }

        renderImportList(data.models);
        document.getElementById('importContent').classList.remove('d-none');
        document.getElementById('importSaveBtn').classList.remove('d-none');
    } catch {
        document.getElementById('importLoading').classList.add('d-none');
        showImportError('خطا در دریافت مدل‌ها.');
    }
}

function showImportError(msg) {
    const err = document.getElementById('importError');
    err.textContent = msg;
    err.classList.remove('d-none');
}

function renderImportList(models) {
    const total = models.length;
    const newCount = models.filter(m => !m.alreadyAdded).length;
    document.getElementById('importCountLabel').textContent = `${total} مدل یافت شد — ${newCount} مدل جدید`;

    const list = document.getElementById('importModelList');
    list.innerHTML = models.map(m => `
        <label class="d-flex align-items-center gap-2 px-3 py-2 border-bottom ${m.alreadyAdded ? 'text-muted bg-light' : ''}"
               style="cursor:${m.alreadyAdded ? 'default' : 'pointer'}">
            <input type="checkbox" class="form-check-input import-model-check flex-shrink-0"
                   value="${escHtml(m.id)}" ${m.alreadyAdded ? 'disabled checked' : ''} />
            <div>
                <div class="small fw-semibold">${escHtml(m.name || m.id)}</div>
                <code class="x-small text-muted">${escHtml(m.id)}</code>
                ${m.alreadyAdded ? '<span class="badge bg-secondary ms-1" style="font-size:.7rem">قبلاً اضافه شده</span>' : ''}
            </div>
        </label>`).join('');
}

function escHtml(str) {
    return String(str).replace(/&/g,'&amp;').replace(/</g,'&lt;').replace(/>/g,'&gt;').replace(/"/g,'&quot;');
}

function selectAllImport(checked) {
    document.querySelectorAll('.import-model-check:not(:disabled)').forEach(cb => cb.checked = checked);
}

async function saveImportedModels() {
    const selected = [...document.querySelectorAll('.import-model-check:not(:disabled):checked')]
        .map(cb => cb.value);

    if (!selected.length) {
        showImportError('حداقل یک مدل انتخاب کنید.');
        return;
    }

    const caps = [...document.querySelectorAll('.import-cap-check:checked')].map(cb => cb.value);
    if (!caps.length) {
        showImportError('حداقل یک قابلیت برای مدل‌های انتخابی تعیین کنید.');
        return;
    }

    const btn = document.getElementById('importSaveBtn');
    btn.disabled = true;
    btn.innerHTML = '<span class="spinner-border spinner-border-sm me-1"></span> در حال ذخیره...';

    const token = document.querySelector('input[name="__RequestVerificationToken"]')?.value ?? '';
    try {
        const resp = await fetch('?handler=ImportModels', {
            method: 'POST',
            headers: { 'Content-Type': 'application/json', 'RequestVerificationToken': token },
            body: JSON.stringify({ providerId: currentImportProviderId, models: selected, capabilities: caps })
        });
        const data = await resp.json();
        if (data.success) {
            bootstrap.Modal.getInstance(document.getElementById('modelModal'))?.hide();
            location.reload();
        } else {
            showImportError(data.message);
            btn.disabled = false;
            btn.innerHTML = '<i class="fas fa-save me-1"></i> ذخیره مدل‌های انتخابی';
        }
    } catch {
        showImportError('خطا در ذخیره.');
        btn.disabled = false;
        btn.innerHTML = '<i class="fas fa-save me-1"></i> ذخیره مدل‌های انتخابی';
    }
}

// ─── Init ─────────────────────────────────────────────────────────────────────

document.addEventListener('DOMContentLoaded', () => {
    document.querySelectorAll('[data-bs-toggle="tooltip"]').forEach(el =>
        new bootstrap.Tooltip(el, { placement: 'top' }));
    document.getElementById('formProviderId')?.addEventListener('change', updatePricingGuide);
});
