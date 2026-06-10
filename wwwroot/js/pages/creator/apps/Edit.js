document.addEventListener('DOMContentLoaded', function() {
    if (typeof editPageData === 'undefined') return;

    // populate مدل‌ها بر اساس نوع خروجی فعلی
    var selOt = document.querySelector('input[name="Form.OutputType"]:checked');
    if (selOt) filterModels(selOt.value);

    // انتخاب مدل فعلی بعد از populate
    if (editPageData.selectedModelId) {
        setTimeout(function() {
            var aiSel = document.getElementById('ai-model-select');
            if (aiSel) aiSel.value = String(editPageData.selectedModelId);
        }, 50);
    }

    // بارگذاری فیلدهای موجود
    if (editPageData.existingFields && editPageData.existingFields.length > 0) {
        definedFields = editPageData.existingFields.map(function(f) {
            return {
                name: f.Name || f.name,
                label: f.Label || f.label,
                type: f.Type || f.type || 'Text',
                placeholder: f.Placeholder || f.placeholder || '',
                required: f.IsRequired !== undefined ? f.IsRequired : (f.isRequired !== undefined ? f.isRequired : true),
                options: f.Options || f.options || null
            };
        });
        renderFields();
        updateQuickpick();
        updateVariableChips();
    }
});
