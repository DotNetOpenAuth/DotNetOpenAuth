using System;
using System.Configuration;
using System.Data;
using System.Web;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;
using System.Web.UI.WebControls.WebParts;
using System.IO;

public partial class HostTest : System.Web.UI.Page {
	protected void Page_Load(object sender, EventArgs e) {
		queryLabel.Text = Request.Url.Query;
		using (StreamReader sr = new StreamReader(Request.InputStream))
			bodyLabel.Text = sr.ReadToEnd();
	}
}
