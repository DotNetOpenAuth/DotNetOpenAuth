$(function() {
	var ajaxbox = $('#openid_identifier')[0];
	ajaxbox.value = $.cookie('openid_identifier') || '';

	if (ajaxbox.value.length > 0) {
		var ops = $('ul.OpenIdProviders li');
		ops.addClass('grayedOut');
		var matchFound = false;
		ops.each(function(i, li) {
			if (li.id == ajaxbox.value) {
				$(li)
					.removeClass('grayedOut')
					.addClass('focused');
				matchFound = true;
			}
		});
		if (!matchFound) {
			$('#OpenIDButton')
					.removeClass('grayedOut')
					.addClass('focused');
			$('#OpenIDForm').show('slow', function() {
				$('#openid_identifier').focus();
			});
		}
	}

	function showLoginSuccess(userSuppliedIdentifier) {
		var li = document.getElementById(userSuppliedIdentifier);
		if (li) {
			$(li).addClass('loginSuccess');
		}
	}

	function checkidSetup(identifier, timerBased) {
		var retain = !$('#NotMyComputer')[0].selected;
		$.cookie('openid_identifier', retain ? identifier : null, { path: '/' });
		var openid = new window.OpenIdIdentifier(identifier);
		if (!openid) { throw 'checkidSetup called without an identifier.'; }
		openid.login(function(discoveryResult, respondingEndpoint, extensionResponses) {
			showLoginSuccess(discoveryResult.userSuppliedIdentifier);
			doLogin(respondingEndpoint.claimedIdentifier);
		});
	}

	// Sends the positive assertion we've collected to the server and actually logs the user into the RP.
	function doLogin(identifier) {
		alert('at this point, the whole page would refresh and you would be logged in as ' + identifier);
		//window.postLoginAssertion(respondingEndpoint.response, window.parent.location.href);
	}

	// This FrameManager will be used for background logins for the OP buttons
	// and the last used identifier.  It is NOT the frame manager used by the
	// OpenIdAjaxTextBox, as it has its own.
	var backgroundTimeout = 3000;

	$(document).ready(function() {
		var ops = $('ul.OpenIdProviders li');
		ops.each(function(i, li) {
			if (li.id != 'OpenIDButton') {
				li.authenticationIFrames = new window.dnoa_internal.FrameManager(1/*throttle*/);
				var openid = new window.OpenIdIdentifier(li.id);
				openid.loginBackground(li.authenticationIFrames, function(discoveryResult, respondingEndpoint, extensionResponses) {
					showLoginSuccess(li.id);
					//alert('OP button background login as ' + respondingEndpoint.claimedIdentifier + ' was successful!');
				}, null, backgroundTimeout);
			}
		});
	});

	$('ul.OpenIdProviders li').click(function() {
		var lastFocus = $('.focused')[0];
		if (lastFocus != $(this)[0]) {
			$('ul.OpenIdProviders li').removeClass('focused');
			$(this).addClass('focused');
		}

		// Make sure we're not graying out any OPs if the user clicked on a gray button.
		if ($(this).hasClass('grayedOut')) {
			$('ul.OpenIdProviders li').removeClass('grayedOut');
		}

		if ($(this)[0] != $('#OpenIDButton')[0]) {
			$('#OpenIDForm').hide('slide', { direction: 'up' }, 1000);
		}

		// If the user clicked on a button that has the "we're ready to log you in immediately",
		// then log them in!
		if ($(this).hasClass('loginSuccess')) {
			doLogin($(this)[0].id);
		} else if ($(this)[0] != $('#OpenIDButton')[0]) {
			// Be sure to hide the openid_identifier text box unless the OpenID button is selected.
			checkidSetup($(this)[0].id);
		}
	});
	$('#OpenIDButton').click(function() {
		$('#OpenIDForm').show('slide', { direction: 'up' }, 1000, function() {
			$('#openid_identifier').focus();
		});
	});

	// Make popup window close on escape (the dialog style is already taken care of)
	$(document).keydown(function(e) {
		if (e.keyCode == $.ui.keyCode.ESCAPE) {
			window.close();
		}
	});
});