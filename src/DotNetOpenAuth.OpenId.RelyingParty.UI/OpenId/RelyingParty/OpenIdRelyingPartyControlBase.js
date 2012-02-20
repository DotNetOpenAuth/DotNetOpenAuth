//-----------------------------------------------------------------------
// <copyright file="OpenIdRelyingPartyControlBase.js" company="Outercurve Foundation>
//     Copyright (c) Outercurve Foundation. All rights reserved.
//     This file may be used and redistributed under the terms of the
//     Microsoft Public License (Ms-PL) http://opensource.org/licenses/ms-pl.html
// </copyright>
//-----------------------------------------------------------------------

// Options that can be set on the host page:
//window.openid_visible_iframe = true; // causes the hidden iframe to show up
//window.openid_trace = true; // causes lots of messages

trace = function(msg, color) {
	if (window.openid_trace) {
		if (!window.openid_tracediv) {
			window.openid_tracediv = document.createElement("ol");
			document.body.appendChild(window.openid_tracediv);
		}
		var el = document.createElement("li");
		if (color) { el.style.color = color; }
		el.appendChild(document.createTextNode(msg));
		window.openid_tracediv.appendChild(el);
		//alert(msg);
	}
};

if (window.dnoa_internal === undefined) {
	window.dnoa_internal = {};
}

/// <summary>Instantiates an object that provides string manipulation services for URIs.</summary>
window.dnoa_internal.Uri = function(url) {
	this.originalUri = url.toString();

	this.toString = function() {
		return this.originalUri;
	};

	this.getAuthority = function() {
		var authority = this.getScheme() + "://" + this.getHost();
		return authority;
	};

	this.getHost = function() {
		var hostStartIdx = this.originalUri.indexOf("://") + 3;
		var hostEndIndex = this.originalUri.indexOf("/", hostStartIdx);
		if (hostEndIndex < 0) { hostEndIndex = this.originalUri.length; }
		var host = this.originalUri.substr(hostStartIdx, hostEndIndex - hostStartIdx);
		return host;
	};

	this.getScheme = function() {
		var schemeStartIdx = this.indexOf("://");
		return this.originalUri.substr(this.originalUri, schemeStartIdx);
	};

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
	}

	this.pairs = [];

	var queryBeginsAt = this.originalUri.indexOf('?');
	if (queryBeginsAt >= 0) {
		this.queryString = this.originalUri.substr(queryBeginsAt + 1);
		var queryStringPairs = this.queryString.split('&');

		for (var i = 0; i < queryStringPairs.length; i++) {
			var equalsAt = queryStringPairs[i].indexOf('=');
			left = (equalsAt >= 0) ? queryStringPairs[i].substring(0, equalsAt) : null;
			right = (equalsAt >= 0) ? queryStringPairs[i].substring(equalsAt + 1) : queryStringPairs[i];
			this.pairs.push(new KeyValuePair(unescape(left), unescape(right)));
		}
	}

	this.getQueryArgValue = function(key) {
		for (var i = 0; i < this.pairs.length; i++) {
			if (this.pairs[i].key == key) {
				return this.pairs[i].value;
			}
		}
	};

	this.getPairs = function() {
		return this.pairs;
	};

	this.containsQueryArg = function(key) {
		return this.getQueryArgValue(key);
	};

	this.getUriWithoutQueryOrFragement = function() {
		var queryBeginsAt = this.originalUri.indexOf('?');
		if (queryBeginsAt >= 0) {
			return this.originalUri.substring(0, queryBeginsAt);
		} else {
			var fragmentBeginsAt = this.originalUri.indexOf('#');
			if (fragmentBeginsAt >= 0) {
				return this.originalUri.substring(0, fragmentBeginsAt);
			} else {
				return this.originalUri;
			}
		}
	};

	this.indexOf = function(args) {
		return this.originalUri.indexOf(args);
	};

	return this;
};

/// <summary>Creates a hidden iframe.</summary>
window.dnoa_internal.createHiddenIFrame = function() {
	var iframe = document.createElement("iframe");
	if (!window.openid_visible_iframe) {
		iframe.setAttribute("width", 0);
		iframe.setAttribute("height", 0);
		iframe.setAttribute("style", "display: none");
		iframe.setAttribute("border", 0);
	}

	return iframe;
};

/// <summary>Redirects the current window/frame to the given URI, 
/// either using a GET or a POST as required by the length of the URL.</summary>
window.dnoa_internal.GetOrPost = function(uri) {
	var maxGetLength = 2 * 1024; // keep in sync with DotNetOpenAuth.Messaging.Channel.IndirectMessageGetToPostThreshold
	uri = new window.dnoa_internal.Uri(uri);

	if (uri.toString().length <= maxGetLength) {
		window.location = uri.toString();
	} else {
		trace("Preparing to POST: " + uri.toString());
		var iframe = window.dnoa_internal.createHiddenIFrame();
		document.body.appendChild(iframe);
		var doc = iframe.ownerDocument;
		var form = doc.createElement('form');
		form.action = uri.getUriWithoutQueryOrFragement();
		form.method = "POST";
		form.target = "_top";
		for (var i = 0; i < uri.getPairs().length; i++) {
			var input = doc.createElement('input');
			input.type = 'hidden';
			input.name = uri.getPairs()[i].key;
			input.value = uri.getPairs()[i].value;
			trace(input.name + " = " + input.value);
			form.appendChild(input);
		}
		doc.body.appendChild(form);
		form.submit();
	}
};
