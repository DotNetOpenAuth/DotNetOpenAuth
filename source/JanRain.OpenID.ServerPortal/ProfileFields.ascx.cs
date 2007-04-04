using System;
using System.Net.Mail;
using Janrain.OpenId.Server;
using Janrain.OpenId.RegistrationExtension;

/// <summary>
/// Handles the collection of the simple registration fields.
/// Only mandatory or optional fields are displayed. Mandatory fields have a '*' next to them.
/// No validation occurs here.
/// </summary>
public partial class ProfileFields : System.Web.UI.UserControl
{
    protected void Page_Load(object sender, EventArgs e)
    {
        
    }
    
    public void SetRequiredFieldsFromRequest(CheckIdRequest request)
    {
       this.dobRequiredLabel.Visible = (request.RequestBirthdateDefault == ProfileRequest.Require);
       this.countryRequiredLabel.Visible = (request.RequestCountryDefault == ProfileRequest.Require);
       this.emailRequiredLabel.Visible = (request.RequestEmailDefault == ProfileRequest.Require);
       this.fullnameRequiredLabel.Visible = (request.RequestFullNameDefault == ProfileRequest.Require);
       this.genderRequiredLabel.Visible = (request.RequestGenderDefault == ProfileRequest.Require);
       this.languageRequiredLabel.Visible = (request.RequestLanguageDefault == ProfileRequest.Require);
       this.nicknameRequiredLabel.Visible = (request.RequestNicknameDefault == ProfileRequest.Require);
       this.postcodeRequiredLabel.Visible = (request.RequestPostalCodeDefault == ProfileRequest.Require);
       this.timezoneRequiredLabel.Visible = (request.RequestTimeZoneDefault == ProfileRequest.Require);

       this.dateOfBirthRow .Visible = !(request.RequestBirthdateDefault == ProfileRequest.NoRequest);
       this.countryRow.Visible = !(request.RequestCountryDefault == ProfileRequest.NoRequest);
       this.emailRow.Visible = !(request.RequestEmailDefault == ProfileRequest.NoRequest);
       this.fullnameRow.Visible = !(request.RequestFullNameDefault == ProfileRequest.NoRequest);
       this.genderRow.Visible = !(request.RequestGenderDefault == ProfileRequest.NoRequest);
       this.languageRow.Visible = !(request.RequestLanguageDefault == ProfileRequest.NoRequest);
       this.nicknameRow.Visible = !(request.RequestNicknameDefault == ProfileRequest.NoRequest);
       this.postcodeRow.Visible = !(request.RequestPostalCodeDefault == ProfileRequest.NoRequest);
       this.timezoneRow.Visible = !(request.RequestTimeZoneDefault == ProfileRequest.NoRequest);
    }
    
    public bool DoesAnyFieldHaveAValue
    {
        get
        {
            return  !((DateOfBirth == DateTime.MinValue)
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
    
    public DateTime ?DateOfBirth
    {
        get
        {
            try
            {
                int day = Convert.ToInt32(dobDayDropdownlist.SelectedValue);
                int month = Convert.ToInt32(dobMonthDropdownlist.SelectedValue);
                int year = Convert.ToInt32(dobYearDropdownlist.SelectedValue);
                DateTime newDate = new DateTime(year, month, day);
                return newDate;
            }
            catch (Exception)
            {
                return null;
            }
        }
    }
    
    public Gender ?Gender
    {
        get
        {
            if (this.genderDropdownList.SelectedValue == "Male")
            {
                return Janrain.OpenId.RegistrationExtension.Gender.Male;
            }
            if (this.genderDropdownList.SelectedValue == "Female")
            {
                return Janrain.OpenId.RegistrationExtension.Gender.Female;
            }
            return null;
        }
    }

    public OpenIdProfileFields OpenIdProfileFields
    {
        get
        {
            OpenIdProfileFields fields = new OpenIdProfileFields();
            fields.Birthdate = DateOfBirth;
            fields.Country = countryDropdownList.SelectedValue;
            fields.Email =emailTextBox.Text;
            fields.Fullname = fullnameTextBox.Text;
            fields.Gender = Gender;
            fields.Language = languageDropdownList.SelectedValue;
            fields.Nickname = nicknameTextBox.Text;
            fields.PostalCode = postcodeTextBox.Text;
            fields.TimeZone = timezoneDropdownList.SelectedValue;
            return fields;
        }
    }
    
}
