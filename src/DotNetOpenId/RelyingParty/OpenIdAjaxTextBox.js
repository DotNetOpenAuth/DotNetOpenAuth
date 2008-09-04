function trace(msg) {
	//alert(msg);
	//window.status = msg;
}

function initAjaxOpenId(box, openid_logo_url, dotnetopenid_logo_url, spinner_url, success_icon_url, failure_icon_url,
		timeout, assertionReceivedCode,
		loginButtonText, loginButtonToolTip, retryButtonText, retryButtonToolTip, busyToolTip,
		identifierRequiredMessage, loginInProgressMessage,
		authenticationSucceededToolTip, authenticationFailedToolTip) {
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

	box.dnoi_internal.constructIcon = function(imageUrl, tooltip, rightSide, visible, height) {
		var icon = document.createElement('img');
		icon.src = imageUrl;
		icon.title = tooltip != null ? tooltip : '';
		if (!visible) {
			icon.style.visibility = 'hidden';
		}
		icon.style.position = 'absolute';
		icon.style.top = "2px";
		icon.style.bottom = "2px"; // for FireFox (and IE7, I think)
		if (height) {
			icon.style.height = height; // for Chrome and IE8
		}
		if (rightSide) {
			icon.style.right = "2px";
		} else {
			icon.style.left = "2px";
		}
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
	box.dnoi_internal.openid_logo = box.dnoi_internal.constructIcon(openid_logo_url, null, false, true);
	box.dnoi_internal.op_logo = box.dnoi_internal.constructIcon('', null, false, false, "16px");
	box.dnoi_internal.spinner = box.dnoi_internal.constructIcon(spinner_url, busyToolTip, true);
	box.dnoi_internal.success_icon = box.dnoi_internal.constructIcon(success_icon_url, authenticationSucceededToolTip, true);
	box.dnoi_internal.failure_icon = box.dnoi_internal.constructIcon(failure_icon_url, authenticationFailedToolTip, true);

	// Disable the display of the DotNetOpenId logo
	//box.dnoi_internal.dnoi_logo = box.dnoi_internal.constructIcon(dotnetopenid_logo_url);
	box.dnoi_internal.dnoi_logo = box.dnoi_internal.openid_logo;

	box.dnoi_internal.setVisualCue = function(state) {
		box.dnoi_internal.openid_logo.style.visibility = 'hidden';
		box.dnoi_internal.dnoi_logo.style.visibility = 'hidden';
		box.dnoi_internal.op_logo.style.visibility = 'hidden';
		box.dnoi_internal.spinner.style.visibility = 'hidden';
		box.dnoi_internal.success_icon.style.visibility = 'hidden';
		box.dnoi_internal.failure_icon.style.visibility = 'hidden';
		box.dnoi_internal.loginButton.style.visibility = 'hidden';
		box.dnoi_internal.retryButton.style.visibility = 'hidden';
		box.title = null;
		if (state == "discovering") {
			box.dnoi_internal.dnoi_logo.style.visibility = 'visible';
			box.dnoi_internal.spinner.style.visibility = 'visible';
			box.dnoi_internal.claimedIdentifier = null;
			box.title = null;
			window.status = "Discovering OpenID Identifier '" + box.value + "'...";
		} else if (state == "authenticated") {
			var opLogo = box.dnoi_internal.deriveOPFavIcon();
			if (opLogo) {
				box.dnoi_internal.op_logo.src = opLogo;
				box.dnoi_internal.op_logo.style.visibility = 'visible';
			} else {
				box.dnoi_internal.openid_logo.style.visibility = 'visible';
			}
			box.dnoi_internal.success_icon.style.visibility = 'visible';
			box.title = box.dnoi_internal.claimedIdentifier;
			window.status = "Authenticated as " + box.value;
		} else if (state == "setup") {
			var opLogo = box.dnoi_internal.deriveOPFavIcon();
			if (opLogo) {
				box.dnoi_internal.op_logo.src = opLogo;
				box.dnoi_internal.op_logo.style.visibility = 'visible';
			} else {
				box.dnoi_internal.openid_logo.style.visibility = 'visible';
			}
			box.dnoi_internal.loginButton.style.visibility = 'visible';
			box.dnoi_internal.claimedIdentifier = null;
			window.status = "Authentication requires setup.";
		} else if (state == "failed") {
			box.dnoi_internal.openid_logo.style.visibility = 'visible';
			//box.dnoi_internal.failure_icon.style.visibility = 'visible';
			box.dnoi_internal.retryButton.style.visibility = 'visible';
			box.dnoi_internal.claimedIdentifier = null;
			window.status = authenticationFailedToolTip;
			box.title = authenticationFailedToolTip;
		} else if (state == 'required') {
			box.dnoi_internal.openid_logo.style.visibility = 'visible';
			box.dnoi_internal.failure_icon.style.visibility = 'visible';
			box.dnoi_internal.claimedIdentifier = null;
			box.title = identifierRequiredMessage;
		} else if (state = '' || state == null) {
			box.dnoi_internal.openid_logo.style.visibility = 'visible';
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

	box.dnoi_internal.blockForSetupCompletion = function() {
		// block until the popup window closes
		while (box.dnoi_internal.popup && !box.dnoi_internal.popup.closed);
	};

	box.dnoi_internal.onSubmit = function() {
		if (box.lastAuthenticationResult != 'authenticated') {
			if (box.dnoi_internal.isBusy()) {
				alert(loginInProgressMessage);
			} else {
				if (box.value.length > 0) {
					// submitPending will be true if we've already tried deferring submit for a login,
					// in which case we just want to display a box to the user.
					if (box.dnoi_internal.submitPending) {
						alert(identifierRequiredMessage);
					} else {
						// The user hasn't clicked "Login" yet.  We'll click login for him,
						// after leaving a note for ourselves to automatically click submit
						// when login is complete.
						box.dnoi_internal.submitPending = box.dnoi_internal.submitButtonJustClicked;
						if (box.dnoi_internal.submitPending == null) {
							box.dnoi_internal.submitPending = true;
						}
						box.dnoi_internal.loginButton.onclick();
						return false; // abort submit for now
					}
				} else {
					box.dnoi_internal.setVisualCue('required');
				}
			}
			return false;
		}
		return true;
	};

	box.dnoi_internal.setLastSubmitButtonClicked = function(evt) {
		var button;
		if (evt.target) {
			button = evt.target;
		} else {
			button = evt.srcElement;
		}

		box.dnoi_internal.submitButtonJustClicked = button;
	};

	// box.hookAllSubmitElements = function(searchNode) {
		var inputs = document.getElementsByTagName('input');
		for (var i = 0; i < inputs.length; i++) {
			var el = inputs[i];
			if (el.type == 'submit') {
				if (el.attachEvent) {
					el.attachEvent("onclick", box.dnoi_internal.setLastSubmitButtonClicked);
				} else {
					el.addEventListener("click", box.dnoi_internal.setLastSubmitButtonClicked, true);
				}
			}
		}
	//};

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

	box.parentForm = findParentForm(box);

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
			if (box.dnoi_internal.submitPending) {
				// We submit the form BEFORE resetting the submitPending so
				// the submit handler knows we've already tried this route.
				if (box.dnoi_internal.submitPending == true) {
					box.parentForm.submit();
				} else {
					box.dnoi_internal.submitPending.click();
				}
			}
		} else {
			// visual cue that auth failed
			box.dnoi_internal.setVisualCue('setup');
			box.lastAuthenticationResult = 'setup';
		}

		box.dnoi_internal.submitPending = null;
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
