(function () {
    'use strict';

    let modalEl = null;
    let bsModal  = null;
    let pendingCb = null;

    const TYPES = {
        danger:  { icon: 'fa-trash-alt',           color: '#ef4444', bg: '#fef2f2', btnClass: 'btn-danger',  title: 'تأیید حذف'     },
        warning: { icon: 'fa-exclamation-triangle', color: '#f59e0b', bg: '#fffbeb', btnClass: 'btn-warning', title: 'تأیید عملیات' },
        info:    { icon: 'fa-question-circle',      color: '#3b82f6', bg: '#eff6ff', btnClass: 'btn-primary', title: 'تأیید'         },
    };

    function buildModal() {
        const el = document.createElement('div');
        el.id = 'appConfirmModal';
        el.className = 'modal fade';
        el.tabIndex = -1;
        el.setAttribute('data-bs-backdrop', 'static');
        el.innerHTML = `
<div class="modal-dialog modal-dialog-centered" style="max-width:380px">
  <div class="modal-content border-0 shadow-xl confirm-card">
    <div class="confirm-header">
      <div id="confirmCircle" class="confirm-circle">
        <i id="confirmIcon" class="fas fa-trash-alt"></i>
      </div>
      <h6 id="confirmTitle" class="confirm-title"></h6>
    </div>
    <div class="confirm-body">
      <p id="confirmMessage" class="confirm-msg"></p>
    </div>
    <div class="confirm-footer">
      <button type="button" class="btn confirm-cancel" id="confirmCancelBtn">
        <i class="fas fa-times me-1"></i>انصراف
      </button>
      <button type="button" class="btn confirm-ok fw-semibold" id="confirmOkBtn">
        <i class="fas fa-check me-1"></i>تأیید
      </button>
    </div>
  </div>
</div>`;
        document.body.appendChild(el);
        document.getElementById('confirmCancelBtn').addEventListener('click', () => bsModal.hide());
        document.getElementById('confirmOkBtn').addEventListener('click', () => {
            bsModal.hide();
            if (pendingCb) pendingCb();
        });
        return el;
    }

    function showConfirm(message, callback, type) {
        if (!modalEl) {
            modalEl = buildModal();
            bsModal  = new bootstrap.Modal(modalEl);
        }

        const cfg = TYPES[type] || TYPES.danger;

        const circle = document.getElementById('confirmCircle');
        circle.style.background = cfg.bg;
        circle.style.color      = cfg.color;
        circle.style.boxShadow  = `0 0 0 6px ${cfg.color}1a`;

        document.getElementById('confirmIcon').className    = 'fas ' + cfg.icon;
        document.getElementById('confirmTitle').textContent = cfg.title;
        document.getElementById('confirmMessage').textContent = message;

        const okBtn = document.getElementById('confirmOkBtn');
        okBtn.className = `btn confirm-ok fw-semibold ${cfg.btnClass}`;

        pendingCb = callback;
        bsModal.show();
    }

    // Intercept buttons with data-confirm (capture phase so it fires before form submit)
    document.addEventListener('click', function (e) {
        const btn = e.target.closest('button[data-confirm]');
        if (!btn) return;
        e.preventDefault();
        e.stopImmediatePropagation();
        const msg  = btn.getAttribute('data-confirm');
        const type = btn.getAttribute('data-confirm-type') || 'danger';
        showConfirm(msg, () => {
            btn.removeAttribute('data-confirm');
            btn.click();
        }, type);
    }, true);

    // Intercept forms with data-confirm
    document.addEventListener('submit', function (e) {
        const form = e.target;
        if (!form.hasAttribute('data-confirm')) return;
        e.preventDefault();
        e.stopImmediatePropagation();
        const msg  = form.getAttribute('data-confirm');
        const type = form.getAttribute('data-confirm-type') || 'danger';
        showConfirm(msg, () => {
            form.removeAttribute('data-confirm');
            form.submit();
        }, type);
    }, true);

    window.showConfirm = showConfirm;
})();
