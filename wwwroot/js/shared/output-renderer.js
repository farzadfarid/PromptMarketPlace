function copyOutput() {
    var text = document.querySelector('.output-text-area, #outputCode')?.innerText ?? '';
    navigator.clipboard.writeText(text).then(function () {
        var btn = document.querySelector('[onclick="copyOutput()"]');
        var orig = btn.innerHTML;
        btn.innerHTML = '<i class="fas fa-check me-1"></i>کپی شد';
        setTimeout(function () { btn.innerHTML = orig; }, 2000);
    });
}

function downloadPdf() {
    var contentEl = document.getElementById('outputTextContent');
    if (!contentEl) return;
    var text = contentEl.innerText || '';
    if (!text.trim()) return;

    var escaped = text
        .replace(/&/g, '&amp;')
        .replace(/</g, '&lt;')
        .replace(/>/g, '&gt;');

    var html = '<!DOCTYPE html>' +
        '<html dir="rtl" lang="fa"><head><meta charset="utf-8"><title>خروجی ابزار</title><style>' +
        '@page{margin:20mm}' +
        'body{font-family:Tahoma,Arial,sans-serif;direction:rtl;text-align:right;margin:0;line-height:2.2;font-size:13pt;color:#111;}' +
        'pre{white-space:pre-wrap;word-break:break-word;font-family:inherit;margin:0;}' +
        '@media screen{body{padding:32px;max-width:820px;margin:0 auto;}}' +
        '</style></head><body>' +
        '<pre>' + escaped + '</pre>' +
        '<script>window.addEventListener("load",function(){setTimeout(function(){window.print();},250);});<\/script>' +
        '</body></html>';

    var blob = new Blob([html], { type: 'text/html; charset=utf-8' });
    var url = URL.createObjectURL(blob);
    var win = window.open(url, '_blank');
    if (!win && window.showAlert) showAlert('لطفاً popup را برای این سایت مجاز کنید.', 'warning');
    setTimeout(function () { URL.revokeObjectURL(url); }, 60000);
}
