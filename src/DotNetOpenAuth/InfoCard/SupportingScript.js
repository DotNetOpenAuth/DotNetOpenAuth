/*jslint white: true, onevar: true, browser: true, undef: true, nomen: true, plusplus: true, bitwise: true, regexp: true, strict: true, newcap: true, immed: true */
"use strict";
document.infoCard = {
	isSupported: function () {
		/// <summary>
		/// Determines if information cards are supported by the 
		/// browser.
		/// </summary>
		/// <returns>
		/// true-if the browser supports information cards.
		///</returns>
		var IEVer, embed, x, event;

		IEVer = -1;
		if (navigator.appName === 'Microsoft Internet Explorer') {
			if (new RegExp("MSIE ([0-9]{1,}[\\.0-9]{0,})").exec(navigator.userAgent) !== null) {
				IEVer = parseFloat(RegExp.$1);
			}
		}

		// Look for IE 7+. 
		if (IEVer >= 7) {
			embed = document.createElement("object");
			embed.type = "application/x-informationcard";
			return embed.issuerPolicy !== undefined && embed.isInstalled;
		}

		// not IE (any version)
		if (IEVer < 0 && navigator.mimeTypes && navigator.mimeTypes.length) {
			// check to see if there is a mimeType handler. 
			x = navigator.mimeTypes['application/x-informationcard'];
			if (x && x.enabledPlugin) {
				return true;
			}

			// check for the IdentitySelector event handler is there. 
			if (document.addEventListener) {
				event = document.createEvent("Events");
				event.initEvent("IdentitySelectorAvailable", true, true);
				top.dispatchEvent(event);

				if (top.IdentitySelectorAvailable === true) {
					return true;
				}
			}
		}

		return false;
	},

	activate: function (selectorId, hiddenFieldName) {
		var selector, hiddenField;
		selector = document.getElementById(selectorId);
		hiddenField = document.getElementsByName(hiddenFieldName)[0];
		try {
			hiddenField.value = selector.value;
		} catch (e) {
			// Selector was canceled
			return false;
		}
		if (hiddenField.value == 'undefined') { // really the string, not === undefined
			// We're dealing with a bad FireFox selector plugin.
			// Just add the control to the form by setting its name property and submit to activate.
			selector.name = hiddenFieldName;
			hiddenField.parentNode.removeChild(hiddenField);
			return true;
		}
		return true;
	},

	hideStatic: function (divName) {
		var div = document.getElementById(divName);
		if (div) {
			div.style.visibility = 'hidden';
		}
	},

	showStatic: function (divName) {
		var div = document.getElementById(divName);
		if (div) {
			div.style.visibility = 'visible';
		}
	},

	hideDynamic: function (divName) {
		var div = document.getElementById(divName);
		if (div) {
			div.style.display = 'none';
		}
	},

	showDynamic: function (divName) {
		var div = document.getElementById(divName);
		if (div) {
			div.style.display = '';
		}
	},

	checkDynamic: function (controlDiv, unsupportedDiv) {
		if (this.isSupported()) {
			this.showDynamic(controlDiv);
			if (unsupportedDiv) {
				this.hideDynamic(unsupportedDiv);
			}
		} else {
			this.hideDynamic(controlDiv);
			if (unsupportedDiv) {
				this.showDynamic(unsupportedDiv);
			}
		}
	},

	checkStatic: function (controlDiv, unsupportedDiv) {
		if (this.isSupported()) {
			this.showStatic(controlDiv);
			if (unsupportedDiv) {
				this.hideStatic(unsupportedDiv);
			}
		} else {
			this.hideStatic(controlDiv);
			if (unsupportedDiv) {
				this.showDynamic(unsupportedDiv);
			}
		}
	}
};
