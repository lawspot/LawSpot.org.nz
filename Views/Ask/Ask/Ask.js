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