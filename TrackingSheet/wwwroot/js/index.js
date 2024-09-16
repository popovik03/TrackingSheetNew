document.addEventListener('DOMContentLoaded', () => {
    const menuItems = document.querySelectorAll('#menuList li');
    const menuList = document.getElementById('menuList');
    const mainContainer = document.querySelector('main');

    // Функция для добавления анимации
    function fadeInMain() {
        // Добавляем класс с небольшой задержкой, чтобы анимация началась плавно
        setTimeout(() => {
            mainContainer.classList.add('show');
        }, 400); // Задержка для запуска анимации
    }

    // Запуск анимации для main при загрузке страницы
    fadeInMain();

    // Получаем текущую страницу из window.currentPage
    const currentPage = window.currentPage;

    const contentMap = {
        main: 'main.html',
        journal: 'journal_incidents.html',
        new_incident: 'new_incident.html',
        vsat_bha: 'vsat_bha.html',
        statistics: 'statistics.html',
        kanban: 'KanbanView.cshtml',
        about: 'about.html'
    };

    menuList.addEventListener('click', async (e) => {
        const target = e.target.closest('li');
        if (target) {
            const selectedId = target.id;
            const url = contentMap[selectedId];

            if (url) {
                try {
                    mainContainer.innerHTML = ''; // Очистить существующий контент

                    const response = await fetch(url);
                    if (!response.ok) throw new Error(`HTTP error! status: ${response.status}`);
                    const data = await response.text();

                    // Временный контейнер для загруженного контента
                    const tempContainer = document.createElement('div');
                    tempContainer.innerHTML = data;

                    // Переместить контент в основной контейнер
                    mainContainer.appendChild(tempContainer);

                    // Анимация появления
                    animateFadeInUp(mainContainer);

                    // Обновить активное состояние элементов меню
                    menuItems.forEach(item => item.classList.remove('active'));
                    target.classList.add('active');

                    // Обработка скриптов
                    const scripts = mainContainer.querySelectorAll('script');
                    await loadAndExecuteScripts(scripts);

                } catch (error) {
                    console.error('Error loading content:', error);
                }
            }
        }
    });
});


document.addEventListener('DOMContentLoaded', function () {
    // Делегируем событие click на document для элементов с классом select-wrapper
    document.addEventListener('click', function (e) {
        const selectWrapper = e.target.closest('.select-wrapper');

        // Если клик был внутри select-wrapper
        if (selectWrapper) {
            e.stopPropagation();
            selectWrapper.classList.toggle('open');
        } else {
            // Если клик был вне всех select-wrapper, закрыть все выпадающие списки
            document.querySelectorAll('.select-wrapper.open').forEach(function (wrapper) {
                wrapper.classList.remove('open');
            });
        }
    });

    // Закрываем все открытые селекторы при клике вне их
    document.addEventListener('click', function (e) {
        document.querySelectorAll('.select-wrapper.open').forEach(function (selectWrapper) {
            if (!selectWrapper.contains(e.target)) {
                selectWrapper.classList.remove('open');
            }
        });
    });
});
