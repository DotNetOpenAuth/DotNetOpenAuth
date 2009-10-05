//-----------------------------------------------------------------------
// <copyright file="OpenIdRelyingPartyAjaxControlBase.js" company="Andrew Arnott">
//     Copyright (c) Andrew Arnott. All rights reserved.
//     This file may be used and redistributed under the terms of the
//     Microsoft Public License (Ms-PL) http://opensource.org/licenses/ms-pl.html
// </copyright>
//-----------------------------------------------------------------------

if (window.dnoa_internal === undefined) {
	window.dnoa_internal = new Object();
};

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

window.dnoa_internal.discoveryResults = new Array(); // user supplied identifiers and discovery results

// The possible authentication results
window.dnoa_internal.authSuccess = new Object();
window.dnoa_internal.authRefused = new Object();
window.dnoa_internal.timedOut = new Object();

/// <summary>Instantiates a new FrameManager.</summary>
/// <param name="maxFrames">The maximum number of concurrent 'jobs' (authentication attempts).</param>
window.dnoa_internal.FrameManager = function(maxFrames) {
	this.queuedWork = new Array();
	this.frames = new Array();
	this.maxFrames = maxFrames;

	/// <summary>Called to queue up some work that will use an iframe as soon as it is available.</summary>
	/// <param name="job">
	/// A delegate that must return the url to point the iframe to.  
	/// Its first parameter is the iframe created to service the request.
	/// It will only be called when the work actually begins.
	/// </param>
	/// <param name="p1">Arbitrary additional parameter to pass to the job.</param>
	this.enqueueWork = function(job, p1) {
		// Assign an iframe to this task immediately if there is one available.
		if (this.frames.length < this.maxFrames) {
			this.createIFrame(job, p1);
		} else {
			this.queuedWork.unshift({ job: job, p1: p1 });
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
		if (jobDesc = this.queuedWork.pop()) {
			this.createIFrame(jobDesc.job, jobDesc.p1);
		}
	};

	this.createIFrame = function(job, p1) {
		var iframe = document.createElement("iframe");
		if (!window.openid_visible_iframe) {
			iframe.setAttribute("width", 0);
			iframe.setAttribute("height", 0);
			iframe.setAttribute("style", "display: none");
		}
		iframe.setAttribute("src", job(iframe, p1));
		iframe.dnoa_internal = window.dnoa_internal;
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
};

/// <summary>Instantiates an object that represents an OpenID Identifier.</summary>
window.OpenIdIdentifier = function(identifier) {
	/// <summary>Performs discovery on the identifier.</summary>
	/// <param name="onDiscoverSuccess">A function(DiscoveryResult) callback to be called when discovery has completed successfully.</param>
	/// <param name="onDiscoverFailure">A function callback to be called when discovery has completed in failure.</param>
	this.discover = function(onDiscoverSuccess, onDiscoverFailure) {
		/// <summary>Receives the results of a successful discovery (even if it yielded 0 results).</summary>
		function discoverSuccessCallback(discoveryResult, identifier) {
			trace('Discovery completed for: ' + identifier);

			// Deserialize the JSON object and store the result if it was a successful discovery.
			discoveryResult = eval('(' + discoveryResult + ')');

			// Add behavior for later use.
			discoveryResult = new window.dnoa_internal.DiscoveryResult(identifier, discoveryResult);
			window.dnoa_internal.discoveryResults[identifier] = discoveryResult;

			if (onDiscoverSuccess) {
				onDiscoverSuccess(discoveryResult);
			}
		};

		/// <summary>Receives the discovery failure notification.</summary>
		function discoverFailureCallback(message, userSuppliedIdentifier) {
			trace('Discovery failed for: ' + identifier);

			if (onDiscoverFailure) {
				onDiscoverFailure();
			}
		};

		if (window.dnoa_internal.discoveryResults[identifier]) {
			trace("We've already discovered " + identifier + " so we're skipping it this time.");
			if (onDiscoverSuccess) {
				onDiscoverSuccess(window.dnoa_internal.discoveryResults[identifier]);
			}
			return;
		};

		trace('starting discovery on ' + identifier);
		window.dnoa_internal.callbackAsync(identifier, discoverSuccessCallback, discoverFailureCallback);
	};

	/// <summary>Performs discovery and immediately begins checkid_setup to authenticate the user using a given identifier.</summary>
	this.login = function(onSuccess, onLoginFailure) {
		this.discover(function(discoveryResult) {
			if (discoveryResult) {
				trace('Discovery succeeded and found ' + discoveryResult.length + ' OpenID service endpoints.');
				if (discoveryResult.length > 0) {
					discoveryResult[0].loginPopup(onSuccess, onLoginFailure);
				} else {
					trace("This doesn't look like an OpenID Identifier.  Aborting login.");
					if (onLoginFailure) {
						onLoginFailure();
					}
				}
			}
		});
	};

	/// <summary>Performs discovery and immediately begins checkid_immediate on all discovered endpoints.</summary>
	this.loginBackground = function(frameManager, onLoginSuccess, onLoginFailure, timeout) {
		this.discover(function(discoveryResult) {
			if (discoveryResult) {
				trace('Discovery succeeded and found ' + discoveryResult.length + ' OpenID service endpoints.');
				if (discoveryResult.length > 0) {
					discoveryResult.loginBackground(frameManager, onLoginSuccess, onLoginFailure, onLoginFailure, timeout);
				} else {
					trace("This doesn't look like an OpenID Identifier.  Aborting login.");
					if (onLoginFailure) {
						onLoginFailure();
					}
				}
			}
		});
	};
};

/// <summary>Invoked by RP web server when an authentication has completed.</summary>
/// <remarks>The duty of this method is to distribute the notification to the appropriate tracking object.</remarks>
window.dnoa_internal.processAuthorizationResult = function(resultUrl, extensionResponses) {
	//trace('processAuthorizationResult ' + resultUrl);
	var resultUri = new window.dnoa_internal.Uri(resultUrl);
	trace('processing auth result with extensionResponses: ' + extensionResponses);
	if (extensionResponses) {
		extensionResponses = eval(extensionResponses);
	}

	// Find the tracking object responsible for this request.
	var userSuppliedIdentifier = resultUri.getQueryArgValue('dnoa.userSuppliedIdentifier');
	if (!userSuppliedIdentifier) {
		trace('processAuthorizationResult called but no userSuppliedIdentifier parameter was found.  Exiting function.');
		return;
	}
	var discoveryResult = window.dnoa_internal.discoveryResults[userSuppliedIdentifier];
	if (discoveryResult == null) {
		trace('processAuthorizationResult called but no discovery result matching user supplied identifier ' + userSuppliedIdentifier + ' was found.  Exiting function.');
		return;
	}

	var opEndpoint = resultUri.getQueryArgValue("openid.op_endpoint") ? resultUri.getQueryArgValue("openid.op_endpoint") : resultUri.getQueryArgValue("dnoa.op_endpoint");
	var respondingEndpoint = discoveryResult.findByEndpoint(opEndpoint);
	trace('Auth result for ' + respondingEndpoint.host + ' received.'); //: ' + resultUrl);

	if (window.dnoa_internal.isAuthSuccessful(resultUri)) {
		discoveryResult.successAuthData = resultUrl;
		respondingEndpoint.onAuthSuccess(resultUri, extensionResponses);

		var parsedPositiveAssertion = new window.dnoa_internal.PositiveAssertion(resultUri);
		if (parsedPositiveAssertion.claimedIdentifier && parsedPositiveAssertion.claimedIdentifier != discoveryResult.claimedIdentifier) {
			discoveryResult.claimedIdentifier = parsedPositiveAssertion.claimedIdentifier;
			trace('Authenticated as ' + parsedPositiveAssertion.claimedIdentifier);
		}
	} else {
		respondingEndpoint.onAuthFailed();
	}
};

window.dnoa_internal.isAuthSuccessful = function(resultUri) {
	if (window.dnoa_internal.isOpenID2Response(resultUri)) {
		return resultUri.getQueryArgValue("openid.mode") == "id_res";
	} else {
		return resultUri.getQueryArgValue("openid.mode") == "id_res" && !resultUri.containsQueryArg("openid.user_setup_url");
	}
};

window.dnoa_internal.isOpenID2Response = function(resultUri) {
	return resultUri.containsQueryArg("openid.ns");
};

/// <summary>Instantiates an object that stores discovery results of some identifier.</summary>
window.dnoa_internal.DiscoveryResult = function(identifier, discoveryInfo) {
	var thisDiscoveryResult = this;

	/// <summary>
	/// Instantiates an object that describes an OpenID service endpoint and facilitates 
	/// initiating and tracking an authentication request.
	/// </summary>
	function ServiceEndpoint(requestInfo, userSuppliedIdentifier) {
		this.immediate = requestInfo.immediate ? new window.dnoa_internal.Uri(requestInfo.immediate) : null;
		this.setup = requestInfo.setup ? new window.dnoa_internal.Uri(requestInfo.setup) : null;
		this.endpoint = new window.dnoa_internal.Uri(requestInfo.endpoint);
		this.host = this.endpoint.getHost();
		this.userSuppliedIdentifier = userSuppliedIdentifier;
		var thisServiceEndpoint = this; // closure so that delegates have the right instance
		this.loginPopup = function(onAuthSuccess, onAuthFailed) {
			thisServiceEndpoint.abort(); // ensure no concurrent attempts
			thisDiscoveryResult.onAuthSuccess = onAuthSuccess;
			thisDiscoveryResult.onAuthFailed = onAuthFailed;
			var width = 800;
			var height = 600;
			if (thisServiceEndpoint.setup.getQueryArgValue("openid.return_to").indexOf("dnoa.popupUISupported") >= 0) {
				trace('This OP supports the UI extension.  Using smaller window size.');
				width = 450;
				height = 500;
			} else {
				trace("This OP doesn't appear to support the UI extension.  Using larger window size.");
			}

			var left = (screen.width - width) / 2;
			var top = (screen.height - height) / 2;
			thisServiceEndpoint.popup = window.open(thisServiceEndpoint.setup, 'opLogin', 'status=0,toolbar=0,location=1,resizable=1,scrollbars=1,left=' + left + ',top=' + top + ',width=' + width + ',height=' + height);

			// If the OP supports the UI extension it MAY close its own window
			// for a negative assertion.  We must be able to recover from that scenario.
			var thisServiceEndpointLocal = thisServiceEndpoint;
			thisServiceEndpoint.popupCloseChecker = window.setInterval(function() {
				if (thisServiceEndpointLocal.popup && thisServiceEndpointLocal.popup.closed) {
					// The window closed, either because the user closed it, canceled at the OP,
					// or approved at the OP and the popup window closed itself due to our script.
					// If we were graying out the entire page while the child window was up,
					// we would probably revert that here.
					window.clearInterval(thisServiceEndpointLocal.popupCloseChecker);
					thisServiceEndpointLocal.popup = null;

					// The popup may have managed to inform us of the result already,
					// so check whether the callback method was cleared already, which
					// would indicate we've already processed this.
					if (window.dnoa_internal.processAuthorizationResult) {
						trace('User or OP canceled by closing the window.');
						if (thisDiscoveryResult.onAuthFailed) {
							thisDiscoveryResult.onAuthFailed(thisDiscoveryResult, thisServiceEndpoint);
						}
						window.dnoa_internal.processAuthorizationResult = null;
					}
				}
			}, 250);
		};

		this.loginBackgroundJob = function(iframe, timeout) {
			thisServiceEndpoint.abort(); // ensure no concurrent attempts
			if (timeout) {
				thisServiceEndpoint.timeout = setTimeout(function() { thisServiceEndpoint.onAuthenticationTimedOut(); }, timeout);
			}
			trace('iframe hosting ' + thisServiceEndpoint.endpoint + ' now OPENING (timeout ' + timeout + ').');
			//trace('initiating auth attempt with: ' + thisServiceEndpoint.immediate);
			thisServiceEndpoint.iframe = iframe;
			return thisServiceEndpoint.immediate.toString();
		};

		this.busy = function() {
			return thisServiceEndpoint.iframe != null || thisServiceEndpoint.popup != null;
		};

		this.completeAttempt = function(successful) {
			if (!thisServiceEndpoint.busy()) return false;
			window.clearInterval(thisServiceEndpoint.timeout);
			if (thisServiceEndpoint.iframe) {
				trace('iframe hosting ' + thisServiceEndpoint.endpoint + ' now CLOSING.');
				thisDiscoveryResult.frameManager.closeFrame(thisServiceEndpoint.iframe);
				thisServiceEndpoint.iframe = null;
			}
			if (thisServiceEndpoint.popup) {
				thisServiceEndpoint.popup.close();
				thisServiceEndpoint.popup = null;
			}
			if (thisServiceEndpoint.timeout) {
				window.clearTimeout(thisServiceEndpoint.timeout);
				thisServiceEndpoint.timeout = null;
			}

			if (!successful && !thisDiscoveryResult.busy() && thisDiscoveryResult.findSuccessfulRequest() == null) {
				if (thisDiscoveryResult.onLastAttemptFailed) {
					thisDiscoveryResult.onLastAttemptFailed(thisDiscoveryResult);
				}
			}

			return true;
		};

		this.onAuthenticationTimedOut = function() {
			if (thisServiceEndpoint.completeAttempt()) {
				trace(thisServiceEndpoint.host + " timed out");
				thisServiceEndpoint.result = window.dnoa_internal.timedOut;
			}
		};

		this.onAuthSuccess = function(authUri, extensionResponses) {
			if (thisServiceEndpoint.completeAttempt(true)) {
				trace(thisServiceEndpoint.host + " authenticated!");
				thisServiceEndpoint.result = window.dnoa_internal.authSuccess;
				thisServiceEndpoint.claimedIdentifier = authUri.getQueryArgValue('openid.claimed_id');
				thisServiceEndpoint.response = authUri;
				thisDiscoveryResult.abortAll();
				if (thisDiscoveryResult.onAuthSuccess) {
					thisDiscoveryResult.onAuthSuccess(thisDiscoveryResult, thisServiceEndpoint, extensionResponses);
				}
			}
		};

		this.onAuthFailed = function() {
			if (thisServiceEndpoint.completeAttempt()) {
				trace(thisServiceEndpoint.host + " failed authentication");
				thisServiceEndpoint.result = window.dnoa_internal.authRefused;
				if (thisDiscoveryResult.onAuthFailed) {
					thisDiscoveryResult.onAuthFailed(thisDiscoveryResult, thisServiceEndpoint);
				}
			}
		};

		this.abort = function() {
			if (thisServiceEndpoint.completeAttempt()) {
				trace(thisServiceEndpoint.host + " aborted");
				// leave the result as whatever it was before.
			}
		};

	};

	this.cloneWithOneServiceEndpoint = function(serviceEndpoint) {
		var clone = window.dnoa_internal.clone(this);
		clone.userSuppliedIdentifier = serviceEndpoint.claimedIdentifier;

		// Erase all SEPs except the given one, and put it into first position.
		clone.length = 1;
		for (var i = 0; i < this.length; i++) {
			if (clone[i].endpoint.toString() == serviceEndpoint.endpoint.toString()) {
				var tmp = clone[i];
				clone[i] = null;
				clone[0] = tmp;
			} else {
				clone[i] = null;
			}
		}

		return clone;
	};

	this.userSuppliedIdentifier = identifier;

	if (discoveryInfo) {
		this.claimedIdentifier = discoveryInfo.claimedIdentifier; // The claimed identifier may be null if the user provided an OP Identifier.
		this.length = discoveryInfo.requests.length;
		for (var i = 0; i < discoveryInfo.requests.length; i++) {
			this[i] = new ServiceEndpoint(discoveryInfo.requests[i], identifier);
		}
	} else {
		this.length = 0;
	}

	trace('Discovered claimed identifier: ' + (this.claimedIdentifier ? this.claimedIdentifier : "(directed identity)"));

	// Add extra tracking bits and behaviors.
	this.findByEndpoint = function(opEndpoint) {
		for (var i = 0; i < thisDiscoveryResult.length; i++) {
			if (thisDiscoveryResult[i].endpoint == opEndpoint) {
				return thisDiscoveryResult[i];
			}
		}
	};

	this.busy = function() {
		for (var i = 0; i < thisDiscoveryResult.length; i++) {
			if (thisDiscoveryResult[i].busy()) {
				return true;
			}
		}
	};

	// Add extra tracking bits and behaviors.
	this.findSuccessfulRequest = function() {
		for (var i = 0; i < thisDiscoveryResult.length; i++) {
			if (thisDiscoveryResult[i].result === window.dnoa_internal.authSuccess) {
				return thisDiscoveryResult[i];
			}
		}
	};

	this.abortAll = function() {
		if (thisDiscoveryResult.frameManager) {
			// Abort all other asynchronous authentication attempts that may be in progress.
			thisDiscoveryResult.frameManager.cancelAllWork();
			for (var i = 0; i < thisDiscoveryResult.length; i++) {
				thisDiscoveryResult[i].abort();
			}
		} else {
			trace('abortAll called without a frameManager being previously set.');
		}
	};

	/// <summary>Initiates an asynchronous checkid_immediate login attempt against all possible service endpoints for an Identifier.</summary>
	/// <param name="frameManager">The work queue for authentication iframes.</param>
	/// <param name="onAuthSuccess">Fired when an endpoint responds affirmatively.</param>
	/// <param name="onAuthFailed">Fired when an endpoint responds negatively.</param>
	/// <param name="onLastAuthFailed">Fired when all authentication attempts have responded negatively or timed out.</param>
	/// <param name="timeout">Timeout for an individual service endpoint to respond before the iframe closes.</param>
	this.loginBackground = function(frameManager, onAuthSuccess, onAuthFailed, onLastAuthFailed, timeout) {
		if (!frameManager) {
			throw "No frameManager specified.";
		}
		if (thisDiscoveryResult.findSuccessfulRequest() != null) {
			onAuthSuccess(thisDiscoveryResult, thisDiscoveryResult.findSuccessfulRequest());
		} else {
			thisDiscoveryResult.frameManager = frameManager;
			thisDiscoveryResult.onAuthSuccess = onAuthSuccess;
			thisDiscoveryResult.onAuthFailed = onAuthFailed;
			thisDiscoveryResult.onLastAttemptFailed = onLastAuthFailed;
			if (thisDiscoveryResult.length > 0) {
				for (var i = 0; i < thisDiscoveryResult.length; i++) {
					thisDiscoveryResult.frameManager.enqueueWork(thisDiscoveryResult[i].loginBackgroundJob, timeout);
				}
			}
		}
	};
};

/// <summary>
/// Called in a page had an AJAX control that had already obtained a positive assertion
/// when a postback occurred, and now that control wants to restore its 'authenticated' state.
/// </summary>
/// <param name="positiveAssertion">The string form of the URI that contains the positive assertion.</param>
/// <param name="onAuthSuccess">Fired if the positive assertion is successfully processed, as if it had just come in.</param>
window.dnoa_internal.deserializePreviousAuthentication = function(positiveAssertion, onAuthSuccess) {
	if (!positiveAssertion || positiveAssertion.length === 0) {
		return;
	}

	trace('Revitalizing an old positive assertion from a prior postback.');
	var oldAuthResult = new window.dnoa_internal.Uri(positiveAssertion);

	// The control ensures that we ALWAYS have an OpenID 2.0-style claimed_id attribute, even against
	// 1.0 Providers via the return_to URL mechanism.
	var parsedPositiveAssertion = new window.dnoa_internal.PositiveAssertion(positiveAssertion);

	// We weren't given a full discovery history, but we can spoof this much from the
	// authentication assertion.
	trace('Deserialized claimed_id: ' + parsedPositiveAssertion.claimedIdentifier + ' and endpoint: ' + parsedPositiveAssertion.endpoint);
	var discoveryInfo = {
		claimedIdentifier: parsedPositiveAssertion.claimedIdentifier,
		requests: [{ endpoint: parsedPositiveAssertion.endpoint }]
	};

	window.dnoa_internal.discoveryResults[box.value] = discoveryResult = new window.dnoa_internal.DiscoveryResult(parsedPositiveAssertion.userSuppliedIdentifier, discoveryInfo);
	discoveryResult[0].result = window.dnoa_internal.authSuccess;
	discoveryResult.successAuthData = positiveAssertion;

	// restore old state from before postback
	if (onAuthSuccess) {
		onAuthSuccess(discoveryResult, discoveryResult[0]);
	}
};

window.dnoa_internal.PositiveAssertion = function(uri) {
	uri = new window.dnoa_internal.Uri(uri.toString());
	this.endpoint = new window.dnoa_internal.Uri(uri.getQueryArgValue("dnoa.op_endpoint"));
	this.userSuppliedIdentifier = uri.getQueryArgValue('dnoa.userSuppliedIdentifier');
	this.claimedIdentifier = uri.getQueryArgValue('openid.claimed_id');
	if (!this.claimedIdentifier) {
		this.claimedIdentifier = uri.getQueryArgValue('dnoa.claimed_id');
	}
	this.toString = function() { return uri.toString(); };
};

window.dnoa_internal.clone = function(obj) {
	if (obj == null || typeof (obj) != 'object') {
		return obj;
	}

	var temp = new Object();
	for (var key in obj) {
		temp[key] = window.dnoa_internal.clone(obj[key]);
	}

	return temp;
};