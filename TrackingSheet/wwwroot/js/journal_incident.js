document.addEventListener('DOMContentLoaded', async function () {
    try {
        // Подключение сортировки дат через moment.js
        $.fn.dataTable.moment('DD/MM/YYYY HH:mm');

        const response = await fetch('/api/incidents/all');
        const result = await response.json();

        // Инициализация DataTable
        var table = $('#journal_table').DataTable({
            data: result.data,
            columns: [
                {
                    title: "Дата и время",
                    data: "date",
                    render: function (data) {
                        const date = new Date(data);
                        const formattedDateTime = `${date.getDate().toString().padStart(2, '0')}/${(date.getMonth() + 1).toString().padStart(2, '0')}/${date.getFullYear()} ${date.getHours().toString().padStart(2, '0')}:${date.getMinutes().toString().padStart(2, '0')}`;
                        return formattedDateTime;
                    }
                },
                { title: "Смена", data: "shift" },
                { title: "431", data: "reporter" },
                { title: "VSAT", data: "vsat" },
                { title: "Скважина", data: "well" },
                { title: "Рейс", data: "run" },
                { title: "Сохр. НПВ", data: "savedNPT" },
                { title: "Тип проблемы", data: "problemType" },
                { title: "Статус", data: "status" },
                { title: "Описание/Решение", data: "solution" },
                { title: "Отметка", data: "highLight", visible: false },
                {
                    title: "Обзор",
                    data: "id",
                    visible: false, // Скрываем столбец
                    render: function (data) {
                        return `<a href="/Incidents/View/${data}" target="_blank">Обзор</a>`;
                    }
                }
            ],
            pageLength: 25, // Количество записей на странице по умолчанию
            order: [[0, 'desc']], // Сортировка по первой колонке ("Дата и время") по убыванию
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
            },
            createdRow: function (row, data, dataIndex) {
                // Проверяем наличие символа "🚩" в ячейке "Отметка"
                if (data.highLight && data.highLight.includes('🚩')) {
                    // Красим строку в желтый цвет
                    $(row).css('background-color', '#FFFFCC');
                }
            }
        }).on('click', 'tr', function () {
            // Получаем данные строки
            const data = $('#journal_table').DataTable().row(this).data();

            // Открываем ссылку в новой вкладке, используя скрытые данные
            if (data && data.id) {
                window.open(`/Incidents/View/${data.id}`, '_blank');
            }
        });

        // Фильтрация по диапазону дат
        $.fn.dataTable.ext.search.push(function (settings, data, dataIndex) {
            var min = $('#min-date').val() ? new Date($('#min-date').val()) : null;
            var max = $('#max-date').val() ? new Date($('#max-date').val()) : null;
            var date = new Date(data[0]); // Предполагается, что дата находится в первой колонке

            if (
                (min === null && max === null) ||
                (min === null && date <= max) ||
                (min <= date && max === null) ||
                (min <= date && date <= max)
            ) {
                return true;
            }
            return false;
        });

        // Обработчики изменения диапазона дат
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

    } catch (error) {
        console.error('Ошибка при загрузке данных:', error);
    }
});
