//--- this has multiple AJAX functions that are used into multiple views
$(document).ready(function () {

    //--- AJAX to fill Master/Details combo-boxes into ExpesnseRecord Create & Edit forms
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
        if ($("#formExpenseRecord").valid()) {
            var file = $("#file")[0];
            if (file != null && file.files != null
                && file.files.length > 0) {
                //stop submit the form, we will post it manually.
                event.preventDefault(); // <------------------ stop default behaviour of button -<<< STOP FORM SUBMIT >>>

                
                $("#FilesLabel").val('');

                // disable buttons
                
                $("#buttonSubmit").prop("disabled", true);
                //$("#buttonCancel").removeAttr('href');
                $('#buttonCancel').each(function () {$(this).data("href", $(this).attr("href")).removeAttr("href");});
                $('#buttonDelete').each(function () {$(this).data("href", $(this).attr("href")).removeAttr("href");});

                // WebAPI Url
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
                            $("#FilesLabel").val('Submitting the form, please wait ...');
                            //--- add attachments info to formData
                            var jsonData = JSON.stringify(data);
                            $("#attachmentsJson").val(jsonData);
                            //--- 2nd - Submit the form
                            $('#formExpenseRecord').submit(); //<------------ submit form
                        }
                    },
                    error: function (jqXHR, textStatus, errorThrown) {
                        
                        $('#buttonCancel').each(function () {$(this).attr("href", $(this).data("href"));});
                        $('#buttonDelete').each(function () {$(this).attr("href", $(this).data("href"));});

                        alert("Error on file upload.\n\nStatus Code: " + jqXHR.status + "\n\nStatus Text: " + jqXHR.statusText + "\n\nMessage: " + jqXHR.responseText);
                    }

                });
            }
        }
        return true;
    });

    //------------------------------------------------------
    //---------------- DATATABLES --------------------------
    //------------------------------------------------------
    $("#dataTable").DataTable({
        "paging": false,
        "responsive": true,
        "searching": false,     // Search Box will Be Disabled
        "ordering": true,       // Ordering (Sorting on Each Column)will Be Disabled
        "info": false,          // Will show "1 to n of n entries" Text at bottom
        "lengthChange": false,  // Will Disabled Record number per page
        "paginate": {
            "next": ">",
            "previous": "<"
        },



    });

});
