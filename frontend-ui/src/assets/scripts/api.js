(function (window) {
    'use strict';

    function baseUrl() {
        return window.ATC_CONFIG.baseUrl;
    }

    function buildUrl(path, query) {
        var url = baseUrl() + path;
        var parts = [];

        Object.keys(query || {}).forEach(function (key) {
            var value = query[key];
            if (value === undefined || value === null) return;

            parts.push(encodeURIComponent(key) + '=' + encodeURIComponent(value));
        });

        return parts.length ? url + '?' + parts.join('&') : url;
    }

    function request(method, path, options) {
        options = options || {};

        var init = { method: method, headers: {} };

        if (options.body !== undefined && options.body !== null) {
            init.headers['Content-Type'] = 'application/json';
            init.body = JSON.stringify(options.body);
        }

        return fetch(buildUrl(path, options.query), init).then(function (response) {
            if (response.status === 404) return null;

            if (!response.ok) {
                return response.text().then(function (text) {
                    throw new Error(method + ' ' + path + ' → ' + response.status + ' ' + text);
                });
            }

            return response.text().then(function (text) {
                return text ? JSON.parse(text) : null;
            });
        });
    }

    window.AtcApi = {
        get: function (path, query) {
            return request('GET', path, { query: query });
        },
        post: function (path, body, query) {
            return request('POST', path, { body: body, query: query });
        },
        put: function (path, body, query) {
            return request('PUT', path, { body: body, query: query });
        },
        del: function (path, query) {
            return request('DELETE', path, { query: query });
        }
    };
})(window);
