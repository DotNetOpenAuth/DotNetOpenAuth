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
		openid.login(function(discoveryResult, respondingEndpoint, extensionResponses) {
			window.postLoginAssertion(respondingEndpoint.response, window.parent.location.href);
		});
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
	$('#OpenIdLoginButton').click(function() {
		doLogin(ajaxbox.value);
	});

	// Make popup window close on escape (the dialog style is already taken care of)
	$(document).keydown(function(e) {
		if (e.keyCode == $.ui.keyCode.ESCAPE) {
			window.close();
		}
	});

	{
		var log = function(msg) {
			//$('#log')[0].innerText += '\n' + msg;
		}

		var rate = NaN;
		var lastValue = ajaxbox.value;
		var keyPresses = 0;
		var startTime = null;
		var lastKeyPress = null;
		var discoveryTimer;

		function cancelTimer() {
			if (discoveryTimer) {
				log('canceling timer');
				clearTimeout(discoveryTimer);
				discoveryTimer = null;
			}
		}

		function identifierSanityCheck(id) {
			return id.match("^[=@+$!(].+|.*?\\.[^\.]+");
		}

		function discover() {
			cancelTimer();
			var id = ajaxbox.value;
			log(id + ": discover() invoked");
			if (identifierSanityCheck(id)) {
				log(id + ": discovering");
				setState('discovering ' + id);
				doLogin(id, true);
			} else {
				log(id + ": incomplete identifier. please continue");
				setState(id.length > 0 ? 'please continue' : '');
			}

			// TODO: run this code (and its opposite) to show the div if a claimed identifier
			// was provided, and to hide it if an OP identifier was provided.
			////$('#NotMyComputerDiv').show('fast');
		}

		function reset() {
			keyPresses = 0;
			startTime = null;
			rate = NaN;
			setState('');
			log('resetting state');
		}

		function setState(msg) {
			var state = $('#state')[0];
			if (state) {
				while (state.childNodes.length > 0) {
					state.removeChild(state.childNodes[0]);
				}

				state.appendChild(document.createTextNode(msg));
			}
		}

		$('#openid_identifier').keyup(function(e) {
			if (new Date() - lastKeyPress > 3000) {
				// the user seems to have altogether stopped typing,
				// so reset our typist speed detector.
				reset();
			}
			lastKeyPress = new Date();

			if (e.keyCode == $.ui.keyCode.ENTER) {
				discover();
			} else {
				var newValue = ajaxbox.value;
				if (lastValue != newValue) {
					if (newValue.length == 0) {
						reset();
					} else if (Math.abs(lastValue.length - newValue.length) > 1) {
						// One key press is responsible for multiple character changes.
						// The user may have pasted in his identifier in which case
						// we want to begin discovery immediately.
						log(newValue + ': paste detected (old value ' + lastValue + ')');
						discover();
					} else {
						keyPresses++;
						setState('');
						var timeout = 3000;
						if (startTime === null) {
							startTime = new Date();
						} else if (keyPresses > 1) {
							cancelTimer();
							rate = (new Date() - startTime) / keyPresses;
							var minTimeout = 300;
							var maxTimeout = 3000;
							var typistFactor = 5;
							timeout = Math.max(minTimeout, Math.min(rate * typistFactor, maxTimeout));
						}

						log(newValue + ': setting timer for ' + timeout);
						discoveryTimer = setTimeout(discover, timeout);
					}
				}
			}

			log(newValue + ': updating lastValue');
			lastValue = newValue;
		});

		$('#openid_identifier').blur(function() {
			reset();
			discover();
		});
	}
});