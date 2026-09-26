(function () {
    'use strict';

    function populateActivitySelect() {
        var select = document.getElementById('activity');
        var activities = Atc.getActivities();
        select.innerHTML = '';

        activities.forEach(function (a) {
            var opt = document.createElement('option');
            opt.value = a.id;
            opt.textContent = a.name;
            if (a.isDefault) opt.selected = true;
            select.appendChild(opt);
        });
    }

    function getSelectedBuyerType() {
        var checked = document.querySelector('input[name="customer_type"]:checked');
        return checked ? checked.value : 'individual';
    }

    function updateButtonsAvailability() {
        var buyerType = getSelectedBuyerType();
        var payBtn = document.querySelector('.button_individual');
        var hint = document.getElementById('legal_hint');

        if (buyerType === 'legal') {
            payBtn.disabled = true;
            payBtn.classList.add('button--disabled');
            payBtn.title = 'Автосвязка с Robokassa пока доступна только для физ. лиц';
            if (hint) hint.hidden = false;
        } else {
            payBtn.disabled = false;
            payBtn.classList.remove('button--disabled');
            payBtn.removeAttribute('title');
            if (hint) hint.hidden = true;
        }
    }

    function getFormData() {
        var form = document.getElementById('receipt-form');

        return {
            amount: Number(form.amount.value),
            buyerType: getSelectedBuyerType(),
            activityId: form.activity.value
        };
    }

    function validate(data) {
        if (!data.amount || data.amount <= 0) {
            alert('Укажите сумму больше нуля');
            return false;
        }

        if (!data.activityId) {
            alert('Сначала добавьте вид деятельности в профиле');
            return false;
        }

        return true;
    }

    function handleSendLink(e) {
        e.preventDefault();
        var data = getFormData();
        if (!validate(data)) return;

        if (data.buyerType === 'legal') {
            alert('Отправка ссылки на оплату доступна только для физ. лиц');
            return;
        }

        Atc.createReceipt(data, 'auto')
            .then(function (receipt) {
                window.location.href = 'paid_in_progress.html?id=' + receipt.id;
            })
            .catch(handleSubmitError);
    }

    function handleFixPaid() {
        var data = getFormData();
        if (!validate(data)) return;

        Atc.createReceipt(data, 'manual')
            .then(function (receipt) {
                window.location.href = 'paid_success.html?id=' + receipt.id;
            })
            .catch(handleSubmitError);
    }

    function handleSubmitError(error) {
        console.error(error);
        alert('Не удалось создать чек. Попробуйте ещё раз.');
    }

    function init() {
        populateActivitySelect();
        updateButtonsAvailability();

        document.querySelectorAll('input[name="customer_type"]').forEach(function (input) {
            input.addEventListener('change', updateButtonsAvailability);
        });

        document.getElementById('receipt-form').addEventListener('submit', handleSendLink);
        document.getElementById('fix_paid').addEventListener('click', handleFixPaid);

        var prefillAmount = Atc.getQueryParam('prefill_amount');
        if (prefillAmount) {
            document.getElementById('amount').value = prefillAmount;
        }
    }

    document.addEventListener('DOMContentLoaded', function () {
        Atc.load(['activities']).then(init).catch(Atc.showBootError);
    });
})();
