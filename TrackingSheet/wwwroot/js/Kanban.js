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
    var subtaskItem = `
        <li class="list-group-item d-flex justify-content-between align-items-center">
            <div>
                <input type="checkbox" class="subtask-checkbox" ${isCompleted ? 'checked' : ''}>
                <input type="text" class="form-control subtask-description" value="${escapeHtml(description)}">
            </div>
            <button type="button" class="btn btn-danger btn-sm remove-subtask-button" data-subtask-id="${id}" data-row-version="${rowVersion}">Удалить</button>
            <input type="hidden" name="subtasksRowVersion" value="${rowVersion}">
        </li>
    `;
    $('#subtasksList').append(subtaskItem);
}




// Функция для добавления новой подзадачи 
function addSubtask() {
    var description = $('#newSubtaskDescription').val().trim();
    if (description === '') {
        alert('Подзадача не может быть пустой.');
        return;
    }

    // Новая подзадача: id = Guid.Empty, rowVersion = 'AAAAAAAAAAA='
    var subtaskId = '00000000-0000-0000-0000-000000000000';
    var rowVersion = 'AAAAAAAAAAA='; // Дефолтное значение для новых подзадач

    addSubtaskToList(subtaskId, description, false, rowVersion);

    // Очистка поля ввода
    $('#newSubtaskDescription').val('');
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
        // Получаем subtaskId из кнопки удаления
        var subtaskId = $(this).find('.remove-subtask-button').data('subtask-id') || '00000000-0000-0000-0000-000000000000';
        var description = $(this).find('input[type="text"]').val();
        var isCompleted = $(this).find('input[type="checkbox"]').is(':checked');
        var rowVersion = $(this).find('input[name="subtasksRowVersion"]').val() || 'AAAAAAAAAAA='; // Дефолтное значение

        subtasks.push({
            id: subtaskId, // camelCase
            subtaskDescription: description, // camelCase
            isCompleted: isCompleted, // camelCase
            rowVersion: rowVersion // camelCase
        });
    });
    console.log("SubtasksData:", subtasks); // Для отладки
    return subtasks;
}






$(document).ready(function () {
    console.log("JavaScript загружен и готов к работе");

    // Обработчик клика для кнопок редактирования задач
    $(document).ready(function () {
        // Обработчик клика по кнопке редактирования задачи
        $('.edit-task-button').on('click', function () {
            // Получаем JSON строку из атрибута data-task
            var taskDataJson = $(this).attr('data-task');
            console.log("Task Data JSON:", taskDataJson); // Для отладки

            if (!taskDataJson) {
                console.error('No task data found in data-task attribute.');
                return;
            }

            // Парсим JSON строку в объект
            var task = JSON.parse(taskDataJson);
            console.log("Parsed Task Data:", task); // Для отладки

            // Заполняем поля модального окна
            $('#editTaskId').val(task.id);
            $('#editTaskName').val(task.taskName);
            $('#editTaskDescription').val(task.taskDescription);
            $('#editTaskColor').val(task.taskColor);
            $('#editDueDate').val(task.dueDate);
            $('#editPriority').val(task.priority);
            $('#editTaskAuthor').val(task.taskAuthor);
            $('#editCreatedAt').val(task.createdAt);
            $('#editTaskRowVersion').val(task.rowVersion);
            $('#editTaskColumnId').val(task.columnId);

            // Заполняем подзадачи
            if (task.subtasks && task.subtasks.length > 0) {
                $('#subtasksList').empty(); // Очищаем список подзадач
                task.subtasks.forEach(function (subtask) {
                    addSubtaskToList(subtask.id, subtask.subtaskDescription, subtask.isCompleted, subtask.rowVersion);
                });
            } else {
                $('#subtasksList').empty();
            }

            // Заполняем комментарии
            if (task.comments && task.comments.length > 0) {
                $('#commentsList').empty(); // Очищаем список комментариев
                task.comments.forEach(function (comment) {
                    addCommentToList(comment.id, comment.commentAuthor, comment.commentText, comment.createdAt, comment.rowVersion);
                });
            } else {
                $('#commentsList').empty();
            }

            // Устанавливаем SubtasksJson перед отправкой формы
            $('#editSubtasksJson').val(JSON.stringify(getSubtasksData()));
        });
    });



    // Обработчик отправки формы редактирования задачи
    $('#editTaskForm').on('submit', function (event) {
        event.preventDefault(); // Останавливаем стандартную отправку формы

        var form = $(this);
        var formData = new FormData(this);

        // Добавляем подзадачи в виде JSON-строки
        formData.set('subtasksJson', JSON.stringify(getSubtasksData())); // camelCase

        // Проверка обязательных полей
        var taskName = $('#editTaskName').val().trim();
        var taskDescription = $('#editTaskDescription').val().trim();
        var columnId = $('#editTaskColumnId').val();

        if (!taskName || !taskDescription || !columnId) {
            alert('Пожалуйста, заполните все обязательные поля.');
            return;
        }

        // Отправляем запрос AJAX
        $.ajax({
            url: '/Kanban/EditTask',
            method: 'POST',
            data: formData,
            processData: false,
            contentType: false,
            success: function (response) {
                console.log("Task updated successfully:", response);

                if (response.updatedTask) {
                    console.log("Updated Task:", response.updatedTask);
                    updateTaskInView(response.updatedTask);

                    if (response.updatedTask.rowVersion) {
                        $('#editTaskRowVersion').val(response.updatedTask.rowVersion);
                        console.log("RowVersion updated to:", response.updatedTask.rowVersion);
                    } else {
                        console.error('RowVersion is missing in the updatedTask response.');
                    }

                    // Закрываем модальное окно
                    $('#editTaskModal').modal('hide');
                } else {
                    console.error('Updated task data is missing in the response.');
                }
            },
            error: function (xhr, status, error) {
                console.error('Error updating task:', xhr.responseText, error);

                if (xhr.status === 400) {
                    var response = JSON.parse(xhr.responseText);
                    var errorMessages = response.errors.map(e => `${e.field}: ${e.errorMessage}`).join('\n');
                    alert(`${response.message}\n${errorMessages}`);
                } else if (xhr.status === 409) {
                    alert(xhr.responseJSON.message);
                    // Предложите пользователю обновить страницу
                    location.reload();
                } else {
                    alert('Произошла ошибка при обновлении задачи. Пожалуйста, попробуйте снова.');
                }
            }
        });
    });

});


function updateTaskInView(task) {
    if (!task || !task.id) {
        console.error('Invalid task data:', task);
        return;
    }

    var taskElement = $('.kanban-task[data-id="' + task.id + '"]');

    if (taskElement.length) {
        // Обновление основных данных задачи
        taskElement.find('strong').text(task.taskName);
        taskElement.find('small').text(task.taskDescription);
        taskElement.css('background-color', task.taskColor);
        taskElement.attr('data-row-version', task.rowVersion);

        // Обновление подзадач (если отображаются на карточке задачи)
        var subtasksList = taskElement.find('.subtasks-list');
        if (subtasksList.length) {
            subtasksList.empty();
            task.subtasks.forEach(function (subtask) {
                var subtaskItem = `<li>${escapeHtml(subtask.subtaskDescription)} ${subtask.isCompleted ? '(Выполнено)' : ''}</li>`;
                subtasksList.append(subtaskItem);
            });
        }

        // Обновление других данных задачи при необходимости
    } else {
        console.error('Task element not found for task ID:', task.id);
    }
}



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
                taskId: taskId,
                __RequestVerificationToken: $('input[name="__RequestVerificationToken"]').val()
            },
            success: function () {
                // Удаляем задачу из DOM без перезагрузки страницы
                var taskElement = $('.kanban-task[data-id="' + taskId + '"]');
                if (taskElement.length) {
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
function addCommentToList(id, author, text, createdAt, rowVersion) {
    // Парсим дату
    var date = new Date(createdAt);
    // Добавляем смещение +5 часов
    date.setHours(date.getHours() + 5);
    // Форматируем дату
    var options = {
        year: 'numeric', month: '2-digit', day: '2-digit',
        hour: '2-digit', minute: '2-digit'
    };
    var formattedDate = date.toLocaleString('ru-RU', options);

    var commentItem = `
        <li class="list-group-item d-flex justify-content-between align-items-center">
            <div>
                <strong>${escapeHtml(author)}</strong> (${escapeHtml(formattedDate)}):
                <p>${escapeHtml(text)}</p>
            </div>
            <button type="button" class="btn btn-danger btn-sm remove-comment-button" data-comment-id="${id}" data-row-version="${rowVersion}">Удалить</button>
            <input type="hidden" name="commentsRowVersion" value="${rowVersion}">
        </li>
    `;
    $('#commentsList').append(commentItem);
}

//функция добавления нового комментария
function addComment() {
    var taskId = $('#editTaskId').val();
    var commentAuthor = reporter; // Используем переменную reporter
    var commentText = $('#newCommentText').val().trim();
    var rowVersion = $('#editTaskRowVersion').val();

    // Проверка наличия reporter
    if (!commentAuthor) {
        commentAuthor = prompt('Пожалуйста, введите ваше имя для добавления комментария:');
        if (!commentAuthor) {
            alert('Имя автора комментария обязательно.');
            return;
        }
        // Сохраняем введённое имя в переменную reporter
        reporter = commentAuthor;
    }

    if (commentText === '') {
        alert('Комментарий не может быть пустым.');
        return;
    }

    $.ajax({
        url: '/Kanban/AddComment',
        method: 'POST',
        contentType: 'application/json',
        data: JSON.stringify({
            TaskId: taskId,
            CommentAuthor: commentAuthor,
            CommentText: commentText,
            RowVersion: rowVersion
        }),
        success: function (response) {
            console.log(response); // Для отладки

            if (response.comment) { // Используем 'comment' с маленькой буквы
                var comment = response.comment;

                // Добавляем комментарий в список
                addCommentToList(comment.id, comment.commentAuthor, comment.commentText, comment.createdAt, comment.rowVersion); // Используем camelCase

                // Обновляем RowVersion задачи
                $('#editTaskRowVersion').val(response.rowVersion); // Используем camelCase

                // Очищаем поле ввода
                $('#newCommentText').val('');
            } else {
                console.error('Comment data is missing in the response.');
            }
        },
        error: function (xhr) {
            // Обработка ошибок
            alert('Произошла ошибка при добавлении комментария.');
        }
    });
}



$(document).on('click', '.remove-comment-button', function () {
    var commentId = $(this).data('comment-id');
    var rowVersion = $(this).data('row-version');
    var taskId = $('#editTaskId').val();

    if (confirm('Вы уверены, что хотите удалить этот комментарий?')) {
        $.ajax({
            url: '/Kanban/DeleteComment',
            method: 'POST',
            contentType: 'application/json',
            data: JSON.stringify({
                CommentId: commentId,
                TaskId: taskId,
                RowVersion: rowVersion
            }),
            success: function (response) {
                console.log("Comment deleted successfully:", response);
                if (response.success) {
                    // Удаляем комментарий из списка
                    $(`button[data-comment-id="${commentId}"]`).closest('li').remove();
                    // Обновляем RowVersion задачи
                    $('#editTaskRowVersion').val(response.rowVersion);
                } else {
                    alert(response.message || 'Не удалось удалить комментарий.');
                }
            },
            error: function (xhr) {
                console.error('Error deleting comment:', xhr.responseText);
                alert('Произошла ошибка при удалении комментария.');
            }
        });
    }
});


// Функция для удаления комментария
function deleteComment(commentId, commentRowVersion) {
    if (confirm("Вы действительно хотите удалить этот комментарий?")) {
        $.ajax({
            url: '/Kanban/DeleteComment',
            method: 'POST',
            contentType: 'application/json',
            data: JSON.stringify({
                CommentId: commentId,
                RowVersion: commentRowVersion
            }),
            success: function (response) {
                console.log("DeleteComment Response:", response); // Для отладки

                if (response.message) {
                    alert(response.message);

                    // Удаляем комментарий из списка
                    $(`li[data-id='${commentId}']`).remove();

                    // Обновляем RowVersion задачи, если он был обновлён
                    if (response.RowVersion) {
                        $('#editTaskRowVersion').val(response.RowVersion);
                    }
                } else {
                    console.error('Unexpected response format.');
                }
            },
            error: function (xhr) {
                if (xhr.status === 409) {
                    alert(xhr.responseJSON.message);
                    location.reload();
                } else if (xhr.status === 400) {
                    var errors = xhr.responseJSON.errors || [];
                    var message = xhr.responseJSON.message || 'Ошибки при удалении комментария.';
                    if (errors.length > 0) {
                        alert(message + '\n' + errors.join('\n'));
                    } else {
                        alert(message);
                    }
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
    return text ? text.replace(/[&<>"']/g, function (m) { return map[m]; }) : '';
}