<%@ Application Language="C#" %>

<script RunAt="server">
	public static log4net.ILog Logger = log4net.LogManager.GetLogger(typeof(global_asax));

	void Application_Error(object sender, EventArgs e) {
		// Code that runs when an unhandled error occurs
		Exception ex = HttpContext.Current.Error;
		Logger.Error(ex.ToString());
	}

</script>

