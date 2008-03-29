<%@ Application Language="C#" %>

<script RunAt="server">

	void Application_Error(object sender, EventArgs e) {
		// Code that runs when an unhandled error occurs
		Exception ex = HttpContext.Current.Error;
		System.Diagnostics.Trace.WriteLine(ex.ToString());
	}

</script>

