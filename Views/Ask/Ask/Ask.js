/* {{Include //ajax.googleapis.com/ajax/libs/jquery/1.7.2/jquery.min.js}} */

// Display and limit the number of characters allowed in the question textboxes.
function countCharacters(textElement, charCountElement, limit) {
    var count = function () {
        if (textElement.value.length > limit)
            textElement.value = textElement.value.substr(0, limit);
        charCountElement.innerHTML = (limit - textElement.value.length) + " characters remaining";
    }
    textElement.onkeyup = count;
    count();
}
countCharacters(document.getElementById("Title"), document.getElementById("TitleCharCount"), 150);
countCharacters(document.getElementById("Details"), document.getElementById("DetailsCharCount"), 600);

function stretchDivider() {
    var height = document.getElementById("qsubmit1").offsetHeight;
    document.getElementById("qsubmit-overlay2").style.borderTop = (height / 2) + "px solid transparent";
    document.getElementById("qsubmit-overlay2").style.borderBottom = (height / 2) + "px solid transparent";
}

// Stretch the divider to the full height of the form.
if (document.getElementById("qsubmit-overlay2")) {

    stretchDivider();

    document.getElementById("login-link").onclick = function () {
        document.getElementById("register").style.display = "none";
        document.getElementById("login").style.display = "block";
        stretchDivider();
        document.getElementById("ShowRegistration").value = "false";
        document.getElementById("LoginEmailAddress").focus();
        return false;
    };

    document.getElementById("register-link").onclick = function () {
        document.getElementById("login").style.display = "none";
        document.getElementById("register").style.display = "block";
        stretchDivider();
        document.getElementById("ShowRegistration").value = "true";
        document.getElementById("EmailAddress").focus();
        return false;
    };

}

// Update the search suggestions now.
updateSuggestions(Model);

// Update the search suggestions when the user tabs out of the question textbox.
$("#Title").blur(function () {
    var questionText = $("#Title").val();
    if (questionText === "") {
        updateSuggestions({ Suggestions: [] });
        return;
    }
    jQuery.ajax({
        type: "GET",
        url: "/suggestions",
        data: { text: questionText },
        success: function (data) {
            updateSuggestions(data);
        }
    });
});

function updateSuggestions(data) {
    if (data.Suggestions === null || data.Suggestions.length === 0)
        $("#suggestions").fadeOut("fast");
    else {
        $("#suggestions").html(Mustache.render(document.getElementById("suggestions-template").text, data));
        $("#suggestions").fadeIn("fast");
    }
}