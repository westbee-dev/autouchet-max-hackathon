(function () {
    'use strict';

    function init() {
        renderProfile();
        renderActivities();
        bindListEvents();
        bindForm();
    }

    function checksWord(count) {
        var n = Math.abs(count) % 100;
        var n1 = n % 10;

        if (n > 10 && n < 20) return 'чеков';
        if (n1 > 1 && n1 < 5) return 'чека';
        if (n1 === 1) return 'чек';

        return 'чеков';
    }

    function renderProfile() {
        var profile = Atc.getProfile();

        var nameEl = document.getElementById('profile-name');
        var avatarEl = document.querySelector('.profile_avatar');
        var userIdEl = document.getElementById('profile-user-id');
        var countEl = document.getElementById('receipts-count');
        var countLabelEl = document.getElementById('receipts-count-label');
        var incomeEl = document.getElementById('annual-income');

        if (!profile) return;

        if (nameEl) nameEl.textContent = profile.firstName || 'Пользователь';
        if (avatarEl) avatarEl.textContent = (profile.firstName || 'П')[0].toUpperCase();
        if (userIdEl) userIdEl.textContent = 'id: ' + profile.maxUserId;
        if (countEl) countEl.textContent = profile.receiptCountYear;
        if (countLabelEl) countLabelEl.textContent = checksWord(profile.receiptCountYear) + ' за год';
        if (incomeEl) incomeEl.textContent = Atc.formatMoney(profile.totalIncomeYear);
    }

    function renderStatusActivity() {
        var el = document.getElementById('status-activity');
        if (!el) return;

        var activities = Atc.getActivities();
        var current = activities.filter(function (a) { return a.isDefault; })[0] || activities[0];

        el.textContent = current ? 'Самозанятый · ' + current.name : 'Самозанятый';
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
        document.getElementById('activity-list').addEventListener('click', function (e) {
            var star = e.target.closest('.activity_item_star');
            var removeBtn = e.target.closest('.activity_item_remove');

            if (star) {
                Atc.setDefaultActivity(star.dataset.id)
                    .then(renderActivities)
                    .catch(handleError);
            }

            if (removeBtn) {
                Atc.removeActivity(removeBtn.dataset.id)
                    .then(renderActivities)
                    .catch(handleError);
            }
        });
    }

    function bindForm() {
        document.getElementById('activity-form').addEventListener('submit', function (e) {
            e.preventDefault();

            var input = document.getElementById('new-activity-input');
            var name = input.value.trim();
            if (!name) return;

            Atc.addActivity(name).then(function () {
                input.value = '';
                renderActivities();
            }).catch(handleError);
        });
    }

    function handleError(error) {
        console.error(error);
        alert('Не удалось сохранить изменения. Попробуйте ещё раз.');
    }

    document.addEventListener('DOMContentLoaded', function () {
        Atc.load(['profile', 'activities']).then(init).catch(Atc.showBootError);
    });
})();
