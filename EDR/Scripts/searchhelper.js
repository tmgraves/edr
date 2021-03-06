﻿/*!
 * Common Functions for map search
 *
 * Date: 2016-05-06
 */

$(function () {
    if ($('.map_canvas').length != 0)
    {
        if (navigator.geolocation) {
            //  navigator.geolocation.getCurrentPosition(success);
            navigator.geolocation.getCurrentPosition(success, error);
        }
        else
        {
            var loc = new google.maps.LatLng(34.052235, -118.243683);
            success(loc);
        }
    }

    function success(position) {
        var loc = new google.maps.LatLng(position.coords.latitude, position.coords.longitude);
        if ($('.clatfield').val() != "" && $('.clngfield').val() != "") {
            loc = new google.maps.LatLng($('.clatfield').val(), $('.clngfield').val());
        }

        if ($('.locationsearch').length != 0)
        {
            var map = $('.locationsearch').geocomplete("map");
            $(".locationsearch").geocomplete("find", $('.locationsearch').val());

            map.setCenter(loc);

            google.maps.event.addListenerOnce(map, 'idle', function () {
                var bounds = map.getBounds();
                var ne = bounds.getNorthEast(); // LatLng of the north-east corner
                var sw = bounds.getSouthWest(); // LatLng of the south-west corder

                //alert(ne.lat());

                $('.nelatfield').val(ne.lat());
                $('.nelngfield').val(ne.lng());
                $('.swlatfield').val(sw.lat());
                $('.swlngfield').val(sw.lng());
                //alert(map);
            });
        }
    }

    function error(err) {
        //  alert('ERROR(' + err.code + '): ' + err.message);

        if (err.code == 1)
        {
            var loc = new google.maps.LatLng(34.052235, -118.243683);
            SetMap(loc);
        }
    };

    function SetMap(loc) {
        if ($('.clatfield').val() != "" && $('.clngfield').val() != "") {
            loc = new google.maps.LatLng($('.clatfield').val(), $('.clngfield').val());
        }

        if ($('.locationsearch').length != 0) {
            var map = $('.locationsearch').geocomplete("map");
            map.setCenter(loc);

            google.maps.event.addListenerOnce(map, 'idle', function () {
                var bounds = map.getBounds();
                var ne = bounds.getNorthEast(); // LatLng of the north-east corner
                var sw = bounds.getSouthWest(); // LatLng of the south-west corder

                //alert(ne.lat());

                $('.nelatfield').val(ne.lat());
                $('.nelngfield').val(ne.lng());
                $('.swlatfield').val(sw.lat());
                $('.swlngfield').val(sw.lng());
                //alert(map);
            });
        }
    };
});

$(function () {
    if ($('.locationsearch').length != 0) {
        var options = {
            map: ".map_canvas"
        };
        $(".locationsearch").geocomplete(options)
            .on("geocode:result", function (event, result) {
            });
    }

});

$(function () {
    if ($('.locationsearch').length != 0) {
        var map = $('.locationsearch').geocomplete("map");
        if ($('.czoomfield').val() != "") {
            map.setZoom(parseInt($('.czoomfield').val()));
        }
        else {
            map.setZoom(10);
        }
    }
});

$('.stylesearch').autocomplete({
    source: function (request, response) {
        $.ajax({
            url: '../Style/Search',
            data: { searchString: request.term },
            dataType: 'json',
            type: 'GET',
            success: function (data) {
                response($.map(data, function (item) {
                    return {
                        label: item.Name,
                        value: item.Id
                    }
                }));
            }
        })
    },
    select: function (event, ui) {
        $('.stylesearch').val(ui.item.label);
        $('.stylesearchid').val(ui.item.value);
        return false;
    },
    minLength: 1
});

$('.teachersearch').autocomplete({
    source: function (request, response) {
        $.ajax({
            url: '../../Teacher/Search',
            data: { searchString: request.term },
            dataType: 'json',
            type: 'GET',
            success: function (data) {
                response($.map(data, function (item) {
                    return {
                        label: item.Name,
                        value: item.Id
                    }
                }));
            }
        })
    },
    select: function (event, ui) {
        $('.teachersearch').val(ui.item.label);
        $('.teachersearchid').val(ui.item.value);
        return false;
    },
    minLength: 1
});

$('.promotersearch').autocomplete({
    source: function (request, response) {
        $.ajax({
            url: '../../Promoter/Search',
            data: { searchString: request.term },
            dataType: 'json',
            type: 'GET',
            success: function (data) {
                response($.map(data, function (item) {
                    return {
                        label: item.Name,
                        value: item.Id
                    }
                }));
            }
        })
    },
    select: function (event, ui) {
        $('.promotersearch').val(ui.item.label);
        $('.promotersearchid').val(ui.item.value);
        return false;
    },
    minLength: 1
});

$('.ownersearch').autocomplete({
    source: function (request, response) {
        $.ajax({
            url: '../../Owner/Search',
            data: { searchString: request.term },
            dataType: 'json',
            type: 'GET',
            success: function (data) {
                response($.map(data, function (item) {
                    return {
                        label: item.Name,
                        value: item.Id
                    }
                }));
            }
        })
    },
    select: function (event, ui) {
        $('.ownersearch').val(ui.item.label);
        $('.ownersearchid').val(ui.item.value);
        return false;
    },
    minLength: 1
});

$('.formmain').submit(function () {
    var map = $('.locationsearch').geocomplete("map");
    $('.czoomfield').val(map.getZoom());
    var bounds = map.getBounds();
    var ne = bounds.getNorthEast(); // LatLng of the north-east corner
    var sw = bounds.getSouthWest(); // LatLng of the south-west corder
    var center = map.getCenter();
    $('.nelatfield').val(ne.lat());
    $('.nelngfield').val(ne.lng());
    $('.swlatfield').val(sw.lat());
    $('.swlngfield').val(sw.lng());
    $('.clatfield').val(center.lat());
    $('.clngfield').val(center.lng());
    if ($('.teachersearch').val() == "") {
        $('.teachersearchid').prop('value', "");
    }
    if ($('.stylesearch').val() == "") {
        $('.stylesearchid').prop('value', "");
    }
    if ($('.promotersearch').val() == "") {
        $('.promotersearchid').prop('value', "");
    }
    if ($('.ownersearch').val() == "") {
        $('.ownersearchid').prop('value', "");
    }
});
