(function () {
    'use strict';

    var POLL_INTERVAL_MS = 4000;
    var POLL_TIMEOUT_MS = 10 * 60 * 1000;

    var timer = null;

    function init() {
        var id = Atc.getQueryParam('id');
        var receipt = id ? Atc.getReceiptById(id) : null;

        if (!receipt) {
            window.location.href = 'home.html';
            return;
        }

        renderReceipt(receipt);
        startPolling(id);
    }

    function renderReceipt(receipt) {
        var buyerLabel = receipt.buyerType === 'legal' ? 'Юр. лицо' : 'Физ. лицо';

        document.getElementById('pay-amount').textContent = Atc.formatMoney(receipt.amount);
        document.getElementById('pay-desc').textContent =
            buyerLabel + ' — ' + receipt.description;

        var copyBtn = document.getElementById('copy_link');
        var paymentUrl = receipt.paymentUrl || '';

        copyBtn.disabled = !paymentUrl;
        copyBtn.addEventListener('click', function () {
            Atc.copyToClipboard(paymentUrl).then(function () {
                var original = copyBtn.textContent;
                copyBtn.textContent = 'Скопировано!';
                setTimeout(function () { copyBtn.textContent = original; }, 1500);
            }).catch(function (err) {
                console.error('Не удалось скопировать:', err);
            });
        });
    }

    function startPolling(id) {
        var startedAt = Date.now();

        timer = setInterval(function () {
            if (Date.now() - startedAt > POLL_TIMEOUT_MS) {
                stopPolling();
                return;
            }

            Atc.load(['receipts'], true).then(function () {
                var current = Atc.getReceiptById(id);
                if (!current) {
                    stopPolling();
                    window.location.href = 'home.html';
                } else if (current.status === 'expired') {
                    stopPolling();
                    window.location.href = 'paid_expired.html?id=' + id;
                } else if (current.status === 'paid') {
                    stopPolling();
                    window.location.href = 'paid_success.html?id=' + id;
                }
            }).catch(function () {
                console.warn('Не удалось опросить статус чека');
            });
        }, POLL_INTERVAL_MS);
    }

    function stopPolling() {
        if (timer) clearInterval(timer);
    }

    document.addEventListener('DOMContentLoaded', function () {
        Atc.load(['receipts']).then(init).catch(Atc.showBootError);
    });
})();
