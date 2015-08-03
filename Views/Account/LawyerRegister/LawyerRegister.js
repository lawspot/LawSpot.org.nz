/* {{Include //ajax.googleapis.com/ajax/libs/jquery/1.7.2/jquery.min.js}} */

$("#canPublishSelect").change(function () {
    if (this.selectedIndex == 1)
        $("#canNotPublishMessage").show(400);
    else
        $("#canNotPublishMessage").hide(400);
    if (this.selectedIndex == 2)
        $("#canPublishMessage").show(400);
    else
        $("#canPublishMessage").hide(400);
});