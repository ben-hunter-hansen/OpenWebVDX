
var FORM_DATA = undefined;

//--Test variables for web socket
var upload_file = undefined;
var upload_filename = "";

// getElementById
function $id(id) {
    return document.getElementById(id);
}

//
// output information
function Output(msg) {
    var m = $id("messages");
    m.innerHTML = msg;
}

// call initialization file
if (window.File && window.FileList && window.FileReader) {
    Init();
}

//
// initialize
function Init() {

    var fileselect = $id("fileselect"),
		filedrag = $id("filedrag"),
		submitbutton = $id("submitbutton");

    // file select
    fileselect.addEventListener("change", FileSelectHandler, false);

    // is XHR2 available?
    var xhr = new XMLHttpRequest();
    if (xhr.upload) {

        // file drop
        filedrag.addEventListener("dragover", FileDragHover, false);
        filedrag.addEventListener("dragleave", FileDragHover, false);
        filedrag.addEventListener("drop", FileSelectHandler, false);
        filedrag.style.display = "block";
    }

}

// file drag hover
function FileDragHover(e) {
    e.stopPropagation();
    e.preventDefault();
    e.target.className = (e.type == "dragover" ? "hover" : "");
}

// file selection
function FileSelectHandler(e) {

    // cancel event and hover styling
    FileDragHover(e);

    // fetch FileList object
    var files = e.target.files || e.dataTransfer.files;

    // process all File objects
    var formData = new FormData();
    for (var i = 0, f; f = files[i]; i++) {
        formData.append(f.name, f);
        upload_file = f;
    }

    FORM_DATA = formData;
    $("#filedrag").hide();
    $("#vid_select_btn").hide();
    $("#name_container").show();
    $("#upload_btn").show();
    ParseFile(files[0]);
}

function ParseFile(file) {
    
    Output(
		"<p>Selected Video: <strong>" + file.name +
		"</strong></p>"
	);
}

function isNamed(input) {
    var char_count = 0;
    var input_str = input;

    while (char_count < input_str.length) {
        char_count++;
        if (input_str.charAt(char_count) == '+') {
            return "Invalid charachter '+'";
        }
    }

    if (char_count == 0) {
        return "Name must not be empty";
    }else if (char_count < 5) {
        return "Name must be at least 5 characters";
    } else {
        console.log("returning true..");
        return true;
    }
}

$("#video_name").focus(function () {
    $(this).find("input").css("color", "#68A1EC");
});

function SocketConnect(data, name, title, type) {
    var videoSocket = new WebSocket("wss://" + window.location.hostname + "/FileUpload/SocketUpload?name="+name+"&title="+title+"&type="+type);
    
    var bytes = data.size;
    var kb = bytes / 1024;
    var mb = Math.floor(kb / 1024);

    videoSocket.onopen = function () {
        $("#upload_btn").hide();
        $("#video_name").val("");
        $("#name_container").hide();
        $("#loading").show();
        videoSocket.send(data);
    };

    videoSocket.onmessage = function (event) {
        var cur_bytes = event.data;
        var perc_compl = Math.floor(cur_bytes / mb * 100);

        $("#messages").text(cur_bytes + " of " + mb + " MB uploaded. ( " + perc_compl + "% )");
    };

    videoSocket.onclose = function (event) {
        $("#messages").text(event.reason);
        $("#loading").hide();
        $("#filedrag").show();
        $("#vid_select_btn").show();
    }
    videoSocket.onerror = function () {
        $("#messages").text("Upload Failed: Web Socket Error.");
    }
}

$("#upload_btn").click(function () {
    var nameInput = $("#video_name");
    var validationResult = isNamed($(nameInput).val());
    if (typeof validationResult == "string") {
        $(nameInput).val("");
        $(nameInput).animate({ "opacity": "0.3" }, "slow", function () {
            $(nameInput).animate({ "opacity": "1.0" }, "slow");
        });
        $(nameInput).attr("placeholder", validationResult);

    } else if(validationResult === true) {
        var xhr = new XMLHttpRequest();
        var vid_name = $id("video_name");
        console.log(vid_name.value);

        try {
            FORM_DATA.append(vid_name.name,vid_name.value);
        } catch (e) {
            console.log(e);
        }

        SocketConnect(upload_file, upload_file.name, vid_name.value, upload_file.type);
    } 
});

$(document).ready(function () {
    $('html, body').animate({ scrollTop: $(document).height() }, 'slow');
    $("#vid_select_btn").click(function () {
        $("#fileselect").trigger("click");
    });
});