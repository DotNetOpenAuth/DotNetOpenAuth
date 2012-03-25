var utilities = {
	assembleQueryString: function(args) {
		var query = '?';
		for (var key in args) {
			if (query.length > 1) query += '&';
			query += encodeURIComponent(key) + '=' + encodeURIComponent(args[key])
		};
		return query;
	},

	parseQueryString: function (query) {
		var result = new Array();
		var pairs = query.split('&');
		for (var i = 0; i < pairs.length; i++) {
			var pair = pairs[i].split('=');
			var key = decodeURIComponent(pair[0]);
			var value = decodeURIComponent(pair[1]);
			result[key] = value;
		};
		return result;
	},

	stripQueryAndFragment: function (url) {
		var index = url.indexOf('?');
		if (index < 0) index = url.indexOf('#');
		url = index < 0 ? url : url.substring(0, index);
		return url;
	}
};

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

	function requestAuthorizationButton_onClick(evt) {
		var args = new Array();
		args['scope'] = gatherRequestedScopes();
		args['redirect_uri'] = utilities.stripQueryAndFragment(document.location.href);
		args['response_type'] = 'token';
		args['client_id'] = 'sampleImplicitConsumer';

		var authorizeUrl = "http://localhost:50172/OAuth/Authorize" + utilities.assembleQueryString(args);
		document.location = authorizeUrl;
	};

	requestAuthorizationButton.click(requestAuthorizationButton_onClick);
});

$(document).ready(function () {
	$.support.cors = true; // force cross-site scripting (as of jQuery 1.5)
	function serviceCall(operation, accessToken, label) {
		label.text('fetching...');
		$.ajax({
			url: "http://localhost:65170" + encodeURI(operation),
			headers: {
				"Authorization": "Bearer " + accessToken
			},
			cache: false,
			success: function (data, textStatus, jqXHR) { label.text(data.toString()); },
			error: function (jqXHR, textStatus, errorThrown) { label.text(textStatus + ": " + errorThrown); }
		});
	};

	var fragmentIndex = document.location.href.indexOf('#');
	if (fragmentIndex > 0) {
		var fragment = document.location.href.substring(fragmentIndex + 1);
		var args = utilities.parseQueryString(fragment);
		if (args['access_token']) {
			var authorizationLabel = $('#authorizationLabel');
			var lifetimeInSeconds = args['expires_in'];
			var suffix = '(access token expires in ' + lifetimeInSeconds + ' seconds)';
			authorizationLabel.text('Authorization received! ' + suffix);

			var scopes = args['scope'].split(' ');
			for (var scopeIndex in scopes) {
				var scope = scopes[scopeIndex];
				var button = $('input[operation="' + scope + '"]');
				button[0].disabled = false;
				button.click((function (value) {
					return function () {
						var label = $('span[operation="' + value + '"]');
						serviceCall(value, args['access_token'], label);
					};
				})(scope));
				var checkbox = $('input[value="' + scope + '"]')[0];
				checkbox.checked = true;
			};
		}
	}
});