document.addEventListener('DOMContentLoaded', function () {
    // Обработчик для переключения кнопок
    document.querySelectorAll('.note-task-icons').forEach(function (toggleButtonsContainer) {
        toggleButtonsContainer.addEventListener('click', function (event) {
            const button = event.target.closest('.btn');
            if (button && toggleButtonsContainer.contains(button)) {
                toggleButton(button);
            }
        });
    });

    // Обработчик для добавления подзадачи
    document.addEventListener('click', function (event) {
        if (event.target && event.target.classList.contains('add-subtask-button')) {
            const button = event.target;
            const taskId = button.getAttribute('data-task-id');
            const input = button.closest('.input-group').querySelector('.add-subtask-input');
            const subtaskDescription = input.value.trim();

            if (subtaskDescription === '') {
                console.log('Описание подзадачи не может быть пустым.');
                return;
            }

            // Получаем антифродовый токен
            const antiForgeryToken = document.querySelector('input[name="__RequestVerificationToken"]').value;

            // AJAX-запрос для добавления подзадачи
            $.ajax({
                url: '/Kanban/AddSubtask',
                method: 'POST',
                headers: {
                    'RequestVerificationToken': antiForgeryToken
                },
                contentType: 'application/json',
                data: JSON.stringify({
                    TaskId: taskId,
                    SubtaskDescription: subtaskDescription
                }),
                success: function (response) {
                    if (response.success && response.subtask) {
                        const subtask = response.subtask;
                        const subtasksList = button.closest('.subtasks-section').querySelector('.list-group');
                        const newSubtaskItem = `
                            <li class="list-group-item d-flex justify-content-between align-items-center">
                                <div>
                                    <input type="checkbox" class="subtask-checkbox" data-subtask-id="${subtask.id}" data-row-version="${subtask.rowVersion}" onchange="toggleSubtaskCompletion('${subtask.id}')" ${subtask.isCompleted ? 'checked' : ''}>
                                    <span>${escapeHtml(subtask.subtaskDescription)}</span>
                                </div>
                                <button type="button" class="btn btn-danger btn-sm remove-subtask-button" data-subtask-id="${subtask.id}" data-row-version="${subtask.rowVersion}">Удалить</button>
                            </li>
                        `;
                        subtasksList.insertAdjacentHTML('beforeend', newSubtaskItem);
                        input.value = ''; // Очистить поле ввода
                    } else {
                        console.log('Не удалось добавить подзадачу.');
                    }
                },
                error: function (xhr) {
                    console.error('Ошибка при добавлении подзадачи:', xhr.responseJSON);
                    console.log('Произошла ошибка при добавлении подзадачи: ' + (xhr.responseJSON?.message || ''));
                }
            });
        }
    });

    // Обработчик для удаления подзадачи (делегирование)
    document.addEventListener('click', function (event) {
        if (event.target && event.target.classList.contains('remove-subtask-button')) {
            const button = event.target;
            const subtaskId = button.getAttribute('data-subtask-id');
            const rowVersion = button.getAttribute('data-row-version');
            const subtaskItem = button.closest('li');

            if (confirm('Вы уверены, что хотите удалить эту подзадачу?')) {
                // Получаем антифродовый токен
                const antiForgeryToken = document.querySelector('input[name="__RequestVerificationToken"]').value;

                // AJAX-запрос для удаления подзадачи
                $.ajax({
                    url: '/Kanban/DeleteSubtask',
                    method: 'POST',
                    headers: {
                        'RequestVerificationToken': antiForgeryToken
                    },
                    contentType: 'application/json',
                    data: JSON.stringify({
                        SubtaskId: subtaskId,
                        RowVersion: rowVersion
                    }),
                    success: function (response) {
                        if (response.success) {
                            subtaskItem.remove();
                            console.log('Подзадача успешно удалена.');
                        } else {
                            console.log('Не удалось удалить подзадачу.');
                        }
                    },
                    error: function (xhr) {
                        if (xhr.status === 409) { // Conflict
                            console.log('Подзадача была изменена другим процессом. Пожалуйста, обновите страницу и попробуйте снова.');
                        } else if (xhr.status === 404) { // Not Found
                            console.log('Подзадача не найдена.');
                        } else {
                            console.log('Произошла ошибка при удалении подзадачи: ' + (xhr.responseJSON?.message || ''));
                        }
                    }
                });
            }
        }
    });

    // Обработчик для отметки подзадачи как выполненной (делегирование)
    document.addEventListener('change', function (event) {
        if (event.target.classList.contains('subtask-checkbox')) {
            const subtaskId = event.target.getAttribute('data-subtask-id');
            toggleSubtaskCompletion(subtaskId);
        }
    });
});

function toggleSubtaskCompletion(subtaskId) {
    const checkbox = document.querySelector(`.subtask-checkbox[data-subtask-id="${subtaskId}"]`);
    const isCompleted = checkbox.checked;
    const rowVersion = checkbox.getAttribute('data-row-version');

    // Получаем антифродовый токен
    const antiForgeryToken = document.querySelector('input[name="__RequestVerificationToken"]').value;

    // AJAX-запрос для обновления статуса подзадачи
    $.ajax({
        url: '/Kanban/UpdateSubtaskStatus',
        method: 'POST',
        headers: {
            'RequestVerificationToken': antiForgeryToken
        },
        contentType: 'application/json',
        data: JSON.stringify({
            SubtaskId: subtaskId,
            IsCompleted: isCompleted,
            RowVersion: rowVersion
        }),
        success: function (response) {
            if (response.success) {
                // Обновляем RowVersion в чекбоксе
                checkbox.setAttribute('data-row-version', response.subtask.rowVersion);
                console.log('Статус подзадачи обновлён.');
            } else {
                console.log('Не удалось обновить статус подзадачи.');
                checkbox.checked = !isCompleted; // Откат состояния
            }
        },
        error: function (xhr) {
            if (xhr.status === 409) { // Conflict
                console.log('Подзадача была изменена другим процессом. Пожалуйста, обновите страницу и попробуйте снова.');
            } else if (xhr.status === 404) { // Not Found
                console.log('Подзадача не найдена.');
            } else {
                console.log('Произошла ошибка при обновлении статуса подзадачи.');
            }
            checkbox.checked = !isCompleted; // Откат состояния
        },
        complete: function () {
            checkbox.disabled = false;
        }
    });
}

// Функция для переключения активности кнопок
function toggleButton(button) {
    button.classList.toggle('active');

    const taskId = button.getAttribute('data-task-id');
    if (!taskId) {
        console.warn('Task ID не найден для кнопки:', button);
        return;
    }

    switch (true) {
        case button.classList.contains('toggle-attachments-button'):
            toggleAttachments(taskId);
            break;
        case button.classList.contains('toggle-comments-button'):
            toggleComments(taskId);
            break;
        case button.classList.contains('toggle-description-button'):
            toggleDescription(taskId);
            break;
        case button.classList.contains('toggle-subtasks-button'):
            toggleSubtasks(taskId);
            break;
        default:
            console.warn('Неизвестный тип кнопки:', button);
    }
}

function escapeHtml(text) {
    const map = {
        '&': '&amp;',
        '<': '&lt;',
        '>': '&gt;',
        '"': '&quot;',
        "'": '&#039;'
    };
    return text.replace(/[&<>"']/g, function (m) { return map[m]; });
}
