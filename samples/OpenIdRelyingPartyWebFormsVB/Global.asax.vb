Imports System
Imports System.Collections.Specialized
Imports System.Configuration
Imports System.IO
Imports System.Text
Imports System.Web
Imports DotNetOpenAuth.ApplicationBlock
Imports OpenIdRelyingPartyWebFormsVB

Public Class Global_asax
	Inherits HttpApplication

	Public Shared Logger As log4net.ILog = log4net.LogManager.GetLogger(GetType(Global_asax))

	Friend Shared LogMessages As StringBuilder = New StringBuilder

	Public Shared Function CollectionToString(ByVal collection As NameValueCollection) As String
		Dim sw As StringWriter = New StringWriter
		For Each key As String In collection.Keys
			sw.WriteLine("{0} = '{1}'", key, collection(key))
		Next
		Return sw.ToString
	End Function

	Protected Sub Application_Start(ByVal sender As Object, ByVal e As EventArgs)
		log4net.Config.XmlConfigurator.Configure()
		Logger.Info("Sample starting...")
	End Sub

	Protected Sub Application_End(ByVal sender As Object, ByVal e As EventArgs)
		Logger.Info("Sample shutting down...")
		' this would be automatic, but in partial trust scenarios it is not.
		log4net.LogManager.Shutdown()
	End Sub

	Protected Sub Application_BeginRequest(ByVal sender As Object, ByVal e As EventArgs)
		' System.Diagnostics.Debugger.Launch();
		Logger.DebugFormat("Processing {0} on {1} ", Request.HttpMethod, stripQueryString(Request.Url))
		If (Request.QueryString.Count > 0) Then
			Logger.DebugFormat("Querystring follows: " & vbLf & "{0}", CollectionToString(Request.QueryString))
		End If
		If (Request.Form.Count > 0) Then
			Logger.DebugFormat("Posted form follows: " & vbLf & "{0}", CollectionToString(Request.Form))
		End If
	End Sub

	Protected Sub Application_AuthenticateRequest(ByVal sender As Object, ByVal e As EventArgs)
		Logger.DebugFormat("User {0} authenticated.", (Not (HttpContext.Current.User) Is Nothing))
		'TODO: Warning!!!, inline IF is not supported ?
	End Sub

	Protected Sub Application_EndRequest(ByVal sender As Object, ByVal e As EventArgs)

	End Sub

	Protected Sub Application_Error(ByVal sender As Object, ByVal e As EventArgs)
		Logger.ErrorFormat("An unhandled exception was raised. Details follow: {0}", HttpContext.Current.Server.GetLastError)
	End Sub

	Private Shared Function stripQueryString(ByVal uri As Uri) As String
		Dim builder As UriBuilder = New UriBuilder(uri)
		builder.Query = Nothing
		Return builder.ToString
	End Function
End Class