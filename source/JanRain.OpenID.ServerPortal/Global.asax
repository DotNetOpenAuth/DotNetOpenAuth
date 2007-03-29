<%@ Application Language="C#" %>

<script runat="server">

	protected void Application_BeginRequest(Object sender, EventArgs e)
	{
        /*
         * The URLRewriter was taken from http://www.codeproject.com/aspnet/URLRewriter.asp and modified slightly.
         * It will read the config section called 'urlrewrites' from web.config and process each rule 
         * The rules are set of url transformations defined using regular expressions with support for substitutions (the ability to extract regex-matched portions of a string).
         * There is only one rule currenty defined. It rewrites urls like: user/john ->user.aspx?username=john
         */
        //System.Diagnostics.Debugger.Launch();
        Janrain.OpenId.ServerPortal.URLRewriter.Process();
	}
       
</script>
