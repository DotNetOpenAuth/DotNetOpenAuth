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
	var splitResult = result.split(' ');
	var immediateUrl = splitResult[0];
	var setupUrl = splitResult[1];
	
	//    statusupdates.innerHTML += "Attempting authentication... " + escape(result) + "<br/>";
	iframe = document.createElement("iframe");
	iframe.setAttribute("width", 0);
	iframe.setAttribute("height", 0);
	iframe.setAttribute("style", "display: none");
	iframe.setAttribute("src", immediateUrl);
	openIdBox.parentNode.insertBefore(iframe, openIdBox);
}

function openidAuthResult(resultUrl) {
	iframe.parentNode.removeChild(iframe);
	var resultUri = new Uri(resultUrl);
	if (isAuthSuccessful(resultUri)) {
		alert('finished auth successfully');
	} else {
		alert('auth failed.');
	}
	//    statusupdates.innerHTML += "auth result: " + escape(resultUrl) + "<br/>";
}

function isAuthSuccessful(resultUri) {
	if (isOpenID2Response(resultUri)) {
		return resultUri.getQueryArgValue("openid.mode") == "id_res";
	} else {
		return resultUri.getQueryArgValue("openid.mode") == "id_res" && !resultUri.containsQueryArg("openid.user_setup_url");
	}
}

function isOpenID2Response(resultUri) {
	return resultUri.containsQueryArg("openid.ns");
}

function Uri(url) {
	function KeyValuePair(key, value) {
		this.key = key;
		this.value = value;
	}

	this.Pairs = Array();

	var queryBeginsAt = url.indexOf('?');
	if (queryBeginsAt >= 0) {
		var queryString = url.substr(queryBeginsAt + 1);
		var queryStringPairs = queryString.split('&');

		for (var i = 0; i < queryStringPairs.length; i++) {
			var pair = queryStringPairs[i].split('=');
			this.Pairs.push(new KeyValuePair(unescape(pair[0]), unescape(pair[1])))
		}
	}

	this.getQueryArgValue = function(key) {
		for(var i = 0; i < this.Pairs.length; i++) {
			if (this.Pairs[i].key == key) {
				return this.Pairs[i].value;
			}
		}
	}
	
	this.containsQueryArg = function(key) {
		return this.getQueryArgValue(key);
	}

	return this;
}

