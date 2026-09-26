(function (window) {
    'use strict';

    var MAX_USER_ID_KEY = 'atc_max_user_id';

    function getMaxUserId() {
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
        baseUrl: 'http://localhost:5232',
        getMaxUserId: getMaxUserId
    };
})(window);
