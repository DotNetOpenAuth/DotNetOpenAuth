function trace(msg) {
	//alert(msg);
	window.status = msg;
}

function initAjaxOpenId(box) {
	box.originalBackground = box.style.background;

	// Construct the login button
	var loginButton = document.createElement('button');
	loginButton.type = "button";
	loginButton.textContent = "LOG IN";
	loginButton.title = "Click here to log in using a pop-up window."
	loginButton.onclick = function() {
		box.popup = window.open(getAuthenticationUrl(), 'opLogin', 'status=0,toolbar=0,resizable=1,scrollbars=1,width=800,height=600');
		self.waiting_openidBox = box;
		return false;
	};
	loginButton.style.visibility = 'hidden';
	loginButton.style.position = 'absolute';
	loginButton.style.padding = "0px";
	loginButton.style.fontSize = '8px';
	loginButton.style.top = "1px";
	loginButton.style.bottom = "1px";
	loginButton.style.right = "2px";
	box.parentNode.appendChild(loginButton);
	box.loginButton = loginButton;

	box.setVisualCue = function(state) {
		box.loginButton.style.visibility = 'hidden';
		if (state == "discovering") {
			box.style.background = 'url(' + dotnetopenid_logo_url + ') no-repeat';
			box.style.backgroundColor = 'yellow';
			window.status = "Discovering OpenID Identifier '" + box.value + "'...";
		} else if (state == "authenticated") {
			box.style.background = box.originalBackground;
			box.style.backgroundColor = 'lightgreen';
			window.status = "Authenticated as " + box.value;
		} else if (state == "failed") {
			box.style.background = box.originalBackground;
			box.style.backgroundColor = 'pink';
			box.loginButton.style.visibility = 'visible';
			window.status = "Authentication failed.";
		} else if (state = '' || state == null) {
			box.style.background = box.originalBackground;
			window.status = null;
		} else {
			trace('unrecognized state ' + state);
		}
	}

	this.getAuthenticationUrl = function(immediateMode) {
		var frameLocation = new Uri(document.location.href);
		var discoveryUri = frameLocation.trimQueryAndFragment().toString() + '?' + 'dotnetopenid.userSuppliedIdentifier=' + escape(box.value);
		if (immediateMode) {
			discoveryUri += "&dotnetopenid.immediate=true";
		}
		return discoveryUri;
	}

	box.performDiscovery = function() {
		box.setVisualCue('discovering');
		box.lastDiscoveredIdentifier = box.value;
		box.lastAuthenticationResult = null;
		var discoveryUri = getAuthenticationUrl(true);
		if (box.discoveryIFrame) {
			box.discoveryIFrame.parentNode.removeChild(box.discoveryIFrame);
			box.discoveryIFrame = null;
		}
		trace('Performing discovery using url: ' + discoveryUri);
		box.discoveryIFrame = createHiddenFrame(discoveryUri);
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
		iframe.openidBox = box;
		box.parentNode.insertBefore(iframe, box);
		return iframe;
	}

	this.parentForm = findParentForm(box);

	box.openidDiscoveryFailure = function(msg) {
		trace('Discovery failure: ' + msg);
	}

	box.openidAuthResult = function(resultUrl) {
		self.waiting_openidBox = null;
		trace('openidAuthResult ' + resultUrl);
		if (box.discoveryIFrame) {
			box.discoveryIFrame.parentNode.removeChild(box.discoveryIFrame);
			box.discoveryIFrame = null;
		} else if (box.popup) {
			box.popup.close();
			box.popup = null;
		}
		var resultUri = new Uri(resultUrl);

		// stick the result in a hidden field so the RP can verify it (positive or negative)
		var form = findParentForm(box);
		var hiddenField = findOrCreateHiddenField(form, "openidAuthData");
		hiddenField.setAttribute("value", resultUri.queryString);
		if (hiddenField.parentNode == null) {
			form.appendChild(hiddenField);
		}

		if (isAuthSuccessful(resultUri)) {
			// visual cue that auth was successful
			box.setVisualCue('authenticated');
			box.lastAuthenticationResult = 'authenticated';
		} else {
			// visual cue that auth failed
			box.setVisualCue('failed');
			box.lastAuthenticationResult = 'failed';
		}
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

	box.onblur = function(event) {
		if (box.lastDiscoveredIdentifier != box.value) {
			box.performDiscovery();
			box.oldvalue = box.value;
		}
		return true;
	}
	box.onkeyup = function(event) {
		if (box.lastDiscoveredIdentifier != box.value) {
			box.setVisualCue();
		} else {
			box.setVisualCue(box.lastAuthenticationResult);
		}
		return true;
	}
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
		for (var i = 0; i < this.Pairs.length; i++) {
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

