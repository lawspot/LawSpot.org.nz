/* {{Include //ajax.googleapis.com/ajax/libs/jquery/1.7.2/jquery.min.js}} */

function showThumbnail(input) {
    if (input.files && input.files[0]) {
        var reader = new FileReader();

        reader.onload = function (e) {
            $('#logoThumbnail')
                .attr('src', e.target.result)
                .css("visibility", "visible");
        };

        reader.readAsDataURL(input.files[0]);
    }
}