$(document).ready(function () {
    console.log('Initializing DataTables with order:', [[0, 'desc']]);

    // Инициализация DataTables с сортировкой по первому столбцу (по дате) по убыванию
    var table = $('#journal_table').DataTable({
        pageLength: 50, // Устанавливаем количество записей на странице по умолчанию
        order: [[0, 'desc']], // Сортировка по первому столбцу (дата) по убыванию по умолчанию
        columnDefs: [
            {
                targets: 0,        // Применение к первому столбцу
                type: 'datetime',  // Указываем тип данных для сортировки как дата-время
                render: function (data, type, row) {
                    // Приводим дату к нужному формату, если требуется
                    if (type === 'sort' || type === 'type') {
                        return data; // Используем значение из data-order для сортировки
                    }
                    return row[0]; // Отображаем оригинальное значение
                }
            }
        ],
        language: {
            search: "Поиск",
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

    console.log('Current order settings:', table.settings().init().order); // Проверка текущих настроек сортировки

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