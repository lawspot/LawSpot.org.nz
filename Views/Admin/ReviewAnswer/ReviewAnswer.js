/* {{Include //ajax.googleapis.com/ajax/libs/jquery/1.7.2/jquery.min.js}} */

$("*[data-action=go-to-approve]").click(function (e) {
    e.preventDefault();
    var container = $(e.target).closest(".answer");
    container.find(".view-content").hide();
    container.find(".approve-content").show();
    container.find(".approve-content textarea").focus();
    $('html, body').animate({ scrollTop: container.offset().top - 10 }, 500);
});

$("*[data-action=go-to-reject]").click(function (e) {
    e.preventDefault();
    var container = $(e.target).closest(".answer");
    container.find(".view-content").hide();
    container.find(".reject-content").show();
    container.find(".reject-content textarea").focus();
    $('html, body').animate({ scrollTop: container.offset().top - 10 }, 500);
});

$("*[data-action=cancel]").click(function (e) {
    e.preventDefault();
    var container = $(e.target).closest(".answer");
    container.find(".approve-content").hide();
    container.find(".reject-content").hide();
    container.find(".view-content").show();
});

// Hook up the canned rejection reason drop-down.
$(".canned-rejection-reasons").change(function (e) {
    var reason = $(this).val();
    if (reason)
        $(".reason").val(reason);
    this.selectedIndex = 0;
});

$("button[data-action=approve], button[data-action=reject]").click(function (e) {
    var form = $(e.target).closest("form");
    var data = { answerId: form.find("input[name=answerId]").val() };
    data[form.find("textarea").attr("name")] = form.find("textarea").val();

    // Disable the submit button and show the progress indicator.
    form.find("button").attr("disabled", "disabled");
    form.find(".progress-indicator").css("visibility", "visible");

    jQuery.ajax({
        type: form.attr("method"),
        url: form.attr("action"),
        data: data,
        error: function (xhr, status, error) {
            // Re-enable the submit button and hide the progress indicator.
            form.find("button").removeAttr("disabled");
            form.find(".progress-indicator").css("visibility", "hidden");

            // Display an error message.
            alert(status == "error" ? xhr.responseText : "Something went wrong.  Please try again.");
        },
        success: function (response) {
            // Refresh the page.
            window.location.href = "/admin/review-answers?alert=updated";
        }
    });
    e.preventDefault();
});

function toggleReviewedAnswers() {
    $(".approved-answer, .rejected-answer").slideToggle("fast");
    $("#showReviewedLink").toggle();
    $("#hideReviewedLink").toggle();
}