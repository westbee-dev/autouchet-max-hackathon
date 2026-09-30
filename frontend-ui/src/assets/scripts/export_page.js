(function () {
    'use strict';

    function renderQuarterList() {
        var list = document.querySelector('.quarter_list');
        if (!list) return;

        var now = new Date();
        var year = now.getFullYear();
        var currentQuarter = Math.floor(now.getMonth() / 3) + 1;

        var rope = document.createElement('div');
        rope.className = 'rope_line';

        list.innerHTML = '';

        for (var i = 0; i < 4; i++) {
            var quarter = ((currentQuarter - i - 1 + 4) % 4) + 1;

            list.appendChild(rope.cloneNode());

            var label = document.createElement('label');
            label.className = 'quarter_item';

            var text = document.createElement('span');
            text.className = 'quarter_item_label';
            text.textContent = quarter + ' квартал ' + year;

            var radio = document.createElement('input');
            radio.type = 'radio';
            radio.name = 'quarter';
            radio.className = 'quarter_item_radio';
            radio.value = 'q' + quarter + '-' + year;
            radio.checked = i === 0;

            var circle = document.createElement('span');
            circle.className = 'quarter_item_circle';

            label.appendChild(text);
            label.appendChild(radio);
            label.appendChild(circle);
            list.appendChild(label);
        }
    }

    function buildReportText(quarter, year, receipts) {
        var lines = [];
        lines.push('Отчёт за ' + quarter + ' квартал ' + year + ' года');
        lines.push('Документ носит справочный характер, не является официальным отчётом ФНС');
        lines.push('');

        var totalIncome = 0;
        var totalTax = 0;

        receipts.forEach(function (r) {
            var tax = r.amount * r.taxRate;
            totalIncome += r.amount;
            totalTax += tax;
            lines.push(
                Atc.formatDateFull(r.paidAt || r.createdAt) + '  ' +
                r.description + '  ' +
                (r.buyerType === 'legal' ? 'Юр. лицо' : 'Физ. лицо') + '  ' +
                Atc.formatMoney(r.amount)
            );
        });

        lines.push('');
        lines.push('Итого доход: ' + Atc.formatMoney(totalIncome));
        lines.push('Итого налог к уплате: ' + Atc.formatMoney(totalTax));

        return lines.join('\n');
    }

    function downloadTextFile(filename, text) {
        var blob = new Blob([text], { type: 'text/plain;charset=utf-8' });
        var url = URL.createObjectURL(blob);
        var a = document.createElement('a');
        a.href = url;
        a.download = filename;
        document.body.appendChild(a);
        a.click();
        document.body.removeChild(a);
        URL.revokeObjectURL(url);
    }

    function handleExport(e) {
        e.preventDefault();

        var selected = document.querySelector('input[name="quarter"]:checked');
        if (!selected) return;

        var quarterMatch = selected.value.match(/q(\d)-(\d{4})/);
        var quarter = Number(quarterMatch[1]);
        var year = Number(quarterMatch[2]);

        Atc.getReceiptsByQuarter(quarter, year)
            .then(function (receipts) {
                if (receipts.length === 0) {
                    alert('За этот квартал нет оплаченных или зафиксированных чеков');
                    return;
                }

                var text = buildReportText(quarter, year, receipts);
                downloadTextFile('otchet-q' + quarter + '-' + year + '.txt', text);
            })
            .catch(function (error) {
                console.error(error);
                alert('Не удалось сформировать отчёт. Попробуйте ещё раз.');
            });
    }

    document.addEventListener('DOMContentLoaded', function () {
        renderQuarterList();
        document.getElementById('export-form').addEventListener('submit', handleExport);
    });
})();
