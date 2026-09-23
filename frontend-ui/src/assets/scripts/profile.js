(function () {
    'use strict';

    function renderStats() {
        var receipts = Atc.getReceipts().filter(function (r) {
            return r.status === 'paid' || r.status === 'manual_recorded';
        });
        document.getElementById('receipts-count').textContent = receipts.length;
        document.getElementById('annual-income').textContent = Atc.formatMoney(Atc.getAnnualIncome());
    }

    function renderActivities() {
        var list = document.getElementById('activity-list');
        var activities = Atc.getActivities();
        list.innerHTML = '';

        activities.forEach(function (a, index) {
            var line = document.createElement('div');
            line.className = 'rope_line';
            list.appendChild(line);

            var item = document.createElement('div');
            item.className = 'activity_item' + (a.isDefault ? ' activity_item--default' : '');

            var info = document.createElement('div');
            info.className = 'activity_item_info';

            var star = document.createElement('span');
            star.className = 'activity_item_star';
            star.dataset.id = a.id;
            star.textContent = '★';
            star.title = a.isDefault ? 'Основной вид деятельности' : 'Сделать основным';

            var name = document.createElement('span');
            name.className = 'activity_item_name';
            name.textContent = a.name;

            info.appendChild(star);
            info.appendChild(name);

            var actions = document.createElement('div');
            actions.className = 'activity_item_actions';

            if (index > 0) {
                var up = document.createElement('button');
                up.type = 'button';
                up.className = 'activity_item_up';
                up.dataset.id = a.id;
                up.setAttribute('aria-label', 'Поднять выше');
                up.textContent = '↑';
                actions.appendChild(up);
            }

            var removeBtn = document.createElement('button');
            removeBtn.type = 'button';
            removeBtn.className = 'activity_item_remove';
            removeBtn.dataset.id = a.id;
            removeBtn.setAttribute('aria-label', 'Удалить');
            removeBtn.textContent = '×';
            actions.appendChild(removeBtn);

            item.appendChild(info);
            item.appendChild(actions);
            list.appendChild(item);
        });
    }

    function bindListEvents() {
        var list = document.getElementById('activity-list');

        list.addEventListener('click', function (e) {
            var star = e.target.closest('.activity_item_star');
            var up = e.target.closest('.activity_item_up');
            var removeBtn = e.target.closest('.activity_item_remove');

            if (star) {
                Atc.setDefaultActivity(star.dataset.id);
                renderActivities();
            }

            if (up) {
                Atc.moveActivityUp(up.dataset.id);
                renderActivities();
            }

            if (removeBtn) {
                Atc.removeActivity(removeBtn.dataset.id);
                renderActivities();
            }
        });
    }

    function bindForm() {
        document.getElementById('activity-form').addEventListener('submit', function (e) {
            e.preventDefault();
            var input = document.getElementById('new-activity-input');
            var name = input.value.trim();
            if (!name) return;

            Atc.addActivity(name);
            input.value = '';
            renderActivities();
        });
    }

    document.addEventListener('DOMContentLoaded', function () {
        renderStats();
        renderActivities();
        bindListEvents();
        bindForm();
    });
})();
