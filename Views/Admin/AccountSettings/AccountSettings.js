/* {{Include //ajax.googleapis.com/ajax/libs/jquery/1.7.2/jquery.min.js}} */

$(".section .change, .section .hide, .section h2").click(function (e) {
    // Prevent the hyperlink from navigating.
    e.preventDefault();

    // Expand or hide the section.
    var section = $(this).closest(".section");
    section.toggleClass("expanded");

    if (section.hasClass("expanded")) {
        // Set the focus to the first form element.
        $("input[type=text],input[type=password],select", section).first().focus();
    }
});