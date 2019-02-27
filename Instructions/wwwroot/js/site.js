// Please see documentation at https://docs.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

// Write your JavaScript code.
$(document).ready(function () {
    var data = new FormData();
    var id = 0;
   
    $("#progress").hide();

    $("#fileBasket").on("dragenter", function (evt) {
        evt.preventDefault();
        evt.stopPropagation();
    });

    $("#fileBasket").on("dragover", function (evt) {
        evt.preventDefault();
        evt.stopPropagation();
    });

    $("#fileBasket").on("drop", function (evt) {
        evt.preventDefault();
        evt.stopPropagation();
        var files=evt.originalEvent.dataTransfer.files;
        var fileNames = "";        
        if (files.length > 0) {
           
            for (var i = 0; i < files.length; i++) {
                fileNames += "<div>" + files[i].name + "<input id='"+files[i].name+"'  type='button' onclick=this.parentNode.remove(); class='btn btn - primary' value='cancel' /><br /></div >";
                id++;
            }
        }
        $("#fileBasket").html(fileNames)
        for (var i = 0; i < files.length; i++) {
            data.append(files[i].name, files[i]);
        }
        $('#upload').on('click', function (e) {
        $.ajax({
            type: "POST",
            url: "/records/UploadFiles",
            contentType: false,
            processData: false,
            data: data,
            success: function (message) {
                $("#fileBasket").html(message);
            },
            error: function () {
                $("#fileBasket").html
                    ("There was error uploading files!");
            },
            beforeSend: function () {
                $("#progress").show();
            },
            complete: function () {
                $("#progress").hide();
            }
        });
        });
    });

});