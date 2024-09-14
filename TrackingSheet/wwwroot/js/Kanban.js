document.addEventListener('DOMContentLoaded', function () {
    console.log('Initializing SortableJS');

    // Инициализация сортировки для всех колонок
    document.querySelectorAll('.kanban-column').forEach(function (column) {
        new Sortable(column.querySelector('.kanban-tasks'), {
            group: 'kanban', // Разрешает перемещение между колонками с той же группой
            draggable: '.kanban-task', // Определяет, какие элементы можно перетаскивать
            animation: 150, // Анимация при перетаскивании
            ghostClass: 'sortable-ghost', // Класс для стиля "призрака" при перетаскивании
            onEnd: function (evt) {
                // Получаем идентификаторы задачи и колонок
                var taskId = evt.item.getAttribute('data-id');
                var newColumnId = evt.to.closest('.kanban-column').getAttribute('data-id');
                var oldColumnId = evt.from.closest('.kanban-column').getAttribute('data-id');
                var newIndex = evt.newIndex;

                // Проверяем, изменилась ли колонка или индекс
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

    // Валидация формы редактирования задачи
    var editTaskForm = document.getElementById('editTaskForm');
    if (editTaskForm) {
        editTaskForm.addEventListener('submit', function (event) {
            // Получаем значения полей
            var taskNameInput = document.getElementById('editTaskName');
            var taskDescriptionInput = document.getElementById('editTaskDescription');
            var validationMessage = document.getElementById('editValidationMessage');

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
    }
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
    var hexColor = rgb && rgb.length === 3 ? '#' + ((1 << 24) + (parseInt(rgb[0]) << 16) + (parseInt(rgb[1]) << 8) + parseInt(rgb[2])).toString(16).slice(1) : "#ffffff";

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

// Функция для редактирования задачи
function editTask(taskId) {
    var taskElement = document.querySelector('.kanban-task[data-id="' + taskId + '"]');
    var taskName = taskElement.querySelector('strong').textContent;
    var taskDescription = taskElement.querySelector('small').textContent;
    var taskColor = taskElement.style.backgroundColor;
    var dueDateElement = taskElement.querySelector('div:nth-of-type(1)');
    var priorityElement = taskElement.querySelector('div:nth-of-type(2)');

    var dueDate = dueDateElement ? dueDateElement.textContent.replace('Дедлайн: ', '') : '';
    var priority = priorityElement ? priorityElement.textContent.replace('Приоритет: ', '') : '';

    var rgb = taskColor.match(/\d+/g);
    var hexColor = rgb && rgb.length === 3 ? '#' + ((1 << 24) + (parseInt(rgb[0]) << 16) + (parseInt(rgb[1]) << 8) + parseInt(rgb[2])).toString(16).slice(1) : "#ffffff";

    var columnId = taskElement.closest('.kanban-column').getAttribute('data-id');

    // Проверка наличия элементов перед установкой значений
    if (document.getElementById('editTaskId')) {
        document.getElementById('editTaskId').value = taskId;
    }

    if (document.getElementById('editTaskColumnId')) {
        document.getElementById('editTaskColumnId').value = columnId;
    }

    if (document.getElementById('editTaskName')) {
        document.getElementById('editTaskName').value = taskName;
    }

    if (document.getElementById('editTaskDescription')) {
        document.getElementById('editTaskDescription').value = taskDescription;
    }

    if (document.getElementById('editTaskColor')) {
        document.getElementById('editTaskColor').value = hexColor;
    }

    if (document.getElementById('editDueDate')) {
        document.getElementById('editDueDate').value = dueDate;
    }

    if (document.getElementById('editPriority')) {
        document.getElementById('editPriority').value = priority;
    }

    // Показываем модальное окно только если все элементы существуют
    if ($('#editTaskModal').length) {
        $('#editTaskModal').modal('show');
    } else {
        console.error("Modal not found.");
    }
}

// Функция для удаления задачи
function deleteTask(taskId) {
    if (confirm("Вы действительно хотите удалить эту задачу?")) {
        $.ajax({
            url: '/Kanban/DeleteTask',
            method: 'POST',
            data: {
                taskId: taskId
            },
            success: function () {
                // Удаляем задачу из DOM без перезагрузки страницы
                var taskElement = document.querySelector('.kanban-task[data-id="' + taskId + '"]');
                if (taskElement) {
                    taskElement.remove();
                }
            },
            error: function () {
                alert('Ошибка при удалении задачи');
            }
        });
    }
}

// Привязка события к кнопке "Отмена" для редактирования колонки
document.addEventListener('DOMContentLoaded', function () {
    document.querySelector('#editColumnModal .btn-secondary').addEventListener('click', function () {
        $('#editColumnModal').modal('hide');
    });

    document.querySelector('#editTaskModal .btn-secondary').addEventListener('click', function () {
        $('#editTaskModal').modal('hide');
    });

    document.querySelectorAll('.close[data-dismiss="modal"]').forEach(function (btn) {
        btn.addEventListener('click', function () {
            $(this).closest('.modal').modal('hide');
        });
    });
});
