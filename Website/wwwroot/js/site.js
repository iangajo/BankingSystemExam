﻿// Write your JavaScript code.
$(document).ready(function () {

    if ($("#transactionType").val() == 0 || $("#transactionType").val() == 1) {
        $("#accountNumberFormGroup").hide();
    } else {
        $("#accountNumberFormGroup").show();
    }

    $("#transferBtn").text("Deposit");
    $("#modalBodyText").text("Are you sure you want to make the deposit?");
    

    $("#transactionType").on("change",
        function () {
            if (this.value == 0 || this.value == 1) {
                $("#accountNumberFormGroup").hide();
            } else {
                $("#accountNumberFormGroup").show();
            }

            if (this.value == 0) {
                $("#transferBtn").text("Deposit");
                $("#modalBodyText").text("Are you sure you want to make the deposit?");
            } else if (this.value == 1) {
                $("#transferBtn").text("Withdraw");
                $("#modalBodyText").text("Are you sure you want to make the withdraw?");
            } else {
                $("#transferBtn").text("Transfer");
                
               
            }
            
        });

    $("#transferBtn").on("click",
        function () {
            
            if ($("#transactionType").val() == 2) {
                var accountNumberReceiver = $("#accountNumberInput").val();
                $("#modalBodyText").text("Are you sure you want to transfer funds to account number " + accountNumberReceiver);
            }
            
        });

});