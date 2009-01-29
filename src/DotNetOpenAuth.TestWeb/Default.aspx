<%@ Page Language="C#" %>

<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">

<script runat="server">

</script>

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
	<title></title>
</head>
<body>
	<form id="form1" runat="server">
	<div>
		<!-- this next string is assembled deliberately so that a test can string search
		     for the generated result and determine that ASP.NET is setup correctly. -->
		<p>
			<%="Test" + " home page" %>
		</p>
	</div>
	</form>
</body>
</html>
