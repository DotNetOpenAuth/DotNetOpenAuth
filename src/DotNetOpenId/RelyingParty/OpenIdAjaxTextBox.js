function trace(msg) {
	//alert(msg);
	//window.status = msg;
}

function initAjaxOpenId(box, dotnetopenid_logo_url, spinner_url, timeout, assertionReceivedCode,
		loginButtonText, loginButtonToolTip, retryButtonText, retryButtonToolTip, busyToolTip,
		identifierRequiredMessage, loginInProgressMessage) {
	box.dnoi_internal = new Object();
	if (assertionReceivedCode) {
		box.dnoi_internal.onauthenticated = function(sender, e) { eval(assertionReceivedCode); }
	}

	box.dnoi_internal.originalBackground = box.style.background;
	box.timeout = timeout;

	box.dnoi_internal.constructButton = function(text, tooltip, onclick) {
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

	box.dnoi_internal.constructIcon = function(imageUrl, tooltip) {
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

	box.dnoi_internal.prefetchImage = function(imageUrl) {
		var img = document.createElement('img');
		img.src = imageUrl;
		img.style.display = 'none';
		box.parentNode.appendChild(img);
		return img;
	}

	box.dnoi_internal.loginButton = box.dnoi_internal.constructButton(loginButtonText, loginButtonToolTip, function() {
		box.dnoi_internal.popup = window.open(box.dnoi_internal.getAuthenticationUrl(), 'opLogin', 'status=0,toolbar=0,resizable=1,scrollbars=1,width=800,height=600');
		self.waiting_openidBox = box;
		return false;
	});
	box.dnoi_internal.retryButton = box.dnoi_internal.constructButton(retryButtonText, retryButtonToolTip, function() {
		box.timeout += 5000; // give the retry attempt 5s longer than the last attempt
		box.dnoi_internal.performDiscovery();
		return false;
	});
	box.dnoi_internal.spinner = box.dnoi_internal.constructIcon(spinner_url, busyToolTip);
	box.dnoi_internal.prefetchImage(dotnetopenid_logo_url);

	box.dnoi_internal.setVisualCue = function(state) {
		box.dnoi_internal.spinner.style.visibility = 'hidden';
		box.dnoi_internal.loginButton.style.visibility = 'hidden';
		box.dnoi_internal.retryButton.style.visibility = 'hidden';
		box.title = null;
		if (state == "discovering") {
			box.style.background = 'url(' + dotnetopenid_logo_url + ') no-repeat';
			box.dnoi_internal.spinner.style.visibility = 'visible';
			box.dnoi_internal.claimedIdentifier = null;
			box.title = null;
			window.status = "Discovering OpenID Identifier '" + box.value + "'...";
		} else if (state == "authenticated") {
			var opLogo = box.dnoi_internal.deriveOPFavIcon();
			if (opLogo) {
				box.style.background = 'url(' + opLogo + ') no-repeat';
			} else {
				box.style.background = box.dnoi_internal.originalBackground;
			}
			box.style.backgroundColor = 'lightgreen';
			box.title = box.dnoi_internal.claimedIdentifier;
			window.status = "Authenticated as " + box.value;
		} else if (state == "setup") {
			var opLogo = box.dnoi_internal.deriveOPFavIcon();
			if (opLogo) {
				box.style.background = 'url(' + opLogo + ') no-repeat';
			} else {
				box.style.background = box.dnoi_internal.originalBackground;
			}
			box.style.backgroundColor = 'pink';
			box.dnoi_internal.loginButton.style.visibility = 'visible';
			box.dnoi_internal.claimedIdentifier = null;
			window.status = "Authentication requires setup.";
		} else if (state == "failed") {
			box.style.background = box.dnoi_internal.originalBackground;
			box.style.backgroundColor = 'pink';
			box.dnoi_internal.retryButton.style.visibility = 'visible';
			box.dnoi_internal.claimedIdentifier = null;
			window.status = "Authentication failed.";
			box.title = "Authentication failed.";
		} else if (state == 'required') {
			box.style.background = box.dnoi_internal.originalBackground;
			box.style.backgroundColor = 'pink';
			box.dnoi_internal.claimedIdentifier = null;
			box.title = "This field is required.";
		} else if (state = '' || state == null) {
			box.style.background = box.dnoi_internal.originalBackground;
			box.title = null;
			box.dnoi_internal.claimedIdentifier = null;
			window.status = null;
		} else {
			box.dnoi_internal.claimedIdentifier = null;
			trace('unrecognized state ' + state);
		}
	}

	box.dnoi_internal.isBusy = function() {
		return box.discoveryIFrame != null;
	};

	box.dnoi_internal.onSubmit = function() {
		if (box.lastAuthenticationResult != 'authenticated') {
			if (box.dnoi_internal.isBusy()) {
				alert(loginInProgressMessage);
			} else {
				if (box.value.length > 0) {
					alert(identifierRequiredMessage);
				} else {
					box.dnoi_internal.setVisualCue('required');
				}
			}
			return false;
		}
		return true;
	}

	box.dnoi_internal.getAuthenticationUrl = function(immediateMode) {
		var frameLocation = new Uri(document.location.href);
		var discoveryUri = frameLocation.trimQueryAndFragment().toString() + '?' + 'dotnetopenid.userSuppliedIdentifier=' + escape(box.value);
		if (immediateMode) {
			discoveryUri += "&dotnetopenid.immediate=true";
		}
		return discoveryUri;
	};

	box.dnoi_internal.performDiscovery = function() {
		box.dnoi_internal.closeDiscoveryIFrame();
		box.dnoi_internal.setVisualCue('discovering');
		box.lastDiscoveredIdentifier = box.value;
		box.lastAuthenticationResult = null;
		var discoveryUri = box.dnoi_internal.getAuthenticationUrl(true);
		if (box.discoveryIFrame) {
			box.discoveryIFrame.parentNode.removeChild(box.discoveryIFrame);
			box.discoveryIFrame = null;
		}
		trace('Performing discovery using url: ' + discoveryUri);
		box.discoveryIFrame = createHiddenFrame(discoveryUri);
	};

	function findParentForm(element) {
		if (element == null || element.nodeName == "FORM") {
			return element;
		}

		return findParentForm(element.parentNode);
	};

	function findOrCreateHiddenField(form, name) {
		if (box.hiddenField) {
			return box.hiddenField;
		}

		box.hiddenField = document.createElement('input');
		box.hiddenField.setAttribute("name", name);
		box.hiddenField.setAttribute("type", "hidden");
		form.appendChild(box.hiddenField);
		return box.hiddenField;
	};

	box.dnoi_internal.deriveOPFavIcon = function() {
		if (!box.hiddenField) return;
		var authResult = new Uri(box.hiddenField.value);
		var opUri;
		if (authResult.getQueryArgValue("openid.op_endpoint")) {
			opUri = new Uri(authResult.getQueryArgValue("openid.op_endpoint"));
		} else if (authResult.getQueryArgValue("openid.user_setup_url")) {
			opUri = new Uri(authResult.getQueryArgValue("openid.user_setup_url"));
		} else return null;
		var favicon = opUri.getAuthority() + "/favicon.ico";
		return favicon;
	};

	function createHiddenFrame(url) {
		var iframe = document.createElement("iframe");
		iframe.setAttribute("width", 0);
		iframe.setAttribute("height", 0);
		iframe.setAttribute("style", "display: none");
		iframe.setAttribute("src", url);
		iframe.openidBox = box;
		box.parentNode.insertBefore(iframe, box);
		box.discoveryTimeout = setTimeout(function() { trace("timeout"); box.dnoi_internal.openidDiscoveryFailure("Timed out"); }, box.timeout);
		return iframe;
	};

	this.parentForm = findParentForm(box);

	box.dnoi_internal.openidDiscoveryFailure = function(msg) {
		box.dnoi_internal.closeDiscoveryIFrame();
		trace('Discovery failure: ' + msg);
		box.lastAuthenticationResult = 'failed';
		box.dnoi_internal.setVisualCue('failed');
		box.title = msg;
	};

	box.dnoi_internal.closeDiscoveryIFrame = function() {
		if (box.discoveryTimeout) {
			clearTimeout(box.discoveryTimeout);
		}
		if (box.discoveryIFrame) {
			box.discoveryIFrame.parentNode.removeChild(box.discoveryIFrame);
			box.discoveryIFrame = null;
		}
	};

	box.dnoi_internal.openidAuthResult = function(resultUrl) {
		self.waiting_openidBox = null;
		trace('openidAuthResult ' + resultUrl);
		if (box.discoveryIFrame) {
			box.dnoi_internal.closeDiscoveryIFrame();
		} else if (box.dnoi_internal.popup) {
			box.dnoi_internal.popup.close();
			box.dnoi_internal.popup = null;
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
			box.dnoi_internal.claimedIdentifier = isOpenID2Response(resultUri) ? resultUri.getQueryArgValue("openid.claimed_id") : resultUri.getQueryArgValue("openid.identity");
			box.dnoi_internal.setVisualCue('authenticated');
			box.lastAuthenticationResult = 'authenticated';
			if (box.dnoi_internal.onauthenticated) {
				box.dnoi_internal.onauthenticated(box);
			}
		} else {
			// visual cue that auth failed
			box.dnoi_internal.setVisualCue('setup');
			box.lastAuthenticationResult = 'setup';
		}
	};

	function isAuthSuccessful(resultUri) {
		if (isOpenID2Response(resultUri)) {
			return resultUri.getQueryArgValue("openid.mode") == "id_res";
		} else {
			return resultUri.getQueryArgValue("openid.mode") == "id_res" && !resultUri.containsQueryArg("openid.user_setup_url");
		}
	};

	function isOpenID2Response(resultUri) {
		return resultUri.containsQueryArg("openid.ns");
	};

	box.onblur = function(event) {
		if (box.lastDiscoveredIdentifier != box.value) {
			if (box.value.length > 0) {
				box.dnoi_internal.performDiscovery();
			} else {
				box.dnoi_internal.setVisualCue();
			}
			box.oldvalue = box.value;
		}
		return true;
	};
	box.onkeyup = function(event) {
		if (box.lastDiscoveredIdentifier != box.value) {
			box.dnoi_internal.setVisualCue();
		} else {
			box.dnoi_internal.setVisualCue(box.lastAuthenticationResult);
		}
		return true;
	};
	box.getClaimedIdentifier = function() { return box.dnoi_internal.claimedIdentifier; };
}

function Uri(url) {
	this.originalUri = url;

	this.toString = function() {
		return this.originalUri;
	};

	this.getAuthority = function() {
		var authority = this.getScheme() + "://" + this.getHost();
		return authority;
	}

	this.getHost = function() {
		var hostStartIdx = this.originalUri.indexOf("://") + 3;
		var hostEndIndex = this.originalUri.indexOf("/", hostStartIdx);
		if (hostEndIndex < 0) hostEndIndex = this.originalUri.length;
		var host = this.originalUri.substr(hostStartIdx, hostEndIndex - hostStartIdx);
		return host;
	}

	this.getScheme = function() {
		var schemeStartIdx = this.indexOf("://");
		return this.originalUri.substr(this.originalUri, schemeStartIdx);
	}

	this.trimQueryAndFragment = function() {
		var qmark = this.originalUri.indexOf('?');
		var hashmark = this.originalUri.indexOf('#');
		if (qmark < 0) { qmark = this.originalUri.length; }
		if (hashmark < 0) { hashmark = this.originalUri.length; }
		return new Uri(this.originalUri.substr(0, Math.min(qmark, hashmark)));
	};

	function KeyValuePair(key, value) {
		this.key = key;
		this.value = value;
	};

	this.Pairs = Array();

	var queryBeginsAt = this.originalUri.indexOf('?');
	if (queryBeginsAt >= 0) {
		this.queryString = url.substr(queryBeginsAt + 1);
		var queryStringPairs = this.queryString.split('&');

		for (var i = 0; i < queryStringPairs.length; i++) {
			var pair = queryStringPairs[i].split('=');
			this.Pairs.push(new KeyValuePair(unescape(pair[0]), unescape(pair[1])))
		}
	};

	this.getQueryArgValue = function(key) {
		for (var i = 0; i < this.Pairs.length; i++) {
			if (this.Pairs[i].key == key) {
				return this.Pairs[i].value;
			}
		}
	};

	this.containsQueryArg = function(key) {
		return this.getQueryArgValue(key);
	};

	this.indexOf = function(args) {
		return this.originalUri.indexOf(args);
	};

	return this;
};
