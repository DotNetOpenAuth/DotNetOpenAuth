Imports System.Net
Imports System.Web.Security
Imports DotNetOpenAuth.Messaging
Imports DotNetOpenAuth.OpenId
Imports DotNetOpenAuth.OpenId.Extensions.SimpleRegistration
Imports DotNetOpenAuth.OpenId.RelyingParty

Public Class LoginProgrammatic
	Inherits System.Web.UI.Page

	Private Shared relyingParty As New OpenIdRelyingParty

	Protected Sub openidValidator_ServerValidate(ByVal source As Object, ByVal args As ServerValidateEventArgs)
		' This catches common typos that result in an invalid OpenID Identifier.
		args.IsValid = Identifier.IsValid(args.Value)
	End Sub

	Protected Sub loginButton_Click(ByVal sender As Object, ByVal e As EventArgs)
		If Not Me.Page.IsValid Then
			Return
			' don't login if custom validation failed.
		End If
		Try
			Dim request As IAuthenticationRequest = relyingParty.CreateRequest(Me.openIdBox.Text)
			' This is where you would add any OpenID extensions you wanted
			' to include in the authentication request.
			request.AddExtension(New ClaimsRequest() With { _
			.Country = DemandLevel.Request, _
			.Email = DemandLevel.Request, _
			.Gender = DemandLevel.Require, _
			.PostalCode = DemandLevel.Require, _
			.TimeZone = DemandLevel.Require _
			})
			' Send your visitor to their Provider for authentication.
			request.RedirectToProvider()
		Catch ex As ProtocolException
			' The user probably entered an Identifier that 
			' was not a valid OpenID endpoint.
			Me.openidValidator.Text = ex.Message
			Me.openidValidator.IsValid = False
		End Try
	End Sub

	Protected Sub Page_Load(ByVal sender As Object, ByVal e As EventArgs) Handles Me.Load
		Me.openIdBox.Focus()
		' For debugging/testing, we allow remote clearing of all associations...
		' NOT a good idea on a production site.
		If (Request.QueryString("clearAssociations") = "1") Then
			Application.Remove("DotNetOpenAuth.OpenId.RelyingParty.OpenIdRelyingParty.ApplicationStore")
			' Force a redirect now to prevent the user from logging in while associations
			' are constantly being cleared.
			Dim builder As UriBuilder = New UriBuilder(Request.Url)
			builder.Query = Nothing
			Me.Response.Redirect(builder.Uri.AbsoluteUri)
		End If
		Dim response As IAuthenticationResponse = relyingParty.GetResponse
		If response IsNot Nothing Then
			Select Case response.Status
				Case AuthenticationStatus.Authenticated
					' This is where you would look for any OpenID extension responses included
					' in the authentication assertion.
					Dim claimsResponse As ClaimsResponse = response.GetExtension(Of ClaimsResponse)()
					State.ProfileFields = claimsResponse
					' Store off the "friendly" username to display -- NOT for username lookup
					State.FriendlyLoginName = response.FriendlyIdentifierForDisplay
					' Use FormsAuthentication to tell ASP.NET that the user is now logged in,
					' with the OpenID Claimed Identifier as their username.
					FormsAuthentication.RedirectFromLoginPage(response.ClaimedIdentifier, False)
				Case AuthenticationStatus.Canceled
					Me.loginCanceledLabel.Visible = True
				Case AuthenticationStatus.Failed
					Me.loginFailedLabel.Visible = True
			End Select
		End If
	End Sub
End Class