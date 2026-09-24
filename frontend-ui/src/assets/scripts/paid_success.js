(function () {
    'use strict';

    document.addEventListener('DOMContentLoaded', function () {
        var id = Atc.getQueryParam('id');
        var receipt = id ? Atc.getReceiptById(id) : null;
        if (receipt) {
            document.querySelector('.sum').textContent = Atc.formatMoney(receipt.amount);
        }
    });
})();
