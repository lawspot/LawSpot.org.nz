/* {{Include mustache.js}} */

document.getElementById("_content").innerHTML = Mustache.render(document.getElementById("_contentTemplate").text, Model);
document.getElementById("_login").innerHTML = Mustache.render(document.getElementById("_loginTemplate").text, Model);
document.getElementById("_message").innerHTML = Mustache.render(document.getElementById("_messageTemplate").text, Model);