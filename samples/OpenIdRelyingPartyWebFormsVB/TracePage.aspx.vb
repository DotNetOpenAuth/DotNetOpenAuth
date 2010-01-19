Public Class TracePage
	Inherits System.Web.UI.Page

	Protected Sub Page_Load(ByVal sender As Object, ByVal e As EventArgs)
		Me.placeHolder1.Controls.Add(New Label() With {.Text = HttpUtility.HtmlEncode(Global_asax.LogMessages.ToString())})
	End Sub

	Protected Sub clearLogButton_Click(ByVal sender As Object, ByVal e As EventArgs)
		Global_asax.LogMessages.Length = 0
		' clear the page immediately, and allow for F5 without a Postback warning.
		Me.Response.Redirect(Me.Request.Url.AbsoluteUri)
	End Sub
End Class