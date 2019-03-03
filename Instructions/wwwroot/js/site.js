// Please see documentation at https://docs.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

// Write your JavaScript code.

    $('#delete').on('click', function (e) {

        $.ajax({
            url: '/Home/DeleteComment',
            type: 'POST',
            cache: false,
            async: true,
            data: { "id": $(this).attr("commid") },
            dataType: "json"
        })
            .done(function (result) {
                $(this).closest(".mb-1").remove();

            }).fail(function (xhr) {
                console.log('error : ' + xhr.status + ' - ' + xhr.statusText + ' - ' + xhr.responseText);
            })});
