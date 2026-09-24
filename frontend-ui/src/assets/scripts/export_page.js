(function () {
    'use strict';

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

    document.addEventListener('DOMContentLoaded', function () {
        document.getElementById('export-form').addEventListener('submit', function (e) {
            e.preventDefault();

            var selected = document.querySelector('input[name="quarter"]:checked');
            if (!selected) return;

            var quarterMatch = selected.value.match(/q(\d)-(\d{4})/);
            var quarter = Number(quarterMatch[1]);
            var year = Number(quarterMatch[2]);

            var receipts = Atc.getReceiptsByQuarter(quarter, year);

            if (receipts.length === 0) {
                alert('За этот квартал нет оплаченных или зафиксированных чеков');
                return;
            }

            var text = buildReportText(quarter, year, receipts);
            downloadTextFile('otchet-q' + quarter + '-' + year + '.txt', text);
        });
    });
})();
