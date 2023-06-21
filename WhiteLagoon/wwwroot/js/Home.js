$(document).ready(function () {
    // Initialize the date picker
    var today = new Date();
    var dd = String(today.getDate()).padStart(2, '0');
    var mm = String(today.getMonth() + 1).padStart(2, '0');
    var yyyy = today.getFullYear();

    today = yyyy + '-' + mm + '-' + dd;
    $('#checkindate').attr('min', today);
});

function fnLoadVillaList() {
    var objData = {
        checkInDate: $("#CheckInDate").val(),
        nights: $("#Nights").val()
    };
    $('.spinner').show();
    $.ajax({
        type: "POST",
        url: "Home/GetVillasByDate",
        data: objData,
        success: function (response) {
            $('#VillasList').empty();
            $("#VillasList").html(response);
            $('.spinner').hide();
        },
        failure: function (response) {
            $('.spinner').hide();
            alert(response.responseText);
        },
        error: function (response) {
            $('.spinner').hide();
            alert(response.responseText);
        }
    });
}