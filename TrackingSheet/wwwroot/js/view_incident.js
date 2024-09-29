// Глобальная переменная для модального окна
var modal;

// Функция для подтверждения и удаления инцидента
function confirmDelete(deleteUrl) {
    if (confirm('Вы уверены, что хотите удалить этот инцидент?')) {
        window.location.href = deleteUrl;
    }
}

// Функция для удаления файла
function deleteFile(fileName) {
    if (confirm('Вы уверены, что хотите удалить этот файл?')) {
        fetch(`/Incidents/DeleteFile?fileName=${encodeURIComponent(fileName)}&incidentId=${window.appSettings.incidentId}`, {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json'
            }
        })
            .then(response => response.json())
            .then(data => {
                if (data.success) {
                    alert('Файл успешно удален.');
                    location.reload(); // Полная перезагрузка страницы после удаления файла
                } else {
                    alert('Ошибка при удалении файла: ' + (data.message || 'Неизвестная ошибка.'));
                }
            })
            .catch(error => {
                console.error('Ошибка:', error);
                alert('Ошибка при удалении файла.');
            });
    }
}

// Функция для обработки выбора файла
function handleFileSelect(event) {
    console.log('handleFileSelect called');
    console.log('Selected file:', event.target.files[0]);

    // Используем setTimeout для отсрочки нажатия кнопки "Сохранить"
    setTimeout(function () {
        var submitButton = document.querySelector('button[type="submit"]');
        if (submitButton) {
            console.log('Submitting form...');
            submitButton.click();
        } else {
            console.error('Submit button not found');
        }
    }, 100); // Задержка в 100 мс для уверенности, что файл выбран
}

// Функция для открытия модального окна и инициализации формы
function openModal() {
    if (modal) {
        modal.style.display = "block";

        // Инициализация формы внутри модального окна
        initializeUpdateForm();
    } else {
        console.error('Modal is not defined.');
    }
}

// Функция для закрытия модального окна
function closeModal() {
    if (modal) {
        modal.style.display = "none";
    } else {
        console.error('Modal is not defined.');
    }
}

// Функция для инициализации формы обновления
function initializeUpdateForm() {
    var updateForm = document.getElementById("incidentUpdateForm");
    if (!updateForm) {
        console.error('Форма с ID "incidentUpdateForm" не найдена.');
        return;
    }

    var updateDate = document.getElementById("updateDate");
    var updateReporter = document.getElementById("updateReporter");
    var updateRun = document.getElementById("updateRun");
    var updateSolution = document.getElementById("updateSolution");

    // Устанавливаем текущую дату и время с учетом смещения
    var dateWithOffset = getDateWithTimezoneOffset(5);
    updateDate.value = dateWithOffset;

    // Устанавливаем значение updateReporter из appSettings или оставляем текущее значение
    updateReporter.value = window.appSettings.reporter || updateReporter.value;

    // Устанавливаем значение updateRun из appSettings или оставляем текущее значение
    updateRun.value = window.appSettings.run || updateRun.value;

    // Очищаем поле updateSolution
    updateSolution.value = "";

    // Обработчик отправки формы
    updateForm.onsubmit = function (event) {
        event.preventDefault();

        var formData = {
            IncidentID: window.appSettings.incidentId,
            Date: updateDate.value,
            UpdateReporter: updateReporter.value,
            Run: updateRun.value,
            UpdateSolution: updateSolution.value
        };
        console.log('Отправляемые данные:', formData);

        fetch(window.appSettings.addUpdateUrl, {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json',
                'RequestVerificationToken': getAntiForgeryToken()
            },
            body: JSON.stringify(formData)
        })
            .then(response => {
                if (!response.ok) {
                    // Если ответ не OK, выбрасываем ошибку
                    return response.text().then(text => { throw new Error(text) });
                }
                return response.json();
            })
            .then(data => {
                if (data.success) {
                    alert('Инцидент успешно обновлен.');
                    closeModal();
                    location.reload();
                } else {
                    alert('Ошибка при обновлении инцидента: ' + data.message);
                }
            })
            .catch(error => {
                console.error('Ошибка:', error);
                alert('Произошла ошибка при обновлении инцидента.');
            });
    };

    // Обработчик для закрытия модального окна
    var cancelButton = document.getElementById('cancelUpdateButton');
    if (cancelButton) {
        cancelButton.onclick = function () {
            closeModal();
        };
    } else {
        console.error('Cancel button with ID "cancelUpdateButton" not found.');
    }
}

// Функция для получения CSRF-токена
function getAntiForgeryToken() {
    var tokenElement = document.querySelector('input[name="__RequestVerificationToken"]');
    return tokenElement ? tokenElement.value : '';
}

// Функция для получения даты с учётом смещения часового пояса
function getDateWithTimezoneOffset(offset) {
    var currentDate = new Date();
    var utcTime = currentDate.getTime() + (currentDate.getTimezoneOffset() * 60000);
    var targetTime = utcTime + (offset * 3600000);
    var targetDate = new Date(targetTime);

    var year = targetDate.getFullYear();
    var month = ('0' + (targetDate.getMonth() + 1)).slice(-2);
    var day = ('0' + targetDate.getDate()).slice(-2);
    var hours = ('0' + targetDate.getHours()).slice(-2);
    var minutes = ('0' + targetDate.getMinutes()).slice(-2);

    return `${year}-${month}-${day}T${hours}:${minutes}`;
}

// Обработчик событий после загрузки DOM
document.addEventListener('DOMContentLoaded', function () {
    // Инициализируем глобальную переменную modal
    modal = document.getElementById("incidentUpdateModal");
    var btn = document.getElementById("openModalButton");
    var span = document.getElementsByClassName("close-modal")[0];

    if (!modal) {
        console.error('Modal element with ID "incidentUpdateModal" not found.');
        return;
    }

    if (!btn) {
        console.error('Button with ID "openModalButton" not found.');
        return;
    }

    if (!span) {
        console.error('Element with class "close-modal" not found.');
    }

    // Обработчик открытия модального окна
    btn.onclick = function (event) {
        event.preventDefault();
        openModal();
    };

    // Обработчик закрытия модального окна по клику на крестик
    if (span) {
        span.onclick = function () {
            closeModal();
        };
    }

    // Обработчик закрытия модального окна при клике вне его области
    window.onclick = function (event) {
        if (event.target == modal) {
            closeModal();
        }
    }
});
