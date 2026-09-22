(function () {
    'use strict';

    var DEMO_PAYMENT_DELAY_MS = 6000;

    document.addEventListener('DOMContentLoaded', function () {
        var id = Atc.getQueryParam('id');
        var receipt = id ? Atc.getReceiptById(id) : null;

        if (!receipt) {
            window.location.href = 'home.html';
            return;
        }

        var buyerLabel = receipt.buyerType === 'legal' ? 'Юр. лицо' : 'Физ. лицо';
        document.getElementById('pay-amount').textContent = Atc.formatMoney(receipt.amount);
        document.getElementById('pay-desc').textContent = buyerLabel + ' — счёт RB-' + receipt.id.slice(-5).toUpperCase();

        var copyBtn = document.getElementById('copy_link');
        var fakeLink = window.location.origin + '/pay/' + receipt.id;

        copyBtn.addEventListener('click', function () {
            Atc.copyToClipboard(fakeLink).then(function () {
                var original = copyBtn.textContent;
                copyBtn.textContent = 'Скопировано!';
                setTimeout(function () { copyBtn.textContent = original; }, 1500);
            }).catch(function (err) {
                console.error('Не удалось скопировать:', err);
            });
        });

        if (receipt.status === 'awaiting_payment') {
            setTimeout(function () {
                var current = Atc.getReceiptById(receipt.id);
                if (current && current.status === 'awaiting_payment') {
                    Atc.updateReceipt(receipt.id, { status: 'paid', paidAt: new Date().toISOString() });
                    window.location.href = 'paid_success.html?id=' + receipt.id;
                }
            }, DEMO_PAYMENT_DELAY_MS);
        }
    });
})();
