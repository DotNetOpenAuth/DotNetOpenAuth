//-----------------------------------------------------------------------
// <copyright file="OpenIdRelyingPartyAjaxControlBase.js" company="Outercurve Foundation>
//     Copyright (c) Outercurve Foundation. All rights reserved.
//     This file may be used and redistributed under the terms of the
//     Microsoft Public License (Ms-PL) http://opensource.org/licenses/ms-pl.html
// </copyright>
//-----------------------------------------------------------------------

if (window.dnoa_internal === undefined) {
	window.dnoa_internal = {};
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

// Renders all the parameters in their string form, surrounded by parentheses.
window.dnoa_internal.argsToString = function() {
	result = "(";
	for (var i = 0; i < arguments.length; i++) {
		if (i > 0) { result += ', '; }
		var arg = arguments[i];
		if (typeof (arg) == 'string') {
			arg = '"' + arg + '"';
		} else if (arg === null) {
			arg = '[null]';
		} else if (arg === undefined) {
			arg = '[undefined]';
		}
		result += arg.toString();
	}
	result += ')';
	return result;
};

window.dnoa_internal.registerEvent = function(name) {
	var filterOnApplicability = function(fn, domElement) {
		/// <summary>Wraps a given function with a check so that the function only executes when a given element is still in the DOM.</summary>
		return function() {
			var args = Array.prototype.slice.call(arguments);
			if (!domElement) {
				// no element used as a basis of applicability indicates we always fire this callback.
				fn.apply(null, args);
			} else {
				var elements = document.getElementsByTagName(domElement.tagName);
				var isElementInDom = false;
				for (var i = 0; i < elements.length; i++) {
					if (elements[i] === domElement) {
						isElementInDom = true;
						break;
					}
				}
				if (isElementInDom) {
					fn.apply(null, args);
				}
			}
		}
	};

	window.dnoa_internal[name + 'Listeners'] = [];
	window.dnoa_internal['add' + name] = function(fn, whileDomElementApplicable) { window.dnoa_internal[name + 'Listeners'].push(filterOnApplicability(fn, whileDomElementApplicable)); };
	window.dnoa_internal['remove' + name] = function(fn) { window.dnoa_internal[name + 'Listeners'].remove(fn); };
	window.dnoa_internal['fire' + name] = function() {
		var args = Array.prototype.slice.call(arguments);
		trace('Firing event ' + name + window.dnoa_internal.argsToString.apply(null, args), 'blue');
		var listeners = window.dnoa_internal[name + 'Listeners'];
		for (var i = 0; i < listeners.length; i++) {
			listeners[i].apply(null, args);
		}
	};
};

window.dnoa_internal.registerEvent('DiscoveryStarted'); // (identifier) - fired when a discovery callback is ACTUALLY made to the RP
window.dnoa_internal.registerEvent('DiscoverySuccess'); // (identifier, discoveryResult, { fresh: true|false }) - fired after a discovery callback is returned from the RP successfully or a cached result is retrieved
window.dnoa_internal.registerEvent('DiscoveryFailed'); // (identifier, message) - fired after a discovery callback fails
window.dnoa_internal.registerEvent('AuthStarted'); // (discoveryResult, serviceEndpoint, { background: true|false })
window.dnoa_internal.registerEvent('AuthFailed'); // (discoveryResult, serviceEndpoint, { background: true|false }) - fired for each individual ServiceEndpoint, and once at last with serviceEndpoint==null if all failed
window.dnoa_internal.registerEvent('AuthSuccess'); // (discoveryResult, serviceEndpoint, extensionResponses, { background: true|false, deserialized: true|false })
window.dnoa_internal.registerEvent('AuthCleared'); // (discoveryResult, serviceEndpoint)

window.dnoa_internal.discoveryResults = []; // user supplied identifiers and discovery results
window.dnoa_internal.discoveryInProgress = []; // identifiers currently being discovered and their callbacks

// The possible authentication results
window.dnoa_internal.authSuccess = 'auth-success';
window.dnoa_internal.authRefused = 'auth-refused';
window.dnoa_internal.timedOut = 'timed-out';

/// <summary>Instantiates a new FrameManager.</summary>
/// <param name="maxFrames">The maximum number of concurrent 'jobs' (authentication attempts).</param>
window.dnoa_internal.FrameManager = function(maxFrames) {
	this.queuedWork = [];
	this.frames = [];
	this.maxFrames = maxFrames;

	/// <summary>Called to queue up some work that will use an iframe as soon as it is available.</summary>
	/// <param name="job">
	/// A delegate that must return { url: /*to point the iframe to*/, onCanceled: /* callback */ }
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
		while (this.queuedWork.pop()) { }
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
		var jobDescription = job(iframe, p1);
		iframe.setAttribute("src", jobDescription.url);
		iframe.onCanceled = jobDescription.onCanceled;
		iframe.dnoa_internal = window.dnoa_internal;
		document.body.insertBefore(iframe, document.body.firstChild);
		this.frames.push(iframe);
		return iframe;
	};

	this.closeFrames = function() {
		if (this.frames.length === 0) { return false; }
		for (var i = 0; i < this.frames.length; i++) {
			this.frames[i].src = "about:blank"; // doesn't have to exist.  Just stop its processing.
			if (this.frames[i].parentNode) { this.frames[i].parentNode.removeChild(this.frames[i]); }
		}
		while (this.frames.length > 0) {
			var frame = this.frames.pop();
			if (frame.onCanceled) { frame.onCanceled(); }
		}
		return true;
	};

	this.closeFrame = function(frame) {
		frame.src = "about:blank"; // doesn't have to exist.  Just stop its processing.
		if (frame.parentNode) { frame.parentNode.removeChild(frame); }
		var removed = this.frames.remove(frame);
		this.onJobCompleted();
		return removed;
	};
};

/// <summary>Instantiates an object that represents an OpenID Identifier.</summary>
window.OpenIdIdentifier = function(identifier) {
	if (!identifier || identifier.length === 0) {
		throw 'Error: trying to create OpenIdIdentifier for null or empty string.';
	}

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

			window.dnoa_internal.fireDiscoverySuccess(identifier, discoveryResult, { fresh: true });

			// Clear our "in discovery" state and fire callbacks
			var callbacks = window.dnoa_internal.discoveryInProgress[identifier];
			window.dnoa_internal.discoveryInProgress[identifier] = null;

			if (callbacks) {
				for (var i = 0; i < callbacks.onSuccess.length; i++) {
					if (callbacks.onSuccess[i]) {
						callbacks.onSuccess[i](discoveryResult);
					}
				}
			}
		}

		/// <summary>Receives the discovery failure notification.</summary>
		function discoverFailureCallback(message, userSuppliedIdentifier) {
			trace('Discovery failed for: ' + identifier);

			// Clear our "in discovery" state and fire callbacks
			var callbacks = window.dnoa_internal.discoveryInProgress[identifier];
			window.dnoa_internal.discoveryInProgress[identifier] = null;

			if (callbacks) {
				for (var i = 0; i < callbacks.onSuccess.length; i++) {
					if (callbacks.onFailure[i]) {
						callbacks.onFailure[i](message);
					}
				}
			}

			window.dnoa_internal.fireDiscoveryFailed(identifier, message);
		}

		if (window.dnoa_internal.discoveryResults[identifier]) {
			trace("We've already discovered " + identifier + " so we're using the cached version.");

			// In this special case, we never fire the DiscoveryStarted event.
			window.dnoa_internal.fireDiscoverySuccess(identifier, window.dnoa_internal.discoveryResults[identifier], { fresh: false });

			if (onDiscoverSuccess) {
				onDiscoverSuccess(window.dnoa_internal.discoveryResults[identifier]);
			}

			return;
		}

		window.dnoa_internal.fireDiscoveryStarted(identifier);

		if (!window.dnoa_internal.discoveryInProgress[identifier]) {
			trace('starting discovery on ' + identifier);
			window.dnoa_internal.discoveryInProgress[identifier] = {
				onSuccess: [onDiscoverSuccess],
				onFailure: [onDiscoverFailure]
			};
			window.dnoa_internal.callbackAsync(identifier, discoverSuccessCallback, discoverFailureCallback);
		} else {
			trace('Discovery on ' + identifier + ' already started. Registering an additional callback.');
			window.dnoa_internal.discoveryInProgress[identifier].onSuccess.push(onDiscoverSuccess);
			window.dnoa_internal.discoveryInProgress[identifier].onFailure.push(onDiscoverFailure);
		}
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
	this.loginBackground = function(frameManager, onLoginSuccess, onLoginFailure, timeout, onLoginLastFailure) {
		this.discover(function(discoveryResult) {
			if (discoveryResult) {
				trace('Discovery succeeded and found ' + discoveryResult.length + ' OpenID service endpoints.');
				if (discoveryResult.length > 0) {
					discoveryResult.loginBackground(frameManager, onLoginSuccess, onLoginFailure, onLoginLastFailure || onLoginFailure, timeout);
				} else {
					trace("This doesn't look like an OpenID Identifier.  Aborting login.");
					if (onLoginFailure) {
						onLoginFailure();
					}
				}
			}
		});
	};

	this.toString = function() {
		return identifier;
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
		throw 'processAuthorizationResult called but no userSuppliedIdentifier parameter was found.  Exiting function.';
	}
	var discoveryResult = window.dnoa_internal.discoveryResults[userSuppliedIdentifier];
	if (!discoveryResult) {
		throw 'processAuthorizationResult called but no discovery result matching user supplied identifier ' + userSuppliedIdentifier + ' was found.  Exiting function.';
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
			window.dnoa_internal.fireAuthStarted(thisDiscoveryResult, thisServiceEndpoint, { background: false });
			thisDiscoveryResult.onAuthSuccess = onAuthSuccess;
			thisDiscoveryResult.onAuthFailed = onAuthFailed;
			var chromeHeight = 55; // estimated height of browser title bar and location bar
			var bottomMargin = 45; // estimated bottom space on screen likely to include a task bar
			var width = 1000;
			var height = 600;
			if (thisServiceEndpoint.setup.getQueryArgValue("openid.return_to").indexOf("dnoa.popupUISupported") >= 0) {
				trace('This OP supports the UI extension.  Using smaller window size.');
				width = 500; // spec calls for 450px, but Yahoo needs 500px
				height = 500;
			} else {
				trace("This OP doesn't appear to support the UI extension.  Using larger window size.");
			}

			var left = (screen.width - width) / 2;
			var top = (screen.height - bottomMargin - height - chromeHeight) / 2;
			thisServiceEndpoint.popup = window.open(thisServiceEndpoint.setup, 'opLogin', 'status=0,toolbar=0,location=1,resizable=1,scrollbars=1,left=' + left + ',top=' + top + ',width=' + width + ',height=' + height);

			// If the OP supports the UI extension it MAY close its own window
			// for a negative assertion.  We must be able to recover from that scenario.
			var thisServiceEndpointLocal = thisServiceEndpoint;
			thisServiceEndpoint.popupCloseChecker = window.setInterval(function() {
				if (thisServiceEndpointLocal.popup) {
					try {
						if (thisServiceEndpointLocal.popup.closed) {
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
								window.dnoa_internal.fireAuthFailed(thisDiscoveryResult, thisServiceEndpoint, { background: false });
								if (thisDiscoveryResult.onAuthFailed) {
									thisDiscoveryResult.onAuthFailed(thisDiscoveryResult, thisServiceEndpoint);
								}
							}
						}
					} catch (e) {
						// This usually happens because the popup is currently displaying the OP's
						// page from another domain, which makes the popup temporarily off limits to us.
						// Just skip this interval and wait for the next callback.
					}
				} else {
					// if there's no popup, there's no reason to keep this timer up.
					window.clearInterval(thisServiceEndpointLocal.popupCloseChecker);
				}
			}, 250);
		};

		this.loginBackgroundJob = function(iframe, timeout) {
			thisServiceEndpoint.abort(); // ensure no concurrent attempts
			if (timeout) {
				thisServiceEndpoint.timeout = setTimeout(function() { thisServiceEndpoint.onAuthenticationTimedOut(); }, timeout);
			}
			window.dnoa_internal.fireAuthStarted(thisDiscoveryResult, thisServiceEndpoint, { background: true });
			trace('iframe hosting ' + thisServiceEndpoint.endpoint + ' now OPENING (timeout ' + timeout + ').');
			//trace('initiating auth attempt with: ' + thisServiceEndpoint.immediate);
			thisServiceEndpoint.iframe = iframe;
			return {
				url: thisServiceEndpoint.immediate.toString(),
				onCanceled: function() {
					thisServiceEndpoint.abort();
					window.dnoa_internal.fireAuthFailed(thisDiscoveryResult, thisServiceEndpoint, { background: true });
				}
			};
		};

		this.busy = function() {
			return thisServiceEndpoint.iframe || thisServiceEndpoint.popup;
		};

		this.completeAttempt = function(successful) {
			if (!thisServiceEndpoint.busy()) { return false; }
			window.clearInterval(thisServiceEndpoint.timeout);
			var background = thisServiceEndpoint.iframe !== null;
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

			if (!successful && !thisDiscoveryResult.busy() && !thisDiscoveryResult.findSuccessfulRequest()) {
				// fire the failed event with NO service endpoint indicating the entire auth attempt has failed.
				window.dnoa_internal.fireAuthFailed(thisDiscoveryResult, null, { background: background });
				if (thisDiscoveryResult.onLastAttemptFailed) {
					thisDiscoveryResult.onLastAttemptFailed(thisDiscoveryResult);
				}
			}

			return true;
		};

		this.onAuthenticationTimedOut = function() {
			var background = thisServiceEndpoint.iframe !== null;
			if (thisServiceEndpoint.completeAttempt()) {
				trace(thisServiceEndpoint.host + " timed out");
				thisServiceEndpoint.result = window.dnoa_internal.timedOut;
			}
			window.dnoa_internal.fireAuthFailed(thisDiscoveryResult, thisServiceEndpoint, { background: background });
		};

		this.onAuthSuccess = function(authUri, extensionResponses) {
			var background = thisServiceEndpoint.iframe !== null;
			if (thisServiceEndpoint.completeAttempt(true)) {
				trace(thisServiceEndpoint.host + " authenticated!");
				thisServiceEndpoint.result = window.dnoa_internal.authSuccess;
				thisServiceEndpoint.successReceived = new Date();
				thisServiceEndpoint.claimedIdentifier = authUri.getQueryArgValue('openid.claimed_id');
				thisServiceEndpoint.response = authUri;
				thisServiceEndpoint.extensionResponses = extensionResponses;
				thisDiscoveryResult.abortAll();
				if (thisDiscoveryResult.onAuthSuccess) {
					thisDiscoveryResult.onAuthSuccess(thisDiscoveryResult, thisServiceEndpoint, extensionResponses);
				}
				window.dnoa_internal.fireAuthSuccess(thisDiscoveryResult, thisServiceEndpoint, extensionResponses, { background: background });
			}
		};

		this.onAuthFailed = function() {
			var background = thisServiceEndpoint.iframe !== null;
			if (thisServiceEndpoint.completeAttempt()) {
				trace(thisServiceEndpoint.host + " failed authentication");
				thisServiceEndpoint.result = window.dnoa_internal.authRefused;
				window.dnoa_internal.fireAuthFailed(thisDiscoveryResult, thisServiceEndpoint, { background: background });
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

		this.clear = function() {
			thisServiceEndpoint.result = null;
			thisServiceEndpoint.extensionResponses = null;
			thisServiceEndpoint.successReceived = null;
			thisServiceEndpoint.claimedIdentifier = null;
			thisServiceEndpoint.response = null;
			if (this.onCleared) {
				this.onCleared(thisServiceEndpoint, thisDiscoveryResult);
			}
			if (thisDiscoveryResult.onCleared) {
				thisDiscoveryResult.onCleared(thisDiscoveryResult, thisServiceEndpoint);
			}
			window.dnoa_internal.fireAuthCleared(thisDiscoveryResult, thisServiceEndpoint);
		};

		this.toString = function() {
			return "[ServiceEndpoint: " + thisServiceEndpoint.host + "]";
		};
	}

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
	this.error = discoveryInfo.error;

	if (discoveryInfo) {
		this.claimedIdentifier = discoveryInfo.claimedIdentifier; // The claimed identifier may be null if the user provided an OP Identifier.
		this.length = discoveryInfo.requests.length;
		for (var i = 0; i < discoveryInfo.requests.length; i++) {
			this[i] = new ServiceEndpoint(discoveryInfo.requests[i], identifier);
		}
	} else {
		this.length = 0;
	}

	if (this.length === 0) {
		trace('Discovery completed, but yielded no service endpoints.');
	} else {
		trace('Discovered claimed identifier: ' + (this.claimedIdentifier ? this.claimedIdentifier : "(directed identity)"));
	}

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
			// Abort all other asynchronous authentication attempts that may be in progress
			// for this particular claimed identifier.
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
		var priorSuccessRespondingEndpoint = thisDiscoveryResult.findSuccessfulRequest();
		if (priorSuccessRespondingEndpoint) {
			// In this particular case, we do not fire an AuthStarted event.
			window.dnoa_internal.fireAuthSuccess(thisDiscoveryResult, priorSuccessRespondingEndpoint, priorSuccessRespondingEndpoint.extensionResponses, { background: true });
			if (onAuthSuccess) {
				onAuthSuccess(thisDiscoveryResult, priorSuccessRespondingEndpoint);
			}
		} else {
			if (thisDiscoveryResult.busy()) {
				trace('Warning: DiscoveryResult.loginBackground invoked while a login attempt is already in progress. Discarding second login request.', 'red');
				return;
			}
			thisDiscoveryResult.frameManager = frameManager;
			thisDiscoveryResult.onAuthSuccess = onAuthSuccess;
			thisDiscoveryResult.onAuthFailed = onAuthFailed;
			thisDiscoveryResult.onLastAttemptFailed = onLastAuthFailed;
			// Notify listeners that general authentication is beginning.  Individual ServiceEndpoints
			// will fire their own events as each of them begin their iframe 'job'.
			window.dnoa_internal.fireAuthStarted(thisDiscoveryResult, null, { background: true });
			if (thisDiscoveryResult.length > 0) {
				for (var i = 0; i < thisDiscoveryResult.length; i++) {
					thisDiscoveryResult.frameManager.enqueueWork(thisDiscoveryResult[i].loginBackgroundJob, timeout);
				}
			}
		}
	};

	this.toString = function() {
		return "[DiscoveryResult: " + thisDiscoveryResult.userSuppliedIdentifier + "]";
	};
};

/// <summary>
/// Called in a page had an AJAX control that had already obtained a positive assertion
/// when a postback occurred, and now that control wants to restore its 'authenticated' state.
/// </summary>
/// <param name="positiveAssertion">The string form of the URI that contains the positive assertion.</param>
window.dnoa_internal.deserializePreviousAuthentication = function(positiveAssertion) {
	if (!positiveAssertion || positiveAssertion.length === 0) {
		return;
	}

	trace('Revitalizing an old positive assertion from a prior postback.');

	// The control ensures that we ALWAYS have an OpenID 2.0-style claimed_id attribute, even against
	// 1.0 Providers via the return_to URL mechanism.
	var parsedPositiveAssertion = new window.dnoa_internal.PositiveAssertion(positiveAssertion);

	// We weren't given a full discovery history, but we can spoof this much from the
	// authentication assertion.
	trace('Deserialized claimed_id: ' + parsedPositiveAssertion.claimedIdentifier + ' and endpoint: ' + parsedPositiveAssertion.endpoint);
	var discoveryInfo = {
		claimedIdentifier: parsedPositiveAssertion.claimedIdentifier,
		requests: [{ endpoint: parsedPositiveAssertion.endpoint}]
	};

	discoveryResult = new window.dnoa_internal.DiscoveryResult(parsedPositiveAssertion.userSuppliedIdentifier, discoveryInfo);
	window.dnoa_internal.discoveryResults[parsedPositiveAssertion.userSuppliedIdentifier] = discoveryResult;
	discoveryResult[0].result = window.dnoa_internal.authSuccess;
	discoveryResult.successAuthData = positiveAssertion;

	// restore old state from before postback
	window.dnoa_internal.fireAuthSuccess(discoveryResult, discoveryResult[0], null, { background: true, deserialized: true });
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
	if (obj === null || typeof (obj) != 'object' || !isNaN(obj)) { // !isNaN catches Date objects
		return obj;
	}

	var temp = {};
	for (var key in obj) {
		temp[key] = window.dnoa_internal.clone(obj[key]);
	}

	// Copy over some built-in methods that were not included in the above loop,
	// but nevertheless may have been overridden.
	temp.toString = window.dnoa_internal.clone(obj.toString);

	return temp;
};

// Deserialized the preloaded discovery results
window.dnoa_internal.loadPreloadedDiscoveryResults = function(preloadedDiscoveryResults) {
	trace('found ' + preloadedDiscoveryResults.length + ' preloaded discovery results.');
	for (var i = 0; i < preloadedDiscoveryResults.length; i++) {
		var result = preloadedDiscoveryResults[i];
		if (!window.dnoa_internal.discoveryResults[result.userSuppliedIdentifier]) {
			window.dnoa_internal.discoveryResults[result.userSuppliedIdentifier] = new window.dnoa_internal.DiscoveryResult(result.userSuppliedIdentifier, result.discoveryResult);
			trace('Preloaded discovery on: ' + window.dnoa_internal.discoveryResults[result.userSuppliedIdentifier].userSuppliedIdentifier);
		} else {
			trace('Skipped preloaded discovery on: ' + window.dnoa_internal.discoveryResults[result.userSuppliedIdentifier].userSuppliedIdentifier + ' because we have a cached discovery result on it.');
		}
	}
};

window.dnoa_internal.clearExpiredPositiveAssertions = function() {
	for (identifier in window.dnoa_internal.discoveryResults) {
		var discoveryResult = window.dnoa_internal.discoveryResults[identifier];
		if (typeof (discoveryResult) != 'object') { continue; } // skip functions
		for (var i = 0; i < discoveryResult.length; i++) {
			if (discoveryResult[i] && discoveryResult[i].result === window.dnoa_internal.authSuccess) {
				if (new Date() - discoveryResult[i].successReceived > window.dnoa_internal.maxPositiveAssertionLifetime) {
					// This positive assertion is too old, and may eventually be rejected by DNOA during verification.
					// Let's clear out the positive assertion so it can be renewed.
					trace('Clearing out expired positive assertion from ' + discoveryResult[i].host);
					discoveryResult[i].clear();
				}
			}
		}
	}
};

window.setInterval(window.dnoa_internal.clearExpiredPositiveAssertions, 1000);
