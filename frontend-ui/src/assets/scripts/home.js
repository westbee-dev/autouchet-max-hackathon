(function () {
    'use strict';

    function buildHistoryItem(r) {
        var meta = Atc.STATUS_META[r.status];
        var buyerLabel = r.buyerType === 'legal' ? 'Юр. лицо' : 'Физ. лицо';

        var link = document.createElement('a');
        link.href = 'details.html?id=' + r.id;

        var item = document.createElement('div');
        item.className = 'history_item';

        var info = document.createElement('div');
        info.className = 'history_item_info';

        var title = document.createElement('span');
        title.className = 'history_item_title';
        title.textContent = r.description;

        var data = document.createElement('div');
        data.className = 'history_item_data';

        var tag = document.createElement('span');
        tag.className = 'history_item_tag';
        tag.textContent = buyerLabel;

        var date = document.createElement('span');
        date.className = 'history_item_date';
        date.textContent = Atc.formatDateShort(r.createdAt);

        data.appendChild(tag);
        data.appendChild(date);
        info.appendChild(title);
        info.appendChild(data);

        var price = document.createElement('div');
        price.className = 'history_item_price';

        var sum = document.createElement('span');
        sum.className = 'price';
        sum.textContent = Atc.formatMoney(r.amount);

        var status = document.createElement('span');
        status.className = 'status ' + meta.css;
        status.textContent = meta.label;

        price.appendChild(sum);
        price.appendChild(status);

        item.appendChild(info);
        item.appendChild(price);
        link.appendChild(item);

        return link;
    }

    function renderReceipts() {
        var itemsList = document.getElementById('items-list');
        var emptyState = document.getElementById('empty-history');
        var receipts = Atc.getReceipts();

        if (receipts.length === 0) {
            itemsList.hidden = true;
            emptyState.hidden = false;
            return;
        }

        itemsList.hidden = false;
        emptyState.hidden = true;
        itemsList.innerHTML = '';

        receipts.forEach(function (r) {
            itemsList.appendChild(document.createElement('div')).className = 'rope_line';
            itemsList.appendChild(buildHistoryItem(r));
        });
    }
    
    function renderDueAmount() {
        var due = Atc.getPreviousMonthDueAmount();
        var el = document.getElementById('due-amount');
        if (el) el.textContent = Atc.formatMoney(due);

        var prevMonthName = new Date(new Date().getFullYear(), new Date().getMonth() - 1, 1)
            .toLocaleString('ru-RU', { month: 'long' });
        var label = document.getElementById('due-month-label');
        if (label) label.textContent = 'К уплате за ' + prevMonthName;
    }

    function renderIncome() {
        var el = document.getElementById('annual-income');
        if (el) el.textContent = Atc.formatMoney(Atc.getAnnualIncome());
    }

    function renderLimit() {
        var info = Atc.getAnnualLimitInfo();
        var percentEl = document.getElementById('limit-percent');
        var barEl = document.getElementById('progress-bar-filled');

        if (percentEl) percentEl.textContent = info.percent.toFixed(1) + '%';
        if (barEl) {
            barEl.style.width = Math.min(info.percent, 100) + '%';
            barEl.classList.remove('progress_bar_filled--warning', 'progress_bar_filled--danger');
            if (info.level === 'warning') barEl.classList.add('progress_bar_filled--warning');
            if (info.level === 'danger') barEl.classList.add('progress_bar_filled--danger');
        }
    }

    document.addEventListener('DOMContentLoaded', function () {
        renderReceipts();
        renderDueAmount();
        renderIncome();
        renderLimit();
    });
})();
