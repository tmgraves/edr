$(document).ready(function () {
    $('.spotifyplayer').each(function () {
        $(this).css('width', $(this).parent(1).css('width'));
        $(this).attr('src', $(this).attr('src'));
    });
});
$(window).resize(function () {
    $('.spotifyplayer').each(function () {
        $(this).css('width', $(this).parent(1).css('width'));
        $(this).attr('src', $(this).attr('src'));
    });
});
