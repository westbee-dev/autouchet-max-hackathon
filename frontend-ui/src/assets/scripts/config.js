(function (window) {
    'use strict';

    var MAX_USER_ID_KEY = 'atc_max_user_id';

    function getInitDataUser() {
        var app = window.MAX || window.Max || window.WebApp;
        var initData = app && app.initData;

        if (!initData || !initData.user) return null;
        return initData.user;
    }

    function getMaxUserId() {
        var user = getInitDataUser();

        if (user && user.id) {
            var fromMax = String(user.id);

            try {
                localStorage.setItem(MAX_USER_ID_KEY, fromMax);
            } catch (e) {
                console.error('Не удалось сохранить maxUserId:', e);
            }
            return fromMax;
        }

        var fromQuery = new URLSearchParams(window.location.search).get('maxUserId');

        if (fromQuery) {
            try {
                localStorage.setItem(MAX_USER_ID_KEY, fromQuery);
            } catch (e) {
                console.error('Не удалось сохранить maxUserId:', e);
            }
            return fromQuery;
        }

        try {
            return localStorage.getItem(MAX_USER_ID_KEY);
        } catch (e) {
            return null;
        }
    }

    window.ATC_CONFIG = {
        baseUrl: '',
        getMaxUserId: getMaxUserId,
        getInitData: function () {
            var app = window.MAX || window.Max || window.WebApp;
            return app && app.initData ? app.initData : null;
        }
    };
})(window);
