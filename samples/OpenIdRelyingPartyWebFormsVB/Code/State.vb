Imports DotNetOpenAuth.OpenId.Extensions.ProviderAuthenticationPolicy
Imports DotNetOpenAuth.OpenId.Extensions.SimpleRegistration
Imports DotNetOpenAuth.OpenId.Extensions.AttributeExchange

Public Class State
	Public Shared Property ProfileFields() As ClaimsResponse
		Get
			Return HttpContext.Current.Session("ProfileFields")
		End Get
		Set(ByVal value As ClaimsResponse)
			HttpContext.Current.Session("ProfileFields") = value
		End Set
	End Property

	Public Shared Property FetchResponse() As FetchResponse
		Get
			Return HttpContext.Current.Session("FetchResponse")
		End Get
		Set(ByVal value As FetchResponse)
			HttpContext.Current.Session("FetchResponse") = value
		End Set
	End Property

	Public Shared Property FriendlyLoginName() As String
		Get
			Return HttpContext.Current.Session("FriendlyLoginName")
		End Get
		Set(ByVal value As String)
			HttpContext.Current.Session("FriendlyLoginName") = value
		End Set
	End Property

	Public Shared Property PapePolicies() As PolicyResponse
		Get
			Return HttpContext.Current.Session("PapePolicies")
		End Get
		Set(ByVal value As PolicyResponse)
			HttpContext.Current.Session("PapePolicies") = value
		End Set
	End Property

	Public Shared Sub Clear()
		FriendlyLoginName = Nothing
	End Sub

End Class
