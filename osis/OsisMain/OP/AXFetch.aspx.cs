using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using DotNetOpenAuth.OpenId.RelyingParty;
using DotNetOpenAuth.OpenId.Extensions.AttributeExchange;
using System.Data;

public partial class OP_AXFetch : System.Web.UI.Page {
	private List<AttributeDescription> Attributes;

	public class AttributeDescription {
		public AttributeDescription(string description, string typeUri) {
			this.Description = description;
			this.TypeUri = typeUri;
		}

		public string Description;
		public string TypeUri;
		public Label ValueLabel;

		public TextBox RequestCountBox;
		public int RequestCount {
			get { return int.Parse(this.RequestCountBox.Text); }
		}
		public CheckBox RequestRequiredCheckBox;
		public bool RequestRequired {
			get { return RequestRequiredCheckBox.Checked; }
		}
	}

	protected override void CreateChildControls() {
		base.CreateChildControls();

		this.BuildRequestTable();
		this.BuildResponseTable();
	}

	protected void Page_Load(object sender, EventArgs e) {
		this.OpenIdBox.LoggingIn += this.OpenIdBox_LoggingIn;
		this.OpenIdBox.LoggedIn += this.OpenIdBox_LoggedIn;
	}

	private void BuildAttributes() {
		if (this.Attributes != null) {
			return;
		}

		this.Attributes = new List<AttributeDescription>() {
			new AttributeDescription("Nickname", WellKnownAttributes.Name.Alias),
			new AttributeDescription("Email", WellKnownAttributes.Contact.Email),
			new AttributeDescription("Country", WellKnownAttributes.Contact.HomeAddress.Country),
			new AttributeDescription("Gender", WellKnownAttributes.Person.Gender),
			new AttributeDescription("Language", WellKnownAttributes.Preferences.Language),
			new AttributeDescription("Timezone", WellKnownAttributes.Preferences.TimeZone),
			new AttributeDescription("Full name", WellKnownAttributes.Name.FullName),
			new AttributeDescription("Birthdate", WellKnownAttributes.BirthDate.WholeBirthDate),
			new AttributeDescription("Postal code", WellKnownAttributes.Contact.HomeAddress.PostalCode),
		};
	}

	private void BuildRequestTable() {
		this.BuildAttributes();
		foreach (var att in Attributes) {
			TableRow row = new TableRow();

			TableCell descriptionCell = new TableCell { Text = att.Description };
			row.Cells.Add(descriptionCell);

			TableCell typeUriCell = new TableCell { Text = att.TypeUri };
			row.Cells.Add(typeUriCell);

			TableCell countCell = new TableCell();
			countCell.Controls.Add(att.RequestCountBox = new TextBox {
				MaxLength = 2,
				Columns = 2,
				Text = "1",
			});
			row.Cells.Add(countCell);

			TableCell requiredCell = new TableCell();
			requiredCell.Controls.Add(att.RequestRequiredCheckBox = new CheckBox());
			row.Cells.Add(requiredCell);

			FetchRequestTable.Rows.Add(row);
		}
	}

	private void BuildResponseTable() {
		this.BuildAttributes();
		if (this.FetchResponseTable.Rows.Count > 1) {
			return;
		}

		foreach (var att in Attributes) {
			TableRow row = new TableRow();

			TableCell descriptionCell = new TableCell { Text = att.Description };
			row.Cells.Add(descriptionCell);

			TableCell valueCell = new TableCell();
			valueCell.Controls.Add(att.ValueLabel = new Label());
			row.Cells.Add(valueCell);

			FetchResponseTable.Rows.Add(row);
		}
	}

	private void OpenIdBox_LoggingIn(object sender, OpenIdEventArgs e) {
		var fetch = new FetchRequest();
		foreach (var att in this.Attributes) {
			fetch.AddAttribute(new AttributeRequest(att.TypeUri, att.RequestRequired, att.RequestCount));
		}
		e.Request.AddExtension(fetch);
	}

	private void OpenIdBox_LoggedIn(object sender, OpenIdEventArgs e) {
		e.Cancel = true;
		MultiView1.ActiveViewIndex = 1;

		FetchResponse fetch = e.Response.GetExtension<FetchResponse>();
		this.BuildAttributes();
		this.BuildResponseTable();
		foreach (var att in this.Attributes) {
			att.ValueLabel.Text = HttpUtility.HtmlEncode("<absent>");
			if (fetch != null) {
				AttributeValues attValue = fetch.GetAttribute(att.TypeUri);
				if (attValue != null) {
					att.ValueLabel.Text = HttpUtility.HtmlEncode(attValue.Values.FirstOrDefault());

				}
			}
		}
	}
}
