(function () {
    'use strict';

    document.addEventListener('DOMContentLoaded', function () {
        Atc.load(['settings']).then(function () {
            var toggle = document.querySelector('.switch_input');
            if (!toggle) return;

            toggle.checked = !!Atc.getSettings().remindAboutTax;

            toggle.addEventListener('change', function () {
                Atc.saveSettings({ remindAboutTax: toggle.checked })
                    .catch(function (error) {
                        console.error(error);
                        toggle.checked = !!Atc.getSettings().remindAboutTax;
                    });
            });
        }).catch(Atc.showBootError);
    });
})();
