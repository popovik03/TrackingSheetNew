// Функция для отображения выбранной доски
function showBoard(boardId) {
    // Скрываем все доски
    document.querySelectorAll('.board-content').forEach(function (element) {
        element.classList.remove('show', 'active');
    });

    // Отображаем выбранную доску
    document.getElementById('board-content-' + boardId).classList.add('show', 'active');

    // Обновляем активную вкладку
    document.querySelectorAll('.nav-link').forEach(function (element) {
        element.classList.remove('active');
    });
    document.querySelector('.nav-link[onclick="showBoard(\'' + boardId + '\')"]').classList.add('active');
}

// Функция для редактирования названия доски
function editBoardName(boardId) {
    var boardNameElement = document.getElementById('board-name-' + boardId);
    var currentName = boardNameElement.textContent;
    var newName = prompt("Введите новое название доски:", currentName);

    if (newName && newName.trim() !== "") {
        $.ajax({
            url: '/Kanban/RenameBoard',
            method: 'POST',
            data: {
                id: boardId,
                newName: newName
            },
            success: function () {
                boardNameElement.textContent = newName;
                alert('Название доски обновлено успешно');
            },
            error: function () {
                alert('Ошибка при обновлении названия доски');
            }
        });
    }
}

// Функция для удаления доски
function deleteBoard(boardId) {
    if (confirm("Вы действительно хотите удалить эту доску?")) {
        $.ajax({
            url: '/Kanban/DeleteBoard',
            method: 'POST',
            data: {
                id: boardId
            },
            success: function () {
                location.reload();
            },
            error: function () {
                alert('Ошибка при удалении доски');
            }
        });
    }
}

// Открыть модальное окно для редактирования колонки
function editColumnName(columnId) {
    var columnElement = document.querySelector('[data-id="' + columnId + '"]');
    var currentName = columnElement.querySelector('.column-header h4').textContent;
    var currentOrder = parseInt(columnElement.getAttribute('data-order')) + 1; // Начинаем с 1
    var currentColor = columnElement.style.backgroundColor;

    // Преобразуем цвет из RGB в HEX
    var rgb = currentColor.match(/\d+/g);
    var hexColor = rgb && rgb.length === 3 ?
        '#' + ((1 << 24) + (parseInt(rgb[0]) << 16) + (parseInt(rgb[1]) << 8) + parseInt(rgb[2])).toString(16).slice(1) : "#ffffff";

    document.getElementById('editColumnId').value = columnId;
    document.getElementById('editColumnName').value = currentName;
    document.getElementById('editColumnOrder').value = currentOrder;
    document.getElementById('editColumnColor').value = hexColor;

    $('#editColumnModal').modal('show');
}

// Сохранить изменения колонки
function saveColumnChanges() {
    var columnId = document.getElementById('editColumnId').value;
    var newName = document.getElementById('editColumnName').value;
    var newOrder = parseInt(document.getElementById('editColumnOrder').value) - 1; // Преобразуем обратно к 0-индексации
    var newColor = document.getElementById('editColumnColor').value;

    if (newName && newOrder >= 0) {
        $.ajax({
            url: '/Kanban/RenameReorderAndRecolorColumn',
            method: 'POST',
            data: {
                columnId: columnId,
                newName: newName,
                newOrder: newOrder,
                newColor: newColor
            },
            success: function () {
                $('#editColumnModal').modal('hide'); // Закрываем модальное окно
                location.reload(); // Обновляем страницу после успешного обновления
            },
            error: function () {
                alert('Ошибка при обновлении колонки');
            }
        });
    } else {
        alert('Пожалуйста, введите корректные данные.');
    }
}

// Функция для удаления колонки
function deleteColumn(columnId) {
    if (confirm("Вы действительно хотите удалить эту колонку?")) {
        $.ajax({
            url: '/Kanban/DeleteColumn',
            method: 'POST',
            data: {
                columnId: columnId
            },
            success: function () {
                location.reload();
            },
            error: function () {
                alert('Ошибка при удалении колонки');
            }
        });
    }
}

// Инициализация сортировки для задач в колонках с использованием SortableJS
document.querySelectorAll('.kanban-columns').forEach(function (columnsContainer) {
    new Sortable(columnsContainer, {
        group: 'kanban',
        draggable: '.kanban-task',
        animation: 150,
        onEnd: function (evt) {
            // Логика для обработки изменений порядка задач (например, сохранение нового порядка на сервере)
            var columnId = evt.to.closest('.kanban-column').getAttribute('data-id');
            var taskId = evt.item.getAttribute('data-id');
            var newIndex = evt.newIndex;

            $.ajax({
                url: '/Kanban/UpdateColumnOrder',
                method: 'POST',
                contentType: 'application/json',
                data: JSON.stringify({
                    columnId: columnId,
                    taskId: taskId,
                    newIndex: newIndex
                }),
                success: function () {
                    alert('Порядок задач обновлен успешно');
                },
                error: function () {
                    alert('Ошибка при обновлении порядка задач');
                }
            });
        }
    });
});
