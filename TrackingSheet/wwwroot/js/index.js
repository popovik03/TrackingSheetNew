document.addEventListener('DOMContentLoaded', () => {
    const menuItems = document.querySelectorAll('#menuList li');
    const mainContainer = document.querySelector('main');
    const menuList = document.getElementById('menuList');

    // Очищаем активное состояние всех элементов меню при загрузке страницы
    menuItems.forEach(item => item.classList.remove('active'));

    // Получаем текущую страницу из window.currentPage
    const currentPage = window.currentPage;

    const contentMap = {
        main: 'main.html',
        journal: 'journal_incidents.html',
        new_incident: 'new_incident.html',
        vsat_bha: 'vsat_bha.html',
        statistics: 'statistics.html',
        kanban: 'kanban.html',
        about: 'about.html'
    };

    menuList.addEventListener('click', async (e) => {
        const target = e.target.closest('li');
        if (target) {
            const selectedId = target.id;
            const url = contentMap[selectedId];

            if (url) {
                try {
                    mainContainer.innerHTML = ''; // Очистка существующего контента

                    const response = await fetch(url);
                    if (!response.ok) throw new Error(`HTTP error! status: ${response.status}`);
                    const data = await response.text();

                    // Временный контейнер для загруженного контента
                    const tempContainer = document.createElement('div');
                    tempContainer.innerHTML = data;

                    // Перемещение контента в основной контейнер
                    mainContainer.appendChild(tempContainer);

                    // Обработка скриптов
                    const scripts = mainContainer.querySelectorAll('script');
                    await loadAndExecuteScripts(scripts);

                    // Устанавливаем активное меню после полной загрузки контента
                    window.currentPage = selectedId;
                    setTimeout(() => {
                        menuItems.forEach(item => item.classList.remove('active'));
                        target.classList.add('active');
                    }, 0); // Задержка в 0 мс помогает убедиться, что это произойдет после основной задачи

                } catch (error) {
                    console.error('Error loading content:', error);
                }
            }
        }
    });

    // Подсвечиваем активную страницу при загрузке
    if (currentPage) {
        setTimeout(() => {
            menuItems.forEach(item => {
                if (item.id === currentPage) {
                    item.classList.add('active');
                } else {
                    item.classList.remove('active');
                }
            });
        }, 0); // Задержка в 0 мс помогает убедиться, что это произойдет после основной задачи
    }

    async function loadAndExecuteScripts(scripts) {
        for (let script of scripts) {
            if (script.src) {
                try {
                    await loadScript(script.src);
                } catch (err) {
                    console.error('Error loading script:', err);
                }
            } else {
                try {
                    eval(script.innerText);
                } catch (err) {
                    console.error('Error executing script:', err);
                }
            }
        }
    }

    function loadScript(src) {
        return new Promise((resolve, reject) => {
            const script = document.createElement('script');
            script.src = src;
            script.onload = resolve;
            script.onerror = reject;
            document.head.appendChild(script);
        });
    }

    // Scroll behavior for sidebar
    let lastScrollTop = 0;
    const sidebar = document.querySelector('aside');
    const hideOffset = 300;
    const showOffset = 50;

    window.addEventListener('scroll', () => {
        const scrollTop = window.pageYOffset || document.documentElement.scrollTop;

        if (scrollTop > lastScrollTop && scrollTop > hideOffset) {
            sidebar.classList.add('hidden');
            mainContainer.classList.add('collapsed');
        } else if (scrollTop < lastScrollTop - showOffset) {
            sidebar.classList.remove('hidden');
            mainContainer.classList.remove('collapsed');
        }

        lastScrollTop = scrollTop;
    });

    // Custom select wrapper handling
    document.addEventListener('click', (e) => {
        const selectWrapper = e.target.closest('.select-wrapper');
        if (selectWrapper) {
            e.stopPropagation();
            selectWrapper.classList.toggle('open');
        } else {
            document.querySelectorAll('.select-wrapper.open').forEach(wrapper => wrapper.classList.remove('open'));
        }
    });
});
