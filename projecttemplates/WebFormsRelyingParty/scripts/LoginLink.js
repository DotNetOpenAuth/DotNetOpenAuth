$(function() {
	var loginContent = 'LoginFrame.aspx';
	var popupWindowName = 'openidlogin';
	var popupWidth = 355;
	var popupHeight = 270; // use 205 for 1 row of OP buttons

	{
		var div = window.document.createElement('div');
		div.style.padding = 0;
		div.id = 'loginDialog';

		var iframe = window.document.createElement('iframe');
		iframe.src = loginContent;
		iframe.frameBorder = 0;
		iframe.width = popupWidth;
		iframe.height = popupHeight;
		div.appendChild(iframe);

		window.document.body.appendChild(div);
	}

	$(document).ready(function() {
		$("#loginDialog").dialog({
			bgiframe: true,
			modal: true,
			title: 'Login or register',
			resizable: false,
			hide: 'clip',
			width: popupWidth,
			height: popupHeight + 50,
			buttons: {},
			closeOnEscape: true,
			autoOpen: false,
			focus: function(event, ui) {
				//				var box = $('#openid_identifier')[0];
				//				if (box.style.display != 'none') {
				//					box.focus();
				//				}
			}
		});

		$('.loginPopupLink').click(function() {
			$("#loginDialog").dialog('open');
		});
		$('.loginWindowLink').click(function() {
			if (window.showModalDialog) {
				window.showModalDialog(loginContent, popupWindowName, 'status:0;resizable:1;scroll:1;center:1;dialogHeight:' + popupHeight + 'px;dialogWidth:' + popupWidth + 'px');
			} else {
				window.open(loginContent, popupWindowName, 'modal=yes,status=0,location=1,toolbar=0,menubar=0,resizable=0,scrollbars=0,height=' + popupHeight + 'px,width=' + popupWidth + 'px');
			}
		});
	});
});
