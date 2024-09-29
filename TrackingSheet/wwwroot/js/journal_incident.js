// Определяем функцию parseLong
function parseLong(value) {
    var parsed = parseInt(value, 10);
    return isNaN(parsed) ? 0 : parsed;
}

document.addEventListener('DOMContentLoaded', function () {
    try {
        // Подключение сортировки дат через moment.js
        $.fn.dataTable.moment('DD/MM/YYYY HH:mm');

        // Массив для хранения изменённых записей
        let modifiedRecords = [];

        // Флаг режима редактирования
        let isEditMode = false;

        // Инициализация DataTable с параметром ajax
        var table = $('#journal_table').DataTable({
            ajax: {
                url: '/api/incidents/all', // URL для получения данных
                type: 'GET', // Метод запроса
                dataSrc: 'data' // Поле в JSON, содержащее массив данных
            },
            columns: [
                {
                    title: "Дата",
                    data: "date",
                    render: function (data, type, row) {
                        if (type === 'display' || type === 'filter') {
                            const date = new Date(data);
                            const formattedDateTime = `${String(date.getDate()).padStart(2, '0')}/${String(date.getMonth() + 1).padStart(2, '0')}/${date.getFullYear()} ${String(date.getHours()).padStart(2, '0')}:${String(date.getMinutes()).padStart(2, '0')}`;
                            return formattedDateTime;
                        }
                        return data;
                    }
                },
                {
                    title: "Смена",
                    data: "shift",
                    render: function (data) {
                        return `<p>${data}</p>`;
                    }
                },
                { title: "431", data: "reporter" },
                { title: "VSAT", data: "vsat" },
                { title: "Скважина", data: "well" },
                { title: "Рейс", data: "run" },
                { title: "Сохр. НПВ", data: "savedNPT" },
                { title: "Тип", data: "problemType" },
                {
                    title: "Статус",
                    data: "status",
                    render: function (data) {
                        return `<p>${data}</p>`;
                    }
                },
                { title: "Описание/Решение", data: "solution" },
                {
                    title: "", // highLight column
                    data: "highLight",
                    render: function (data) {
                        if (data && data.includes('🚩')) {
                            return '<img src="../img/fire.gif" alt="fire">';
                        }
                        return data;
                    }
                }
                // Удалили колонку "Обзор"
            ],
            pageLength: 25, // Количество записей на странице по умолчанию
            order: [[0, 'desc']], // Сортировка по первой колонке ("Дата и время") по убыванию
            language: {
                search: "Поиск:",
                lengthMenu: "Показать _MENU_ записей на странице.",
                zeroRecords: "Ничего не найдено.",
                info: "Показаны записи с _START_ по _END_ из _TOTAL_.",
                infoEmpty: "",
                infoFiltered: "(отфильтровано из _MAX_ записей)",
                paginate: {
                    first: "Первая",
                    previous: "Предыдущая",
                    next: "Следующая",
                    last: "Последняя"
                }
            },
            createdRow: function (row, data, dataIndex) {
                // Применяем стили в зависимости от смены
                if (data.shift && data.shift.includes('Day')) {
                    $(row).find('td').eq(1).find('p').eq(0).addClass('status day');
                } else {
                    $(row).find('td').eq(1).find('p').eq(0).addClass('status night');
                }

                // Применяем стили в зависимости от статуса
                if (data.status && data.status.includes('Success')) {
                    $(row).find('td').eq(8).find('p').eq(0).addClass('status success');
                } else if (data.status && data.status.includes('Process')) {
                    $(row).find('td').eq(8).find('p').eq(0).addClass('status workinprogress');
                } else if (data.status && data.status.includes('Fail')) {
                    $(row).find('td').eq(8).find('p').eq(0).addClass('status failed');
                }

                // Добавляем атрибут data-id для удобства
                $(row).attr('data-id', data.id);

                // Применяем градиент, если есть отметка "🚩"
                if (data.highLight && data.highLight.includes('🚩')) {
                    // Красим строку градиентом
                    $(row).css('background', 'linear-gradient(to right, #FFA500, #FFFFCC, #FFFFFF)');
                }


            }
        });

        // Кастомный фильтр для диапазона дат
        $.fn.dataTable.ext.search.push(
            function (settings, data, dataIndex) {
                var minDate = $('#min-date').val();
                var maxDate = $('#max-date').val();

                // Если даты не выбраны, показываем все записи
                if (!minDate && !maxDate) {
                    return true;
                }

                // Получаем дату из первого столбца (индекс 0)
                var dateStr = data[0]; // Предполагается, что дата в первом столбце
                var date = moment(dateStr, 'DD/MM/YYYY HH:mm');

                // Если дата некорректна, не отображаем запись
                if (!date.isValid()) {
                    return false;
                }

                // Проверяем минимальную дату
                if (minDate) {
                    var min = moment(minDate, 'YYYY-MM-DD');
                    if (date.isBefore(min)) {
                        return false;
                    }
                }

                // Проверяем максимальную дату
                if (maxDate) {
                    var max = moment(maxDate, 'YYYY-MM-DD').endOf('day');
                    if (date.isAfter(max)) {
                        return false;
                    }
                }

                // Если запись попадает в диапазон, отображаем её
                return true;
            }
        );

        // Обработчики событий для полей ввода дат
        $('#min-date, #max-date').on('change', function () {
            table.draw();
        });

        // Добавление полей ввода в заголовки
        $('#journal_table thead th').each(function (index) {
            // Убедитесь, что мы не добавляем поля ввода для последнего столбца, если он скрыт или отсутствует
            if (index < table.columns().count()) {
                var title = $(this).text();
                if (title !== "") { // Не добавляем для столбца "highLight" или других, если нужно
                    $(this).html('<div class="search-row">' + title + '<br><input type="text" placeholder="Найти..." /></div>');
                }
            }
        });

        // Применение поиска
        table.columns().every(function () {
            var that = this;

            $('input', this.header()).on('keyup change clear', function () {
                if (that.search() !== this.value) {
                    that.search(this.value).draw();
                }
            });

            $('input', this.header()).on('click', function (e) {
                e.stopPropagation();
            });
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

            var today = new Date();
            var dd = String(today.getDate()).padStart(2, '0');
            var mm = String(today.getMonth() + 1).padStart(2, '0'); // Январь — это 0!
            var yyyy = today.getFullYear();
            var formattedDate = dd + '-' + mm + '-' + yyyy;

            // Формируем имя файла с датой
            var fileName = 'TrackingSheet_' + formattedDate + '.xlsx';

            // Сохраняем файл
            saveAs(new Blob([s2ab(wbout)], { type: "application/octet-stream" }), fileName);
        });

        // Переключение в режим редактирования
        $('#toggle-edit').on('click', function () {
            // Изменяем текст кнопки
            $(this).hide();
            $('#save-edit').show();
            $('#cancel-edit').show();

            isEditMode = true;

            // Делаем ячейки редактируемыми
            $('#journal_table tbody tr').each(function () {
                $(this).find('td').each(function () {
                    var cell = table.cell(this);
                    var cellData = cell.data();

                    // Проверяем, чтобы ячейки не содержали ссылок или изображений
                    if (!$(this).find('a').length && !$(this).find('img').length) {
                        var colIdx = table.cell(this).index().column;
                        var columnName = table.column(colIdx).dataSrc();

                        // Определяем, какие колонки будут редактироваться как текст, какие как select
                        if (columnName === 'status') {
                            // Заменяем содержимое на select
                            $(this).html(`
                                <select class="status-select">
                                    <option value="Success">Success</option>
                                    <option value="Fail">Fail</option>
                                    <option value="Process">Process</option>
                                </select>
                            `);
                            // Устанавливаем текущее значение
                            $(this).find('select').val(cellData);
                        } else if (columnName === 'problemType') {
                            // Заменяем содержимое на select с вашими опциями
                            $(this).html(`
                                <select class="problemtype-select">
                                    <option value="Advantage">Advantage</option>
                                    <option value="ATK issue">ATK issue</option>
                                    <option value="APS">APS</option>
                                    <option value="BCPM II">BCPM II</option>
                                    <option value="Cadence">Cadence</option>
                                    <option value="Computer">Computer</option>
                                    <option value="Curve Failure">Curve Failure</option>
                                    <option value="Decoding">Decoding</option>
                                    <option value="Desync">Desync</option>
                                    <option value="Downlink">Downlink</option>
                                    <option value="LTK">LTK</option>
                                    <option value="M30">M30</option>
                                    <option value="Memfix">Memfix</option>
                                    <option value="Organization">Organisation</option>
                                    <option value="OTK">OTK</option>
                                    <option value="Pressure">Pressure</option>
                                    <option value="Procedures">Procedures</option>
                                    <option value="Programming | Tip">Programming | Tip</option>
                                    <option value="Pulser issue">Pulser issue</option>
                                    <option value="Service delivery">Service delivery</option>
                                    <option value="Surface issue">Surface issue</option>
                                    <option value="Survey issue">Survey issue</option>
                                    <option value="UsMPR">UsMPR</option>
                                    <option value="WellArchitect">WellArchitect</option>
                                    <option value="Win10">Win10</option>
                                    <option value="WITS">WITS</option>
                                    <option value="Other">Other</option>
                                </select>
                            `);
                            // Устанавливаем текущее значение
                            $(this).find('select').val(cellData);

                        } else if (columnName === 'shift') {
                            // Заменяем содержимое на select с вашими опциями
                            $(this).html(`
                                <select class="problemtype-select">
                                    <option value="Day">Day</option>
                                    <option value="Night">Night</option>
                                   
                                </select>
                            `);
                            // Устанавливаем текущее значение
                            $(this).find('select').val(cellData);
                        } else if (columnName === 'highLight') {
                            // Заменяем содержимое на select с вашими опциями
                            $(this).html(`
                                <select class="problemtype-select">
                                    <option value="🚩">🚩</option>
                                    <option value=""></option>
                                   
                                </select>
                            `);
                            // Устанавливаем текущее значение
                            $(this).find('select').val(cellData);
                        } else {
                            $(this).attr('contenteditable', 'true').addClass('editable');
                        }
                    }
                });
            });

            // Блокируем кликабельность строк
            $('#journal_table tbody').addClass('no-click');
        });

        // Отмена редактирования
        $('#cancel-edit').on('click', function () {
            // Скрываем кнопки "Сохранить" и "Отмена", показываем "Редактировать"
            $('#toggle-edit').show();
            $('#save-edit').hide();
            $('#cancel-edit').hide();

            isEditMode = false;

            // Перезагружаем таблицу с исходными данными
            table.ajax.reload(null, false); // Перезагружаем данные без сброса текущей страницы

            // Удаляем классы и атрибуты
            $('#journal_table tbody tr').each(function () {
                $(this).find('td').each(function () {
                    $(this).removeAttr('contenteditable').removeClass('editable');
                });
            });

            // Разрешаем кликабельность строк
            $('#journal_table tbody').removeClass('no-click');

            // Очищаем массив изменённых записей
            modifiedRecords = [];
        });

        // Обработчик изменения ячеек
        $('#journal_table tbody').on('input change', 'td[contenteditable="true"], select', function () {
            var $cellElement = $(this).closest('td');
            var row = $(this).closest('tr');
            var rowData = table.row(row).data();

            // Находим ячейку DataTable
            var cell = table.cell($cellElement);
            if (!cell || !cell.node()) {
                console.error('Не удалось найти ячейку DataTable для этого элемента:', this);
                return;
            }

            var cellIndex = cell.index();
            if (!cellIndex) {
                console.error('Не удалось получить индекс ячейки:', cell);
                return;
            }

            var colIdx = cellIndex.column;
            var columnName = table.column(colIdx).dataSrc();

            var newValue;

            if ($(this).is('select')) {
                newValue = $(this).val();
            } else {
                newValue = $(this).text().trim();
            }

            // Проверка и ограничение ввода для числовых полей
            if (['vsat', 'run'].includes(columnName)) {
                // Ограничиваем ввод только цифрами
                newValue = newValue.replace(/\D/g, '');
                $(this).text(newValue);
            }

            if (columnName === 'savedNPT') {
                // Разрешаем вводить числа с десятичной точкой
                newValue = newValue.replace(/[^0-9.]/g, '');
                // Убедимся, что только одна точка
                var dotCount = (newValue.match(/\./g) || []).length;
                if (dotCount > 1) {
                    var firstDotIndex = newValue.indexOf('.');
                    newValue = newValue.substring(0, firstDotIndex + 1) + newValue.substring(firstDotIndex + 1).replace(/\./g, '');
                }
                $(this).text(newValue);
            }

            // Проверяем, есть ли уже запись с этим ID в массиве
            var existingRecord = modifiedRecords.find(record => record.ID === rowData.ID);

            if (existingRecord) {
                // Обновляем соответствующее поле
                if (columnName === 'date') {
                    existingRecord[columnName] = new Date(newValue.split(' ')[0].split('/').reverse().join('-') + 'T' + newValue.split(' ')[1] + ':00Z').toISOString();
                } else if (['vsat', 'run', 'file'].includes(columnName)) {
                    existingRecord[columnName] = parseInt(newValue, 10) || 0;
                } else if (columnName === 'savedNPT') {
                    existingRecord[columnName] = parseFloat(newValue) || 0;
                } else {
                    existingRecord[columnName] = newValue;
                }
            } else {
                // Клонируем данные строки и добавляем в массив
                var updatedData = { ...rowData };
                if (columnName === 'date') {
                    updatedData[columnName] = new Date(newValue.split(' ')[0].split('/').reverse().join('-') + 'T' + newValue.split(' ')[1] + ':00Z').toISOString();
                } else if (['vsat', 'run', 'file'].includes(columnName)) {
                    updatedData[columnName] = parseInt(newValue, 10) || 0;
                } else if (columnName === 'savedNPT') {
                    updatedData[columnName] = parseFloat(newValue) || 0;
                } else {
                    updatedData[columnName] = newValue;
                }
                modifiedRecords.push(updatedData);
            }
        });

        // Обработчик клика по строке для открытия детального просмотра
        $('#journal_table tbody').on('click', 'tr', function () {
            // Проверяем, не находится ли таблица в режиме редактирования и не заблокированы ли строки
            if (isEditMode || $(this).hasClass('no-click')) {
                return; // Если в режиме редактирования, ничего не делаем
            }

            var rowData = table.row(this).data();
            if (rowData && rowData.id) {
                window.open(`/Incidents/View/${rowData.id}`, '_blank');
            }
        });

        // Сохранение изменений
        $('#save-edit').on('click', async function () {
            if (modifiedRecords.length === 0) {
                alert('Нет изменений для сохранения.');
                return;
            }

            console.log('Отправляемые изменённые записи:', modifiedRecords);

            // Показываем индикатор загрузки (можно реализовать более красиво)
            $('#save-edit').prop('disabled', true).text('Сохранение...');

            // Отправляем данные на сервер
            try {
                let response = await fetch('/Incidents/UpdateIncidents', {
                    method: 'POST',
                    headers: {
                        'Content-Type': 'application/json',
                        'RequestVerificationToken': $('input[name="__RequestVerificationToken"]').val()
                    },
                    body: JSON.stringify(modifiedRecords)
                });

                if (response.ok) {
                    let result = await response.json();
                    // Перезагружаем таблицу с обновленными данными
                    table.ajax.reload(null, false);

                    // Скрываем кнопки "Сохранить" и "Отмена", показываем "Редактировать"
                    $('#toggle-edit').show();
                    $('#save-edit').hide();
                    $('#cancel-edit').hide();

                    isEditMode = false;

                    // Удаляем классы и атрибуты
                    $('#journal_table tbody tr').each(function () {
                        $(this).find('td').each(function () {
                            $(this).removeAttr('contenteditable').removeClass('editable');
                        });
                    });

                    // Разрешаем кликабельность строк
                    $('#journal_table tbody').removeClass('no-click');

                    // Очищаем массив изменённых записей
                    modifiedRecords = [];

                    // Сброс состояния кнопки сохранения
                    $('#save-edit').prop('disabled', false).text('Сохранить');

                    alert(result.message || 'Изменения успешно сохранены.');
                } else {
                    let errorText = await response.text();
                    console.error('Ответ ошибки:', errorText);
                    let errorResult;
                    try {
                        errorResult = JSON.parse(errorText);
                    } catch (e) {
                        errorResult = { message: errorText };
                    }
                    alert(errorResult.message || 'Ошибка при сохранении данных.');

                    // Сброс состояния кнопки сохранения
                    $('#save-edit').prop('disabled', false).text('Сохранить');
                }
            } catch (error) {
                console.error('Ошибка при сохранении данных:', error);
                alert('Произошла ошибка при сохранении данных.');

                // Сброс состояния кнопки сохранения
                $('#save-edit').prop('disabled', false).text('Сохранить');
            }
        });

    } catch (error) {
        console.error('Ошибка при загрузке данных:', error);
    }
});
