$(document).ready(function () {
	var requestAuthorizationButton = $('#requestAuthorizationButton');

	function gatherRequestedScopes() {
		scopes = '';
		var scopeElements = $("input[name='scope']");
		for (var i = 0; i < scopeElements.length; i++) {
			if (scopeElements[i].checked) {
				if (scopes.length > 0) scopes += ' ';
				scopes += scopeElements[i].value;
			}
		};
		return scopes;
	};

	function assembleQueryString(args) {
		var query = '?';
		for (var key in args) {
			if (query.length > 1) query += '&';
			query += encodeURIComponent(key) + '=' + encodeURIComponent(args[key])
		};
		return query;
	};

	function stripQueryAndFragment(url) {
		var index = url.indexOf('?');
		if (index < 0) index = url.indexOf('#');
		url = index < 0 ? url : url.substring(0, index);
		return url;
	};

	function requestAuthorizationButton_onClick(evt) {
		var args = new Array();
		args['scope'] = gatherRequestedScopes();
		args['redirect_uri'] = stripQueryAndFragment(document.location.href);
		args['response_type'] = 'token';
		args['client_id'] = 'sampleImplicitConsumer';

		var authorizeUrl = "http://localhost:50172/OAuth/Authorize" + assembleQueryString(args);
		document.location = authorizeUrl;
	};

	requestAuthorizationButton.click(requestAuthorizationButton_onClick);
});
