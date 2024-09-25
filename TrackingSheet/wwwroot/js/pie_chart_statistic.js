am5.ready(function() {

    if (typeof chartData === 'undefined' || chartData.length === 0) {
        console.warn('Нет данных для отображения графиков.');
        return;
    }

    console.log(chartData); // Для отладки

    const pieChartElement = document.getElementById('pie_chart');

    // Извлекаем данные из chartData
    const problemType = chartData.map(stat => stat.problemType.trim());
    const count = chartData.map(stat => stat.count);

    // Получаем данные из C#
    var root = am5.Root.new("pie_chart");

    root.setThemes([
        am5themes_Animated.new(root),
        am5themes_Frozen.new(root)
    ]);

    var chart = root.container.children.push(am5percent.PieChart.new(root, {
        innerRadius: 100,
        layout: root.verticalLayout
    }));

    var series = chart.series.push(am5percent.PieSeries.new(root, {
        valueField: "count",  // Поле с количеством из модели
        categoryField: "problemType"  // Поле с типом проблемы из модели
    }));

    // Передаем данные в диаграмму
    series.data.setAll(chartData);  // chartData передается из C#

    series.appear(1000, 100);

    var label = root.tooltipContainer.children.push(am5.Label.new(root, {
        x: am5.p50,
        y: am5.p50,
        centerX: am5.p50,
        centerY: am5.p50,
        fill: am5.color(0x000000),
        fontSize: 50
    }));
});
