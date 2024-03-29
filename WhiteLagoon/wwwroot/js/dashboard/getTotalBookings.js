﻿
$(document).ready(function () {
    loadTotalBookingChart();
});

function loadTotalBookingChart() {
    $(".chart-spinner").show();

    $.ajax({
        url: '/Dashboard/GetTotalBookingsChartData',
        type: 'GET',
        dataType: 'json',
        success: function (data) {

            // Handle the response data
            document.querySelector("#spnTotalCount").innerHTML = data.totalCount;
            var sectionBookingRatioHtml = document.createElement("span");
            if (data.isRatioIncrease) {
                sectionBookingRatioHtml.className = "text-success me-1";
                sectionBookingRatioHtml.innerHTML = "<i class='bi bi-arrow-up-right-circle me-1'></i><span>" + data.increaseDecreaseRatio + "%</span>";
            }
            else {
                sectionBookingRatioHtml.className = "text-danger me-1";
                sectionBookingRatioHtml.innerHTML = "<i class='bi bi-arrow-down-right-circle me-1'></i><span>" + data.increaseDecreaseRatio + "%</span>";
            }
            document.querySelector("#sectionBookingRatio").append(sectionBookingRatioHtml);
            document.querySelector("#sectionBookingRatio").append("since last month");

            loadRadialBarChart("total-bookings-chart", data);

            $(".chart-spinner").hide();
        },
        error: function (xhr, textStatus, errorThrown) {
            console.error('Error:', errorThrown);
        }
    });
}


