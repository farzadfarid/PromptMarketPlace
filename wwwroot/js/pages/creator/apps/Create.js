var capabilityMap = {
    'Text':  'TextGeneration',
    'Code':  'CodeGeneration',
    'Form':  'TextGeneration',
    'Image': 'ImageGeneration',
    'Video': 'VideoGeneration',
    'Audio': 'AudioGeneration'
};

function goToStep(n) {
    document.querySelectorAll('.wizard-step-content').forEach(function (el) {
        el.classList.add('d-none');
    });
    document.getElementById('step-' + n).classList.remove('d-none');

    for (var i = 1; i <= 3; i++) {
        var ind = document.getElementById('step-indicator-' + i);
        var active = i === n;
        ind.classList.toggle('border-primary', active);
        ind.classList.toggle('bg-primary', active);
        ind.classList.toggle('fw-semibold', active);
        ind.classList.toggle('text-muted', !active);
        var badge = ind.querySelector('.badge');
        if (badge) {
            badge.classList.toggle('bg-white', active);
            badge.classList.toggle('text-primary', active);
            badge.classList.toggle('bg-secondary', !active);
        }
    }
}

function filterModels(outputType) {
    var cap = capabilityMap[outputType];
    var select = document.getElementById('ai-model-select');
    select.innerHTML = '<option value="0">انتخاب مدل...</option>';
    var compatible = (createPageData.allModels || []).filter(function (m) {
        return m.Capabilities.includes(cap);
    });
    var defaultId = null;
    compatible.forEach(function (m) {
        var opt = document.createElement('option');
        opt.value = m.Id;
        opt.textContent = m.IsDefault ? m.Name + ' ★' : m.Name;
        select.appendChild(opt);
        if (m.IsDefault) defaultId = m.Id;
    });
    // انتخاب خودکار مدل پیش‌فرض این نوع خروجی
    if (defaultId) select.value = defaultId;
}

document.addEventListener('DOMContentLoaded', function () {
    var promptTextarea = document.getElementById('prompt-textarea');
    if (promptTextarea) {
        promptTextarea.addEventListener('input', function () {
            var vars = Array.from(this.value.matchAll(/\{(\w+)\}/g)).map(function (m) { return m[1]; });
            var unique = vars.filter(function (v, i) { return vars.indexOf(v) === i; });
            var el = document.getElementById('variable-list');
            if (el) {
                el.textContent = unique.length
                    ? 'متغیرها: ' + unique.map(function (v) { return '{' + v + '}'; }).join(', ')
                    : '';
            }
        });
    }
});
