
// Переключение радиокнопок

document.addEventListener('DOMContentLoaded', (event) => {
    const labels = document.querySelectorAll('.toggle-switch label');

    labels.forEach(label => {
        label.addEventListener('click', () => {
            const radioButton = document.getElementById(label.htmlFor);
            if (radioButton) {
                radioButton.checked = true;
            }
        });
    });


});

// Добавляем год в строку с годом 

var yearInput = document.getElementById('year');
var currentYear = new Date().getFullYear();
yearInput.value = currentYear;