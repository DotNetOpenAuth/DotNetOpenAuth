using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

public partial class TestResultDisplay : System.Web.UI.UserControl {
	public bool Pass {
		set {
			this.passLabel.Visible = value;
			this.failLabel.Visible = !value;
		}
	}

	public Uri ProviderEndpoint {
		set {
			this.endpointLabel.Text = value.AbsoluteUri;
		}
	}

	public Version ProtocolVersion {
		set {
			this.protocolVersion.Text = value.ToString();
		}
	}

	public string Details {
		set {
			this.detailsLabel.Text = HttpUtility.HtmlEncode(value);
		}
	}

	protected void Page_Load(object sender, EventArgs e) {

	}
}
