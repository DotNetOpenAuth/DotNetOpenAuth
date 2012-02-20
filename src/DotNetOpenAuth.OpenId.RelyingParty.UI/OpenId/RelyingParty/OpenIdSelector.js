//-----------------------------------------------------------------------
// <copyright file="OpenIdSelector.js" company="Outercurve Foundation>
//     Copyright (c) Outercurve Foundation. All rights reserved.
//     This file may be used and redistributed under the terms of the
//     Microsoft Public License (Ms-PL) http://opensource.org/licenses/ms-pl.html
// </copyright>
//-----------------------------------------------------------------------

$(function() {
	var hint = $.cookie('openid_identifier') || '';

	var ajaxbox = document.getElementsByName('openid_identifier')[0];
	if (ajaxbox && hint != 'infocard') {
		ajaxbox.setValue(hint);
	}

	if (document.infoCard && document.infoCard.isSupported()) {
		$('ul.OpenIdProviders li.infocard')[0].style.display = 'inline-block';
	}

	if (hint.length > 0) {
		var ops = $('ul.OpenIdProviders li');
		ops.addClass('grayedOut');
		var matchFound = false;
		ops.each(function(i, li) {
			if (li.id == hint || (hint == 'infocard' && $(li).hasClass('infocard'))) {
				$(li)
					.removeClass('grayedOut')
					.addClass('focused');
				matchFound = true;
			}
		});
		if (!matchFound) {
			if (ajaxbox) {
				$('#OpenIDButton')
				.removeClass('grayedOut')
				.addClass('focused');
				$('#OpenIDForm').show('slow', function() {
					ajaxbox.focus();
				});
			} else {
				// No OP button matched the last identifier, and there is no text box,
				// so just un-gray all buttons.
				ops.removeClass('grayedOut');
			}
		}
	}

	function showLoginSuccess(userSuppliedIdentifier, success) {
		var li = document.getElementById(userSuppliedIdentifier);
		if (li) {
			if (success) {
				$(li).addClass('loginSuccess');
			} else {
				$(li).removeClass('loginSuccess');
			}
		}
	}

	window.dnoa_internal.addAuthSuccess(function(discoveryResult, serviceEndpoint, extensionResponses, state) {
		showLoginSuccess(discoveryResult.userSuppliedIdentifier, true);
	});

	window.dnoa_internal.addAuthCleared(function(discoveryResult, serviceEndpoint) {
		showLoginSuccess(discoveryResult.userSuppliedIdentifier, false);

		// If this is an OP button, renew the positive assertion.
		var li = document.getElementById(discoveryResult.userSuppliedIdentifier);
		if (li) {
			li.loginBackground();
		}
	});

	if (ajaxbox) {
		ajaxbox.onStateChanged = function(state) {
			if (state == "authenticated") {
				showLoginSuccess('OpenIDButton', true);
			} else {
				showLoginSuccess('OpenIDButton', false); // hide checkmark
			}
		};
	}

	function checkidSetup(identifier, timerBased) {
		var openid = new window.OpenIdIdentifier(identifier);
		if (!openid) { throw 'checkidSetup called without an identifier.'; }
		openid.login(function(discoveryResult, respondingEndpoint, extensionResponses) {
			doLogin(discoveryResult, respondingEndpoint);
		});
	}

	// Sends the positive assertion we've collected to the server and actually logs the user into the RP.
	function doLogin(discoveryResult, respondingEndpoint) {
		var retain = true; //!$('#NotMyComputer')[0].selected;
		$.cookie('openid_identifier', retain ? discoveryResult.userSuppliedIdentifier : null, { path: window.aspnetapppath });
		window.postLoginAssertion(respondingEndpoint.response.toString(), window.parent.location.href);
	}

	if (ajaxbox) {
		// take over how the text box does postbacks.
		ajaxbox.dnoi_internal.postback = doLogin;
	}

	// This FrameManager will be used for background logins for the OP buttons
	// and the last used identifier.  It is NOT the frame manager used by the
	// OpenIdAjaxTextBox, as it has its own.
	var backgroundTimeout = 3000;

	$(document).ready(function() {
		var ops = $('ul.OpenIdProviders li');
		ops.each(function(i, li) {
			if ($(li).hasClass('OPButton')) {
				li.authenticationIFrames = new window.dnoa_internal.FrameManager(1/*throttle*/);
				var openid = new window.OpenIdIdentifier(li.id);
				var authFrames = li.authenticationIFrames;
				if ($(li).hasClass('NoAsyncAuth')) {
					li.loginBackground = function() { };
				} else {
					li.loginBackground = function() {
						openid.loginBackground(authFrames, null, null, backgroundTimeout);
					};
				}
				li.loginBackground();
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
		var wasGrayedOut = false;
		if ($(this).hasClass('grayedOut')) {
			wasGrayedOut = true;
			$('ul.OpenIdProviders li').removeClass('grayedOut');
		}

		// Be sure to hide the openid_identifier text box unless the OpenID button is selected.
		if ($(this)[0] != $('#OpenIDButton')[0] && $('#OpenIDForm').is(':visible')) {
			$('#OpenIDForm').hide('slow');
		}

		var relevantUserSuppliedIdentifier = null;
		// Don't immediately login if the user clicked OpenID and he can't see the identifier box.
		if ($(this)[0].id != 'OpenIDButton') {
			relevantUserSuppliedIdentifier = $(this)[0].id;
		} else if (ajaxbox && $('#OpenIDForm').is(':visible')) {
			relevantUserSuppliedIdentifier = ajaxbox.value;
		}

		var discoveryResult = window.dnoa_internal.discoveryResults[relevantUserSuppliedIdentifier];
		var respondingEndpoint = discoveryResult ? discoveryResult.findSuccessfulRequest() : null;

		// If the user clicked on a button that has the "we're ready to log you in immediately",
		// then log them in!
		if (respondingEndpoint) {
			doLogin(discoveryResult, respondingEndpoint);
		} else if ($(this).hasClass('OPButton')) {
			checkidSetup($(this)[0].id);
		} else if ($(this).hasClass('infocard') && wasGrayedOut) {
			// we need to forward the click onto the InfoCard image so it is handled, since our
			// gray overlaying div captured the click event.
			$('img', this)[0].click();
		}
	});
	if (ajaxbox) {
		$('#OpenIDButton').click(function() {
			// Be careful to only try to select the text box once it is available.
			if ($('#OpenIDForm').is(':hidden')) {
				$('#OpenIDForm').show('slow', function() {
					ajaxbox.focus();
				});
			} else {
				ajaxbox.focus();
			}
		});

		$(ajaxbox.form).keydown(function(e) {
			if (e.keyCode == $.ui.keyCode.ENTER) {
				// we do NOT want to submit the form on ENTER.
				e.preventDefault();
			}
		});
	}

	// Make popup window close on escape (the dialog style is already taken care of)
	$(document).keydown(function(e) {
		if (e.keyCode == $.ui.keyCode.ESCAPE) {
			window.close();
		}
	});
});