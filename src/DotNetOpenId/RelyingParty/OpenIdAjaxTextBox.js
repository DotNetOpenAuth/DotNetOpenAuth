function trace(msg) {
	//alert(msg);
	//window.status = msg;
}

function constructButton(box, text, tooltip, onclick) {
	var button = document.createElement('button');
	button.textContent = text; // Mozilla
	button.value = text; // IE
	button.title = tooltip;
	button.onclick = onclick;
	button.style.visibility = 'hidden';
	button.style.position = 'absolute';
	button.style.padding = "0px";
	button.style.fontSize = '8px';
	button.style.top = "1px";
	button.style.bottom = "1px";
	button.style.right = "2px";
	box.parentNode.appendChild(button);
	return button;
}

function constructIcon(box, imageUrl, tooltip) {
	var icon = document.createElement('img');
	icon.src = imageUrl;
	icon.title = tooltip;
	icon.style.visibility = 'hidden';
	icon.style.position = 'absolute';
	icon.style.top = "2px";
	icon.style.right = "2px";
	box.parentNode.appendChild(icon);
	return icon;
}

function initAjaxOpenId(box, dotnetopenid_logo_url, spinner_url, timeout) {
	box.originalBackground = box.style.background;
	box.timeout = timeout;

	box.loginButton = constructButton(box, "LOG IN", "Click here to log in using a pop-up window.", function() {
		box.popup = window.open(getAuthenticationUrl(), 'opLogin', 'status=0,toolbar=0,resizable=1,scrollbars=1,width=800,height=600');
		self.waiting_openidBox = box;
		return false;
	});
	box.retryButton = constructButton(box, "RETRY", "Retry a failed identifier discovery.", function() {
		box.timeout += 5000; // give the retry attempt 5s longer than the last attempt
		box.performDiscovery();
		return false;
	});
	box.spinner = constructIcon(box, spinner_url, "Discovering/authenticating");

	// pre-fetch the DNOI icon
	var prefetchImage = document.createElement('img');
	prefetchImage.src = dotnetopenid_logo_url;
	prefetchImage.style.display = 'none';
	box.parentNode.appendChild(prefetchImage);

	box.setVisualCue = function(state) {
		box.spinner.style.visibility = 'hidden';
		box.loginButton.style.visibility = 'hidden';
		box.retryButton.style.visibility = 'hidden';
		box.title = null;
		if (state == "discovering") {
			box.style.background = 'url(' + dotnetopenid_logo_url + ') no-repeat';
			box.spinner.style.visibility = 'visible';
			box.title = null;
			window.status = "Discovering OpenID Identifier '" + box.value + "'...";
		} else if (state == "authenticated") {
			box.style.background = box.originalBackground;
			box.style.backgroundColor = 'lightgreen';
			box.title = box.claimedIdentifier;
			window.status = "Authenticated as " + box.value;
		} else if (state == "setup") {
			box.style.background = box.originalBackground;
			box.style.backgroundColor = 'pink';
			box.loginButton.style.visibility = 'visible';
			window.status = "Authentication requires setup.";
		} else if (state == "failed") {
			box.style.background = box.originalBackground;
			box.style.backgroundColor = 'pink';
			box.retryButton.style.visibility = 'visible';
			window.status = "Authentication failed.";
			box.title = "Authentication failed.";
		} else if (state = '' || state == null) {
			box.style.background = box.originalBackground;
			box.title = null;
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
		box.closeDiscoveryIFrame();
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
		if (box.hiddenField) {
			return box.hiddenField;
		}

		box.hiddenField = document.createElement('input');
		box.hiddenField.setAttribute("name", name);
		box.hiddenField.setAttribute("type", "hidden");
		form.appendChild(box.hiddenField);
		return box.hiddenField;
	}

	function createHiddenFrame(url) {
		var iframe = document.createElement("iframe");
		iframe.setAttribute("width", 0);
		iframe.setAttribute("height", 0);
		iframe.setAttribute("style", "display: none");
		iframe.setAttribute("src", url);
		iframe.openidBox = box;
		box.parentNode.insertBefore(iframe, box);
		box.discoveryTimeout = setTimeout(function() { trace("timeout"); box.openidDiscoveryFailure("Timed out"); }, box.timeout);
		return iframe;
	}

	this.parentForm = findParentForm(box);

	box.openidDiscoveryFailure = function(msg) {
		box.closeDiscoveryIFrame();
		trace('Discovery failure: ' + msg);
		box.setVisualCue('failed');
		box.title = msg;
	}

	box.closeDiscoveryIFrame = function() {
		if (box.discoveryTimeout) {
			clearTimeout(box.discoveryTimeout);
		}
		if (box.discoveryIFrame) {
			box.discoveryIFrame.parentNode.removeChild(box.discoveryIFrame);
			box.discoveryIFrame = null;
		}
	}

	box.openidAuthResult = function(resultUrl) {
		self.waiting_openidBox = null;
		trace('openidAuthResult ' + resultUrl);
		if (box.discoveryIFrame) {
			box.closeDiscoveryIFrame();
		} else if (box.popup) {
			box.popup.close();
			box.popup = null;
		}
		var resultUri = new Uri(resultUrl);

		// stick the result in a hidden field so the RP can verify it (positive or negative)
		var form = findParentForm(box);
		var hiddenField = findOrCreateHiddenField(form, "openidAuthData");
		hiddenField.setAttribute("value", resultUri.toString());
		trace("set openidAuthData = " + resultUri.queryString);
		if (hiddenField.parentNode == null) {
			form.appendChild(hiddenField);
		}
		trace("review: " + box.hiddenField.value);

		if (isAuthSuccessful(resultUri)) {
			// visual cue that auth was successful
			box.claimedIdentifier = isOpenID2Response(resultUri) ? resultUri.getQueryArgValue("openid.claimed_id") : resultUri.getQueryArgValue("openid.identity");
			box.setVisualCue('authenticated');
			box.lastAuthenticationResult = 'authenticated';
		} else {
			// visual cue that auth failed
			box.setVisualCue('setup');
			box.lastAuthenticationResult = 'setup';
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
			if (box.value.length > 0) {
				box.performDiscovery();
			} else {
				box.setVisualCue();
			}
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

