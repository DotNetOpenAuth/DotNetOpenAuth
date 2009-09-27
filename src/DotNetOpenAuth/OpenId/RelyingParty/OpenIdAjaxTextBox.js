//-----------------------------------------------------------------------
// <copyright file="OpenIdAjaxTextBox.js" company="Andrew Arnott">
//     Copyright (c) Andrew Arnott. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

function initAjaxOpenId(box, openid_logo_url, dotnetopenid_logo_url, spinner_url, success_icon_url, failure_icon_url,
		throttle, timeout, assertionReceivedCode,
		loginButtonText, loginButtonToolTip, retryButtonText, retryButtonToolTip, busyToolTip,
		identifierRequiredMessage, loginInProgressMessage,
		authenticatedByToolTip, authenticatedAsToolTip, authenticationFailedToolTip,
		discoverCallback/*removeme*/, discoveryFailedCallback) {
	box.dnoi_internal = new Object();
	if (assertionReceivedCode) {
		box.dnoi_internal.onauthenticated = function(sender, e) { eval(assertionReceivedCode); }
	}

	box.dnoi_internal.originalBackground = box.style.background;
	box.timeout = timeout;

	box.dnoi_internal.authenticationIFrames = new window.dnoa_internal.FrameManager(throttle);

	box.dnoi_internal.constructButton = function(text, tooltip, onclick) {
		var button = document.createElement('input');
		button.textContent = text; // Mozilla
		button.value = text; // IE
		button.type = 'button';
		button.title = tooltip != null ? tooltip : '';
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
		icon.originalTitle = icon.title;
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

	function findParentForm(element) {
		if (element == null || element.nodeName == "FORM") {
			return element;
		}

		return findParentForm(element.parentNode);
	};

	box.parentForm = findParentForm(box);

	function findOrCreateHiddenField() {
		var name = box.name + '_openidAuthData';
		var existing = window.document.getElementsByName(name);
		if (existing && existing.length > 0) {
			return existing[0];
		}

		var hiddenField = document.createElement('input');
		hiddenField.setAttribute("name", name);
		hiddenField.setAttribute("type", "hidden");
		box.parentForm.appendChild(hiddenField);
		return hiddenField;
	};

	box.dnoi_internal.loginButton = box.dnoi_internal.constructButton(loginButtonText, loginButtonToolTip, function() {
		var discoveryInfo = window.dnoa_internal.discoveryResults[box.lastDiscoveredIdentifier];
		if (discoveryInfo == null) {
			trace('Ooops!  Somehow the login button click event was invoked, but no openid discovery information for ' + box.lastDiscoveredIdentifier + ' is available.');
			return;
		}
		// The login button always sends a setup message to the first OP.
		var selectedProvider = discoveryInfo[0];
		selectedProvider.loginPopup(box.dnoi_internal.onAuthSuccess, box.dnoi_internal.onAuthFailed);
		return false;
	});
	box.dnoi_internal.retryButton = box.dnoi_internal.constructButton(retryButtonText, retryButtonToolTip, function() {
		box.timeout += 5000; // give the retry attempt 5s longer than the last attempt
		box.dnoi_internal.performDiscovery(box.value);
		return false;
	});
	box.dnoi_internal.openid_logo = box.dnoi_internal.constructIcon(openid_logo_url, null, false, true);
	box.dnoi_internal.op_logo = box.dnoi_internal.constructIcon('', authenticatedByToolTip, false, false, "16px");
	box.dnoi_internal.op_logo.style.maxWidth = '16px';
	box.dnoi_internal.spinner = box.dnoi_internal.constructIcon(spinner_url, busyToolTip, true);
	box.dnoi_internal.success_icon = box.dnoi_internal.constructIcon(success_icon_url, authenticatedAsToolTip, true);
	//box.dnoi_internal.failure_icon = box.dnoi_internal.constructIcon(failure_icon_url, authenticationFailedToolTip, true);

	// Disable the display of the DotNetOpenId logo
	//box.dnoi_internal.dnoi_logo = box.dnoi_internal.constructIcon(dotnetopenid_logo_url);
	box.dnoi_internal.dnoi_logo = box.dnoi_internal.openid_logo;

	box.dnoi_internal.setVisualCue = function(state, authenticatedBy, authenticatedAs) {
		box.dnoi_internal.openid_logo.style.visibility = 'hidden';
		box.dnoi_internal.dnoi_logo.style.visibility = 'hidden';
		box.dnoi_internal.op_logo.style.visibility = 'hidden';
		box.dnoi_internal.openid_logo.title = box.dnoi_internal.openid_logo.originalTitle;
		box.dnoi_internal.spinner.style.visibility = 'hidden';
		box.dnoi_internal.success_icon.style.visibility = 'hidden';
		//		box.dnoi_internal.failure_icon.style.visibility = 'hidden';
		box.dnoi_internal.loginButton.style.visibility = 'hidden';
		box.dnoi_internal.retryButton.style.visibility = 'hidden';
		box.title = '';
		box.dnoi_internal.state = state;
		if (state == "discovering") {
			box.dnoi_internal.dnoi_logo.style.visibility = 'visible';
			box.dnoi_internal.spinner.style.visibility = 'visible';
			box.dnoi_internal.claimedIdentifier = null;
			box.title = '';
			window.status = "Discovering OpenID Identifier '" + box.value + "'...";
		} else if (state == "authenticated") {
			var opLogo = box.dnoi_internal.deriveOPFavIcon();
			if (opLogo) {
				box.dnoi_internal.op_logo.src = opLogo;
				box.dnoi_internal.op_logo.style.visibility = 'visible';
				box.dnoi_internal.op_logo.title = box.dnoi_internal.op_logo.originalTitle.replace('{0}', authenticatedBy.getHost());
			}
			//trace("OP icon size: " + box.dnoi_internal.op_logo.fileSize);
			// The filesize check just doesn't seem to work any more.
			if (opLogo == null) {// || box.dnoi_internal.op_logo.fileSize == -1 /*IE*/ || box.dnoi_internal.op_logo.fileSize === undefined /* FF */) {
				trace('recovering from missing OP icon');
				box.dnoi_internal.op_logo.style.visibility = 'hidden';
				box.dnoi_internal.openid_logo.style.visibility = 'visible';
				box.dnoi_internal.openid_logo.title = box.dnoi_internal.op_logo.originalTitle.replace('{0}', authenticatedBy.getHost());
			}
			box.dnoi_internal.success_icon.style.visibility = 'visible';
			box.dnoi_internal.success_icon.title = box.dnoi_internal.success_icon.originalTitle.replace('{0}', authenticatedAs);
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
		} else if (state == '' || state == null) {
			box.dnoi_internal.openid_logo.style.visibility = 'visible';
			box.title = '';
			box.dnoi_internal.claimedIdentifier = null;
			window.status = null;
		} else {
			box.dnoi_internal.claimedIdentifier = null;
			trace('unrecognized state ' + state);
		}
	}

	box.dnoi_internal.isBusy = function() {
		var lastDiscovery = window.dnoa_internal.discoveryResults[box.lastDiscoveredIdentifier];
		return box.dnoi_internal.state == 'discovering' ||
			(lastDiscovery && lastDiscovery.busy());
	};

	box.dnoi_internal.canAttemptLogin = function() {
		if (box.value.length == 0) return false;
		if (window.dnoa_internal.discoveryResults[box.value] == null) return false;
		if (box.dnoi_internal.state == 'failed') return false;
		return true;
	};

	box.dnoi_internal.getUserSuppliedIdentifierResults = function() {
		return window.dnoa_internal.discoveryResults[box.value];
	}

	box.dnoi_internal.isAuthenticated = function() {
		var results = box.dnoi_internal.getUserSuppliedIdentifierResults();
		return results != null && results.findSuccessfulRequest() != null;
	}

	box.dnoi_internal.onSubmit = function() {
		var hiddenField = findOrCreateHiddenField();
		if (box.dnoi_internal.isAuthenticated()) {
			// stick the result in a hidden field so the RP can verify it
			hiddenField.setAttribute("value", window.dnoa_internal.discoveryResults[box.value].successAuthData);
		} else {
			hiddenField.setAttribute("value", '');
			if (box.dnoi_internal.isBusy()) {
				alert(loginInProgressMessage);
			} else {
				if (box.value.length > 0) {
					// submitPending will be true if we've already tried deferring submit for a login,
					// in which case we just want to display a box to the user.
					if (box.dnoi_internal.submitPending || !box.dnoi_internal.canAttemptLogin()) {
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
					return true;
				}
			}
			return false;
		}
		return true;
	};

	/// <summary>
	/// Records which submit button caused this openid box to question whether it
	/// was ready to submit the user's identifier so that that button can be re-invoked
	/// automatically after authentication completes.
	/// </summary>
	box.dnoi_internal.setLastSubmitButtonClicked = function(evt) {
		var button;
		if (evt.target) {
			button = evt.target;
		} else {
			button = evt.srcElement;
		}

		box.dnoi_internal.submitButtonJustClicked = button;
	};

	// Find all submit buttons and hook their click events so that we can validate
	// whether we are ready for the user to postback.
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

	/// <summary>
	/// Returns the URL of the authenticating OP's logo so it can be displayed to the user.
	/// </summary>
	/// <param name="opUri">The OP Endpoint, if known.</param>
	box.dnoi_internal.deriveOPFavIcon = function(opUri) {
		if (!opUri) {
			var idresults = box.dnoi_internal.getUserSuppliedIdentifierResults();
			var response = idresults ? idresults.successAuthData : null;
			if (!response || response.length == 0) {
				trace('No favicon because no successAuthData.');
				return;
			}
			var authResult = new window.dnoa_internal.Uri(response);
			if (authResult.getQueryArgValue("openid.op_endpoint")) {
				opUri = new window.dnoa_internal.Uri(authResult.getQueryArgValue("openid.op_endpoint"));
			} else if (authResult.getQueryArgValue("dnoa.op_endpoint")) {
				opUri = new window.dnoa_internal.Uri(authResult.getQueryArgValue("dnoa.op_endpoint"));
			} else if (authResult.getQueryArgValue("openid.user_setup_url")) {
				opUri = new window.dnoa_internal.Uri(authResult.getQueryArgValue("openid.user_setup_url"));
			} else return null;
		}
		var favicon = opUri.getAuthority() + "/favicon.ico";
		trace('Guessing favicon location of: ' + favicon);
		return favicon;
	};

	/*****************************************
	* Flow
	*****************************************/

	/// <summary>Called to initiate discovery on some identifier.</summary>
	box.dnoi_internal.performDiscovery = function(identifier) {
		box.dnoi_internal.authenticationIFrames.closeFrames();
		box.dnoi_internal.setVisualCue('discovering');
		box.lastDiscoveredIdentifier = identifier;
		var openid = new window.OpenIdIdentifier(identifier);
		openid.discover(box.dnoi_internal.discoverySuccess, box.dnoi_internal.discoveryFailed);
	};

	/// <summary>Callback that is invoked when discovery fails.</summary>
	box.dnoi_internal.discoveryFailed = function(message, identifier) {
		box.dnoi_internal.setVisualCue('failed');
		if (message) { box.title = message; }
	}

	/// <summary>Callback that is invoked when discovery results are available.</summary>
	/// <param name="discoveryResult">The JSON object containing the OpenID auth requests.</param>
	/// <param name="identifier">The identifier that discovery was performed on.</param>
	box.dnoi_internal.discoverySuccess = function(discoveryResult) {
		// Only act on the discovery event if we're still interested in the result.
		// If the user already changed the identifier since discovery was initiated,
		// we aren't interested in it any more.
		if (discoveryResult.userSuppliedIdentifier === box.lastDiscoveredIdentifier) {
			// Start pre-fetching the OP favicons
			for (var i = 0; i < discoveryResult.length; i++) {
				var favicon = box.dnoi_internal.deriveOPFavIcon(discoveryResult[i].endpoint);
				if (favicon) {
					trace('Prefetching ' + favicon);
					box.dnoi_internal.prefetchImage(favicon);
				}
			}
			discoveryResult.loginBackground(
				box.dnoi_internal.authenticationIFrames,
				box.dnoi_internal.onAuthSuccess,
				box.dnoi_internal.onAuthFailed,
				box.dnoi_internal.lastAuthenticationFailed,
				box.timeout);
		}
	}

	box.dnoi_internal.lastAuthenticationFailed = function() {
		trace('No asynchronous authentication attempt is in progress.  Display setup view.');
		// visual cue that auth failed
		box.dnoi_internal.setVisualCue('setup');
	};

	box.dnoi_internal.onAuthSuccess = function(discoveryResult, respondingEndpoint, extensionResponses) {
		// visual cue that auth was successful
		var parsedPositiveAssertion = new window.dnoa_internal.PositiveAssertion(discoveryResult.successAuthData);
		box.dnoi_internal.claimedIdentifier = parsedPositiveAssertion.claimedIdentifier;
		box.dnoi_internal.setVisualCue('authenticated', parsedPositiveAssertion.endpoint, parsedPositiveAssertion.claimedIdentifier);
		if (box.dnoi_internal.onauthenticated) {
			box.dnoi_internal.onauthenticated(box, extensionResponses);
		}

		if (box.dnoi_internal.submitPending) {
			// We submit the form BEFORE resetting the submitPending so
			// the submit handler knows we've already tried this route.
			if (box.dnoi_internal.submitPending == true) {
				box.parentForm.submit();
			} else {
				box.dnoi_internal.submitPending.click();
			}

			box.dnoi_internal.submitPending = null;
		}
	};

	box.dnoi_internal.onAuthFailed = function() {
		box.dnoi_internal.submitPending = null;
	};

	box.onblur = function(event) {
		if (box.lastDiscoveredIdentifier != box.value || !box.dnoi_internal.state) {
			if (box.value.length > 0) {
				box.dnoi_internal.performDiscovery(box.value);
			} else {
				box.dnoi_internal.setVisualCue();
			}
		}

		return true;
	};

	// Closure and encapsulation of typist detection and auto-discovery functionality.
	{
		var rate = NaN;
		var lastValue = box.value;
		var keyPresses = 0;
		var startTime = null;
		var lastKeyPress = null;
		var discoveryTimer;

		function cancelTimer() {
			if (discoveryTimer) {
				trace('canceling timer');
				clearTimeout(discoveryTimer);
				discoveryTimer = null;
			}
		}

		function identifierSanityCheck(id) {
			return id.match("^[=@+$!(].+|.*?\\.[^\.]+");
		}

		function discover() {
			cancelTimer();
			if (identifierSanityCheck(box.value)) {
				box.dnoi_internal.performDiscovery(box.value);
			}
		}

		function reset() {
			keyPresses = 0;
			startTime = null;
			rate = NaN;
			trace('resetting state');
		}

		box.onkeyup = function(e) {
			box.dnoi_internal.setVisualCue();

			if (new Date() - lastKeyPress > 3000) {
				// the user seems to have altogether stopped typing,
				// so reset our typist speed detector.
				reset();
			}
			lastKeyPress = new Date();

			if (e.keyCode == 13) {
				discover();
			} else {
				var newValue = box.value;
				if (lastValue != newValue) {
					if (newValue.length == 0) {
						reset();
					} else if (Math.abs(lastValue.length - newValue.length) > 1) {
						// One key press is responsible for multiple character changes.
						// The user may have pasted in his identifier in which case
						// we want to begin discovery immediately.
						trace(newValue + ': paste detected (old value ' + lastValue + ')');
						discover();
					} else {
						keyPresses++;
						var timeout = 3000; // timeout to use if we don't have enough keying to figure out type rate
						if (startTime === null) {
							startTime = new Date();
						} else if (keyPresses > 1) {
							cancelTimer();
							rate = (new Date() - startTime) / keyPresses;
							var minTimeout = 300;
							var maxTimeout = 3000;
							var typistFactor = 5;
							timeout = Math.max(minTimeout, Math.min(rate * typistFactor, maxTimeout));
						}

						trace(newValue + ': setting timer for ' + timeout);
						discoveryTimer = setTimeout(discover, timeout);
					}
				}
			}

			trace(newValue + ': updating lastValue');
			lastValue = newValue;

			return true;
		};
	}

	box.getClaimedIdentifier = function() { return box.dnoi_internal.claimedIdentifier; };

	// Restore a previously achieved state (from pre-postback) if it is given.
	window.dnoa_internal.deserializePreviousAuthentication(findOrCreateHiddenField().value, box.dnoi_internal.onAuthSuccess);
}
