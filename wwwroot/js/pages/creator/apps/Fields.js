function toggleOptions(type) {
    document.getElementById('options-section').classList.toggle('d-none', type !== 'Select');
}

function updatePreview() {
    var label = document.getElementById('field-label-input').value || 'فیلد جدید';
    document.getElementById('preview-label').textContent = label + ' *';
    document.getElementById('new-field-preview').classList.remove('d-none');
}
