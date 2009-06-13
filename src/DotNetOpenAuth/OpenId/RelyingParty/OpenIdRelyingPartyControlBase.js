//-----------------------------------------------------------------------
// <copyright file="OpenIdRelyingPartyControlBase.js" company="Andrew Arnott">
//     Copyright (c) Andrew Arnott. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

// Options that can be set on the host page:
//window.openid_visible_iframe = true; // causes the hidden iframe to show up
//window.openid_trace = true; // causes lots of messages

window.dnoa_internal = new Object();
window.dnoa_internal.discoveryCompletedCallbacks = new Array(); // user supplied identifiers with functions to call back when discovery completes
window.dnoa_internal.discoveryResults = new Array(); // user supplied identifiers and discovery results

// The possible authentication results
window.dnoa_internal.authSuccess = new Object();
window.dnoa_internal.authRefused = new Object();
window.dnoa_internal.timedOut = new Object();


trace = function(msg) {
	if (window.openid_trace) {
		if (!window.openid_tracediv) {
			window.openid_tracediv = document.createElement("ol");
			document.body.appendChild(window.openid_tracediv);
		}
		var el = document.createElement("li");
		el.appendChild(document.createTextNode(msg));
		window.openid_tracediv.appendChild(el);
		//alert(msg);
	}
};

/// <summary>Initiates asynchronous discovery on an identifier.</summary>
/// <param name="identifier">The identifier on which to perform discovery.<param>
/// <param name="discoveryCompletedCallback">The function(identifier, discoveryResult) to invoke when discovery is completed.</param>
window.dnoa_internal.discover = function(identifier, discoveryCompletedCallback) {
	trace('starting discovery on ' + identifier);
	window.dnoa_internal.callback(identifier, window.dnoa_internal.discoverSuccess, window.dnoa_internal.discoverFailure);

	// Save the discovery completed callback for lookup when discovery is done.
	window.dnoa_internal.discoveryCompletedCallbacks[identifier] = discoveryCompletedCallback;
};

window.dnoa_internal.discoverSuccess = function(discoveryResult, userSuppliedIdentifier) {
	trace('Discovery completed for: ' + userSuppliedIdentifier);

	// Deserialize the JSON object and store the result if it was a successful discovery.
	discoveryResult = eval('(' + discoveryResult + ')');

	// Add behavior for later use.
	window.dnoa_internal.discoveryResults[userSuppliedIdentifier] = discoveryResult = new window.dnoa_internal.DiscoveryResult(discoveryResult, userSuppliedIdentifier);

	var callback = window.dnoa_internal.discoveryCompletedCallbacks[userSuppliedIdentifier];
	if (callback) {
		trace('Calling back discovery completed handler.');
		callback(userSuppliedIdentifier, discoveryResult);
		window.dnoa_internal.discoveryCompletedCallbacks[userSuppliedIdentifier] = null;
	} else {
		trace('No handler registered to receive discovery completed event.');
	}
};

window.dnoa_internal.discoverFailure = function(message, userSuppliedIdentifier) {
	trace('Discovery failed for: ' + identifier);

	var callback = window.dnoa_internal.discoveryCompletedCallbacks[userSuppliedIdentifier];
	if (callback) {
		callback(userSuppliedIdentifier);
		window.dnoa_internal.discoveryCompletedCallbacks[userSuppliedIdentifier] = null;
	}
};

window.dnoa_internal.trySetup = function(userSuppliedIdentifier) {
	window.dnoa_internal.discover(userSuppliedIdentifier, function(identifier, result) {
		trace('discovery completed... now proceeding to trySetup.');
		result[0].trySetup();
	});
};

window.dnoa_internal.DiscoveryResult = function(discoveryInfo, userSuppliedIdentifier) {
	this.userSuppliedIdentifier = userSuppliedIdentifier;
	// The claimed identifier may be null if the user provided an OP Identifier.
	this.claimedIdentifier = discoveryInfo.claimedIdentifier;
	trace('Discovered claimed identifier: ' + (this.claimedIdentifier ? this.claimedIdentifier : "(directed identity)"));

	this.length = discoveryInfo.requests.length;
	for (var i = 0; i < discoveryInfo.requests.length; i++) {
		this[i] = new window.dnoa_internal.TrackingRequest(discoveryInfo.requests[i], userSuppliedIdentifier);
	}
};

window.dnoa_internal.TrackingRequest = function(requestInfo, userSuppliedIdentifier) {
	this.immediate = requestInfo.immediate ? new Uri(requestInfo.immediate) : null;
	this.setup = requestInfo.setup ? new Uri(requestInfo.setup) : null;
	this.endpoint = new Uri(requestInfo.endpoint);
	this.userSuppliedIdentifier = userSuppliedIdentifier;
	var self = this; // closure so that delegates have the right instance
	this.trySetup = function(callback) {
		//self.abort(); // ensure no concurrent attempts
		window.dnoa_internal.authenticationCompleted = callback;
		var width = 800;
		var height = 600;
		if (self.setup.getQueryArgValue("openid.return_to").indexOf("dotnetopenid.popupUISupported") >= 0) {
			width = 450;
			height = 500;
		}

		var left = (screen.width - width) / 2;
		var top = (screen.height - height) / 2;
		self.popup = window.open(self.setup, 'opLogin', 'status=0,toolbar=0,location=1,resizable=1,scrollbars=1,left=' + left + ',top=' + top + ',width=' + width + ',height=' + height);

		// If the OP supports the UI extension it MAY close its own window
		// for a negative assertion.  We must be able to recover from that scenario.
		var localSelf = self;
		self.popupCloseChecker = window.setInterval(function() {
			if (localSelf.popup && localSelf.popup.closed) {
				// So the user canceled and the window closed.
				// It turns out we hae nothing special to do.
				// If we were graying out the entire page while the child window was up,
				// we would probably revert that here.
				trace('User or OP canceled by closing the window.');
				window.clearInterval(localSelf.popupCloseChecker);
				localSelf.popup = null;
			}
		}, 250);
	};
};

/***************************************
 * Uri class
 ***************************************/

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
