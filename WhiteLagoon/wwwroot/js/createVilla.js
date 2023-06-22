$(document).ready(function () {
    $('#frmVilla').validate({
        ignore: ":hidden:not(#rtDescription),.note-editable.panel-body"
    });

    $('#rtDescription').summernote({
        height: 200,
        disableResizeEditor: true
    });

    $('form').each(function () {
        if ($(this).data('validator'))
            $(this).data('validator').settings.ignore = ".note-editor *";
    });
});