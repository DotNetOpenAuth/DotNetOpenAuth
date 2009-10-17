$(function() {
	var ajaxbox = $('#openid_identifier')[0];
	ajaxbox.value = $.cookie('openid_identifier') || '';
	if (ajaxbox.value.length > 0) {
		trace('jumpstarting discovery on ' + box.value + ' because it was in the last used identifier cookie.');
		ajaxbox.login();
	}

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

	function doLogin(identifier, timerBased) {
		var retain = !$('#NotMyComputer')[0].selected;
		$.cookie('openid_identifier', retain ? identifier : null, { path: '/' });
		var openid = new window.OpenIdIdentifier(identifier);
		if (openid) {
			openid.login(function(discoveryResult, respondingEndpoint, extensionResponses) {
				alert("Woot!  We've logged you in as " + respondingEndpoint.claimedIdentifier);
				//window.postLoginAssertion(respondingEndpoint.response, window.parent.location.href);
			});
		} else {
			trace('doLogin called without an identifier.');
		}
	}

	// This FrameManager will be used for background logins for the OP buttons
	// and the last used identifier.  It is NOT the frame manager used by the
	// OpenIdAjaxTextBox, as it has its own.
	var authenticationIFrames = new window.dnoa_internal.FrameManager(5/*throttle*/);
	var backgroundTimeout = 3000;

	$(document).ready(function() {
		function tryOPButtonLogin() {
			var ops = $('ul.OpenIdProviders li');
			ops.each(function(i, li) {
				// don't try an OP button that matches the last used identifier, since we just finished trying that.
				if (li.id != ajaxbox.value && li.id != 'OpenIDButton') {
					var openid = new window.OpenIdIdentifier(li.id);
					openid.loginBackground(authenticationIFrames, function(discoveryResult, respondingEndpoint, extensionResponses) {
						alert('OP button background login as ' + respondingEndpoint.claimedIdentifier + ' was successful!');
					}, null, backgroundTimeout);
				}
			});
		};

		function tryLastIdentifier() {
			// Start by immediately trying to background login using a previously used identifier.
			// And if that fails, try the rest of the OP buttons.
//			alert('tryLastIdentifier starting...');
			if (ajaxbox.value.length > 0) {
				ajaxbox.login(
					function(discoveryResult, respondingEndpoint, extensionResponses) {
						alert('background login as ' + respondingEndpoint.claimedIdentifier + ' was successful!');
					},
					function() {
						alert('last identifier auto-login failed.  Trying OP buttons...');
						tryOPButtonLogin();
					});
			} else {
				tryOPButtonLogin();
			}
		}

//		tryLastIdentifier();
	});

	$('li').click(function() {
		var lastFocus = $('.focused')[0];
		if (lastFocus != $(this)[0]) {
			$('li').removeClass('focused');
			$(this).addClass('focused');
		}

		// Make sure we're not graying out any OPs at this point.
		$('ul.OpenIdProviders li').removeClass('grayedOut');

		// Be sure to hide the openid_identifier text box unless the OpenID button is selected.
		if ($(this)[0] != $('#OpenIDButton')[0]) {
			$('#OpenIDForm').hide('slow');
			doLogin($(this)[0].id);
		}
	});
	$('#OpenIDButton').click(function() {
		$('#OpenIDForm').show('slow', function() {
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