function editCat(id, name, icon, desc) {
    document.getElementById('editId').value = id;
    document.getElementById('nameField').value = name;
    document.getElementById('iconField').value = icon;
    document.getElementById('descField').value = desc;
    document.getElementById('nameField').focus();
}

function resetForm() {
    document.getElementById('editId').value = 0;
    document.getElementById('nameField').value = '';
    document.getElementById('iconField').value = '';
    document.getElementById('descField').value = '';
}
