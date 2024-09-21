function setAddColumnModalData(boardId) {
    $('#addColumnBoardId').val(boardId);
}

function setEditColumnModalData(columnId, columnName, order, columnColor) {
    $('#editColumnId').val(columnId);
    $('#editColumnName').val(columnName);
    $('#editColumnOrder').val(order);
    $('#editColumnColor').val(columnColor);
}

function setAddTaskModalData(columnId) {
    $('#addTaskColumnId').val(columnId);
}

function setEditTaskModalData(task) {
    console.log("Editing task:", task);

    // Установка основных полей
    $('#editTaskId').val(task.Id);
    $('#editTaskName').val(task.TaskName);
    $('#editTaskDescription').val(task.TaskDescription);
    $('#editTaskColor').val(task.TaskColor);
    $('#editDueDate').val(task.DueDate || '');
    $('#editPriority').val(task.Priority);
    $('#editTaskAuthor').val(task.TaskAuthor);
    $('#editCreatedAt').val(task.CreatedAt);

    // Установка RowVersion
    $('#editTaskRowVersion').val(task.RowVersion);

    // Очистка списка подзадач
    $('#subtasksList').empty();

    // Добавление подзадач в список
    if (task.Subtasks && task.Subtasks.length > 0) {
        task.Subtasks.forEach(function (subtask) {
            addSubtaskToList(subtask.Id, subtask.SubtaskDescription, subtask.IsCompleted, subtask.RowVersion);
        });
    }

    // Получаем columnId из DOM
    var taskElement = $('.kanban-task[data-id="' + task.Id + '"]');
    if (taskElement.length) {
        var columnId = taskElement.closest('.kanban-column').data('id');
        console.log("Setting columnId:", columnId); // Логирование для проверки
        $('#editTaskColumnId').val(columnId);
    } else {
        console.error("Task element not found for taskId:", task.Id);
    }

    // Показываем модальное окно
    $('#editTaskModal').modal('show');
}



// Обновленная функция для добавления подзадачи в список с RowVersion
function addSubtaskToList(id, description, isCompleted, rowVersion) {
    var subtaskId = id || generateUUID();
    var checkedAttribute = isCompleted ? 'checked' : '';
    var rowVersionValue = rowVersion || '';
    var listItem = `
        <li class="list-group-item">
            <input type="checkbox" class="mr-2" data-subtask-id="${subtaskId}" ${checkedAttribute}>
            <input type="hidden" name="SubtasksRowVersion" value="${rowVersionValue}" />
            <input type="text" class="form-control d-inline-block" value="${description}" data-subtask-id="${subtaskId}" style="width: 80%;">
            <button type="button" class="btn btn-sm btn-danger float-right" onclick="removeSubtask('${subtaskId}')">&times;</button>
        </li>
    `;
    $('#subtasksList').append(listItem);
    console.log("Добавлена подзадача:", { Id: subtaskId, SubtaskDescription: description, IsCompleted: isCompleted, RowVersion: rowVersionValue });
}

// Функция для добавления новой подзадачи 
function addSubtask() {
    var description = $('#newSubtaskDescription').val().trim();
    if (description) {
        addSubtaskToList(null, description, false);
        $('#newSubtaskDescription').val('');
    }
}

// Функция для удаления подзадачи из списка
function removeSubtask(subtaskId) {
    $(`li input[data-subtask-id='${subtaskId}']`).closest('li').remove();
    console.log("Удалена подзадача с ID:", subtaskId);
}

// Генератор UUID для новых подзадач
function generateUUID() {
    return 'xxxxxxxx-xxxx-4xxx-yxxx-xxxxxxxxxxxx'.replace(/[xy]/g, function (c) {
        var r = Math.random() * 16 | 0,
            v = c === 'x' ? r : (r & 0x3 | 0x8);
        return v.toString(16);
    });
}

function getSubtasksData() {
    var subtasks = [];
    $('#subtasksList li').each(function () {
        var subtaskId = $(this).find('input[data-subtask-id]').data('subtask-id') || '';
        var description = $(this).find('input[type="text"]').val();
        var isCompleted = $(this).find('input[type="checkbox"]').is(':checked');
        var rowVersion = $(this).find('input[name="SubtasksRowVersion"]').val() || '';
        subtasks.push({
            Id: subtaskId,
            SubtaskDescription: description,
            IsCompleted: isCompleted,
            RowVersion: rowVersion
        });
    });
    return subtasks;
}

$(document).ready(function () {
    console.log("JavaScript загружен и готов к работе");

    // Обработчик клика для кнопок редактирования задач
    $(document).on('click', '.edit-task-button', function () {
        var taskData = $(this).data('task');
        setEditTaskModalData(taskData);
    });

    // Отменяем все предыдущие обработчики и привязываем новый
    $('#editTaskForm').on('submit', function (event) {
        event.preventDefault();

        // Собираем данные подзадач
        var subtasks = getSubtasksData();
        $('#editSubtasksJson').val(JSON.stringify(subtasks));

        var form = $(this);
        var formData = {
            taskId: $('#editTaskId').val(),
            taskName: $('#editTaskName').val().trim(),
            taskDescription: $('#editTaskDescription').val().trim(),
            taskColor: $('#editTaskColor').val(),
            dueDate: $('#editDueDate').val(),
            priority: $('#editPriority').val(),
            // Другие поля...
            SubtasksJson: $('#editSubtasksJson').val(),
            RowVersion: $('#editTaskRowVersion').val()
        };

        $.ajax({
            url: form.attr('action'),
            method: 'POST',
            contentType: 'application/json',
            data: JSON.stringify(formData),
            success: function (response) {
                alert(response.message);
                // Обновить RowVersion в скрытом поле
                $('#editTaskRowVersion').val(response.RowVersion);
                // Обновить отображение задачи на странице, если необходимо
                location.reload(); // Или обновить только необходимую часть
            },
            error: function (xhr) {
                if (xhr.status === 409) {
                    alert(xhr.responseJSON.message);
                } else {
                    alert('Произошла ошибка при обновлении задачи.');
                }
            }
        });
    });

});


document.addEventListener('DOMContentLoaded', function () {
    document.querySelector('main').classList.add('show');
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

//функция добавления комментариев в список 
function addCommentToList(commentId, author, text, createdAt, rowVersion) {
    var listItem = `
        <li class="list-group-item d-flex justify-content-between align-items-center" data-comment-id="${commentId}" data-row-version="${rowVersion}">
            <div>
                <strong>${author}</strong> <small class="text-muted">${createdAt}</small>
                <p>${text}</p>
            </div>
            <button type="button" class="btn btn-sm btn-danger" onclick="deleteComment('${commentId}')">Удалить</button>
        </li>
    `;
    $('#commentsList').append(listItem);
}

//функция добавления нового комментария
function addComment() {
    var taskId = $('#editTaskId').val();
    var commentAuthor = $('#editTaskAuthor').val(); // Предполагается, что автор задачи является автором комментария
    var commentText = $('#newCommentText').val().trim();
    var rowVersion = $('#editTaskRowVersion').val();

    if (!commentText) {
        alert('Пожалуйста, введите текст комментария.');
        return;
    }

    var commentData = {
        TaskId: taskId,
        CommentAuthor: commentAuthor,
        CommentText: commentText,
        RowVersion: rowVersion
    };

    $.ajax({
        url: '/Kanban/AddComment',
        method: 'POST',
        contentType: 'application/json',
        data: JSON.stringify(commentData),
        success: function (response) {
            alert(response.message);
            // Обновить RowVersion задачи
            $('#editTaskRowVersion').val(response.RowVersion);
            // Обновить RowVersion в атрибуте задачи на странице
            var taskElement = $('.kanban-task[data-id="' + taskId + '"]');
            taskElement.attr('data-row-version', response.RowVersion);
            // Добавить новый комментарий в список
            var newComment = response.Comment;
            if (newComment) {
                var commentItem = `
                    <li class="list-group-item" data-id="${newComment.Id}" data-row-version="${newComment.RowVersion}">
                        <strong>${escapeHtml(newComment.CommentAuthor)}</strong>: ${escapeHtml(newComment.CommentText)}
                        <button type="button" class="btn btn-sm btn-danger float-right" onclick="deleteComment('${newComment.Id}', '${newComment.RowVersion}')">&times;</button>
                    </li>
                `;
                $('#commentsList').append(commentItem);
                // Очистить поле ввода
                $('#newCommentText').val('');
            } else {
                console.error('Comment data is missing in the response.');
            }
        },
        error: function (xhr) {
            if (xhr.status === 409) {
                alert(xhr.responseJSON.message);
                location.reload(); // Перезагрузка для получения актуальных данных
            } else if (xhr.status === 400) {
                var errors = xhr.responseJSON;
                var errorMessages = [];
                if (errors.errors) {
                    for (var key in errors.errors) {
                        if (errors.errors.hasOwnProperty(key)) {
                            errorMessages.push(errors.errors[key].join('<br>'));
                        }
                    }
                } else if (errors.message) {
                    errorMessages.push(errors.message);
                }
                alert('Ошибки при добавлении комментария:\n' + errorMessages.join('\n'));
            } else {
                alert('Произошла ошибка при добавлении комментария.');
            }
        }
    });
}

// Функция для удаления комментария
function deleteComment(commentId, rowVersion) {
    if (confirm("Вы действительно хотите удалить этот комментарий?")) {
        var taskId = $('#editTaskId').val();

        var deleteData = {
            CommentId: commentId,
            RowVersion: rowVersion
        };

        $.ajax({
            url: '/Kanban/DeleteComment',
            method: 'POST',
            contentType: 'application/json',
            data: JSON.stringify(deleteData),
            success: function (response) {
                alert(response.message);
                // Обновить RowVersion задачи
                $('#editTaskRowVersion').val(response.RowVersion);
                // Обновить RowVersion в атрибуте задачи на странице
                var taskElement = $('.kanban-task[data-id="' + taskId + '"]');
                taskElement.attr('data-row-version', response.RowVersion);
                // Удалить комментарий из списка
                $(`li[data-id="${commentId}"]`).remove();
            },
            error: function (xhr) {
                if (xhr.status === 409) {
                    alert(xhr.responseJSON.message);
                    location.reload(); // Перезагрузка для получения актуальных данных
                } else if (xhr.status === 400) {
                    var errors = xhr.responseJSON;
                    var errorMessages = [];
                    if (errors.errors) {
                        for (var key in errors.errors) {
                            if (errors.errors.hasOwnProperty(key)) {
                                errorMessages.push(errors.errors[key].join('<br>'));
                            }
                        }
                    } else if (errors.message) {
                        errorMessages.push(errors.message);
                    }
                    alert('Ошибки при удалении комментария:\n' + errorMessages.join('\n'));
                } else {
                    alert('Произошла ошибка при удалении комментария.');
                }
            }
        });
    }
}

// Функция для экранирования HTML (предотвращение XSS)
function escapeHtml(text) {
    var map = {
        '&': '&amp;',
        '<': '&lt;',
        '>': '&gt;',
        '"': '&quot;',
        "'": '&#039;'
    };
    return text.replace(/[&<>"']/g, function (m) { return map[m]; });
}
