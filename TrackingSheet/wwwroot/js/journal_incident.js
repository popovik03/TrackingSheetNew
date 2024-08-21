$(document).ready(function () {
    // Инициализация DataTables
    var table = $('#journal_table').DataTable({
        language: {
            search: "Поиск:",
            lengthMenu: "Показать _MENU_ записей на странице",
            zeroRecords: "Ничего не найдено",
            info: "Показаны записи с _START_ по _END_ из _TOTAL_",
            infoEmpty: "Нет записей для отображения",
            infoFiltered: "(отфильтровано из _MAX_ записей)",
            paginate: {
                first: "Первая",
                previous: "Предыдущая",
                next: "Следующая",
                last: "Последняя"
            }
        }
    });

    // Функция для фильтрации по диапазону дат
    $.fn.dataTable.ext.search.push(
        function (settings, data, dataIndex) {
            var min = new Date($('#min-date').val());
            var max = new Date($('#max-date').val());
            var date = new Date(data[0]); // Предполагаем, что дата находится в первом столбце

            if (isNaN(min.getTime()) && isNaN(max.getTime())) {
                return true;
            }
            if (isNaN(min.getTime()) && date <= max) {
                return true;
            }
            if (isNaN(max.getTime()) && date >= min) {
                return true;
            }
            if (date <= max && date >= min) {
                return true;
            }
            return false;
        }
    );

    // Обработчики событий для обновления фильтрации
    $('#min-date, #max-date').on('change', function () {
        table.draw();
    });

    // Обработчик экспорта в Excel
    $('#exportToExcel').on('click', function () {
        var wb = XLSX.utils.table_to_book(document.getElementById('journal_table'), { sheet: "Sheet1" });
        var wbout = XLSX.write(wb, { bookType: 'xlsx', type: 'binary' });

        function s2ab(s) {
            var buf = new ArrayBuffer(s.length);
            var view = new Uint8Array(buf);
            for (var i = 0; i < s.length; i++) view[i] = s.charCodeAt(i) & 0xFF;
            return buf;
        }

        saveAs(new Blob([s2ab(wbout)], { type: "application/octet-stream" }), 'journal_incidents.xlsx');
    });
});
