(function (window) {
    'use strict';

    var MAX_USER_ID_KEY = 'atc_max_user_id';
    var API_URL_KEY = 'atc_api_url';
    var API_URL = 'http://localhost:5000';

    function getApiUrl() {
        var fromQuery = new URLSearchParams(window.location.search).get('apiUrl');

        if (fromQuery) {
            try {
                localStorage.setItem(API_URL_KEY, fromQuery);
            } catch (e) {
                console.error('Не удалось сохранить apiUrl:', e);
            }
            return fromQuery;
        }

        try {
            return localStorage.getItem(API_URL_KEY) || API_URL;
        } catch (e) {
            return API_URL;
        }
    }

    function getMaxUser() {
        var app = window.WebApp;
        return app && app.initDataUnsafe && app.initDataUnsafe.user ? app.initDataUnsafe.user : null;
    }

    function getMaxUserId() {
        var user = getMaxUser();

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
        baseUrl: getApiUrl(),
        getMaxUserId: getMaxUserId,
        getInitData: function () { return window.WebApp ? window.WebApp.initData : null; }
    };
})(window);