/* {{Include //ajax.googleapis.com/ajax/libs/jquery/1.7.2/jquery.min.js}} */
/* {{Include /shared/scripts/mustache.js}} */

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

        // Manipulate the data.
        var data = Model.Questions.Items[container.index()];
        data.Categories = [];
        for (var i in Model.Categories) {
            data.Categories[i] = {
                Text: Model.Categories[i].Text,
                Value: Model.Categories[i].Value,
                Selected: Model.Categories[i].Value == data.CategoryId
            };
        }
        data.CannedRejectionReasons = [];
        for (var i in Model.CannedRejectionReasons) {
            data.CannedRejectionReasons[i] = {
                Text: Model.CannedRejectionReasons[i].Text,
                Value: Model.CannedRejectionReasons[i].Value
            };
        }

        // Exapnd the form.
        innerContent.hide();
        innerContent.html(Mustache.render(document.getElementById("answer-template").text, data));
        innerContent.slideDown("fast");
        container.addClass("expanded");

        // Hook the button click event.
        $("button", innerContent).click(function (e) {
            // Don't submit the form.
            e.preventDefault();

            // Determine what action to take.
            var action = $(this).attr("data-action");
            var url, postData, template;
            if (action == "not-conflicted") {
                // The user clicked reject on the not conflicted button.
                
                jQuery.ajax({
                    type: "POST",
                    url: "post-conflict-declaration",
                    data: { questionId: data.QuestionId },
                    error: function (xhr, status, error) {
                        // Display an error message.
                        alert(status == "error" ? xhr.responseText : "Something went wrong.  Please try again.");
                    },
                    success: function (response) {
                    }
                });

                $("#conflicted-container").slideUp("fast");
                $("#not-conflicted-container").slideDown("fast");

                return;
            }
            else if (action == "accept") {
                // The user clicked on the accept button.

                // Details to send to the server.
                url = "post-accept-referred-question";
                postData = { questionId: data.QuestionId };
                template = "accept-template";
            }

            // Disable the buttons and display the progress indicator.
            $("button", innerContent).attr("disabled", "disabled");
            $(".progress-indicator", innerContent).show();

            jQuery.ajax({
                type: "POST",
                url: url,
                data: postData,
                error: function (xhr, status, error) {
                    // Re-enable the buttons and hide the progress indicator.
                    $("button", innerContent).removeAttr("disabled");
                    $(".progress-indicator", innerContent).hide();

                    // Display an error message.
                    alert(status == "error" ? xhr.responseText : "Something went wrong.  Please try again.");
                },
                success: function (response) {
                    // Render the success box.
                    data.ResponseHtml = response;
                    var successBox = $(Mustache.render(document.getElementById(template).text, data));

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

// If there's only one question, might as well expand it.
if ($(".question-container a").length == 1)
    $(".question-container a").click();