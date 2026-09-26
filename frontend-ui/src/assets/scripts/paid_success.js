(function () {
    'use strict';

    document.addEventListener('DOMContentLoaded', function () {
        Atc.load(['receipts']).then(function () {
            var id = Atc.getQueryParam('id');
            var receipt = id ? Atc.getReceiptById(id) : null;

            if (!receipt) {
                window.location.href = 'home.html';
                return;
            }

            document.querySelector('.sum').textContent = Atc.formatMoney(receipt.amount);
        }).catch(Atc.showBootError);
    });
})();
