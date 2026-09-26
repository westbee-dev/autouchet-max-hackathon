(function () {
    'use strict';

    function renderProfileName() {
        var nameEl = document.getElementById('profile-name');
        var avatarEl = document.querySelector('.profile_avatar');

        if (!nameEl || !avatarEl) return;

        var name = nameEl.textContent.trim();
        avatarEl.textContent = name ? name[0].toUpperCase() : 'П';
    }

    function renderStatusActivity() {
        var el = document.getElementById('status-activity');
        if (!el) return;

        var activities = Atc.getActivities();
        var current = activities.filter(function (a) { return a.isDefault; })[0] || activities[0];

        el.textContent = current ? 'Самозанятый · ' + current.name : 'Самозанятый';
    }

    function renderStats() {
        var receipts = Atc.getReceipts().filter(function (r) {
            return r.status === 'paid' || r.status === 'manual_recorded';
        });
        document.getElementById('receipts-count').textContent = receipts.length;
        document.getElementById('annual-income').textContent = Atc.formatMoney(Atc.getAnnualIncome());
    }

    function renderActivities() {
        renderStatusActivity();

        var list = document.getElementById('activity-list');
        var activities = Atc.getActivities();
        list.innerHTML = '';

        activities.forEach(function (a) {
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
            var removeBtn = e.target.closest('.activity_item_remove');

            if (star) {
                Atc.setDefaultActivity(star.dataset.id);
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
        renderProfileName();
        renderStats();
        renderActivities();
        bindListEvents();
        bindForm();
    });
})();
