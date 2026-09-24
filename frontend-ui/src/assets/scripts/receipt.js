(function () {
    'use strict';
    function toDateInputValue(date) {
        var d = new Date(date);
        var mm = String(d.getMonth() + 1).padStart(2, '0');
        var dd = String(d.getDate()).padStart(2, '0');
        return d.getFullYear() + '-' + mm + '-' + dd;
    }

    function getSelectedDate() {
        var input = document.getElementById('paid_date');
        if (!input || !input.value) return new Date();
        var parts = input.value.split('-');
        return new Date(Number(parts[0]), Number(parts[1]) - 1, Number(parts[2]), 12, 0, 0);
    }

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
        var amount = Number(form.amount.value);
        var buyerType = getSelectedBuyerType();
        var activityId = form.activity.value;
        var activities = Atc.getActivities();
        var activity = activities.find(function (a) { return a.id === activityId; });

        return {
            amount: amount,
            buyerType: buyerType,
            description: activity ? activity.name : 'Без названия'
        };
    }

    function validate(data) {
        if (!data.amount || data.amount <= 0) {
            alert('Укажите сумму больше нуля');
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

        var receipt = Atc.addReceipt(Object.assign({}, data, {
            status: 'awaiting_payment',
            source: 'robokassa'
        }));

        window.location.href = 'paid_in_progress.html?id=' + receipt.id;
    }

    function handleFixPaid() {
        var data = getFormData();
        if (!validate(data)) return;

        var paidDate = getSelectedDate();

        var receipt = Atc.addReceipt(Object.assign({}, data, {
            status: 'manual_recorded',
            source: 'manual',
            createdAt: paidDate.toISOString(),
            paidAt: paidDate.toISOString()
        }));

        window.location.href = 'paid_success.html?id=' + receipt.id;
    }

    document.addEventListener('DOMContentLoaded', function () {
        populateActivitySelect();
        updateButtonsAvailability();
        var paidDateInput = document.getElementById('paid_date');
        if (paidDateInput && !paidDateInput.value) {
            paidDateInput.value = toDateInputValue(new Date());
        }

        document.querySelectorAll('input[name="customer_type"]').forEach(function (input) {
            input.addEventListener('change', updateButtonsAvailability);
        });

        document.getElementById('receipt-form').addEventListener('submit', handleSendLink);
        document.getElementById('fix_paid').addEventListener('click', handleFixPaid);
        var prefillAmount = Atc.getQueryParam('prefill_amount');
        if (prefillAmount) {
            document.getElementById('amount').value = prefillAmount;
        }
    });
})();
