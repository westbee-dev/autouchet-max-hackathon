(function () {
    'use strict';

    document.addEventListener('DOMContentLoaded', function () {
        var openBtn = document.getElementById('menu-open');
        var sheet = document.getElementById('menu-sheet');
        var backdrop = document.getElementById('menu-backdrop');

        if (!openBtn || !sheet || !backdrop) return;

        function openMenu() {
            backdrop.hidden = false;
            sheet.hidden = false;
            requestAnimationFrame(function () {
                backdrop.classList.add('menu_backdrop--visible');
                sheet.classList.add('menu_sheet--open');
            });
            sheet.setAttribute('aria-hidden', 'false');
        }

        function closeMenu() {
            backdrop.classList.remove('menu_backdrop--visible');
            sheet.classList.remove('menu_sheet--open');
            sheet.setAttribute('aria-hidden', 'true');

            setTimeout(function () {
                backdrop.hidden = true;
                sheet.hidden = true;
            }, 250);
        }

        openBtn.addEventListener('click', openMenu);
        backdrop.addEventListener('click', closeMenu);

        document.addEventListener('keydown', function (e) {
            if (e.key === 'Escape' && !sheet.hidden) closeMenu();
        });
    });
})();
