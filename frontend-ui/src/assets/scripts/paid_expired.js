(function () {
    'use strict';

    document.addEventListener('DOMContentLoaded', function () {
        var id = Atc.getQueryParam('id');
        var receipt = id ? Atc.getReceiptById(id) : null;

        var newLinkBtn = document.getElementById('button_link');
        if (receipt && newLinkBtn) {
            newLinkBtn.closest('a').href = 'receipt.html?prefill_amount=' + receipt.amount;
        }
    });
})();
