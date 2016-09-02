var uploadImageUrl; //  "@Url.Action("UploadImageAsync", "School")"
var id; //  SchoolId
var imagesize = 4000000;
var aspectratio = 10 / 10;

function loadImage(input) {
    if (input.files && input.files[0]) {
        if (input.files[0].size < imagesize)
        {
            try{
                var reader = new FileReader();

                reader.onload = function (e) {
                    $('.cropimage').cropper({
                        aspectRatio: aspectratio,
                        zoomable: false,
                    });
                    $(".cropimage").cropper('replace', e.target.result);
                    $(".btnupload").show();
                }

                reader.readAsDataURL(input.files[0]);
            }
            catch (err) {
                alert(err.message);
            }
        }
        else
        {
            alert("Pick a smaller file");
        }
    }
}

$(".uploadfile").change(function () {
    loadImage(this);
    $(".croppic").show();
});

function LoadFacebookPhotos() {
    $.get('../../Dancer/GetFacebookPicturesPartial', {}, function (data) {
        $(".facebookpics").empty();
        $(".facebookpics").html(data);
    });
}

$(function () {
    LoadFacebookPhotos();
    $('.btnupload').click(function () {
        try {
            var imageData = $(".cropimage")
                                .cropper("getCroppedCanvas", {
                                    width: 500,
                                    height: 500
                                }).toDataURL('png', 1);
            $.ajax({
                url: uploadImageUrl,
                type: 'POST',
                data: { 'imageData': imageData, 'id': id },
                dataType: 'json',
                timeout: 300000,
                success: function (result) {
                    var status;
                    var filePath;
                    $.each(result, function (key, value) {
                        if (key == "UploadStatus") {
                            status = value;
                        }
                        else {
                            filePath = value;
                        }
                    });
                    if (status == "Success") {
                        $('.targetpic').attr('src', filePath);
                        //  Set Hidden Value for the Photo
                        $(".cropimage").cropper('destroy');
                        $('.cropimage').attr('src', "");
                        $(".btnupload").hide();
                    }
                    else {
                        alert("Upload Failed");
                    }
                },
                error: function (request, status, error) {
                    alert(error);
                }
            });
        }
        catch (err) {
            alert(err.message);
        }
    });

    $('.changepicbutton').click(function () {
        $('.divPick').show();
        $('.divOptions').hide();
    });

    $('.newpicturebtn').click(function () {
        $('.divPick').hide();
        $('.divOptions').show();
        $('.divUploadPic').show();
        $('.divFacebookPic').hide();
        $('.croppic').hide();
        $('.cropimage').attr('src', "");
    });

    $('.facebookpicturebtn').click(function () {
        $('.divPick').hide();
        $('.divOptions').show();
        $('.divUploadPic').hide();
        $('.divFacebookPic').show();
        $('.croppic').hide();
        $('.cropimage').attr('src', "");
    });

    $('.facebookpics').on("click", ".pickfbpic", function () {
        var src = $(this).prop('value');
        $('.cropimage').attr('src', src);
        $('.croppic').show();
        $('.divFacebookPic').hide();
        $('.cropimage').cropper({
            aspectRatio: aspectratio,
            zoomable: false,
        });
        $(".cropimage").cropper('replace', src);
        $(".btnupload").show();
    });
});

$.getScript("../../Scripts/cropper.js");
$('head').append('<link rel="stylesheet" type="text/css" href="../../Content/cropper.css">');
