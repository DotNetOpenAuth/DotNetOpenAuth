Imports DotNetOpenAuth.OpenId
Imports DotNetOpenAuth.OpenId.Provider

Partial Class RP_TrailingPeriodsInClaimedIdPath
	Inherits System.Web.UI.Page

	Protected Sub Page_Load(ByVal sender As Object, ByVal e As EventArgs)
		If Request.PathInfo = "/a." Then
			IdentityEndpoint1.Enabled = True
			IdentityEndpoint2.Enabled = True
		End If
	End Sub

	Protected Sub ProviderEndpoint1_AuthenticationChallenge(ByVal sender As Object, ByVal e As AuthenticationChallengeEventArgs)
		If e.Request.ClaimedIdentifier.ToString().EndsWith("/a.", StringComparison.Ordinal) Then
			' Proceed with authentication to test whether the RP can actually receive the assertion.
			e.Request.IsAuthenticated = True
		Else
			MultiView1.SetActiveView(ResultsView)
			testResultDisplay.Pass = False
			testResultDisplay.Details = "The RP sent the wrong claimed identifier in the request: " + e.Request.ClaimedIdentifier.ToString()
		End If
	End Sub
End Class
