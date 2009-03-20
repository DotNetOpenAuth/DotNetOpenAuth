function AreCardsSupported() {
	/// <summary>
	/// Determines if information cards are supported by the 
	/// browser.
	/// </summary>
	/// <returns>
	/// true-if the browser supports information cards.
	///</returns>

	var IEVer = -1;
	if (navigator.appName == 'Microsoft Internet Explorer') {
		if (new RegExp("MSIE ([0-9]{1,}[\.0-9]{0,})").exec(navigator.userAgent) != null) {
			IEVer = parseFloat(RegExp.$1);
		}
	}

	// Look for IE 7+. 
	if (IEVer >= 7) {
		var embed = document.createElement("object");
		embed.setAttribute("type", "application/x-informationcard");

		return "" + embed.issuerPolicy != "undefined" && embed.isInstalled;
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
			var event = document.createEvent("Events");
			event.initEvent("IdentitySelectorAvailable", true, true);
			top.dispatchEvent(event);

			if (top.IdentitySelectorAvailable == true) {
				return true;
			}
		}
	}
	
	return false;
}

function HideStatic(divName) {
	document.getElementById(divName).style.visibility = 'hidden';
}

function ShowStatic(divName) {
	document.getElementById(divName).style.visibility = 'visible';
}

function HideDynamic(divName) {
	document.getElementById(divName).style.display = 'none'
}

function ShowDynamic(divName) {
	document.getElementById(divName).style.display = '';
}

function CheckDynamic(controlDiv, unsupportedDiv) {
	if (AreCardsSupported()) {
		ShowDynamic(controlDiv);
		if (unsupportedDiv != '') {
			HideDynamic(unsupportedDiv);
		}
	}
	else {
		HideDynamic(controlDiv);
		if (unsupportedDiv != '') {
			ShowDynamic(unsupportedDiv);
		}
	}
}

function CheckStatic(controlDiv, unsupportedDiv) {
	if (AreCardsSupported()) {
		ShowStatic(controlDiv);
		if (unsupportedDiv != '') {
			HideStatic(unsupportedDiv);
		}
	}
	else {
		HideStatic(controlDiv);
		if (unsupportedDiv != '') {
			ShowDynamic(unsupportedDiv);
		}
	}
}