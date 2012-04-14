/* {{Include //ajax.googleapis.com/ajax/libs/jquery/1.7.2/jquery.min.js}} */

$(".section .change, .section .hide").click(function (e) {
    var section = $(this).closest(".section");
    section.toggleClass("expanded");

    // Prevent the hyperlink from navigating.
    e.preventDefault();
});