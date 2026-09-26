(function (window) {
    'use strict';

    var TAX_RATE = { individual: 0.04, legal: 0.06 };
    var ANNUAL_LIMIT = 2400000;

    var BUYER_TYPE_TO_API = { individual: 'Физ', legal: 'Юр' };
    var BUYER_TYPE_FROM_API = { 'Физ': 'individual', 'Юр': 'legal' };

    var STATUS_FROM_API = {
        WaitingPayment: 'awaiting_payment',
        Expired: 'expired'
    };

    var STATUS_META = {
        paid: { label: 'Оплачено', css: 'approved' },
        manual_recorded: { label: 'Оплачено', css: 'approved' },
        awaiting_payment: { label: 'В обработке', css: 'remaining' },
        expired: { label: 'Не оплачено', css: 'denied' }
    };

    var Store = {
        receipts: null,
        activities: null,
        settings: null,
        dashboard: null,
        profile: null
    };

    var LOADERS = {
        receipts: function () {
            return apiGet('/api/Receipts').then(function (list) {
                Store.receipts = (list || []).map(toReceipt);
            });
        },
        activities: function () {
            return apiGet('/api/Users/activities').then(function (list) {
                Store.activities = sortActivities(list || []);
            });
        },
        settings: function () {
            return apiGet('/api/Users/settings').then(function (dto) {
                Store.settings = dto;
            });
        },
        dashboard: function () {
            return apiGet('/api/Dashboard').then(function (dto) {
                Store.dashboard = dto;
            });
        },
        profile: function () {
            return apiGet('/api/Users/profile').then(function (dto) {
                Store.profile = dto;
            });
        }
    };

    function requireUserId() {
        var id = window.ATC_CONFIG.getMaxUserId();

        if (!id) {
            throw new Error(
                'Не передан maxUserId. Откройте приложение по ссылке из бота (?maxUserId=...).'
            );
        }

        return id;
    }

    function apiGet(path) {
        return window.AtcApi.get(path, { maxUserId: requireUserId() });
    }

    function toReceipt(dto) {
        return {
            id: dto.id,
            amount: dto.amount,
            buyerType: BUYER_TYPE_FROM_API[dto.buyerType] || 'individual',
            description: dto.purposeOfPayment,
            taxRate: dto.taxRate,
            status: resolveStatus(dto),
            source: dto.paymentType === 'Manual' ? 'manual' : 'robokassa',
            paymentUrl: dto.paymentUrl,
            createdAt: dto.createdAt,
            paidAt: dto.paidAt
        };
    }

    function resolveStatus(dto) {
        if (dto.status === 'Paid') {
            return dto.paymentType === 'Manual' ? 'manual_recorded' : 'paid';
        }

        return STATUS_FROM_API[dto.status] || 'awaiting_payment';
    }

    function sortActivities(list) {
        return list.slice().sort(function (a, b) {
            if (a.isDefault === b.isDefault) return 0;
            return a.isDefault ? -1 : 1;
        });
    }

    function showBootError(error) {
        console.error(error);

        var target = document.querySelector('main') || document.body;
        var box = document.createElement('div');
        box.className = 'boot_error';
        box.textContent = error && error.message
            ? error.message
            : 'Не удалось загрузить данные. Проверьте соединение с сервером.';

        target.innerHTML = '';
        target.appendChild(box);
    }

    function load(keys, refresh) {
        return Promise.all(keys.map(function (key) {
            var loader = LOADERS[key];
            if (!loader) return Promise.reject(new Error('Неизвестный раздел данных: ' + key));

            if (refresh || Store[key] === null) {
                try {
                    return Promise.resolve(loader());
                } catch (e) {
                    return Promise.reject(e);
                }
            }

            return Promise.resolve();
        })).then(function () { return Store; });
    }

    function getReceipts() { return Store.receipts || []; }

    function getReceiptById(id) {
        return getReceipts().filter(function (r) {
            return String(r.id) === String(id);
        })[0] || null;
    }

    function createReceipt(data, mode) {
        var payload = {
            maxUserId: Number(requireUserId()),
            buyerType: BUYER_TYPE_TO_API[data.buyerType] || 'Физ',
            amount: data.amount,
            activityId: Number(data.activityId)
        };

        var path = mode === 'manual'
            ? '/api/Receipts/mark-as-paid'
            : '/api/Receipts/send-payment-link';

        return window.AtcApi.post(path, payload).then(function (dto) {
            Store.receipts = null;
            return { id: dto.id, paymentUrl: dto.paymentUrl, paidAt: dto.paidAt };
        });
    }

    function removeReceipt(id) {
        return window.AtcApi.del('/api/Receipts/' + id, { maxUserId: requireUserId() })
            .then(function () {
                Store.receipts = (Store.receipts || []).filter(function (r) {
                    return String(r.id) !== String(id);
                });
            });
    }

    function getActivities() { return Store.activities || []; }

    function addActivity(name) {
        return window.AtcApi
            .post('/api/Users/activities', { name: name }, { maxUserId: requireUserId() })
            .then(function () {
                return LOADERS.activities();
            });
    }

    function removeActivity(id) {
        return window.AtcApi
            .del('/api/Users/activities/' + id, { maxUserId: requireUserId() })
            .then(function () {
                return LOADERS.activities();
            });
    }

    function setDefaultActivity(id) {
        return window.AtcApi
            .put('/api/Users/activities/' + id + '/default', null, { maxUserId: requireUserId() })
            .then(function () {
                return LOADERS.activities();
            });
    }

    function getSettings() { return Store.settings || { remindAboutTax: false }; }

    function getDashboard() { return Store.dashboard; }

    function getProfile() { return Store.profile; }

    function saveSettings(patch) {
        var next = Object.assign({}, getSettings(), patch);

        return window.AtcApi
            .put('/api/Users/settings', next, { maxUserId: requireUserId() })
            .then(function (dto) {
                Store.settings = dto;
                return dto;
            });
    }

    function isCountedStatus(status) {
        return status === 'paid' || status === 'manual_recorded';
    }

    function getPreviousMonthDueAmount() {
        var now = new Date();
        var prev = new Date(now.getFullYear(), now.getMonth() - 1, 1);
        var y = prev.getFullYear(), m = prev.getMonth();

        return getReceipts()
            .filter(function (r) {
                if (!isCountedStatus(r.status)) return false;
                var d = new Date(r.paidAt || r.createdAt);
                return d.getFullYear() === y && d.getMonth() === m;
            })
            .reduce(function (sum, r) { return sum + r.amount * r.taxRate; }, 0);
    }

    function getAnnualIncome() {
        return Store.dashboard ? Store.dashboard.totalIncomeYear : 0;
    }

    function getAnnualLimitInfo() {
        var percent = Store.dashboard ? Store.dashboard.limitPercent : 0;
        var level = 'normal';

        if (percent >= 100) level = 'danger';
        else if (percent >= 80) level = 'warning';

        return {
            income: getAnnualIncome(),
            percent: percent,
            level: level,
            limit: ANNUAL_LIMIT
        };
    }

    function getReceiptsByQuarter(quarter, year) {
        return window.AtcApi
            .get('/api/Receipts/export', {
                maxUserId: requireUserId(),
                quarter: quarter,
                year: year
            })
            .then(function (list) {
                return (list || []).map(toReceipt);
            });
    }

    function formatMoney(amount) {
        var rounded = Math.round(amount);
        var withSpaces = rounded.toString().replace(/\B(?=(\d{3})+(?!\d))/g, '  ');
        return withSpaces + ' ₽';
    }

    function formatDateShort(iso) {
        var d = new Date(iso);
        var dd = String(d.getDate()).padStart(2, '0');
        var mm = String(d.getMonth() + 1).padStart(2, '0');
        return dd + '.' + mm;
    }

    function formatDateFull(iso) {
        var d = new Date(iso);
        var dd = String(d.getDate()).padStart(2, '0');
        var mm = String(d.getMonth() + 1).padStart(2, '0');
        return dd + '.' + mm + '.' + d.getFullYear();
    }

    function getQueryParam(name) {
        return new URLSearchParams(window.location.search).get(name);
    }

    function copyToClipboard(text) {
        if (navigator.clipboard && window.isSecureContext) {
            return navigator.clipboard.writeText(text);
        }

        return new Promise(function (resolve, reject) {
            var textarea = document.createElement('textarea');
            textarea.value = text;
            textarea.style.position = 'fixed';
            textarea.style.opacity = '0';
            document.body.appendChild(textarea);
            textarea.focus();
            textarea.select();
            try {
                document.execCommand('copy');
                resolve();
            } catch (e) {
                reject(e);
            } finally {
                document.body.removeChild(textarea);
            }
        });
    }

    window.Atc = {
        TAX_RATE: TAX_RATE,
        ANNUAL_LIMIT: ANNUAL_LIMIT,
        STATUS_META: STATUS_META,
        load: load,
        showBootError: showBootError,
        getReceipts: getReceipts,
        getReceiptById: getReceiptById,
        createReceipt: createReceipt,
        removeReceipt: removeReceipt,
        getActivities: getActivities,
        addActivity: addActivity,
        removeActivity: removeActivity,
        setDefaultActivity: setDefaultActivity,
        getSettings: getSettings,
        getDashboard: getDashboard,
        getProfile: getProfile,
        saveSettings: saveSettings,
        getPreviousMonthDueAmount: getPreviousMonthDueAmount,
        getAnnualIncome: getAnnualIncome,
        getAnnualLimitInfo: getAnnualLimitInfo,
        getReceiptsByQuarter: getReceiptsByQuarter,
        formatMoney: formatMoney,
        formatDateShort: formatDateShort,
        formatDateFull: formatDateFull,
        getQueryParam: getQueryParam,
        copyToClipboard: copyToClipboard
    };
})(window);
