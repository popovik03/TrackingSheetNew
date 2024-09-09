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

// Функция для редактирования названия колонки
function editColumnName(columnId) {
    var columnElement = document.querySelector('[data-id="' + columnId + '"] .column-header h4');
    var currentName = columnElement.textContent;
    var newName = prompt("Введите новое название колонки:", currentName);

    if (newName && newName.trim() !== "") {
        $.ajax({
            url: '/Kanban/RenameColumn',
            method: 'POST',
            data: {
                columnId: columnId,
                newName: newName
            },
            success: function () {
                columnElement.textContent = newName;
                alert('Название колонки обновлено успешно');
            },
            error: function () {
                alert('Ошибка при обновлении названия колонки');
            }
        });
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
