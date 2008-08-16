var openIdBox;
var statusupdates;
var iframe;

function ajaxOnLoad() {
	openIdBox = document.getElementsByName("openid_identifier")[0];
	//    statusupdates = document.getElementById("statusupdates");
	openIdBox.onchange = function(event) {
		if (openIdBox.oldvalue != openIdBox.value) {
			//            statusupdates.innerHTML += "Performing discovery...<br/>";
			performDiscovery();
			openIdBox.oldvalue = openIdBox.value;
		}
		return true;
	}
	openIdBox.onblur = openIdBox.onchange;
}

function discoveryResult(result) {
	//    statusupdates.innerHTML += "Attempting authentication... " + escape(result) + "<br/>";
	iframe = document.createElement("iframe");
	iframe.setAttribute("width", 0);
	iframe.setAttribute("height", 0);
	iframe.setAttribute("style", "display: none");
	iframe.setAttribute("src", result);
	openIdBox.parentNode.insertBefore(iframe, openIdBox);
}

function openidAuthResult(resultUrl) {
	iframe.parentNode.removeChild(iframe);
	alert('finished auth');
	//    statusupdates.innerHTML += "auth result: " + escape(resultUrl) + "<br/>";
}
