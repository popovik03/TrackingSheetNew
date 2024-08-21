am5.ready(function () {
    var root = am5.Root.new("pie_chart");

    root.setThemes([
        am5themes_Animated.new(root)
    ]);

    var chart = root.container.children.push(am5percent.PieChart.new(root, {
        layout: root.verticalLayout
    }));

    var series = chart.series.push(am5percent.PieSeries.new(root, {
        name: "Series",
        valueField: "value",
        categoryField: "category"
    }));

    series.labels.template.setAll({
        text: "{category}: {value.percent}"
    });

    series.slices.template.setAll({
        tooltipText: "{category}: {value.percent}"
    });

    window.updatePieChart = function (data) {
        series.data.setAll(data);
    }
});
