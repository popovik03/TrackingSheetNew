am5.ready(function () {
    var root = am5.Root.new("bar_chart");

    root.setThemes([
        am5themes_Animated.new(root)
    ]);

    var chart = root.container.children.push(am5xy.XYChart.new(root, {
        panX: true,
        panY: true,
        wheelX: "pan",
        wheelY: "zoom",
        cursor: am5xy.XYCursor.new(root, {})
    }));

    var xAxis = chart.xAxes.push(am5xy.CategoryAxis.new(root, {
        categoryField: "category",
        renderer: am5xy.AxisRendererX.new(root, {})
    }));

    var yAxis = chart.yAxes.push(am5xy.ValueAxis.new(root, {
        renderer: am5xy.AxisRendererY.new(root, {})
    }));

    var series0 = chart.series.push(am5xy.ColumnSeries.new(root, {
        name: "Успешно закрыты",
        xAxis: xAxis,
        yAxis: yAxis,
        valueYField: "success",
        categoryXField: "category",
        clustered: false,
        tooltip: am5.Tooltip.new(root, {
            labelText: "Успешно закрыты: {valueY}"
        })
    }));

    series0.columns.template.setAll({
        width: am5.percent(40),
        tooltipY: 0,
        strokeOpacity: 0,
        cornerRadiusTL: 5,
        cornerRadiusTR: 5
    });

    var series1 = chart.series.push(am5xy.ColumnSeries.new(root, {
        name: "Не закрыты",
        xAxis: xAxis,
        yAxis: yAxis,
        valueYField: "failed",
        categoryXField: "category",
        clustered: false,
        tooltip: am5.Tooltip.new(root, {
            labelText: "Не закрыты: {valueY}"
        })
    }));

    series1.columns.template.setAll({
        width: am5.percent(50),
        tooltipY: 0,
        strokeOpacity: 0,
        cornerRadiusTL: 5,
        cornerRadiusTR: 5
    });

    var series2 = chart.series.push(am5xy.ColumnSeries.new(root, {
        name: "Количество обращений",
        xAxis: xAxis,
        yAxis: yAxis,
        valueYField: "count",
        categoryXField: "category",
        clustered: false,
        tooltip: am5.Tooltip.new(root, {
            labelText: "Количество обращений: {valueY}"
        })
    }));

    series2.columns.template.setAll({
        width: am5.percent(30),
        tooltipY: 0,
        strokeOpacity: 0,
        cornerRadiusTL: 5,
        cornerRadiusTR: 5
    });

    window.updateBarChart = function (data) {
        xAxis.data.setAll(data);
        series0.data.setAll(data);
        series1.data.setAll(data);
        series2.data.setAll(data);
    }
});
