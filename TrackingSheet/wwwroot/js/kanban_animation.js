 // Функция для переключения активности кнопок
 function toggleButton(button) {
    // Переключаем класс 'active' на самой кнопке
    button.classList.toggle('active');
    
    const taskId = button.getAttribute('data-task-id');

    // Проверяем по классу кнопки, какое действие нужно выполнить
    if (button.classList.contains('toggle-attachments-button')) {
        toggleAttachments(taskId);
    } else if (button.classList.contains('toggle-comments-button')) {
        toggleComments(taskId);
    } else if (button.classList.contains('toggle-description-button')) {
        toggleDescription(taskId);
    } else if (button.classList.contains('toggle-subtasks-button')) {
        toggleSubtasks(taskId);
    }
}

// Назначаем событие клика на все кнопки с классом 'btn'
document.addEventListener('DOMContentLoaded', function () {
    // Находим все кнопки внутри div с классом 'note-task-icons'
    const buttons = document.querySelectorAll('.note-task-icons .btn');
    
    // Назначаем обработчик клика для каждой кнопки
    buttons.forEach(button => {
        button.addEventListener('click', function () {
            toggleButton(this); // При клике передаем саму кнопку в функцию
        });
    });
});




