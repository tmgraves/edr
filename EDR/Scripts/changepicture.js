function loadImage(input) {
    if (input.files && input.files[0]) {
        if (input.files[0].size < 1000000)
        {
            var reader = new FileReader();

            reader.onload = function (e) {
                $('#image').cropper({
                    aspectRatio: 10 / 10,
                    zoomable: false,
                });
                $("#image").cropper('replace', e.target.result);
                $("#btnupload").show();
            }

            reader.readAsDataURL(input.files[0]);
        }
        else
        {
            alert("Pick a smaller file");
        }
    }
}

$("#imgInp").change(function () {
    loadImage(this);
    $("#croppic").show();
});

function LoadFacebookPhotos() {
        $.get('@Url.Action("GetFacebookPicturesPartial", "Dancer")', {}, function (data) {
        $("#facebookpics").empty();
        $("#facebookpics").html(data);
    });
}

$(function () {
    LoadFacebookPhotos();
    $('#btnupload').click(function () {
        try {
            var imageData = $("#image").cropper("getCroppedCanvas").toDataURL('png', 1);
            $.ajax({
                url: "../../School/UploadImageAsync",
                type: 'POST',
                data: { 'imageData': imageData, 'id': '@Model.School.Id' },
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
                        $('#profilepic').attr('src', filePath);
                        //  Set Hidden Value for the Photo
                        $("#image").cropper('destroy');
                        $('#image').attr('src', "");
                        $("#btnupload").hide();
                    }
                    else {
                        alert("Upload Failed");
                    }
                }
            });
        }
        catch (err) {
            alert(err.message);
        }
    });

    $('#btnChangePicture').click(function () {
        $('#divPick').show();
        $('#divOptions').hide();
    });

    $('#btnNewPicture').click(function () {
        $('#divPick').hide();
        $('#divOptions').show();
        $('#divUploadPic').show();
        $('#divFacebookPic').hide();
        $('#croppic').hide();
        $('#image').attr('src', "");
    });

    $('#btnPickFacebook').click(function () {
        $('#divPick').hide();
        $('#divOptions').show();
        $('#divUploadPic').hide();
        $('#divFacebookPic').show();
        $('#croppic').hide();
        $('#image').attr('src', "");
    });

    $('#facebookpics').on("click", ".pickfbpic", function () {
        var src = $(this).prop('value');
        $('#image').attr('src', src);
        $('#croppic').show();
        $('#divFacebookPic').hide();
        $('#image').cropper({
            aspectRatio: 10 / 10,
            zoomable: false,
        });
        $("#image").cropper('replace', src);
        $("#btnupload").show();
    });
});