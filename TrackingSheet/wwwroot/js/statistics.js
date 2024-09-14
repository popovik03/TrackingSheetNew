document.addEventListener("DOMContentLoaded", function () {
    // Проверяем, что данные для графиков доступны
    if (typeof chartData === 'undefined' || chartData.length === 0) {
        console.warn('Нет данных для отображения графиков.');
        return;
    }

    console.log(chartData); // Для отладки

    const barChartElement = document.getElementById('barChart');
    const pieChartElement = document.getElementById('pieChart');

    if (!barChartElement || !pieChartElement) {
        console.error('Canvas элементы с id "barChart" или "pieChart" не найдены.');
        return;
    }

    // Извлекаем данные из chartData
    const labels = chartData.map(stat => stat.problemType.trim());
    const countData = chartData.map(stat => stat.count);
    const successFailData = chartData.map(stat => stat.successCount + stat.failCount);
    const savedNPTData = chartData.map(stat => stat.savedNPTCount || 0); // Исправлено имя свойства
    console.log('savedNPTData:', savedNPTData); // Добавьте эту строку для отладки

    // Создание barChart
    const barCtx = barChartElement.getContext('2d');
    const barChart = new Chart(barCtx, {
        type: 'bar',
        data: {
            labels: labels,
            datasets: [
                {
                    label: 'Количество обращений, шт',
                    data: countData,
                    backgroundColor: 'rgba(153, 102, 255, 0.6)',
                    borderColor: 'rgba(0, 0, 0, 1)',
                    borderWidth: 1
                },
                {
                    label: 'Количество закрытых обращений, шт',
                    data: successFailData,
                    backgroundColor: 'rgba(0, 0, 128, 0.6)',
                    borderColor: 'rgba(0, 0, 0, 1)',
                    borderWidth: 1
                },
                {
                    label: 'Сохраненное НПВ, ч',
                    data: savedNPTData,
                    backgroundColor: 'rgba(0, 255, 0, 0.6)',
                    borderColor: 'rgba(0, 0, 0, 1)',
                    borderWidth: 1
                }
            ]
        },
        options: {
            scales: {
                y: {
                    beginAtZero: true
                }
            }
        }
    });

    // Создание pieChart
    const pieCtx = pieChartElement.getContext('2d');
    const pieChart = new Chart(pieCtx, {
        type: 'pie',
        data: {
            labels: labels,
            datasets: [
                {
                    data: countData,
                    backgroundColor: [
                        'rgba(255, 99, 132, 0.6)',   // Красный
                        'rgba(54, 162, 235, 0.6)',   // Синий
                        'rgba(255, 206, 86, 0.6)',   // Желтый
                        'rgba(75, 192, 192, 0.6)',   // Бирюзовый
                        'rgba(153, 102, 255, 0.6)',  // Фиолетовый
                        'rgba(255, 159, 64, 0.6)',   // Оранжевый
                        'rgba(255, 0, 0, 0.6)',      // Ярко-красный
                        'rgba(0, 255, 0, 0.6)',      // Ярко-зеленый
                        'rgba(0, 0, 255, 0.6)',      // Ярко-синий
                        'rgba(128, 0, 128, 0.6)',    // Пурпурный
                        'rgba(255, 165, 0, 0.6)',    // Оранжевый
                        'rgba(0, 255, 255, 0.6)',    // Голубой
                        'rgba(255, 20, 147, 0.6)',   // Глубокий розовый
                        'rgba(124, 252, 0, 0.6)',    // Ярко-зеленый желтоватый
                        'rgba(70, 130, 180, 0.6)',   // Стальной синий
                        'rgba(210, 105, 30, 0.6)',   // Шоколадный
                        'rgba(218, 112, 214, 0.6)',  // Орхидея
                        'rgba(50, 205, 50, 0.6)',    // Лайм
                        'rgba(255, 105, 180, 0.6)',  // Розовый
                        'rgba(189, 183, 107, 0.6)',  // Темный хаки
                        'rgba(220, 20, 60, 0.6)',    // Малиновый
                        'rgba(255, 255, 0, 0.6)',  //  желтый
                        'rgba(64, 224, 208, 0.6)',   // Бирюзовый
                        'rgba(199, 21, 133, 0.6)',   // Средний пурпурный
                        'rgba(128, 128, 128, 0.6)',  // Серый
                        'rgba(0, 51, 0, 0.6)'       // Темно-зеленый
                    ],
                    borderColor: 'rgba(0, 0, 0, 1)',
                    borderWidth: 1
                }
            ]
        },
        options: {
            responsive: true,
            plugins: {
                legend: {
                    position: 'top',
                },
                title: {
                    display: true,
                    text: 'Количество обращений по типам проблем',
                    font: {
                        family: 'Gilroy-Bold',
                        size: 16,
                        weight: 'normal'
                    }
                }
            }
        }
    });
});
