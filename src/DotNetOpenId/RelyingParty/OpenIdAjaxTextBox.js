var openIdBox;
var discoveryIFrame;

function trace(msg) {
//	alert(msg);
}

function ajaxOnLoad() {
	openIdBox = document.getElementsByName("openid_identifier")[0];
	openIdBox.onchange = function(event) {
		if (openIdBox.oldvalue != openIdBox.value) {
			performDiscovery();
			openIdBox.oldvalue = openIdBox.value;
		}
		return true;
	}
	openIdBox.onkeyup = function(event) {
		if (openIdBox.oldvalue != openIdBox.value) {
			visualCueClear();
		}
		return true;
	}
	openIdBox.onblur = openIdBox.onchange;
}

function performDiscovery() {
	visualCueClear();
	var frameLocation = new Uri(document.location.href);
	var discoveryUri = frameLocation.trimQueryAndFragment().toString() + '?' + 'dotnetopenid.userSuppliedIdentifier=' + escape(openIdBox.value);
	if (discoveryIFrame) {
		discoveryIFrame.parentNode.removeChild(discoveryIFrame);
		discoveryIFrame = null;
	}
	trace('Performing discovery using url: ' + discoveryUri);
	discoveryIFrame = createHiddenFrame(discoveryUri);
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

function createHiddenFrame(url) {
	var iframe = document.createElement("iframe");
	iframe.setAttribute("width", 0);
	iframe.setAttribute("height", 0);
	iframe.setAttribute("style", "display: none");
	iframe.setAttribute("src", url);
	openIdBox.parentNode.insertBefore(iframe, openIdBox);
	return iframe;
}

function openidDiscoveryFailure(msg) {
	trace('Discovery failure: ' + msg);
}

function openidAuthResult(resultUrl) {
	discoveryIFrame.parentNode.removeChild(discoveryIFrame);
	discoveryIFrame = null;
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
		visualCueSuccess();
	} else {
		// visual cue that auth failed
		visualCueFailure();
	}
	//    statusupdates.innerHTML += "auth result: " + escape(resultUrl) + "<br/>";
}

function visualCueSuccess() {
	openIdBox.style.backgroundColor = 'lightgreen';
}
function visualCueFailure() {
	openIdBox.style.backgroundColor = 'pink';
}
function visualCueClear() {
	openIdBox.style.backgroundColor = '';
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
	this.originalUri = url;

	this.toString = function() {
		return this.originalUri;
	}

	this.trimQueryAndFragment = function() {
		var qmark = this.originalUri.indexOf('?');
		var hashmark = this.originalUri.indexOf('#');
		if (qmark < 0) { qmark = this.originalUri.length; }
		if (hashmark < 0) { hashmark = this.originalUri.length; }
		return new Uri(this.originalUri.substr(0, Math.min(qmark, hashmark)));
	}

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

	this.indexOf = function(args) {
		return this.originalUri.indexOf(args);
	}

	return this;
}

