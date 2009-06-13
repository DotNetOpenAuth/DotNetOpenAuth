//-----------------------------------------------------------------------
// <copyright file="OpenIdRelyingPartyControlBase.js" company="Andrew Arnott">
//     Copyright (c) Andrew Arnott. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

// Options that can be set on the host page:
//window.openid_visible_iframe = true; // causes the hidden iframe to show up
//window.openid_trace = true; // causes lots of messages

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

window.dnoa_internal = {
	discoveryResults: new Array(), // user supplied identifiers and discovery results
	// The possible authentication results
	authSuccess: new Object(),
	authRefused: new Object(),
	timedOut: new Object(),

	/// <summary>Initiates asynchronous discovery on an identifier.</summary>
	/// <param name="identifier">The identifier on which to perform discovery.<param>
	/// <param name="discoveryCompletedCallback">The function(identifier, discoveryResult) to invoke when discovery is completed.</param>
	discover: function(identifier, discoveryCompletedCallback) {
		successCallback = function(discoveryResult, userSuppliedIdentifier) {
			trace('Discovery completed for: ' + userSuppliedIdentifier);

			// Deserialize the JSON object and store the result if it was a successful discovery.
			discoveryResult = eval('(' + discoveryResult + ')');

			// Add behavior for later use.
			window.dnoa_internal.discoveryResults[userSuppliedIdentifier] = discoveryResult = new window.dnoa_internal.DiscoveryResult(discoveryResult, userSuppliedIdentifier);

			if (discoveryCompletedCallback) {
				discoveryCompletedCallback(userSuppliedIdentifier, discoveryResult);
			}
		};

		failureCallback = function(message, userSuppliedIdentifier) {
			trace('Discovery failed for: ' + identifier);

			if (discoveryCompletedCallback) {
				discoveryCompletedCallback(userSuppliedIdentifier);
			}
		};

		trace('starting discovery on ' + identifier);
		window.dnoa_internal.callback(identifier, successCallback, failureCallback);
	},

	/// <summary>Instantiates an object that stores discovery results of some identifier.</summary>
	DiscoveryResult: function(discoveryInfo, userSuppliedIdentifier) {
		this.userSuppliedIdentifier = userSuppliedIdentifier;
		// The claimed identifier may be null if the user provided an OP Identifier.
		this.claimedIdentifier = discoveryInfo.claimedIdentifier;
		trace('Discovered claimed identifier: ' + (this.claimedIdentifier ? this.claimedIdentifier : "(directed identity)"));

		this.length = discoveryInfo.requests.length;
		for (var i = 0; i < discoveryInfo.requests.length; i++) {
			this[i] = new window.dnoa_internal.TrackingRequest(discoveryInfo.requests[i], userSuppliedIdentifier);
		}
	},

	/// <summary>Instantiates an object that facilitates initiating and tracking an authentication request.</summary>
	TrackingRequest: function(requestInfo, userSuppliedIdentifier) {
		this.immediate = requestInfo.immediate ? new window.dnoa_internal.Uri(requestInfo.immediate) : null;
		this.setup = requestInfo.setup ? new window.dnoa_internal.Uri(requestInfo.setup) : null;
		this.endpoint = new window.dnoa_internal.Uri(requestInfo.endpoint);
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
	},

	/// <summary>Performs discovery and immediately begins checkid_setup to authenticate the user using a given identifier.</summary>
	trySetup: function(userSuppliedIdentifier) {
		onDiscoveryCompleted = function(identifier, result) {
			if (result) {
				trace('discovery completed... now proceeding to trySetup.');
				result[0].trySetup();
			} else {
				trace('discovery completed with no results.');
			}
		};

		window.dnoa_internal.discover(userSuppliedIdentifier, onDiscoveryCompleted);
	},

	/// <summary>Instantiates an object that provides string manipulation services for URIs.</summary>
	Uri: function(url) {
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
				return new window.dnoa_internal.Uri(this.originalUri.substr(0, hashmark));
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
	}
};