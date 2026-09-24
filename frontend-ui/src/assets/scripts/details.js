(function () {
    'use strict';

    document.addEventListener('DOMContentLoaded', function () {
        var id = Atc.getQueryParam('id');
        var receipt = id ? Atc.getReceiptById(id) : null;

        if (!receipt) {
            window.location.href = 'home.html';
            return;
        }

        var meta = Atc.STATUS_META[receipt.status];
        var values = {
            amount: Atc.formatMoney(receipt.amount),
            buyerType: receipt.buyerType === 'legal' ? 'Юр. лицо' : 'Физ. лицо',
            taxRate: (receipt.taxRate * 100) + '%',
            status: meta.label,
            description: receipt.description,
            date: Atc.formatDateFull(receipt.paidAt || receipt.createdAt)
        };

        Object.keys(values).forEach(function (key) {
            var el = document.querySelector('[data-field="' + key + '"]');
            if (el) el.textContent = values[key];
        });

        var copyBtn = document.getElementById('copy_link');
        var fakeLink = window.location.origin + '/pay/' + receipt.id;

        if (receipt.status === 'awaiting_payment') {
            copyBtn.hidden = false;
            copyBtn.addEventListener('click', function () {
                Atc.copyToClipboard(fakeLink).then(function () {
                    var original = copyBtn.textContent;
                    copyBtn.textContent = 'Скопировано!';
                    setTimeout(function () { copyBtn.textContent = original; }, 1500);
                });
            });
        } else {
            copyBtn.hidden = true;
        }

        bindDelete(receipt);
    });

    function bindDelete(receipt) {
        var deleteBtn = document.getElementById('delete_receipt');
        var confirm = document.getElementById('confirm');
        var backdrop = document.getElementById('confirm-backdrop');
        var cancelBtn = document.getElementById('confirm-cancel');
        var confirmBtn = document.getElementById('confirm-delete');

        if (!deleteBtn || !confirm || !backdrop) return;

        function openConfirm() {
            backdrop.hidden = false;
            confirm.hidden = false;
            requestAnimationFrame(function () {
                backdrop.classList.add('confirm_backdrop--visible');
                confirm.classList.add('confirm--open');
            });
            confirm.setAttribute('aria-hidden', 'false');
        }

        function closeConfirm() {
            backdrop.classList.remove('confirm_backdrop--visible');
            confirm.classList.remove('confirm--open');
            confirm.setAttribute('aria-hidden', 'true');

            setTimeout(function () {
                backdrop.hidden = true;
                confirm.hidden = true;
            }, 250);
        }

        deleteBtn.addEventListener('click', openConfirm);
        backdrop.addEventListener('click', closeConfirm);
        if (cancelBtn) cancelBtn.addEventListener('click', closeConfirm);

        document.addEventListener('keydown', function (e) {
            if (e.key === 'Escape' && !confirm.hidden) closeConfirm();
        });

        if (confirmBtn) {
            confirmBtn.addEventListener('click', function () {
                Atc.removeReceipt(receipt.id);
                window.location.href = 'home.html';
            });
        }
    }
})();
