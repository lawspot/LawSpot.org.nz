/* {{Include mustache.js}} */

document.getElementById("_content").innerHTML = Mustache.render(document.getElementById("_contentTemplate").text, Model);
document.getElementById("_header").innerHTML = Mustache.render(document.getElementById("_headerTemplate").text, Model);
document.getElementById("_message").innerHTML = Mustache.render(document.getElementById("_messageTemplate").text, Model);