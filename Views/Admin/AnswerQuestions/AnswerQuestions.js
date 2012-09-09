/* {{Include //ajax.googleapis.com/ajax/libs/jquery/1.7.2/jquery.min.js}} */
/* {{Include /shared/scripts/mustache.js}} */

var saveDraftTimerId = null;
var saveDraftContainer = null;

$(".question-container a").click(function (e) {
    var container = $(this).closest(".question-container");

    // Stop saving drafts.
    if (saveDraftTimerId) {
        saveDraft(saveDraftContainer);
        window.clearInterval(saveDraftTimerId);
        saveDraftTimerId = null;
        saveDraftContainer = null;
    }

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

    // Get the data for the question/answer.
    var data = Model.Questions.Items[container.index()];

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
        innerContent.hide();
        innerContent.html(Mustache.render(document.getElementById("answer-template").text, data));
        innerContent.slideDown("fast");
        container.addClass("expanded");

        // Save drafts intermittently.
        saveDraftContainer = container;
        saveDraft(container);
        saveDraftTimerId = window.setInterval(function () { saveDraft(container); }, 30000);

        // Set the focus to the first textarea.
        $("textarea", innerContent).get(0).focus();

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

            data.Answer = $("textarea[name=LawyerAnswer]", this).val();
            if (data.Answer === "") {
                $(".validation-error", innerContent).text("Please enter your answer.");
                $("textarea", innerContent).focus();
                return;
            }
            data.References = $("textarea[name=References]", this).val();

            // Disable the button and display the progress indicator.
            $("button", innerContent).attr("disabled", "disabled");
            $(".progress-indicator", innerContent).show();

            jQuery.ajax({
                type: "POST",
                url: "post-answer",
                data: { questionId: data.QuestionId, answerText: data.Answer, references: data.References },
                error: function (xhr, status, error) {
                    // Re-enable the submit button and hide the progress indicator.
                    $("button", innerContent).removeAttr("disabled");
                    $(".progress-indicator", innerContent).hide();

                    // Display an error message.
                    alert(status == "error" ? xhr.responseText : "Failed to submit answer.  Please try again.");
                },
                success: function (answerHtml) {
                    data.AnswerHtml = answerHtml;

                    // Stop saving drafts.
                    if (saveDraftTimerId) {
                        window.clearInterval(saveDraftTimerId);
                        saveDraftTimerId = null;
                        saveDraftContainer = null;
                    }

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

function saveDraft(container) {
    var data = Model.Questions.Items[container.index()];
    var answerText = $("textarea[name=LawyerAnswer]", container).val();
    var references = $("textarea[name=References]", container).val();
    var checkQuestionStatus = (data.Answer === answerText && data.References === references);
    var warningElement = $(".warning", container);

    jQuery.ajax({
        type: "POST",
        url: checkQuestionStatus ?  "/admin/check-question-status" : "/admin/save-draft-answer",
        data: checkQuestionStatus ? { questionId: data.QuestionId} : { questionId: data.QuestionId, answerText: answerText, references: references },
        success: function (warningText) {
            if (warningText) {
                $("span", warningElement).text(warningText);
                warningElement.slideDown("fast");
            }
            else
                warningElement.slideUp("fast");
            if (!checkQuestionStatus) {
                data.Answer = answerText;
                data.References = references;
            }
        }
    });
}

window.onbeforeunload = function () {
    // Save a draft.
    if (saveDraftTimerId) {
        saveDraft(saveDraftContainer);
        window.clearInterval(saveDraftTimerId);
        saveDraftTimerId = null;
        saveDraftContainer = null;
    }
}