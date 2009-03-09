//-----------------------------------------------------------------------
// <copyright file="OpenIdAjaxTextBox.js" company="Andrew Arnott">
//     Copyright (c) Andrew Arnott. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

// Options that can be set on the host page:
//window.openid_visible_iframe = true; // causes the hidden iframe to show up
//window.openid_trace = true; // causes lots of messages

function trace(msg) {
	if (window.openid_trace) {
		if (!window.tracediv) {
			window.tracediv = document.createElement("ol");
			document.body.appendChild(window.tracediv);
		}
		var el = document.createElement("li");
		el.appendChild(document.createTextNode(msg));
		window.tracediv.appendChild(el);
		//alert(msg);
	}
}

/// <summary>Removes a given element from the array.</summary>
/// <returns>True if the element was in the array, or false if it was not found.</returns>
Array.prototype.remove = function(element) {
	function elementToRemoveLast(a, b) {
		if (a == element) { return 1; }
		if (b == element) { return -1; }
		return 0;
	}
	this.sort(elementToRemoveLast);
	if (this[this.length - 1] == element) {
		this.pop();
		return true;
	} else {
		return false;
	}
};

function initAjaxOpenId(box, openid_logo_url, dotnetopenid_logo_url, spinner_url, success_icon_url, failure_icon_url,
		throttle, timeout, assertionReceivedCode,
		loginButtonText, loginButtonToolTip, retryButtonText, retryButtonToolTip, busyToolTip,
		identifierRequiredMessage, loginInProgressMessage,
		authenticatedByToolTip, authenticatedAsToolTip, authenticationFailedToolTip,
		discoverCallback, discoveryFailedCallback) {
	box.dnoi_internal = new Object();
	if (assertionReceivedCode) {
		box.dnoi_internal.onauthenticated = function(sender, e) { eval(assertionReceivedCode); }
	}

	box.dnoi_internal.originalBackground = box.style.background;
	box.timeout = timeout;
	box.dnoi_internal.discoverIdentifier = discoverCallback;
	box.dnoi_internal.authenticationRequests = new Array();

	// The possible authentication results
	var authSuccess = new Object();
	var authRefused = new Object();
	var timedOut = new Object();

	function FrameManager(maxFrames) {
		this.queuedWork = new Array();
		this.frames = new Array();
		this.maxFrames = maxFrames;

		/// <summary>Called to queue up some work that will use an iframe as soon as it is available.</summary>
		/// <param name="job">
		/// A delegate that must return the url to point to iframe to.  
		/// Its first parameter is the iframe created to service the request.
		/// It will only be called when the work actually begins.
		/// </param>
		this.enqueueWork = function(job) {
			// Assign an iframe to this task immediately if there is one available.
			if (this.frames.length < this.maxFrames) {
				this.createIFrame(job);
			} else {
				this.queuedWork.unshift(job);
			}
		};

		/// <summary>Clears the job queue and immediately closes all iframes.</summary>
		this.cancelAllWork = function() {
			trace('Canceling all open and pending iframes.');
			while (this.queuedWork.pop());
			this.closeFrames();
		};

		/// <summary>An event fired when a frame is closing.</summary>
		this.onJobCompleted = function() {
			// If there is a job in the queue, go ahead and start it up.
			if (job = this.queuedWork.pop()) {
				this.createIFrame(job);
			}
		}

		this.createIFrame = function(job) {
			var iframe = document.createElement("iframe");
			if (!window.openid_visible_iframe) {
				iframe.setAttribute("width", 0);
				iframe.setAttribute("height", 0);
				iframe.setAttribute("style", "display: none");
			}
			iframe.setAttribute("src", job(iframe));
			iframe.openidBox = box;
			box.parentNode.insertBefore(iframe, box);
			this.frames.push(iframe);
			return iframe;
		};
		this.closeFrames = function() {
			if (this.frames.length == 0) { return false; }
			for (var i = 0; i < this.frames.length; i++) {
				if (this.frames[i].parentNode) { this.frames[i].parentNode.removeChild(this.frames[i]); }
			}
			while (this.frames.length > 0) { this.frames.pop(); }
			return true;
		};
		this.closeFrame = function(frame) {
			if (frame.parentNode) { frame.parentNode.removeChild(frame); }
			var removed = this.frames.remove(frame);
			this.onJobCompleted();
			return removed;
		};
	}
	
	box.dnoi_internal.authenticationIFrames = new FrameManager(throttle);

	box.dnoi_internal.constructButton = function(text, tooltip, onclick) {
		var button = document.createElement('button');
		button.textContent = text; // Mozilla
		button.value = text; // IE
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
		var discoveryInfo = box.dnoi_internal.authenticationRequests[box.lastDiscoveredIdentifier];
		if (discoveryInfo == null) {
			trace('Ooops!  Somehow the login button click event was invoked, but no openid discovery information for ' + box.lastDiscoveredIdentifier + ' is available.');
			return;
		}
		// The login button always sends a setup message to the first OP.
		var selectedProvider = discoveryInfo[0];
		selectedProvider.trySetup();
		return false;
	});
	box.dnoi_internal.retryButton = box.dnoi_internal.constructButton(retryButtonText, retryButtonToolTip, function() {
		box.timeout += 5000; // give the retry attempt 5s longer than the last attempt
		box.dnoi_internal.performDiscovery(box.value);
		return false;
	});
	box.dnoi_internal.openid_logo = box.dnoi_internal.constructIcon(openid_logo_url, null, false, true);
	box.dnoi_internal.op_logo = box.dnoi_internal.constructIcon('', authenticatedByToolTip, false, false, "16px");
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
			} else {
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
		} else if (state = '' || state == null) {
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
		return box.dnoi_internal.state == 'discovering' || 
			box.dnoi_internal.authenticationRequests[box.lastDiscoveredIdentifier].busy();
	};

	box.dnoi_internal.canAttemptLogin = function() {
		if (box.value.length == 0) return false;
		if (box.dnoi_internal.authenticationRequests[box.value] == null) return false;
		if (box.dnoi_internal.state == 'failed') return false;
		return true;
	};

	box.dnoi_internal.getUserSuppliedIdentifierResults = function() {
		return box.dnoi_internal.authenticationRequests[box.value];
	}

	box.dnoi_internal.isAuthenticated = function() {
		var results = box.dnoi_internal.getUserSuppliedIdentifierResults();
		return results != null && results.findSuccessfulRequest() != null;
	}

	box.dnoi_internal.onSubmit = function() {
		var hiddenField = findOrCreateHiddenField();
		if (box.dnoi_internal.isAuthenticated()) {
			// stick the result in a hidden field so the RP can verify it
			hiddenField.setAttribute("value", box.dnoi_internal.authenticationRequests[box.value].successAuthData);
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
	box.dnoi_internal.deriveOPFavIcon = function() {
		var response = box.dnoi_internal.getUserSuppliedIdentifierResults().successAuthData;
		if (!response || response.length == 0) return;
		var authResult = new Uri(response);
		var opUri;
		if (authResult.getQueryArgValue("openid.op_endpoint")) {
			opUri = new Uri(authResult.getQueryArgValue("openid.op_endpoint"));
		} if (authResult.getQueryArgValue("dotnetopenid.op_endpoint")) {
			opUri = new Uri(authResult.getQueryArgValue("dotnetopenid.op_endpoint"));
		} else if (authResult.getQueryArgValue("openid.user_setup_url")) {
			opUri = new Uri(authResult.getQueryArgValue("openid.user_setup_url"));
		} else return null;
		var favicon = opUri.getAuthority() + "/favicon.ico";
		return favicon;
	};

	box.dnoi_internal.createDiscoveryInfo = function(discoveryInfo, identifier) {
		this.identifier = identifier;
		// The claimed identifier may be null if the user provided an OP Identifier.
		this.claimedIdentifier = discoveryInfo.claimedIdentifier;
		trace('Discovered claimed identifier: ' + this.claimedIdentifier);

		// Add extra tracking bits and behaviors.
		this.findByEndpoint = function(opEndpoint) {
			for (var i = 0; i < this.length; i++) {
				if (this[i].endpoint == opEndpoint) {
					return this[i];
				}
			}
		};
		this.findSuccessfulRequest = function() {
			for (var i = 0; i < this.length; i++) {
				if (this[i].result == authSuccess) {
					return this[i];
				}
			}
		};
		this.busy = function() {
			for (var i = 0; i < this.length; i++) {
				if (this[i].busy()) {
					return true;
				}
			}
		};
		this.abortAll = function() {
			// Abort all other asynchronous authentication attempts that may be in progress.
			box.dnoi_internal.authenticationIFrames.cancelAllWork();
			for (var i = 0; i < this.length; i++) {
				this[i].abort();
			}
		};
		this.tryImmediate = function() {
			if (this.length > 0) {
				for (var i = 0; i < this.length; i++) {
					box.dnoi_internal.authenticationIFrames.enqueueWork(this[i].tryImmediate);
				}
			} else {
				box.dnoi_internal.discoveryFailed(null, this.identifier);
			}
		};

		this.length = discoveryInfo.requests.length;
		for (var i = 0; i < discoveryInfo.requests.length; i++) {
			this[i] = new box.dnoi_internal.createTrackingRequest(discoveryInfo.requests[i], identifier);
		}
	};

	box.dnoi_internal.createTrackingRequest = function(requestInfo, identifier) {
		// It's possible during a postback that discovered request URLs are not available.
		this.immediate = requestInfo.immediate ? new Uri(requestInfo.immediate) : null;
		this.setup = requestInfo.setup ? new Uri(requestInfo.setup) : null;
		this.endpoint = new Uri(requestInfo.endpoint);
		this.identifier = identifier;
		var self = this; // closure so that delegates have the right instance

		this.host = self.endpoint.getHost();

		this.getDiscoveryInfo = function() {
			return box.dnoi_internal.authenticationRequests[self.identifier];
		}

		this.busy = function() {
			return self.iframe != null || self.popup != null;
		};

		this.completeAttempt = function() {
			if (!self.busy()) return false;
			if (self.iframe) {
				trace('iframe hosting ' + self.endpoint + ' now CLOSING.');
				box.dnoi_internal.authenticationIFrames.closeFrame(self.iframe);
				self.iframe = null;
			}
			if (self.popup) {
				self.popup.close();
				self.popup = null;
			}
			if (self.timeout) {
				window.clearTimeout(self.timeout);
				self.timeout = null;
			}

			if (!self.getDiscoveryInfo().busy() && self.getDiscoveryInfo().findSuccessfulRequest() == null) {
				trace('No asynchronous authentication attempt is in progress.  Display setup view.');
				// visual cue that auth failed
				box.dnoi_internal.setVisualCue('setup');
			}

			return true;
		};

		this.authenticationTimedOut = function() {
			if (self.completeAttempt()) {
				trace(self.host + " timed out");
				self.result = timedOut;
			}
		};
		this.authSuccess = function(authUri) {
			if (self.completeAttempt()) {
				trace(self.host + " authenticated!");
				self.result = authSuccess;
				self.response = authUri;
				box.dnoi_internal.authenticationRequests[self.identifier].abortAll();
			}
		};
		this.authFailed = function() {
			if (self.completeAttempt()) {
				//trace(self.host + " failed authentication");
				self.result = authRefused;
			}
		};
		this.abort = function() {
			if (self.completeAttempt()) {
				trace(self.host + " aborted");
				// leave the result as whatever it was before.
			}
		};

		this.tryImmediate = function(iframe) {
			self.abort(); // ensure no concurrent attempts
			self.timeout = setTimeout(function() { self.authenticationTimedOut(); }, box.timeout);
			trace('iframe hosting ' + self.endpoint + ' now OPENING.');
			self.iframe = iframe;
			//trace('initiating auth attempt with: ' + self.immediate);
			return self.immediate;
		};
		this.trySetup = function() {
			self.abort(); // ensure no concurrent attempts
			window.waiting_openidBox = box;
			self.popup = window.open(self.setup, 'opLogin', 'status=0,toolbar=0,location=1,resizable=1,scrollbars=1,width=800,height=600');
		};
	};

	/*****************************************
	* Flow
	*****************************************/

	/// <summary>Called to initiate discovery on some identifier.</summary>
	box.dnoi_internal.performDiscovery = function(identifier) {
		box.dnoi_internal.authenticationIFrames.closeFrames();
		box.dnoi_internal.setVisualCue('discovering');
		box.lastDiscoveredIdentifier = identifier;
		box.dnoi_internal.discoverIdentifier(identifier, box.dnoi_internal.discoveryResult, box.dnoi_internal.discoveryFailed);
	};

	/// <summary>Callback that is invoked when discovery fails.</summary>
	box.dnoi_internal.discoveryFailed = function(message, identifier) {
		box.dnoi_internal.setVisualCue('failed');
		if (message) { box.title = message; }
	}

	/// <summary>Callback that is invoked when discovery results are available.</summary>
	/// <param name="discoveryResult">The JSON object containing the OpenID auth requests.</param>
	/// <param name="identifier">The identifier that discovery was performed on.</param>
	box.dnoi_internal.discoveryResult = function(discoveryResult, identifier) {
		// Deserialize the JSON object and store the result if it was a successful discovery.
		discoveryResult = eval('(' + discoveryResult + ')');
		// Store the discovery results and added behavior for later use.
		box.dnoi_internal.authenticationRequests[identifier] = discoveryBehavior = new box.dnoi_internal.createDiscoveryInfo(discoveryResult, identifier);

		// Only act on the discovery event if we're still interested in the result.
		// If the user already changed the identifier since discovery was initiated,
		// we aren't interested in it any more.
		if (identifier == box.lastDiscoveredIdentifier) {
			discoveryBehavior.tryImmediate();
		}
	}

	/// <summary>Invoked by RP web server when an authentication has completed.</summary>
	/// <remarks>The duty of this method is to distribute the notification to the appropriate tracking object.</remarks>
	box.dnoi_internal.processAuthorizationResult = function(resultUrl) {
		self.waiting_openidBox = null;
		//trace('processAuthorizationResult ' + resultUrl);
		var resultUri = new Uri(resultUrl);

		// Find the tracking object responsible for this request.
		var discoveryInfo = box.dnoi_internal.authenticationRequests[resultUri.getQueryArgValue('dotnetopenid.userSuppliedIdentifier')];
		if (discoveryInfo == null) {
			trace('processAuthorizationResult called but no userSuppliedIdentifier parameter was found.  Exiting function.');
			return;
		}
		var opEndpoint = resultUri.getQueryArgValue("openid.op_endpoint") ? resultUri.getQueryArgValue("openid.op_endpoint") : resultUri.getQueryArgValue("dotnetopenid.op_endpoint");
		var tracker = discoveryInfo.findByEndpoint(opEndpoint);
		//trace('Auth result for ' + tracker.host + ' received:\n' + resultUrl);

		if (isAuthSuccessful(resultUri)) {
			tracker.authSuccess(resultUri);

			discoveryInfo.successAuthData = resultUrl;
			var claimed_id = resultUri.getQueryArgValue("openid.claimed_id");
			if (claimed_id && claimed_id != discoveryInfo.claimedIdentifier) {
				discoveryInfo.claimedIdentifier = resultUri.getQueryArgValue("openid.claimed_id");
				trace('Authenticated as ' + claimed_id);
			}

			// visual cue that auth was successful
			box.dnoi_internal.claimedIdentifier = discoveryInfo.claimedIdentifier;
			box.dnoi_internal.setVisualCue('authenticated', tracker.endpoint, discoveryInfo.claimedIdentifier);
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
			tracker.authFailed();
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
		var discoveryInfo = box.dnoi_internal.authenticationRequests[box.value];
		if (discoveryInfo == null) {
			if (box.value.length > 0) {
				box.dnoi_internal.performDiscovery(box.value);
			} else {
				box.dnoi_internal.setVisualCue();
			}
		} else {
			if ((priorSuccess = discoveryInfo.findSuccessfulRequest())) {
				box.dnoi_internal.setVisualCue('authenticated', priorSuccess.endpoint, discoveryInfo.claimedIdentifier);
			} else {
				discoveryInfo.tryImmediate();
			}
		}
		return true;
	};
	box.onkeyup = function(event) {
		box.dnoi_internal.setVisualCue();
		return true;
	};

	box.getClaimedIdentifier = function() { return box.dnoi_internal.claimedIdentifier; };

	// Restore a previously achieved state (from pre-postback) if it is given.
	var oldAuth = findOrCreateHiddenField().value;
	if (oldAuth.length > 0) {
		var oldAuthResult = new Uri(oldAuth);
		// The control ensures that we ALWAYS have an OpenID 2.0-style claimed_id attribute, even against
		// 1.0 Providers via the return_to URL mechanism.
		var claimedId = oldAuthResult.getQueryArgValue("dotnetopenid.claimed_id");
		var endpoint = oldAuthResult.getQueryArgValue("dotnetopenid.op_endpoint");
		// We weren't given a full discovery history, but we can spoof this much from the
		// authentication assertion.
		box.dnoi_internal.authenticationRequests[box.value] = new box.dnoi_internal.createDiscoveryInfo({
			claimedIdentifier: claimedId,
			requests: [{ endpoint: endpoint }]
		}, box.value);

		box.dnoi_internal.processAuthorizationResult(oldAuthResult.toString());
	}
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

	this.trimFragment = function() {
		var hashmark = this.originalUri.indexOf('#');
		if (hashmark >= 0) {
			return new Uri(this.originalUri.substr(0, hashmark));
		}
		return this;
	};

	this.appendQueryVariable = function(name, value) {
		var pair = encodeURI(name) + "=" + encodeURI(value);
		if (this.originalUri.indexOf('?') >= 0) {
			this.originalUri = this.originalUri + "&" + pair;
		} else {
			this.originalUri = this.originalUri + "?" + pair;
		}
	};

	function KeyValuePair(key, value) {
		this.key = key;
		this.value = value;
	};

	this.Pairs = new Array();

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
