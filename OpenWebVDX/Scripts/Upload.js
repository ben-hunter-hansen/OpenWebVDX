

// getElementById
function $id(id) {
    return document.getElementById(id);
}

//
// output information
function Output(msg) {
    var m = $id("messages");
    m.innerHTML = msg + m.innerHTML;
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

        // remove submit button
        submitbutton.style.display = "none";
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
    }

    var xhr = new XMLHttpRequest();
    xhr.open('POST', 'UploadRequest');
    xhr.send(formData);
    $("#filedrag").hide();
    $("#vid_select_btn").hide();
    $("#loading").show();
    ParseFile(files[0]);
    xhr.onreadystatechange = function () {
        if (xhr.readyState == 4 && xhr.status == 200) {
            $("#loading").hide();
            $("#filedrag").show();
            $("#messages").text("Upload Complete.");
            $("#vid_select_btn").show();
        }
    }

}

function ParseFile(file) {
    
    Output(
		"<p>Selected Video: <strong>" + file.name +
		"</strong></p>"
	);
}

$(document).ready(function () {
    $('html, body').animate({ scrollTop: $(document).height() }, 'slow');
    $("#vid_select_btn").click(function () {
        $("#fileselect").trigger("click");
    });
});