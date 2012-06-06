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
        if (container.hasClass("answered")) {
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

        // Unreserve the question.
        reserveQuestion_questionId = 0;
        reserveQuestion_container = null;
        checkForQuestionReservation(false);
    }
    else {
        // The question is not expanded - expand it.
        innerContent.hide();
        var data = Model.Questions.Items[container.index()];
        innerContent.html(Mustache.render(document.getElementById("answer-template").text, data));
        innerContent.slideDown("fast");
        container.addClass("expanded");

        // Reserve the question.
        reserveQuestion_questionId = data.QuestionId;
        reserveQuestion_container = container;
        checkForQuestionReservation(false);

        // Set the focus to the first textarea.
        $("textarea", innerContent).focus();

        // Make the important notice link work.
        $("a.important-notice-link", innerContent).click(function (e) {
            // Prevent the hyperlink from navigating.
            e.preventDefault();

            // Show or hide the important notice text.
            $(".important-notice", innerContent).slideToggle();
        });

        // Hook the form submit event.
        $("form", innerContent).submit(function (e) {
            // Don't submit the form.
            e.preventDefault();

            data.Answer = $("textarea", this).val();
            if (data.Answer === "") {
                $(".validation-error", innerContent).text("Please enter your answer.");
                $("textarea", innerContent).focus();
                return;
            }

            // Disable the button and display the progress indicator.
            $("button", innerContent).attr("disabled", "disabled");
            $(".progress-indicator", innerContent).show();

            jQuery.ajax({
                type: "POST",
                url: "post-answer",
                data: { questionId: data.QuestionId, answerText: data.Answer },
                error: function (xhr, status, error) {
                    // Re-enable the submit button and hide the progress indicator.
                    $("button", innerContent).removeAttr("disabled");
                    $(".progress-indicator", innerContent).hide();

                    // Display an error message.
                    alert(status == "error" ? xhr.responseText : "Failed to submit answer.  Please try again.");
                },
                success: function (answerHtml) {
                    data.AnswerHtml = answerHtml;

                    // Render the success box.
                    var successBox = $(Mustache.render(document.getElementById("success-template").text, data));

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
                                    $("#no-more-questions").show("fast");
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

var reserveQuestion_sessionId = Math.round(Math.random() * 2147483647);
var reserveQuestion_questionId = 0;
var reserveQuestion_container = null;

function checkForQuestionReservation(reserve) {
    var questionId = reserveQuestion_questionId;
    var container = reserveQuestion_container;
    jQuery.ajax({
        type: "POST",
        url: "check-for-question-reservation",
        data: { questionId: questionId, sessionId: reserveQuestion_sessionId, reserve: reserve },
        success: function (displayWarning) {
            if (questionId === reserveQuestion_questionId && questionId > 0 && container) {
                if (displayWarning)
                    $(".reservation-warning", container).slideDown("fast");
                else
                    $(".reservation-warning", container).slideUp("fast");
            }
        }
    });
}

window.setInterval(function () {
    if (reserveQuestion_container)
        checkForQuestionReservation($("textarea", reserveQuestion_container).val().length > 0);
}, 10000);