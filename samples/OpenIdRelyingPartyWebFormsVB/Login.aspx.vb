Imports DotNetOpenAuth.OpenId.Extensions.ProviderAuthenticationPolicy
Imports DotNetOpenAuth.OpenId.RelyingParty
Imports DotNetOpenAuth.OpenId.Extensions.SimpleRegistration

Partial Public Class Login
	Inherits System.Web.UI.Page

	Protected Sub Page_Load(ByVal sender As Object, ByVal e As System.EventArgs) Handles Me.Load
		OpenIdLogin1.Focus()
	End Sub

	Protected Sub requireSslCheckBox_CheckedChanged(ByVal sender As Object, ByVal e As EventArgs)
		OpenIdLogin1.RequireSsl = requireSslCheckBox.Checked
	End Sub

	Protected Sub OpenIdLogin1_LoggingIn(ByVal sender As Object, ByVal e As DotNetOpenAuth.OpenId.RelyingParty.OpenIdEventArgs) Handles OpenIdLogin1.LoggingIn
		prepareRequest(e.Request)
	End Sub

	''' <summary>
	''' Fired upon login.
	''' </summary>
	''' <param name="sender">The source of the event.</param>
	''' <param name="e">The <see cref="DotNetOpenAuth.OpenId.RelyingParty.OpenIdEventArgs"/> instance containing the event data.</param>
	''' <remarks>
	''' Note, that straight after login, forms auth will redirect the user
	''' to their original page. So this page may never be rendererd.
	''' </remarks>
	Protected Sub OpenIdLogin1_LoggedIn(ByVal sender As Object, ByVal e As DotNetOpenAuth.OpenId.RelyingParty.OpenIdEventArgs) Handles OpenIdLogin1.LoggedIn
		State.FriendlyLoginName = e.Response.FriendlyIdentifierForDisplay
		State.ProfileFields = e.Response.GetExtension(Of ClaimsResponse)()
		State.PapePolicies = e.Response.GetExtension(Of PolicyResponse)()
	End Sub

	Private Sub prepareRequest(ByVal request As IAuthenticationRequest)
		' Collect the PAPE policies requested by the user.
		Dim policies As New List(Of String)
		For Each item As ListItem In Me.papePolicies.Items
			If item.Selected Then
				policies.Add(item.Value)
			End If
		Next
		' Add the PAPE extension if any policy was requested.
		If (policies.Count > 0) Then
			Dim pape As New PolicyRequest
			For Each policy As String In policies
				pape.PreferredPolicies.Add(policy)
			Next
			request.AddExtension(pape)
		End If
	End Sub
End Class