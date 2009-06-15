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

if (window.dnoa_internal === undefined) {
	window.dnoa_internal = new Object();
};

// The possible authentication results
window.dnoa_internal.authSuccess = new Object();
window.dnoa_internal.authRefused = new Object();
window.dnoa_internal.timedOut = new Object();

/// <summary>Instantiates an object that provides string manipulation services for URIs.</summary>
window.dnoa_internal.Uri = function(url) {
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
};