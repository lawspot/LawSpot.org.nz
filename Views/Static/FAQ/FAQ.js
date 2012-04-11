/* {{Include //ajax.googleapis.com/ajax/libs/jquery/1.7.2/jquery.min.js}} */

$("a.faq").click(function (e) {
    e.preventDefault();
    $(this).toggleClass("open");
    $(this).next("p").slideToggle("fast");
});