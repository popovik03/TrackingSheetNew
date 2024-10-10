document.addEventListener("DOMContentLoaded", function () {
    // Загрузка скрипта виджета погоды, если он ещё не загружен
    if (!document.getElementById('weatherwidget-io-js')) {
        var js = document.createElement('script');
        js.id = 'weatherwidget-io-js';
        js.src = 'https://weatherwidget.io/js/widget.min.js';
        document.body.appendChild(js);
    }

    // Функция для обновления времени
    function updateClocks() {
        // Массив городов с их временными зонами (UTC смещение)
        const cities = [
            { id: 'moscow-clock', name: 'Москва', utcOffset: 3 },
            { id: 'tyumen-clock', name: 'Тюмень', utcOffset: 5 },
            { id: 'leninsk-clock', name: 'Ленск', utcOffset: 7 },
            { id: 'yuzhno-sakhalinsk-clock', name: 'Южно-Сахалинск', utcOffset: 11 }
        ];

        const now = new Date();

        cities.forEach(city => {
            // Вычисляем время для каждого города
            const utc = now.getTime() + (now.getTimezoneOffset() * 60000);
            const cityTime = new Date(utc + (3600000 * city.utcOffset));

            // Форматируем время
            const hours = String(cityTime.getHours()).padStart(2, '0');
            const minutes = String(cityTime.getMinutes()).padStart(2, '0');

            const formattedTime = `${hours}:${minutes}`;

            // Обновляем содержимое элемента
            const clockElement = document.getElementById(city.id);
            if (clockElement) {
                clockElement.querySelector('.time').textContent = formattedTime;
            }
        });
    }

    // Обновляем время сразу при загрузке
    updateClocks();

    // Обновляем время каждую минуту, так как секунды не нужны
    setInterval(updateClocks, 60000);

    // Инициализация Flatpickr с опцией inline
    flatpickr("#flatpickr-calendar", {
        locale: "ru", // Локализация на русский язык
        dateFormat: "Y-m-d", // Формат даты
        defaultDate: "today", // Установка текущей даты по умолчанию
        showMonths: 1, // Отображение одного месяца
        enableTime: false, // Если не требуется выбирать время
        inline: true, // Закрепленный календарь
    });

});
