/* {{Include //ajax.googleapis.com/ajax/libs/jquery/1.7.2/jquery.min.js}} */

$(".lawyer-row button").click(function (e) {
    // Prevent the button click from submitting the form.
    e.preventDefault();

    // Determine whether this was a rejection or approval.
    var reject = $(this).hasClass("red");

    // Get the lawyer data.
    var container = $(this).closest("tr.lawyer-row");
    var data = Model.Lawyers[container.index("tr.lawyer-row")];

    // Disable the buttons.
    $("button", container).attr("disabled", "disabled");

    jQuery.ajax({
        type: "POST",
        url: reject ? "post-reject-lawyer" : "post-approve-lawyer",
        data: { lawyerId: data.LawyerId },
        error: function () {
            // Re-enable the buttons.
            $("button", container).removeAttr("disabled");

            // Display an error message.
            alert("Failed to submit lawyer " + (reject ? "rejection" : "approval") + ".  Please try again.");
        },
        success: function () {
            // Remove the row.
            container.remove();
        }
    });
});