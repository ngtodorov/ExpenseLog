$(document).ready(function () {

    //--- AJAX to fill Master/Details combo-boxes
    $('#expenseTypeID').change(function () {
        var expenseTypeID = $(this).val();
        var path = window.location.pathname;
        var page = path.split("/").pop();
        var $entity = $('#expenseEntityID');
        $entity.empty();
        $entity.append($('<option></option>').val('').html('Please wait ...'));

        if (expenseTypeID == "") {
            $entity.empty();
            $entity.append($('<option></option>').val('').html('Select Entity'));
            return;
        }

        jQuery.ajax({
            url: '/ExpenseRecord/GetEntityListByType',
            type: 'GET',
            data: { 'expenseTypeID': expenseTypeID, 'page': page },
            dataType: 'json',
            success: function (d) {
                $entity.empty();
                $.each(d, function (i, item) {
                    $entity.append($('<option></option>').val(item.Key).html(item.Value));
                });
            },
            error: function () {
                alert('Error on GET /ExpenseRecord/GetEntityListByType');
            }
        });


    });


    //--- This script will be executed on Expense Record Create and Edit form submission
    //--- it will first upload the files (attachments) and then it will submit the form 
    $("#buttonSubmit").click(function (event) {
        var file = $("#file")[0];
        if (file != null && file.files != null && file.files.length > 0) {
            //stop submit the form, we will post it manually.
            event.preventDefault(); // <------------------ stop default behaviour of button -<<< STOP FORM SUBMIT >>>

            // disable buttons
            $("#buttonSubmit").prop("disabled", true);
            $("#buttonCancel").prop("disabled", true);

            var attachmentUploadWebAPIUrl = $('#attachmentUploadWebAPIUrl').val();

            var form = $('#formExpenseRecord')[0];
            //var formData = $('#formExpenseRecord').serialize();
            var formData = new FormData(form);

            //--- 1st - Upload the attachments
            $.ajax({
                type: 'POST',
                async: true,
                url: attachmentUploadWebAPIUrl,
                data: formData,
                enctype: 'multipart/form-data',
                cache: false,
                contentType: false,
                processData: false,
                timeout: 600000,
                dataType: 'json',
                success: function (data, textStatus, xhr) {//the "data" object contains the data returned from the server
                    var statusCode = xhr.status;
                    if (statusCode == "200" && data != null) {
                        //--- remove the files because we already uploaded them thru the ajax call of WebAPI
                        $("#file").val('');
                        //--- add attachments info to formData
                        var jsonData = JSON.stringify(data);
                        $("#attachmentsJson").val(jsonData);
                        //--- 2nd - Submit the form
                        $('#formExpenseRecord').submit(); //<------------ submit form
                    }
                },
                error: function (jqXHR, textStatus, errorThrown) {
                    $("#buttonCancel").prop("disabled", false);
                    alert("Error on file upload.\n\nStatus Code: " + jqXHR.status + "\n\nStatus Text: " + jqXHR.statusText + "\n\nMessage: " + jqXHR.responseText);
                }

            });
        }
        return true;
    });

});
