/*$('.accordion .questions').click(function () {
    $('.expanded form').hide();
    $('.expanded .question-show a').replaceWith('<a href="#">Show Details</a>');
    $('.expanded').removeClass('expanded');
    $(this).next().toggle('slow');
    $(this).parent().toggleClass('expanded');
    $(this).children('.question-show').replaceWith('<span class="question-show"><a href="#" class="hide">Hide Details</a></span>');
    return false;
}).next().hide();*/

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
    }
    else {
        // The question is not expanded - expand it.
        innerContent.hide();
        var data = Model.Questions[container.index()];
        innerContent.html(Mustache.render(document.getElementById("answer-template").text, data));
        innerContent.slideDown("fast");
        container.addClass("expanded");

        // Set the focus to the first textarea.
        $("textarea", innerContent).focus();

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
                error: function () {
                    // Re-enable the submit button and hide the progress indicator.
                    $("button", innerContent).removeAttr("disabled");
                    $(".progress-indicator", innerContent).hide();

                    // Display an error message.
                    alert("Failed to submit answer.  Please try again.");
                },
                success: function () {
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