// Pricing guide per known provider (matched by name, case-insensitive)
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

function updatePricingGuide() {
    const select = document.getElementById('formProviderId');
    const selectedText = select.options[select.selectedIndex]?.text ?? '';
    const guide = document.getElementById('pricingGuide');
    const guideText = document.getElementById('pricingGuideText');

    if (!selectedText || select.value === '') {
        guideText.innerHTML = 'ابتدا یک سرویس‌دهنده انتخاب کنید تا راهنمای قیمت‌گذاری نمایش داده شود.';
        guide.classList.remove('d-none');
        return;
    }

    const match = PROVIDER_PRICING.find(p => p.match.test(selectedText));
    guideText.innerHTML = match
        ? match.text
        : `به صفحه مدل‌ها یا pricing در سایت «${selectedText}» مراجعه کن.`;
    guide.classList.remove('d-none');
}

function initTooltips() {
    document.querySelectorAll('[data-bs-toggle="tooltip"]').forEach(el => {
        new bootstrap.Tooltip(el, { placement: 'top' });
    });
}

function openCreate() {
    document.getElementById('modalTitle').textContent = 'مدل جدید';
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
    updatePricingGuide();
}

function openEdit(id, providerId, name, modelId, description, capabilitiesJson, costToken, costImage, costVideo, maxTokens, isDefault, sortOrder) {
    document.getElementById('modalTitle').textContent = 'ویرایش مدل';
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

    var caps = [];
    try { caps = JSON.parse(capabilitiesJson); } catch (e) {}
    document.querySelectorAll('.capability-check').forEach(cb => {
        cb.checked = caps.includes(cb.value);
    });

    updatePricingGuide();

    new bootstrap.Modal(document.getElementById('modelModal')).show();
}

document.addEventListener('DOMContentLoaded', () => {
    initTooltips();
    document.getElementById('formProviderId')?.addEventListener('change', updatePricingGuide);
});
