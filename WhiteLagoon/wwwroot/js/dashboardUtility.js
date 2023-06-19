

// Function to get the colors array from the string
function getChartColorsArray(chartId) {
    if (document.getElementById(chartId) !== null) {
        var colors = document.getElementById(chartId).getAttribute("data-colors");
        if (colors) {
            colors = JSON.parse(colors);
            return colors.map(function (value) {
                var newValue = value.replace(" ", "");
                if (newValue.indexOf(",") === -1) {
                    var color = getComputedStyle(document.documentElement).getPropertyValue(newValue);
                    if (color) return color;
                    else return newValue;;
                }
                //else {
                //    var val = value.split(',');
                //    if (val.length == 2) {
                //        var rgbaColor = getComputedStyle(document.documentElement).getPropertyValue(val[0]);
                //        rgbaColor = "rgba(" + rgbaColor + "," + val[1] + ")";
                //        return rgbaColor;
                //    } else {
                //        return newValue;
                //    }
                //}
            });
        }
    }
}


// Function to load the RadialBar chart
function loadRadialBarChart(id, data) {
    var chartColors = getChartColorsArray(id);
    var options = {
        fill: {
            colors: chartColors
        },
        chart: {
            type: 'radialBar',
            width: 90,
            height: 90,
            sparkline: {
                enabled: true
            },
            offsetY: -10,
        },
        series: data.series,
        plotOptions: {
            radialBar: {
                dataLabels: {
                    value: {
                        offsetY: -10,
                        color: chartColors[0],
                    }
                }
            }
        },
        labels: [""]
    };

    var chart = new ApexCharts(document.querySelector("#" + id), options);
    chart.render();
}
