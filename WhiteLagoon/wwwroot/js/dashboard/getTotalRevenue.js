
$(document).ready(function () {
    loadTotalRevenueChart();
});

function loadTotalRevenueChart() {
    $(".chart-spinner").show();

    $.ajax({
        url: '/Dashboard/GetTotalRevenueChartData',
        type: 'GET',
        dataType: 'json',
        success: function (data) {

            // Handle the response data
            document.querySelector("#spnTotalRevenueCount").innerHTML = data.totalCount;
            var sectionRevenueRatioHtml = document.createElement("span");
            if (data.isRatioIncrease) {
                sectionRevenueRatioHtml.className = "text-success me-1";
                sectionRevenueRatioHtml.innerHTML = "<i class='bi bi-arrow-up-right-circle me-1'></i><span>" + data.increaseDecreaseRatio + "%</span>";
            }
            else {
                sectionRevenueRatioHtml.className = "text-danger me-1";
                sectionRevenueRatioHtml.innerHTML = "<i class='bi bi-arrow-down-right-circle me-1'></i><span>" + data.increaseDecreaseRatio + "%</span>";
            }
            document.querySelector("#sectionRevenueRatio").append(sectionRevenueRatioHtml);
            document.querySelector("#sectionRevenueRatio").append("since last month");

            loadRadialBarChart("total-revenue-chart", data);

            $(".chart-spinner").hide();
        },
        error: function (xhr, textStatus, errorThrown) {
            console.error('Error:', errorThrown);
        }
    });
}
