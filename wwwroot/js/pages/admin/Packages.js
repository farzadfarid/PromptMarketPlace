function openCreate() {
    document.getElementById('pkgModalTitle').textContent = 'بسته جدید';
    ['fId', 'fName', 'fCredit', 'fPrice', 'fSort'].forEach(function (id) {
        document.getElementById(id).value = '';
    });
    document.getElementById('fId').value = '0';
    document.getElementById('fSort').value = '0';
    document.getElementById('fActive').checked = true;
    document.getElementById('fBest').checked = false;
}

function openEdit(id, name, credit, price, isActive, isBest, sort) {
    document.getElementById('pkgModalTitle').textContent = 'ویرایش بسته';
    document.getElementById('fId').value = id;
    document.getElementById('fName').value = name;
    document.getElementById('fCredit').value = credit;
    document.getElementById('fPrice').value = price;
    document.getElementById('fSort').value = sort;
    document.getElementById('fActive').checked = isActive;
    document.getElementById('fBest').checked = isBest;
    new bootstrap.Modal(document.getElementById('pkgModal')).show();
}
