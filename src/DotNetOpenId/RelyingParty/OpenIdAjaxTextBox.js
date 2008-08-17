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

function findParentForm(element) {
	if (element == null || element.nodeName == "FORM") {
		return element;
	}

	return findParentForm(element.parentNode);
}

function findOrCreateHiddenField(form, name) {
	if (form.elements[name]) {
		return form.elements[name];
	}

	var element = document.createElement('input');
	element.setAttribute("name", name);
	element.setAttribute("type", "hidden");
	form.appendChild(element);
	return element;
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

	// stick the result in a hidden field so the RP can verify it (positive or negative)
	var form = findParentForm(openIdBox);
	var hiddenField = findOrCreateHiddenField(form, "openidAuthData");
	hiddenField.setAttribute("value", resultUri.queryString);
	if (hiddenField.parentNode == null) {
		form.appendChild(hiddenField);
	}

	if (isAuthSuccessful(resultUri)) {
		// visual cue that auth was successful
		openIdBox.style.backgroundColor = 'lightgreen';
	} else {
		// visual cue that auth failed
		openIdBox.style.backgroundColor = 'pink';
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
		this.queryString = url.substr(queryBeginsAt + 1);
		var queryStringPairs = this.queryString.split('&');

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

