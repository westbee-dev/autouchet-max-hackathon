(function () {
    'use strict';

    function init() {
        var id = Atc.getQueryParam('id');
        var receipt = id ? Atc.getReceiptById(id) : null;

        if (!receipt) {
            window.location.href = 'home.html';
            return;
        }

        renderFields(receipt);
        bindCopyButton(receipt);
        bindDelete(id);
    }

    function renderFields(receipt) {
        var meta = Atc.STATUS_META[receipt.status] || { label: 'Не оплачено', css: 'denied' };
        var values = {
            amount: Atc.formatMoney(receipt.amount),
            buyerType: receipt.buyerType === 'legal' ? 'Юр. лицо' : 'Физ. лицо',
            taxRate: Math.round(receipt.taxRate * 100) + '%',
            status: meta.label,
            description: receipt.description,
            date: Atc.formatDateFull(receipt.paidAt || receipt.createdAt)
        };

        Object.keys(values).forEach(function (key) {
            var el = document.querySelector('[data-field="' + key + '"]');
            if (el) el.textContent = values[key];
        });
    }

    function bindCopyButton(receipt) {
        var copyBtn = document.getElementById('copy_link');
        var paymentUrl = receipt.paymentUrl || '';

        if (!receipt.paymentUrl || receipt.status === 'paid' || receipt.status === 'manual_recorded') {
            copyBtn.hidden = true;
            return;
        }

        copyBtn.hidden = false;
        copyBtn.addEventListener('click', function () {
            Atc.copyToClipboard(paymentUrl).then(function () {
                var original = copyBtn.textContent;
                copyBtn.textContent = 'Скопировано!';
                setTimeout(function () { copyBtn.textContent = original; }, 1500);
            });
        });
    }

    function bindDelete(id) {
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
                Atc.removeReceipt(id).then(function () {
                    window.location.href = 'home.html';
                }).catch(function (error) {
                    console.error(error);
                    alert('Не удалось удалить чек. Попробуйте ещё раз.');
                });
            });
        }
    }

    document.addEventListener('DOMContentLoaded', function () {
        Atc.load(['receipts']).then(init).catch(Atc.showBootError);
    });
})();
