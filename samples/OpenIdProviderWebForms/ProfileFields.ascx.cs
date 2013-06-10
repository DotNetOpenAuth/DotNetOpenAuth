namespace OpenIdProviderWebForms {
	using System;
	using DotNetOpenAuth.OpenId.Extensions;
	using DotNetOpenAuth.OpenId.Extensions.SimpleRegistration;

	/// <summary>
	/// Handles the collection of the simple registration fields.
	/// Only mandatory or optional fields are displayed. Mandatory fields have a '*' next to them.
	/// No validation occurs here.
	/// </summary>
	public partial class ProfileFields : System.Web.UI.UserControl {
		public bool DoesAnyFieldHaveAValue {
			get {
				return !((this.DateOfBirth == null)
				&& string.IsNullOrEmpty(this.countryDropdownList.SelectedValue)
				&& string.IsNullOrEmpty(this.emailTextBox.Text)
				&& string.IsNullOrEmpty(this.fullnameTextBox.Text)
				&& (this.Gender == null)
				&& string.IsNullOrEmpty(this.languageDropdownList.SelectedValue)
				&& string.IsNullOrEmpty(this.nicknameTextBox.Text)
				&& string.IsNullOrEmpty(this.postcodeTextBox.Text)
				&& string.IsNullOrEmpty(this.timezoneDropdownList.SelectedValue));
			}
		}

		public DateTime? DateOfBirth {
			get {
				int day, month, year;
				if (int.TryParse(this.dobDayDropdownlist.SelectedValue, out day)
					&& int.TryParse(this.dobMonthDropdownlist.SelectedValue, out month)
					&& int.TryParse(this.dobYearDropdownlist.SelectedValue, out year)) {
					var newDate = new DateTime(year, month, day);
					return newDate;
				}

				return null;
			}

			set {
				if (value.HasValue) {
					this.dobDayDropdownlist.SelectedValue = value.Value.Day.ToString();
					this.dobMonthDropdownlist.SelectedValue = value.Value.Month.ToString();
					this.dobYearDropdownlist.SelectedValue = value.Value.Year.ToString();
				} else {
					this.dobDayDropdownlist.SelectedValue = string.Empty;
					this.dobMonthDropdownlist.SelectedValue = string.Empty;
					this.dobYearDropdownlist.SelectedValue = string.Empty;
				}
			}
		}

		public Gender? Gender {
			get {
				if (this.genderDropdownList.SelectedValue == "Male") {
					return DotNetOpenAuth.OpenId.Extensions.SimpleRegistration.Gender.Male;
				}
				if (this.genderDropdownList.SelectedValue == "Female") {
					return DotNetOpenAuth.OpenId.Extensions.SimpleRegistration.Gender.Female;
				}
				return null;
			}

			set {
				if (value.HasValue) {
					this.genderDropdownList.SelectedValue = value.Value.ToString();
				} else {
					this.genderDropdownList.SelectedIndex = -1;
				}
			}
		}

		public void SetRequiredFieldsFromRequest(ClaimsRequest requestFields) {
			if (requestFields.PolicyUrl != null) {
				this.privacyLink.NavigateUrl = requestFields.PolicyUrl.AbsoluteUri;
			} else {
				this.privacyLink.Visible = false;
			}

			this.dobRequiredLabel.Visible = requestFields.BirthDate == DemandLevel.Require;
			this.countryRequiredLabel.Visible = requestFields.Country == DemandLevel.Require;
			this.emailRequiredLabel.Visible = requestFields.Email == DemandLevel.Require;
			this.fullnameRequiredLabel.Visible = requestFields.FullName == DemandLevel.Require;
			this.genderRequiredLabel.Visible = requestFields.Gender == DemandLevel.Require;
			this.languageRequiredLabel.Visible = requestFields.Language == DemandLevel.Require;
			this.nicknameRequiredLabel.Visible = requestFields.Nickname == DemandLevel.Require;
			this.postcodeRequiredLabel.Visible = requestFields.PostalCode == DemandLevel.Require;
			this.timezoneRequiredLabel.Visible = requestFields.TimeZone == DemandLevel.Require;

			this.dateOfBirthRow.Visible = !(requestFields.BirthDate == DemandLevel.NoRequest);
			this.countryRow.Visible = !(requestFields.Country == DemandLevel.NoRequest);
			this.emailRow.Visible = !(requestFields.Email == DemandLevel.NoRequest);
			this.fullnameRow.Visible = !(requestFields.FullName == DemandLevel.NoRequest);
			this.genderRow.Visible = !(requestFields.Gender == DemandLevel.NoRequest);
			this.languageRow.Visible = !(requestFields.Language == DemandLevel.NoRequest);
			this.nicknameRow.Visible = !(requestFields.Nickname == DemandLevel.NoRequest);
			this.postcodeRow.Visible = !(requestFields.PostalCode == DemandLevel.NoRequest);
			this.timezoneRow.Visible = !(requestFields.TimeZone == DemandLevel.NoRequest);
		}

		public ClaimsResponse GetOpenIdProfileFields(ClaimsRequest request) {
			if (request == null) {
				throw new ArgumentNullException("request");
			}

			ClaimsResponse fields = request.CreateResponse();
			fields.BirthDate = this.DateOfBirth;
			fields.Country = this.countryDropdownList.SelectedValue;
			fields.Email = this.emailTextBox.Text;
			fields.FullName = this.fullnameTextBox.Text;
			fields.Gender = this.Gender;
			fields.Language = this.languageDropdownList.SelectedValue;
			fields.Nickname = this.nicknameTextBox.Text;
			fields.PostalCode = this.postcodeTextBox.Text;
			fields.TimeZone = this.timezoneDropdownList.SelectedValue;
			return fields;
		}

		public void SetOpenIdProfileFields(ClaimsResponse value) {
			if (value == null) {
				throw new ArgumentNullException("value");
			}

			this.DateOfBirth = value.BirthDate;
			this.countryDropdownList.SelectedValue = value.Country;
			this.emailTextBox.Text = value.Email;
			this.fullnameTextBox.Text = value.FullName;
			this.Gender = value.Gender;
			this.languageDropdownList.SelectedValue = value.Language;
			this.nicknameTextBox.Text = value.Nickname;
			this.postcodeTextBox.Text = value.PostalCode;
			this.timezoneDropdownList.SelectedValue = value.TimeZone;
		}

		protected void Page_Load(object sender, EventArgs e) {
		}
	}
}