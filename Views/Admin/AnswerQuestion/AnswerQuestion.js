/* {{Include //ajax.googleapis.com/ajax/libs/jquery/1.7.2/jquery.min.js}} */

$(document).ready(function () {

    // Save drafts intermittently.
    window.setInterval(function () { saveDraft(true); }, 10000);

    // Make the important notice link work.
    $("#important-notice-link").click(function (e) {
        // Prevent the hyperlink from navigating.
        e.preventDefault();

        // Show or hide the important notice text.
        $("#important-notice").slideToggle();
    });

});

function saveDraft(async) {
    var answerText = $("#AnswerTextArea").val();
    var references = $("#ReferencesTextArea").val();
    if (answerText === "" && references === "")
        return;
    if (Model.DraftAnswer === answerText && Model.DraftReferences === references)
        return;

    jQuery.ajax({
        type: "POST",
        url: "/admin/save-draft-answer",
        data: { questionId: Model.QuestionId, answerText: answerText, references: references },
        async: async,
        timeout: 5000,
        success: function (warningText) {
            Model.DraftAnswer = answerText;
            Model.DraftReferences = references;
        }
    });
}

window.onbeforeunload = function () {
    // Save a draft.
    saveDraft(false);
}