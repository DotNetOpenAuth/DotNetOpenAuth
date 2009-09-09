<%@ Page Language="C#" %>

<%@ Import Namespace="System.IO" %>
<%@ Import Namespace="System.Linq" %>

<script runat="server">
	protected virtual void Page_Load(object sender, EventArgs e) {
		string apiPath = Path.GetDirectoryName(Request.PhysicalPath);
		string htmlPath = Path.Combine(apiPath, "html");
		var namespaceFiles = Directory.GetFiles(htmlPath, "N_*.htm*").Select(ns => Path.GetFileName(ns));
		NamespaceRepeater.DataSource = namespaceFiles.Select(ns => new {
			Href = "html/" + ns,
			Title = Path.GetFileNameWithoutExtension(ns).Substring(2).Replace("_", "."),
		});
		NamespaceRepeater.DataBind();
	}
</script>

<html>
<head>
	<title>DotNetOpenAuth API documentation</title>
</head>
<body>
	<h1>DotNetOpenAuth API documentation</h1>
	<h2>Documentation by namespace</h2>
	<ul>
		<asp:Repeater runat="server" ID="NamespaceRepeater">
			<ItemTemplate>
				<li><a href="<%# Eval("Href") %>">
					<%# Eval("Title") %></a></li>
			</ItemTemplate>
		</asp:Repeater>
	</ul>
</body>
</html>
