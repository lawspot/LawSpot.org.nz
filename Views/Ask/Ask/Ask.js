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

// Stretch the divider to the full height of the form.
function stretchDivider() {
    var height = document.getElementById("qsubmit1").offsetHeight;
    document.getElementById("qsubmit-overlay2").style.borderTop = (height / 2) + "px solid transparent";
    document.getElementById("qsubmit-overlay2").style.borderBottom = (height / 2) + "px solid transparent";
}
stretchDivider();

$("#login-link").click(function () {
    $("#register").hide();
    $("#login").show();
    stretchDivider();
    $("#ShowRegistration").val("false");
    $("#login input[type=text]").focus();
});

$("#register-link").click(function () {
    $("#login").hide();
    $("#register").show();
    stretchDivider();
    $("#ShowRegistration").val("true");
    $("#register input[type=text]").focus();
});