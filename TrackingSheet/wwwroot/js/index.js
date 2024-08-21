document.addEventListener('DOMContentLoaded', () => {
    const menuItems = document.querySelectorAll('#menuList li');
    const mainContainer = document.querySelector('main');
    const menuList = document.getElementById('menuList');

    // Получаем текущую страницу из window.currentPage
    const currentPage = window.currentPage;

    const contentMap = {
        main: 'main.html',
        journal: 'journal_incidents.html',
        new_incident: 'new_incident.html',
        vsat_bha: 'vsat_bha.html',
        statistics: 'statistics.html',
        ro_planer: 'ro_planer.html',
        about: 'about.html'
    };

    menuList.addEventListener('click', async (e) => {
        const target = e.target.closest('li');
        if (target) {
            const selectedId = target.id;
            const url = contentMap[selectedId];

            if (url) {
                try {
                    mainContainer.innerHTML = ''; // Clear existing content

                    const response = await fetch(url);
                    if (!response.ok) throw new Error(`HTTP error! status: ${response.status}`);
                    const data = await response.text();

                    // Temporary container for the fetched content
                    const tempContainer = document.createElement('div');
                    tempContainer.innerHTML = data;

                    // Move content to the main container
                    mainContainer.appendChild(tempContainer);

                    // Update active state of menu items
                    menuItems.forEach(item => item.classList.remove('active'));
                    target.classList.add('active');

                    // Process scripts
                    const scripts = mainContainer.querySelectorAll('script');
                    await loadAndExecuteScripts(scripts);

                } catch (error) {
                    console.error('Error loading content:', error);
                }
            }
        }
    });

    // Подсвечиваем активную страницу при загрузке
    menuItems.forEach(item => {
        if (item.id === currentPage) {
            item.classList.add('active');
        }
    });

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
