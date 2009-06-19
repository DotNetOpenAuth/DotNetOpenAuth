//-----------------------------------------------------------------------
// <copyright file="OpenIdRelyingPartyControlBase.js" company="Andrew Arnott">
//     Copyright (c) Andrew Arnott. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

if (window.dnoa_internal === undefined) {
	window.dnoa_internal = new Object();
};

window.dnoa_internal.discoveryResults = new Array(); // user supplied identifiers and discovery results

/// <summary>Instantiates an object that represents an OpenID Identifier.</summary>
window.OpenId = function(identifier) {
	/// <summary>Performs discovery on the identifier.</summary>
	/// <param name="onCompleted">A function(DiscoveryResult) callback to be called when discovery has completed.</param>
	this.discover = function(onCompleted) {
		/// <summary>Instantiates an object that stores discovery results of some identifier.</summary>
		function DiscoveryResult(identifier, discoveryInfo) {
			/// <summary>
			/// Instantiates an object that describes an OpenID service endpoint and facilitates 
			/// initiating and tracking an authentication request.
			/// </summary>
			function ServiceEndpoint(requestInfo, userSuppliedIdentifier) {
				this.immediate = requestInfo.immediate ? new window.dnoa_internal.Uri(requestInfo.immediate) : null;
				this.setup = requestInfo.setup ? new window.dnoa_internal.Uri(requestInfo.setup) : null;
				this.endpoint = new window.dnoa_internal.Uri(requestInfo.endpoint);
				this.userSuppliedIdentifier = userSuppliedIdentifier;
				var self = this; // closure so that delegates have the right instance
				this.loginPopup = function(onSuccess, onFailure) {
					//self.abort(); // ensure no concurrent attempts
					window.dnoa_internal.processAuthorizationResult = function(childLocation) {
						window.dnoa_internal.processAuthorizationResult = null;
						trace('Received event from child window: ' + childLocation);
						var success = true; // TODO: discern between success and failure, and fire the correct event.

						if (success) {
							if (onSuccess) {
								onSuccess();
							}
						} else {
							if (onFailure) {
								onFailure();
							}
						}
					};
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
							// The window closed, either because the user closed it, canceled at the OP,
							// or approved at the OP and the popup window closed itself due to our script.
							// If we were graying out the entire page while the child window was up,
							// we would probably revert that here.
							window.clearInterval(localSelf.popupCloseChecker);
							localSelf.popup = null;

							// The popup may have managed to inform us of the result already,
							// so check whether the callback method was cleared already, which
							// would indicate we've already processed this.
							if (window.dnoa_internal.processAuthorizationResult) {
								trace('User or OP canceled by closing the window.');
								if (onFailure) {
									onFailure();
								}
								window.dnoa_internal.processAuthorizationResult = null;
							}
						}
					}, 250);
				};
			};

			this.userSuppliedIdentifier = identifier;
			this.claimedIdentifier = discoveryInfo.claimedIdentifier; // The claimed identifier may be null if the user provided an OP Identifier.
			trace('Discovered claimed identifier: ' + (this.claimedIdentifier ? this.claimedIdentifier : "(directed identity)"));

			if (discoveryInfo) {
				this.length = discoveryInfo.requests.length;
				for (var i = 0; i < discoveryInfo.requests.length; i++) {
					this[i] = new ServiceEndpoint(discoveryInfo.requests[i], identifier);
				}
			} else {
				this.length = 0;
			}
		};

		/// <summary>Receives the results of a successful discovery (even if it yielded 0 results).</summary>
		function successCallback(discoveryResult, identifier) {
			trace('Discovery completed for: ' + identifier);

			// Deserialize the JSON object and store the result if it was a successful discovery.
			discoveryResult = eval('(' + discoveryResult + ')');

			// Add behavior for later use.
			discoveryResult = new DiscoveryResult(identifier, discoveryResult);
			window.dnoa_internal.discoveryResults[identifier] = discoveryResult;

			if (onCompleted) {
				onCompleted(discoveryResult);
			}
		};

		/// <summary>Receives the discovery failure notification.</summary>
		failureCallback = function(message, userSuppliedIdentifier) {
			trace('Discovery failed for: ' + identifier);

			if (onCompleted) {
				onCompleted();
			}
		};

		if (window.dnoa_internal.discoveryResults[identifier]) {
			trace("We've already discovered " + identifier + " so we're skipping it this time.");
			onCompleted(window.dnoa_internal.discoveryResults[identifier]);
		}

		trace('starting discovery on ' + identifier);
		window.dnoa_internal.callbackAsync(identifier, successCallback, failureCallback);
	};

	/// <summary>Performs discovery and immediately begins checkid_setup to authenticate the user using a given identifier.</summary>
	this.login = function(onSuccess, onFailure) {
		this.discover(function(discoveryResult) {
			if (discoveryResult) {
				trace('Discovery succeeded and found ' + discoveryResult.length + ' OpenID service endpoints.');
				if (discoveryResult.length > 0) {
					discoveryResult[0].loginPopup(onSuccess, onFailure);
				} else {
					trace("This doesn't look like an OpenID Identifier.  Aborting login.");
					if (onFailure) {
						onFailure();
					}
				}
			}
		});
	};
};

