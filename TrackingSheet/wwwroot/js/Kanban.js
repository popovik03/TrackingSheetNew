document.addEventListener('DOMContentLoaded', function () {
    console.log('Initializing SortableJS');

    // Инициализация SortableJS для всех контейнеров задач внутри колонок
    document.querySelectorAll('.kanban-column').forEach(function (column) {
        var tasksContainer = column.querySelector('.kanban-tasks');
        if (tasksContainer) {
            new Sortable(tasksContainer, {
                group: 'kanban', // Позволяет перемещать задачи между колонками с той же группой
                draggable: '.kanban-task', // Определяет, какие элементы можно перетаскивать
                animation: 150, // Плавная анимация при перетаскивании
                ghostClass: 'sortable-ghost', // Класс для стилизации "призрачного" элемента при перетаскивании
                onEnd: function (evt) {
                    // Получаем идентификаторы задачи и колонок
                    var taskId = evt.item.getAttribute('data-id');
                    var newColumnId = evt.to.closest('.kanban-column').getAttribute('data-id');
                    var oldColumnId = evt.from.closest('.kanban-column').getAttribute('data-id');
                    var newIndex = evt.newIndex;

                    console.log(`Task ID: ${taskId}`);
                    console.log(`Old Column ID: ${oldColumnId}`);
                    console.log(`New Column ID: ${newColumnId}`);
                    console.log(`New Index: ${newIndex}`);

                    // Проверяем, изменилась ли колонка или позиция задачи
                    if (newColumnId !== oldColumnId || evt.oldIndex !== newIndex) {
                        // Отправляем AJAX-запрос для обновления позиции задачи
                        $.ajax({
                            url: '/Kanban/MoveTask',
                            method: 'POST',
                            contentType: 'application/json',
                            data: JSON.stringify({
                                taskId: taskId,
                                newColumnId: newColumnId,
                                oldColumnId: oldColumnId,
                                newIndex: newIndex
                            }),
                            success: function () {
                                console.log('Task moved successfully');
                            },
                            error: function (xhr, status, error) {
                                console.error('Error moving task:', error);
                                alert('Ошибка при перемещении задачи. Пожалуйста, попробуйте снова.');

                                // Возвращаем задачу на исходное место
                                location.reload();
                            }
                        });
                    }
                }
            });
        } else {
            console.warn(`.kanban-tasks контейнер не найден в колонке с ID: ${column.getAttribute('data-id')}`);
        }
    });

    // Валидация форм добавления задач
    var addTaskForms = document.querySelectorAll('form[id^="addTaskForm-"]');

    addTaskForms.forEach(function (form) {
        form.addEventListener('submit', function (event) {
            // Извлекаем columnId из ID формы
            var columnId = form.id.replace('addTaskForm-', '');

            // Получаем элементы полей ввода
            var taskNameInput = document.getElementById('taskName-' + columnId);
            var taskDescriptionInput = document.getElementById('taskDescription-' + columnId);
            var validationMessage = document.getElementById('validationMessage-' + columnId);

            // Получаем значения полей
            var taskName = taskNameInput.value.trim();
            var taskDescription = taskDescriptionInput.value.trim();

            var isValid = true;
            var errorMessages = [];

            // Проверка названия задачи
            if (!taskName) {
                isValid = false;
                errorMessages.push('Пожалуйста, заполните название задачи.');
                taskNameInput.classList.add('is-invalid');
            } else {
                taskNameInput.classList.remove('is-invalid');
            }

            // Проверка описания задачи
            if (!taskDescription) {
                isValid = false;
                errorMessages.push('Пожалуйста, заполните описание задачи.');
                taskDescriptionInput.classList.add('is-invalid');
            } else {
                taskDescriptionInput.classList.remove('is-invalid');
            }

            if (!isValid) {
                event.preventDefault(); // Отменяем отправку формы
                event.stopPropagation();

                // Объединяем сообщения об ошибках
                validationMessage.innerHTML = errorMessages.join('<br>');
                validationMessage.style.display = 'block';
            } else {
                // Скрываем сообщение об ошибке, если форма валидна
                validationMessage.style.display = 'none';
                validationMessage.innerHTML = '';
            }
        });
    });
});

// Функция для отображения выбранной доски
function showBoard(boardId) {
    // Скрываем все доски
    document.querySelectorAll('.board-content').forEach(function (element) {
        element.classList.remove('show', 'active');
    });

    // Отображаем выбранную доску
    var selectedBoard = document.getElementById('board-content-' + boardId);
    if (selectedBoard) {
        selectedBoard.classList.add('show', 'active');
    }

    // Обновляем активную вкладку
    document.querySelectorAll('.nav-link').forEach(function (element) {
        element.classList.remove('active');
    });
    var activeNavLink = document.querySelector('.nav-link[onclick="showBoard(\'' + boardId + '\')"]');
    if (activeNavLink) {
        activeNavLink.classList.add('active');
    }
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
