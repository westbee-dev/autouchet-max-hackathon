(function (window) {
    'use strict';

    var RECEIPTS_KEY = 'atc_receipts';
    var ACTIVITIES_KEY = 'atc_activities';
    var SETTINGS_KEY = 'atc_settings';

    var TAX_RATE = { individual: 0.04, legal: 0.06 };
    var ANNUAL_LIMIT = 2400000;

    var STATUS_META = {
        paid: { label: 'Оплачено',    css: 'approved' },
        manual_recorded: { label: 'Оплачено',    css: 'approved' },
        awaiting_payment: { label: 'В обработке', css: 'remaining' },
        expired: { label: 'Не оплачено', css: 'denied' }
    };

    function uid() {
        return 'r_' + Date.now().toString(36) + Math.random().toString(36).slice(2, 7);
    }

    function readJSON(key, fallback) {
        try {
            var raw = localStorage.getItem(key);
            return raw ? JSON.parse(raw) : fallback;
        } catch (e) {
            console.error('Storage read error:', key, e);
            return fallback;
        }
    }

    function writeJSON(key, value) {
        try {
            localStorage.setItem(key, JSON.stringify(value));
        } catch (e) {
            console.error('Storage write error:', key, e);
        }
    }

    function mkReceipt(data) {
        return Object.assign({
            id: uid(),
            source: 'manual',
            paidAt: null
        }, data, { taxRate: TAX_RATE[data.buyerType] });
    }

    function getReceipts() { return readJSON(RECEIPTS_KEY, []); }
    function saveReceipts(list) { writeJSON(RECEIPTS_KEY, list); }

    function addReceipt(data) {
        var list = getReceipts();
        var receipt = mkReceipt(Object.assign({ createdAt: new Date().toISOString() }, data));
        list.unshift(receipt);
        saveReceipts(list);

        return receipt;
    }

    function updateReceipt(id, patch) {
        var list = getReceipts();
        var idx = list.findIndex(function (r) { return r.id === id; });
        if (idx === -1) {
            return null;
        }

        list[idx] = Object.assign({}, list[idx], patch);
        saveReceipts(list);

        return list[idx];
    }

    function removeReceipt(id) {
        var list = getReceipts().filter(function (r) { return r.id !== id; });
        saveReceipts(list);

        return list;
    }

    function getReceiptById(id) {
        return getReceipts().find(function (r) { return r.id === id; }) || null;
    }

    function sortActivities(list) {
        return list.slice().sort(function (a, b) {
            if (a.isDefault === b.isDefault) return 0;
            return a.isDefault ? -1 : 1;
        });
    }

    function getActivities() { return sortActivities(readJSON(ACTIVITIES_KEY, [])); }
    function saveActivities(list) { writeJSON(ACTIVITIES_KEY, list); }

    function addActivity(name) {
        var list = getActivities();

        list.push({ id: 'a_' + Date.now(), name: name, isDefault: list.length === 0 });
        saveActivities(list);

        return list;
    }

    function removeActivity(id) {
        var list = getActivities().filter(function (a) { return a.id !== id; });

        if (list.length && !list.some(function (a) { return a.isDefault; })) {
            list[0].isDefault = true;
        }
        saveActivities(list);
        
        return list;
    }

    function setDefaultActivity(id) {
        var list = getActivities().map(function (a) {
            return Object.assign({}, a, { isDefault: a.id === id });
        });
        saveActivities(list);

        return sortActivities(list);
    }

    function moveActivityUp(id) {
        var list = getActivities();
        var idx = list.findIndex(function (a) { return a.id === id; });

        if (idx > 0) {
            var moved = list.splice(idx, 1)[0];
            list.splice(idx - 1, 0, moved);
            saveActivities(list);
        }

        return getActivities();
    }

    function getSettings() { return readJSON(SETTINGS_KEY, DEFAULT_SETTINGS); }
    function saveSettings(patch) {
        var settings = Object.assign({}, getSettings(), patch);
        writeJSON(SETTINGS_KEY, settings);

        return settings;
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

    function getAnnualIncome(year) {
        year = year || new Date().getFullYear();
        return getReceipts()
            .filter(function (r) {
                if (!isCountedStatus(r.status)) return false;
                return new Date(r.paidAt || r.createdAt).getFullYear() === year;
            })
            .reduce(function (sum, r) { return sum + r.amount; }, 0);
    }

    function getAnnualLimitInfo() {
        var income = getAnnualIncome();
        var percent = (income / ANNUAL_LIMIT) * 100;
        var level = 'normal';
        if (percent >= 100) level = 'danger';
        else if (percent >= 80) level = 'warning';
        return { income: income, percent: Math.min(percent, 999), level: level, limit: ANNUAL_LIMIT };
    }

    function getReceiptsByQuarter(quarter, year) {
        var ranges = { 1: [0, 2], 2: [3, 5], 3: [6, 8], 4: [9, 11] };
        var range = ranges[quarter];
        return getReceipts().filter(function (r) {
            if (!isCountedStatus(r.status)) return false;
            var d = new Date(r.paidAt || r.createdAt);
            return d.getFullYear() === year && d.getMonth() >= range[0] && d.getMonth() <= range[1];
        });
    }

    function formatMoney(amount) {
        var rounded = Math.round(amount);
        var withSpaces = rounded.toString().replace(/\B(?=(\d{3})+(?!\d))/g, '\u00A0\u00A0');
        return withSpaces + '\u00A0₽';
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
        getReceipts: getReceipts,
        addReceipt: addReceipt,
        updateReceipt: updateReceipt,
        removeReceipt: removeReceipt,
        getReceiptById: getReceiptById,
        getActivities: getActivities,
        addActivity: addActivity,
        removeActivity: removeActivity,
        setDefaultActivity: setDefaultActivity,
        moveActivityUp: moveActivityUp,
        getSettings: getSettings,
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
