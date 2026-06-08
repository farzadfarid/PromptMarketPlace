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

    var modal = new bootstrap.Modal(document.getElementById('modelModal'));
    modal.show();
}
