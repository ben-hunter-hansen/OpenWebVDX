

function Output(title, date, user) {

    var html = "<div class='col-xs-12 col-sm-3 appindexvidcol'>" +
               "<span class='pull-left'><small><span class='glyphicon glyphicon-cloud-upload'></span> " + date + "</small></span>" +
               "<span class='pull-right'><small><span class='glyphicon glyphicon-user'></span> "+user+"</small></span>" +
               "<button class='appindexvidbtn'>"+title+"</button>" +
               "</div>";

    $("#dynamic_vids").append(html);
}

$(document).ready(function () {

    $.get("/Home/GetIndexVids", function (data) {

        var amt_videos = data.Titles.length;
        
        for (var i = 0; i < amt_videos; i++) {
            Output(data.Titles[i],data.Dates[i],data.Users[i]);
        } 

    }, "json")
});