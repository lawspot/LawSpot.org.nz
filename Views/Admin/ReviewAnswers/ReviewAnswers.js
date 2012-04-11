﻿/* {{Include //ajax.googleapis.com/ajax/libs/jquery/1.7.2/jquery.min.js}} */

$(".question-container a").click(function (e) {
    var container = $(this).closest(".question-container");

    // Close any siblings.
    container.siblings().each(function (index, element) {
        var container = $(element);

        // If it's expanded, close it.
        if (container.hasClass("expanded")) {
            $(".inner-content", element).slideUp("fast", function () {
                this.innerHTML = "";
                container.removeClass("expanded");
            });
        }

        // If it's a success box, hide it.
        // Do not remove it, that would screw up the HTML -> Model mapping.
        if (container.hasClass("action-complete")) {
            container.slideUp("fast");
        }
    });

    var innerContent = $(".inner-content", container);
    if (container.hasClass("expanded")) {
        // The question is already expanded - close it.
        innerContent.slideUp("fast", function () {
            this.innerHTML = "";
            container.removeClass("expanded");
        });
    }
    else {
        // The question is not expanded - expand it.
        var data = Model.Answers[container.index()];
        innerContent.hide();
        innerContent.html(Mustache.render(document.getElementById("answer-template").text, data));
        innerContent.slideDown("fast");
        container.addClass("expanded");

        // Hook the button click event.
        $("button", innerContent).click(function (e) {
            // Don't submit the form.
            e.preventDefault();

            // Determine whether the reject button was clicked.
            var reject = $(this).hasClass("red");
            if (reject == false) {
                // Populate the data object from the form.
                data.Answer = $(".answer", this.form).val();
            }

            // Disable the button and display the progress indicator.
            $("button", innerContent).attr("disabled", "disabled");
            $(".progress-indicator", innerContent).show();

            jQuery.ajax({
                type: "POST",
                url: reject ? "post-reject-answer" : "post-approve-answer",
                data: { answerId: data.AnswerId, answerDetails: data.Answer },
                error: function () {
                    // Re-enable the submit button and hide the progress indicator.
                    $("button", innerContent).removeAttr("disabled");
                    $(".progress-indicator", innerContent).hide();

                    // Display an error message.
                    alert("Failed to submit answer " + (reject ? "rejection" : "approval") + ".  Please try again.");
                },
                success: function () {
                    // Render the success box.
                    var successBox = $(Mustache.render(document.getElementById(reject ? "rejection-template" : "approval-template").text, data));

                    // Show the success box.
                    container.replaceWith(successBox);

                    // Hook up the Next Question button.
                    $("button", successBox).click(function () {
                        var nextVisibleBox = successBox.nextAll(":visible").first();
                        if (nextVisibleBox.length > 0)
                            nextVisibleBox.find("a").click();
                        else {
                            successBox.slideUp("fast", function () {
                                if (successBox.siblings(":visible").length === 0)
                                    $("#no-more-answers").show("fast");
                            });
                        }
                    });
                }
            });
        });
    }

    // Prevent the hyperlink from navigating.
    e.preventDefault();
});