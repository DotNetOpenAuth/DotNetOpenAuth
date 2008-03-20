using System;
using System.Net.Mail;
using DotNetOpenId.Provider;
using DotNetOpenId.Extensions;

/// <summary>
/// Handles the collection of the simple registration fields.
/// Only mandatory or optional fields are displayed. Mandatory fields have a '*' next to them.
/// No validation occurs here.
/// </summary>
public partial class ProfileFields : System.Web.UI.UserControl {
	protected void Page_Load(object sender, EventArgs e) {
	}

	public void SetRequiredFieldsFromRequest(SimpleRegistrationRequestFields requestFields) {
		if (requestFields.PolicyUrl != null) {
			privacyLink.NavigateUrl = requestFields.PolicyUrl.AbsoluteUri;
		} else {
			privacyLink.Visible = false;
		}

		dobRequiredLabel.Visible = (requestFields.BirthDate == SimpleRegistrationRequest.Require);
		countryRequiredLabel.Visible = (requestFields.Country == SimpleRegistrationRequest.Require);
		emailRequiredLabel.Visible = (requestFields.Email == SimpleRegistrationRequest.Require);
		fullnameRequiredLabel.Visible = (requestFields.FullName == SimpleRegistrationRequest.Require);
		genderRequiredLabel.Visible = (requestFields.Gender == SimpleRegistrationRequest.Require);
		languageRequiredLabel.Visible = (requestFields.Language == SimpleRegistrationRequest.Require);
		nicknameRequiredLabel.Visible = (requestFields.Nickname == SimpleRegistrationRequest.Require);
		postcodeRequiredLabel.Visible = (requestFields.PostalCode == SimpleRegistrationRequest.Require);
		timezoneRequiredLabel.Visible = (requestFields.TimeZone == SimpleRegistrationRequest.Require);

		dateOfBirthRow.Visible = !(requestFields.BirthDate == SimpleRegistrationRequest.NoRequest);
		countryRow.Visible = !(requestFields.Country == SimpleRegistrationRequest.NoRequest);
		emailRow.Visible = !(requestFields.Email == SimpleRegistrationRequest.NoRequest);
		fullnameRow.Visible = !(requestFields.FullName == SimpleRegistrationRequest.NoRequest);
		genderRow.Visible = !(requestFields.Gender == SimpleRegistrationRequest.NoRequest);
		languageRow.Visible = !(requestFields.Language == SimpleRegistrationRequest.NoRequest);
		nicknameRow.Visible = !(requestFields.Nickname == SimpleRegistrationRequest.NoRequest);
		postcodeRow.Visible = !(requestFields.PostalCode == SimpleRegistrationRequest.NoRequest);
		timezoneRow.Visible = !(requestFields.TimeZone == SimpleRegistrationRequest.NoRequest);
	}

	public bool DoesAnyFieldHaveAValue {
		get {
			return !((DateOfBirth == null)
			&& String.IsNullOrEmpty(countryDropdownList.SelectedValue)
			&& String.IsNullOrEmpty(emailTextBox.Text)
			&& String.IsNullOrEmpty(fullnameTextBox.Text)
			&& (Gender == null)
			&& String.IsNullOrEmpty(languageDropdownList.SelectedValue)
			&& String.IsNullOrEmpty(nicknameTextBox.Text)
			&& String.IsNullOrEmpty(postcodeTextBox.Text)
			&& String.IsNullOrEmpty(timezoneDropdownList.SelectedValue));
		}
	}

	public DateTime? DateOfBirth {
		get {
			try {
				int day = Convert.ToInt32(dobDayDropdownlist.SelectedValue);
				int month = Convert.ToInt32(dobMonthDropdownlist.SelectedValue);
				int year = Convert.ToInt32(dobYearDropdownlist.SelectedValue);
				DateTime newDate = new DateTime(year, month, day);
				return newDate;
			} catch (Exception) {
				return null;
			}
		}
		set {
			if (value.HasValue) {
				dobDayDropdownlist.SelectedValue = value.Value.Day.ToString();
				dobMonthDropdownlist.SelectedValue = value.Value.Month.ToString();
				dobYearDropdownlist.SelectedValue = value.Value.Year.ToString();
			} else {
				dobDayDropdownlist.SelectedValue = string.Empty;
				dobMonthDropdownlist.SelectedValue = string.Empty;
				dobYearDropdownlist.SelectedValue = string.Empty;
			}
		}
	}

	public Gender? Gender {
		get {
			if (this.genderDropdownList.SelectedValue == "Male") {
				return DotNetOpenId.Extensions.Gender.Male;
			}
			if (this.genderDropdownList.SelectedValue == "Female") {
				return DotNetOpenId.Extensions.Gender.Female;
			}
			return null;
		}
		set {
			if (value.HasValue) {
				genderDropdownList.SelectedValue = value.Value.ToString();
			} else {
				genderDropdownList.SelectedIndex = -1;
			}
		}
	}

	public SimpleRegistrationFieldValues OpenIdProfileFields {
		get {
			SimpleRegistrationFieldValues fields = new SimpleRegistrationFieldValues();
			fields.BirthDate = DateOfBirth;
			fields.Country = countryDropdownList.SelectedValue;
			fields.Email = emailTextBox.Text;
			fields.FullName = fullnameTextBox.Text;
			fields.Gender = Gender;
			fields.Language = languageDropdownList.SelectedValue;
			fields.Nickname = nicknameTextBox.Text;
			fields.PostalCode = postcodeTextBox.Text;
			fields.TimeZone = timezoneDropdownList.SelectedValue;
			return fields;
		}
		set {
			DateOfBirth = value.BirthDate;
			countryDropdownList.SelectedValue = value.Country;
			emailTextBox.Text = value.Email;
			fullnameTextBox.Text = value.FullName;
			Gender = value.Gender;
			languageDropdownList.SelectedValue = value.Language;
			nicknameTextBox.Text = value.Nickname;
			postcodeTextBox.Text = value.PostalCode;
			timezoneDropdownList.SelectedValue = value.TimeZone;
		}
	}

}
