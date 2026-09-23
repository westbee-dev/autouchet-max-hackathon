(function () {
    'use strict';

    document.addEventListener('DOMContentLoaded', function () {
        var toggle = document.querySelector('.switch_input');
        var settings = Atc.getSettings();

        toggle.checked = settings.remindersEnabled;

        toggle.addEventListener('change', function () {
            Atc.saveSettings({ remindersEnabled: toggle.checked });
        });
    });
})();
