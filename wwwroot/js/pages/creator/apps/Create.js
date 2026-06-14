// ─── Wizard ────────────────────────────────────────────────────────────────

var capabilityMap = {
    Text: 'TextGeneration', Code: 'CodeGeneration', Form: 'TextGeneration',
    Image: 'ImageGeneration', Video: 'VideoGeneration', Audio: 'AudioGeneration'
};

function goToStep(n) {
    document.querySelectorAll('.wizard-step-content').forEach(function(el) {
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
    if (typeof syncVideoTagsHelper === 'function') syncVideoTagsHelper();
}

function filterModels(outputType) {
    var cap = capabilityMap[outputType];
    var select = document.getElementById('ai-model-select');
    select.innerHTML = '<option value="0">انتخاب مدل...</option>';
    var compatible = (createPageData.allModels || []).filter(function(m) {
        return m.Capabilities.includes(cap);
    });
    var defaultId = null;
    compatible.forEach(function(m) {
        var opt = document.createElement('option');
        opt.value = m.Id;
        opt.textContent = m.IsDefault ? m.Name + ' ★' : m.Name;
        select.appendChild(opt);
        if (m.IsDefault) defaultId = m.Id;
    });
    if (defaultId) select.value = defaultId;
    if (typeof syncVideoTagsHelper === 'function') syncVideoTagsHelper();
}

// ─── Field management ──────────────────────────────────────────────────────

var definedFields = [];

var fieldTypeLabels = {
    Text: 'متن ساده', Textarea: 'چند خطی', Select: 'انتخابی',
    Number: 'عدد', Checkbox: 'بله/خیر', DatePicker: 'تاریخ', FileUpload: 'آپلود تصویر', ImageSelect: 'انتخاب تصویری', AudioUpload: 'آپلود صدا'
};
var fieldTypeIcons = {
    Text: 'fa-align-left', Textarea: 'fa-paragraph', Select: 'fa-list',
    Number: 'fa-hashtag', Checkbox: 'fa-check-square', DatePicker: 'fa-calendar-alt', FileUpload: 'fa-file-upload', ImageSelect: 'fa-th-large', AudioUpload: 'fa-microphone'
};

function toPersian(n) {
    return String(n).replace(/\d/g, function(d) { return '۰۱۲۳۴۵۶۷۸۹'[d]; });
}

function addField() {
    var nameEl = document.getElementById('nf-name');
    var labelEl = document.getElementById('nf-label');
    var name = nameEl.value.trim().toLowerCase().replace(/\s+/g, '_');
    var label = labelEl.value.trim();
    var type = document.getElementById('nf-type').value;
    var placeholder = document.getElementById('nf-placeholder').value.trim();
    var required = document.getElementById('nf-required').checked;
    var optionsRaw = document.getElementById('nf-options').value.trim();

    var nameErr = document.getElementById('nf-name-err');
    var labelErr = document.getElementById('nf-label-err');
    nameErr.classList.add('d-none');
    labelErr.classList.add('d-none');
    nameEl.classList.remove('is-invalid');
    labelEl.classList.remove('is-invalid');

    var valid = true;
    if (!name || !/^[a-z0-9_]+$/.test(name)) {
        nameErr.textContent = 'فقط حروف کوچک انگلیسی، عدد و _ مجاز است.';
        nameErr.classList.remove('d-none');
        nameEl.classList.add('is-invalid');
        valid = false;
    } else if (definedFields.some(function(f, idx) { return f.name === name && idx !== _editingIndex; })) {
        nameErr.textContent = 'این نام قبلاً استفاده شده.';
        nameErr.classList.remove('d-none');
        nameEl.classList.add('is-invalid');
        valid = false;
    }
    if (!label) {
        labelErr.textContent = 'عنوان نمایشی الزامی است.';
        labelErr.classList.remove('d-none');
        labelEl.classList.add('is-invalid');
        valid = false;
    }
    if (!valid) return;

    var options = null;
    if (type === 'Select' && optionsRaw) {
        var lines = optionsRaw.split('\n').map(function(l) { return l.trim(); }).filter(Boolean);
        options = JSON.stringify(lines.map(function(l) { return { value: l, label: l }; }));
    }
    if (type === 'ImageSelect') {
        var imgRows = document.querySelectorAll('#nf-imgopt-rows .img-opt-row');
        var imgOpts = [];
        imgRows.forEach(function(row) {
            var lbl = row.querySelector('.img-opt-label').value.trim();
            var img = row.querySelector('.img-opt-image').value.trim();
            var val = row.querySelector('.img-opt-value').value.trim() || lbl;
            if (lbl) imgOpts.push({ label: lbl, value: val, image: img });
        });
        if (imgOpts.length > 0) options = JSON.stringify(imgOpts);
    }

    if (_editingIndex >= 0) {
        definedFields[_editingIndex] = { name: name, label: label, type: type, placeholder: placeholder, required: required, options: options };
        _editingIndex = -1;
        var btn = document.getElementById('add-field-btn');
        btn.textContent = '+ افزودن فیلد';
        btn.classList.add('btn-outline-primary');
        btn.classList.remove('btn-warning');
    } else {
        definedFields.push({ name: name, label: label, type: type, placeholder: placeholder, required: required, options: options });
    }

    renderFields();
    updateQuickpick();
    updateVariableChips();

    // reset form
    nameEl.value = ''; labelEl.value = '';
    document.getElementById('nf-placeholder').value = '';
    document.getElementById('nf-required').checked = true;
    document.getElementById('nf-options').value = '';
    document.getElementById('nf-options-wrap').classList.add('d-none');
    var imgoptWrap = document.getElementById('nf-imgopt-wrap');
    if (imgoptWrap) {
        imgoptWrap.classList.add('d-none');
        document.getElementById('nf-imgopt-rows').innerHTML = '';
    }
    nameEl.focus();
}

function deleteField(i) {
    definedFields.splice(i, 1);
    renderFields();
    updateQuickpick();
    updateVariableChips();
}

function moveField(i, dir) {
    var j = i + dir;
    if (j < 0 || j >= definedFields.length) return;
    var tmp = definedFields[i]; definedFields[i] = definedFields[j]; definedFields[j] = tmp;
    renderFields();
}

function renderFields() {
    var cards = document.getElementById('fields-cards-list');
    var empty = document.getElementById('fields-empty-state');
    var badge = document.getElementById('field-count-badge');
    if (!cards) return;

    badge.textContent = toPersian(definedFields.length) + ' فیلد';

    if (definedFields.length === 0) {
        empty.classList.remove('d-none');
        cards.innerHTML = '';
        return;
    }
    empty.classList.add('d-none');

    cards.innerHTML = definedFields.map(function(f, i) {
        var icon = fieldTypeIcons[f.type] || 'fa-font';
        var typeLbl = fieldTypeLabels[f.type] || f.type;
        return '<div class="d-flex align-items-center gap-2 p-3 mb-2 rounded-2" style="background:var(--dk-surface2);border:1px solid var(--dk-border2);">' +
            '<div class="d-flex flex-column" style="gap:2px;">' +
            '<button type="button" class="btn p-0 lh-1" style="font-size:.65rem;color:var(--dk-muted);" onclick="moveField(' + i + ',-1)"' + (i === 0 ? ' disabled' : '') + '>▲</button>' +
            '<button type="button" class="btn p-0 lh-1" style="font-size:.65rem;color:var(--dk-muted);" onclick="moveField(' + i + ',1)"' + (i === definedFields.length - 1 ? ' disabled' : '') + '>▼</button>' +
            '</div>' +
            '<div class="flex-fill" style="min-width:0;">' +
            '<div class="d-flex align-items-center gap-1 flex-wrap">' +
            '<span class="fw-semibold small">' + escHtml(f.label) + '</span>' +
            (f.required ? '<span class="text-danger small">*</span>' : '') +
            '<span class="badge" style="background:rgba(249,115,22,.12);border:1px solid rgba(249,115,22,.25);color:#fed7aa;font-size:.6rem;">' +
            '<i class="fas ' + icon + ' me-1"></i>' + typeLbl + '</span>' +
            '</div>' +
            '<code style="font-size:.7rem;color:var(--dk-muted);">{' + escHtml(f.name) + '}</code>' +
            '</div>' +
            '<button type="button" class="btn btn-sm p-1" title="درج در پرامپت" onclick="insertField(\'' + f.name + '\')" style="color:#f97316;">' +
            '<i class="fas fa-level-down-alt fa-rotate-90"></i></button>' +
            '<button type="button" class="btn btn-sm p-1" title="ویرایش" onclick="editField(' + i + ')" style="color:#a5b4fc;">' +
            '<i class="fas fa-pen"></i></button>' +
            '<button type="button" class="btn btn-sm p-1" style="color:var(--dk-muted);" onclick="deleteField(' + i + ')">' +
            '<i class="fas fa-times"></i></button>' +
            '</div>';
    }).join('');
}

var _editingIndex = -1;

function editField(i) {
    var f = definedFields[i];
    if (!f) return;

    document.getElementById('nf-name').value = f.name;
    document.getElementById('nf-label').value = f.label;
    document.getElementById('nf-type').value = f.type;
    document.getElementById('nf-placeholder').value = f.placeholder || '';
    document.getElementById('nf-required').checked = f.required;

    var optionsWrap = document.getElementById('nf-options-wrap');
    var optionsEl = document.getElementById('nf-options');
    var imgoptWrap = document.getElementById('nf-imgopt-wrap');
    var imgoptRows = document.getElementById('nf-imgopt-rows');

    optionsWrap.classList.add('d-none');
    optionsEl.value = '';
    if (imgoptWrap) { imgoptWrap.classList.add('d-none'); imgoptRows.innerHTML = ''; }

    if (f.type === 'Select') {
        optionsWrap.classList.remove('d-none');
        if (f.options) {
            try {
                var opts = JSON.parse(f.options);
                optionsEl.value = opts.map(function(o) { return o.label || o.value; }).join('\n');
            } catch(e) { optionsEl.value = ''; }
        }
    } else if (f.type === 'ImageSelect' && imgoptWrap) {
        imgoptWrap.classList.remove('d-none');
        if (f.options) {
            try {
                var iopts = JSON.parse(f.options);
                iopts.forEach(function(o) { addImgOptRow(o.label || '', o.image || '', o.value || ''); });
            } catch(e) {}
        }
    }

    _editingIndex = i;
    var btn = document.getElementById('add-field-btn');
    btn.textContent = 'ذخیره تغییرات';
    btn.classList.remove('btn-outline-primary');
    btn.classList.add('btn-warning');

    document.getElementById('nf-name').scrollIntoView({ behavior: 'smooth', block: 'center' });
    document.getElementById('nf-name').focus();
}

// ─── ImageSelect helpers ────────────────────────────────────────────────────

function addImgOptRow(label, image, value) {
    var rows = document.getElementById('nf-imgopt-rows');
    if (!rows) return;
    var uid = 'ior_' + Date.now() + '_' + Math.random().toString(36).slice(2);
    var div = document.createElement('div');
    div.className = 'img-opt-row rounded-2 p-2 mb-2';
    div.style.cssText = 'background:var(--dk-surface);border:1px solid var(--dk-border2);';
    div.innerHTML =
        '<div class="d-flex gap-2 align-items-start mb-1">' +
        // thumbnail preview
        '<div class="img-opt-thumb flex-shrink-0" style="width:56px;height:56px;border-radius:8px;overflow:hidden;background:#1c2539;border:1px solid rgba(255,255,255,.1);cursor:pointer;display:flex;align-items:center;justify-content:center;" onclick="document.getElementById(\'' + uid + '\').click()" title="کلیک برای آپلود تصویر">' +
        (image ? '<img src="' + escHtml(image) + '" style="width:100%;height:100%;object-fit:cover;" />' : '<i class="fas fa-image" style="color:rgba(255,255,255,.2);font-size:1.2rem;"></i>') +
        '</div>' +
        '<input type="file" id="' + uid + '" accept="image/*" class="d-none" onchange="uploadImgOpt(this)" />' +
        '<input type="hidden" class="img-opt-image" value="' + escHtml(image || '') + '" />' +
        // label + remove
        '<div class="flex-fill">' +
        '<input type="text" class="form-control form-control-sm img-opt-label mb-1" placeholder="نام فارسی (مثال: استودیو کلاسیک)" value="' + escHtml(label || '') + '" />' +
        '<textarea class="form-control form-control-sm img-opt-value" rows="2" dir="ltr" placeholder="مقدار ارسالی به پرامپت — اگر خالی باشد، نام فارسی استفاده می‌شود">' + escHtml(value || '') + '</textarea>' +
        '</div>' +
        '<button type="button" onclick="this.closest(\'.img-opt-row\').remove()" class="btn btn-sm text-danger p-0 flex-shrink-0" style="line-height:1;font-size:1.1rem;margin-top:2px;">×</button>' +
        '</div>';
    rows.appendChild(div);
}

function uploadImgOpt(input) {
    if (!input.files || !input.files[0]) return;
    var row = input.closest('.img-opt-row');
    var thumb = row.querySelector('.img-opt-thumb');
    var hiddenUrl = row.querySelector('.img-opt-image');
    var formData = new FormData();
    formData.append('file', input.files[0]);
    thumb.innerHTML = '<div style="font-size:.6rem;color:#94a3b8;text-align:center;padding:4px;">آپلود...</div>';
    fetch('/Creator/Apps/UploadOptionImage', { method: 'POST', body: formData, headers: { 'RequestVerificationToken': document.querySelector('input[name=__RequestVerificationToken]')?.value || '' } })
        .then(function(r) { return r.json(); })
        .then(function(data) {
            if (data.url) {
                hiddenUrl.value = data.url;
                thumb.innerHTML = '<img src="' + data.url + '" style="width:100%;height:100%;object-fit:cover;" />';
            } else {
                thumb.innerHTML = '<i class="fas fa-exclamation-triangle" style="color:#f87171;font-size:1rem;"></i>';
                showAlert(data.error || 'خطا در آپلود', 'error');
            }
        })
        .catch(function() {
            thumb.innerHTML = '<i class="fas fa-exclamation-triangle" style="color:#f87171;font-size:1rem;"></i>';
        });
}

function escHtml(s) {
    return String(s).replace(/&/g, '&amp;').replace(/</g, '&lt;').replace(/>/g, '&gt;').replace(/"/g, '&quot;');
}

function serializeFields() {
    if (_editingIndex >= 0) {
        var pendingName = (document.getElementById('nf-name') || {}).value || '';
        if (pendingName.trim()) addField();
        else {
            _editingIndex = -1;
            var btn = document.getElementById('add-field-btn');
            if (btn) { btn.textContent = '+ افزودن فیلد'; btn.classList.add('btn-outline-primary'); btn.classList.remove('btn-warning'); }
        }
    }
    var inp = document.getElementById('fields-json-input');
    if (inp) inp.value = JSON.stringify(definedFields);
}

// ─── Insert field into prompt ───────────────────────────────────────────────

function insertField(name) {
    var ta = document.getElementById('prompt-textarea');
    if (!ta) return;
    var start = ta.selectionStart;
    var end = ta.selectionEnd;
    var before = ta.value.substring(0, start);
    var after = ta.value.substring(end);

    // if user typed { already, replace it
    if (before.endsWith('{')) {
        before = before.slice(0, -1);
    }

    ta.value = before + '{' + name + '}' + after;
    var pos = before.length + name.length + 2;
    ta.focus();
    ta.setSelectionRange(pos, pos);

    hideQuickpick();
    updateVariableChips();
}

// ─── Quick-pick (shown when { is typed) ────────────────────────────────────

function updateQuickpick() {
    var container = document.getElementById('quickpick-chips');
    if (!container) return;
    container.innerHTML = definedFields.map(function(f) {
        return '<button type="button" class="btn btn-sm rounded-pill px-2 py-0" ' +
            'style="background:rgba(249,115,22,.15);border:1px solid rgba(249,115,22,.3);color:#fed7aa;font-size:.75rem;" ' +
            'onmousedown="insertField(\'' + f.name + '\')">' +
            '{' + escHtml(f.name) + '}</button>';
    }).join('');
}

function showQuickpick() {
    if (definedFields.length === 0) return;
    var qp = document.getElementById('field-quickpick');
    if (qp) qp.classList.remove('d-none');
}

function hideQuickpick() {
    var qp = document.getElementById('field-quickpick');
    if (qp) qp.classList.add('d-none');
}

// ─── Variable chips ─────────────────────────────────────────────────────────

function updateVariableChips() {
    var ta = document.getElementById('prompt-textarea');
    var container = document.getElementById('variable-chips');
    if (!ta || !container) return;
    var matches = ta.value.match(/\{(\w+)\}/g) || [];
    var unique = matches.filter(function(v, i, a) { return a.indexOf(v) === i; });
    container.innerHTML = unique.map(function(v) {
        var vname = v.slice(1, -1);
        var defined = definedFields.some(function(f) { return f.name === vname; });
        return defined
            ? '<span class="badge" style="background:rgba(249,115,22,.12);border:1px solid rgba(249,115,22,.3);color:#fed7aa;font-size:.7rem;">' + escHtml(v) + '</span>'
            : '<span class="badge" style="background:rgba(239,68,68,.12);border:1px solid rgba(239,68,68,.3);color:#fca5a5;font-size:.7rem;" title="فیلدی با این نام تعریف نشده">' + escHtml(v) + ' ⚠</span>';
    }).join('');
}

// ─── Direction toggle ───────────────────────────────────────────────────────

function toggleDir(textareaId, btn) {
    var ta = document.getElementById(textareaId);
    if (!ta) return;
    var isRtl = ta.dir !== 'ltr';
    ta.dir = isRtl ? 'ltr' : 'rtl';
    ta.style.textAlign = isRtl ? 'left' : 'right';
    btn.innerHTML = isRtl
        ? '<i class="fas fa-align-left me-1"></i>LTR'
        : '<i class="fas fa-align-right me-1"></i>RTL';
}

// ─── Auto-save draft (localStorage) ────────────────────────────────────────

var DRAFT_KEY = 'creator_app_draft';

function saveDraft() {
    var draft = {
        savedAt: new Date().toISOString(),
        title:            (document.getElementById('form-title')        || {}).value || '',
        shortDesc:        (document.getElementById('form-short-desc')   || {}).value || '',
        description:      (document.getElementById('form-description')  || {}).value || '',
        categoryId:       (document.getElementById('form-category')     || {}).value || '',
        tags:             (document.getElementById('form-tags')         || {}).value || '',
        outputType:       (document.querySelector('input[name="Form.OutputType"]:checked') || {}).value || '',
        aiModelId:        (document.getElementById('ai-model-select')   || {}).value || '',
        creditCost:       (document.getElementById('credit-cost-input') || {}).value || '',
        systemContext:    (document.getElementById('system-context')    || {}).value || '',
        prompt:           (document.getElementById('prompt-textarea')   || {}).value || '',
        fields:           JSON.stringify(definedFields)
    };
    try { localStorage.setItem(DRAFT_KEY, JSON.stringify(draft)); } catch(e) {}
}

function loadDraft() {
    try {
        var raw = localStorage.getItem(DRAFT_KEY);
        if (!raw) return;
        var draft = JSON.parse(raw);

        function setVal(id, val) { var el = document.getElementById(id); if (el && val) el.value = val; }

        setVal('form-title',       draft.title);
        setVal('form-short-desc',  draft.shortDesc);
        setVal('form-description', draft.description);
        setVal('form-category',    draft.categoryId);
        setVal('form-tags',        draft.tags);
        setVal('system-context',   draft.systemContext);
        setVal('prompt-textarea',  draft.prompt);
        setVal('credit-cost-input',draft.creditCost);

        if (draft.outputType) {
            var radio = document.querySelector('input[name="Form.OutputType"][value="' + draft.outputType + '"]');
            if (radio) { radio.checked = true; filterModels(draft.outputType); }
        }
        if (draft.aiModelId) {
            setTimeout(function() {
                var sel = document.getElementById('ai-model-select');
                if (sel) sel.value = draft.aiModelId;
            }, 80);
        }
        if (draft.fields) {
            try {
                var fields = JSON.parse(draft.fields);
                if (Array.isArray(fields) && fields.length > 0) {
                    definedFields = fields;
                    renderFields(); updateQuickpick(); updateVariableChips();
                }
            } catch(e) {}
        }

        // نمایش بنر بازیابی
        var savedAt = new Date(draft.savedAt);
        var timeStr = savedAt.toLocaleTimeString('fa-IR', { hour: '2-digit', minute: '2-digit' });
        var dateStr = savedAt.toLocaleDateString('fa-IR');
        var banner = document.getElementById('draft-restore-banner');
        var timeEl = document.getElementById('draft-saved-time');
        if (banner) { banner.classList.remove('d-none'); }
        if (timeEl) { timeEl.textContent = dateStr + ' — ' + timeStr; }
    } catch(e) {}
}

function clearDraft() {
    try { localStorage.removeItem(DRAFT_KEY); } catch(e) {}
    var banner = document.getElementById('draft-restore-banner');
    if (banner) banner.classList.add('d-none');
}

// ─── DOMContentLoaded ───────────────────────────────────────────────────────

document.addEventListener('DOMContentLoaded', function() {

    // pricing hint
    var pricingMap = window.pricingMapData || {};
    function updatePricingHint() {
        var sel = document.querySelector('input[name="Form.OutputType"]:checked');
        if (sel) {
            var val = pricingMap[sel.value] || 1;
            var el = document.getElementById('pricing-value');
            if (el) el.textContent = val;
            var inp = document.getElementById('credit-cost-input');
            if (inp && (inp.value === '' || inp.value === '1')) inp.value = val;
        }
    }
    document.querySelectorAll('input[name="Form.OutputType"]').forEach(function(r) {
        r.addEventListener('change', updatePricingHint);
    });
    updatePricingHint();

    // بارگذاری مدل‌ها بر اساس نوع خروجی پیش‌فرض
    var defaultOt = document.querySelector('input[name="Form.OutputType"]:checked');
    if (defaultOt) filterModels(defaultOt.value);

    // field type toggle
    var nfType = document.getElementById('nf-type');
    if (nfType) {
        nfType.addEventListener('change', function() {
            document.getElementById('nf-options-wrap').classList.toggle('d-none', this.value !== 'Select');
            var imgWrap = document.getElementById('nf-imgopt-wrap');
            if (imgWrap) imgWrap.classList.toggle('d-none', this.value !== 'ImageSelect');
        });
    }

    // ImageSelect — افزودن ردیف
    var addImgOptBtn = document.getElementById('add-imgopt-btn');
    if (addImgOptBtn) addImgOptBtn.addEventListener('click', function() { addImgOptRow('', '', ''); });

    // add field button
    var addBtn = document.getElementById('add-field-btn');
    if (addBtn) addBtn.addEventListener('click', addField);

    // enter key in name/label inputs triggers add
    ['nf-name', 'nf-label'].forEach(function(id) {
        var el = document.getElementById(id);
        if (el) el.addEventListener('keydown', function(e) {
            if (e.key === 'Enter') { e.preventDefault(); addField(); }
        });
    });

    // prompt textarea: detect { to show quickpick
    var ta = document.getElementById('prompt-textarea');
    if (ta) {
        ta.addEventListener('keyup', function(e) {
            var pos = ta.selectionStart;
            var text = ta.value.substring(0, pos);
            var lastOpen = text.lastIndexOf('{');
            var lastClose = text.lastIndexOf('}');
            if (lastOpen > lastClose) {
                showQuickpick();
            } else {
                hideQuickpick();
            }
            updateVariableChips();
        });

        ta.addEventListener('blur', function() {
            setTimeout(hideQuickpick, 200);
        });
    }

    // init popovers
    document.querySelectorAll('[data-bs-toggle="popover"]').forEach(function(el) {
        new bootstrap.Popover(el, { html: false });
    });

    // ─── Auto-save ─────────────────────────────────────────────────────────
    var autoSaveIds = ['form-title','form-short-desc','form-description','form-category','form-tags',
                       'system-context','prompt-textarea','credit-cost-input','ai-model-select'];
    autoSaveIds.forEach(function(id) {
        var el = document.getElementById(id);
        if (el) el.addEventListener('input', saveDraft);
        if (el) el.addEventListener('change', saveDraft);
    });
    document.querySelectorAll('input[name="Form.OutputType"]').forEach(function(r) {
        r.addEventListener('change', saveDraft);
    });

    // پاک کردن draft موقع submit موفق
    var form = document.querySelector('form');
    if (form) form.addEventListener('submit', clearDraft);

    // دکمه پاک کردن draft
    var clearBtn = document.getElementById('draft-clear-btn');
    if (clearBtn) clearBtn.addEventListener('click', function() {
        clearDraft();
        location.reload();
    });

    // بازیابی draft
    loadDraft();
});
