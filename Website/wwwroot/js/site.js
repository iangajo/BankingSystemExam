// Write your JavaScript code.
$(document).ready(function () {

    if ($("#transactionType").val() == 0 || $("#transactionType").val() == 1) {
        $("#accountNumberFormGroup").hide();
    } else {
        $("#accountNumberFormGroup").show();
    }

    $("#transactionType").on("change",
        function () {
            if (this.value == 0 || this.value == 1) {
                $("#accountNumberFormGroup").hide();
            } else {
                $("#accountNumberFormGroup").show();
            }
        });

});