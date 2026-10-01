(function (window) {
    'use strict';

    var MAX_USER_ID_KEY = 'atc_max_user_id';

    function getApp() {
        return window.MAX || window.Max || window.WebApp;
    }

    function getInitDataUnsafe() {
        var app = getApp();
        return app && app.initDataUnsafe ? app.initDataUnsafe : null;
    }

    function getInitDataUser() {
        var unsafe = getInitDataUnsafe();

        if (unsafe && unsafe.user) return unsafe.user;

        var app = getApp();
        var initData = app && app.initData;

        if (initData && typeof initData === 'object' && initData.user) return initData.user;

        return null;
    }

    function rememberUserId(id) {
        var value = String(id);

        try {
            localStorage.setItem(MAX_USER_ID_KEY, value);
        } catch (e) {
            console.error('Не удалось сохранить maxUserId:', e);
        }

        return value;
    }

    function getMaxUserId() {
        var user = getInitDataUser();

        if (user && user.id) {
            return rememberUserId(user.id);
        }

        var unsafe = getInitDataUnsafe();

        if (unsafe && unsafe.start_param) {
            return rememberUserId(unsafe.start_param);
        }

        var params = new URLSearchParams(window.location.search);
        var fromQuery = params.get('WebAppStartParam') || params.get('maxUserId');

        if (fromQuery) {
            return rememberUserId(fromQuery);
        }

        try {
            return localStorage.getItem(MAX_USER_ID_KEY);
        } catch (e) {
            return null;
        }
    }
    function describe() {
        var app = getApp();
        var unsafe = getInitDataUnsafe();

        return [
            'sdk=' + (app ? 'yes' : 'no'),
            'user=' + (unsafe && unsafe.user ? unsafe.user.id : '-'),
            'start_param=' + (unsafe && unsafe.start_param ? unsafe.start_param : '-'),
            'search=' + (window.location.search || '-')
        ].join(' | ');
    }

    window.ATC_CONFIG = {
        // Для запуска через nginx на том же origin оставьте пустым.
        // При хостинге фронта отдельно (например, Cloudflare) укажите адрес API,
        // например: baseUrl: 'https://<ваш-адрес>.trycloudflare.com',
        baseUrl: '',
        getMaxUserId: getMaxUserId,
        getInitData: function () {
            return getInitDataUnsafe();
        },
        describe: describe
    };
})(window);
