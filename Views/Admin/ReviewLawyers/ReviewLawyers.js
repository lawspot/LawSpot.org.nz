/* {{Include //ajax.googleapis.com/ajax/libs/jquery/1.7.2/jquery.min.js}} */

var selectedRowIndex;

$(".lawyer-row button").click(function (e) {
    // Prevent the button click from submitting the form.
    e.preventDefault();

    // Save the row index.
    var container = $(this).closest("tr.lawyer-row");
    selectedRowIndex = container.index("tr.lawyer-row");

    // Determine whether this was a rejection or approval.
    var action = $(this).attr("data-action");
    if (action == "go-to-reject") {
        var background = document.createElement("div");
        background.id = "background";
        background.style.backgroundColor = "black";
        background.style.opacity = 0.5;
        background.style.zIndex = 99;
        background.style.position = "absolute";
        background.style.top = 0;
        background.style.left = 0;
        background.style.width = "100%";
        background.style.height = "100%";
        $(document.body).append(background);
        $("#rejection-form").css("z-index", "100");
        $("#rejection-form").show();
        return;
    }
    var reject = $(this).hasClass("red");

    // Get the lawyer data.
    var data = Model.Lawyers[selectedRowIndex];

    // Disable the buttons.
    $("button", container).attr("disabled", "disabled");

    jQuery.ajax({
        type: "POST",
        url: "post-approve-lawyer",
        data: { lawyerId: data.LawyerId },
        error: function (xhr, status, error) {
            // Re-enable the buttons.
            $("button", container).removeAttr("disabled");

            // Display an error message.
            alert(status == "error" ? xhr.responseText : "Failed to submit lawyer approval.  Please try again.");
        },
        success: function () {
            // Hide the row.
            container.hide();
        }
    });
});

// Hook up the canned rejection reason drop-down.
$("#rejection-form .canned-rejection-reasons").change(function (e) {
    var reason = $(this).val();
    if (reason)
        $("#rejection-form .reason").val(reason);
    this.selectedIndex = 0;
});

// Hook up the reject button on the rejection form.
$("#rejection-form button[data-action=reject]").click(function (e) {
    // Prevent the button click from submitting the form.
    e.preventDefault();

    // Get the form and the rejection reason.
    var form = this.form;
    var reason = $(".reason", form).val();

    // Get the lawyer data.
    var data = Model.Lawyers[selectedRowIndex];

    // Disable the buttons and display the progress indicator.
    $("button", form).attr("disabled", "disabled");
    $(".progress-indicator", form).show();

    jQuery.ajax({
        type: "POST",
        url: "post-reject-lawyer",
        data: { lawyerId: data.LawyerId, reason: reason },
        error: function (xhr, status, error) {
            // Re-enable the buttons and hide the progress indicator.
            $("button", form).removeAttr("disabled");
            $(".progress-indicator", form).hide();

            // Display an error message.
            alert(status == "error" ? xhr.responseText : "Failed to submit lawyer rejection.  Please try again.");
        },
        success: function () {
            // Re-enable the buttons and hide the progress indicator.
            $("button", form).removeAttr("disabled");
            $(".progress-indicator", form).hide();

            // Hide the row.
            $($("tr.lawyer-row").get(selectedRowIndex)).hide();

            // Turn off the greyed out effect.
            $("#background").remove();

            // Hide the form.
            $("#rejection-form").hide();

            // Clear the reason textbox.
            $("#rejection-form .reason").val("");
        }
    });
});

// Hook up the cancel button on the rejection form.
$("#rejection-form button[data-action=cancel]").click(function (e) {
    // Prevent the button click from submitting the form.
    e.preventDefault();

    // Turn off the greyed out effect.
    $("#background").remove();

    // Hide the form.
    $("#rejection-form").hide();

    // Clear the reason textbox.
    $("#rejection-form .reason").val("");
});