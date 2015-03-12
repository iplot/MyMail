

$(document).ready(function () {

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
        this.needSign = $("#sign").is(':checked');
    }

    var url = $('#isEncrypt').is(':checked') ? '../Home/SendEncryptedMessage' : '../Home/SendMessage';

    $.post(url,
        new Mail(),
        function(success) {
            alert(success);
        }, "text");
}

function viewMail(data) {
    var html =
        '<table class="table table-hover table-striped">' +
            '<thead>' +
            '<tr><th>' + (data.State == 'Incoming' ? 'From' : 'To') + '</th> <th>Subject</th> <th>Date</th></tr>' +
            '</thead>' +
            '<tbody>';

    $(data.Mails).each(function(i, el) {
        html += '<tr><td>' + el.Email + '</td> <td>' + el.Subject + '</td> <td>' + el.Date + '</td></tr>';
    });

    html += '</tbody></table>';

    $('#contentDiv').html(html);
}

function testSend() {
//    $.get('../Home/GetMails', { 'mailsType': 'Incoming' }, viewMail, 'json');
    $.ajax("../Home/GetMails", {
        data: { 'mailsType': 'Incoming' },
        success: viewMail,
        type: 'json',
        timeout: 5000000
    });
}