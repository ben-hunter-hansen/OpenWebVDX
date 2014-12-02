

function OutputButtons(id,title, date, user) {

    var html = "<div class='col-xs-12 col-sm-3 appindexvidcol'>" +
               "<span class='vid-date pull-left'><small><span class='glyphicon glyphicon-cloud-upload'></span> " + date + "</small></span>" +
               "<span class='vid-user pull-right'><small><span class='glyphicon glyphicon-user'></span> " + user + "</small></span>" +
               "<button class='appindexvidbtn'>" + title + "</button>" +
               "<span class='vid-id' hidden>"+id+"</span>" +
               "</div>";

    $("#dynamic_vids").append(html);
}

function OutputMessage(msg) {
    var html = "<div class='col-xs-12'>" +
               "<h4>" + msg + "</h4>";
               "</div>";
    $("#dynamic_vids").append(html);
}

$(document).ready(function () {
    
    //$.get("/Home/GetVideoStream");

    $.get("/Home/GetIndexVids", function (data) {
        if (data.Ids.length == 0) {
            OutputMessage("No videos found.");
        }
        else
        {
            var amt_videos = data.Titles.length;

            for (var i = 0; i < amt_videos; i++) {
                OutputButtons(data.Ids[i], data.Titles[i], data.Dates[i], data.Users[i]);
            }
            init();
        }
    }, "json");

    function init()
    {
        $("button").click(function () {
            var title_str = $(this).text();

            var possible_ids = $(".vid-id");
            var possible_dates = $(".vid-date");
            var possible_users = $(".vid-user");

            var relative_idx = $(".appindexvidbtn").index($(this));
            var video = {
                Id: $(possible_ids[relative_idx]).text(),
                Title: title_str,
                Date: $(possible_dates[relative_idx]).text(),
                User: $(possible_users[relative_idx]).text()
            };
           
            var url = '/Stream/StreamView?id='+video.Id;
            window.location.replace(url);
        });
    }
});