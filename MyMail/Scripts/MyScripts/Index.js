

$(document).ready(function () {
    var mailIndex = 0;

    //функция для a
    $('.list-group-item').click(function() {
        $('.list-group-item').each(function() {
            $(this).removeClass('active');
        })
        $(this).toggleClass("active");
    });

    $(document).on('click', 'tr', function() {
        $.ajax({
            url: "../Home/GetMessage",
            type: "POST",
            data: ({
                'index': this.rowIndex,
                'type': $('.active').attr('data-type')
            }),
            success: function(data) {
                $("#contentDiv").html(data);
            },
            dataType: "html"
        });
    });
});

function writeMessage() {
    $.get("../Home/PrepareSend", function(htmlNew) {
        $("#contentDiv").html(htmlNew);
    }, "html");
}

function sendMessage() {
    function Mail() {
        this.to = $("#to").val();
        this.subject = $("#subject").val();
        this.text = $("#text").text();
    }

    var url = $('#isEncrypt').is(':checked') ? '../Home/SendEncryptedMessage' : '../Home/SendMessage';

    $.post(url,
        new Mail(),
        function(success) {
            alert(success);
        }, "text");
}