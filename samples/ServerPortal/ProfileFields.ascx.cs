using System;
using System.Net.Mail;
using DotNetOpenId.Provider;
using DotNetOpenId.RegistrationExtension;

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
       this.dobRequiredLabel.Visible = (request.RequestBirthdate == ProfileRequest.Require);
       this.countryRequiredLabel.Visible = (request.RequestCountry == ProfileRequest.Require);
       this.emailRequiredLabel.Visible = (request.RequestEmail == ProfileRequest.Require);
       this.fullnameRequiredLabel.Visible = (request.RequestFullName == ProfileRequest.Require);
       this.genderRequiredLabel.Visible = (request.RequestGender == ProfileRequest.Require);
       this.languageRequiredLabel.Visible = (request.RequestLanguage == ProfileRequest.Require);
       this.nicknameRequiredLabel.Visible = (request.RequestNickname == ProfileRequest.Require);
       this.postcodeRequiredLabel.Visible = (request.RequestPostalCode == ProfileRequest.Require);
       this.timezoneRequiredLabel.Visible = (request.RequestTimeZone == ProfileRequest.Require);

       this.dateOfBirthRow .Visible = !(request.RequestBirthdate == ProfileRequest.NoRequest);
       this.countryRow.Visible = !(request.RequestCountry == ProfileRequest.NoRequest);
       this.emailRow.Visible = !(request.RequestEmail == ProfileRequest.NoRequest);
       this.fullnameRow.Visible = !(request.RequestFullName == ProfileRequest.NoRequest);
       this.genderRow.Visible = !(request.RequestGender == ProfileRequest.NoRequest);
       this.languageRow.Visible = !(request.RequestLanguage == ProfileRequest.NoRequest);
       this.nicknameRow.Visible = !(request.RequestNickname == ProfileRequest.NoRequest);
       this.postcodeRow.Visible = !(request.RequestPostalCode == ProfileRequest.NoRequest);
       this.timezoneRow.Visible = !(request.RequestTimeZone == ProfileRequest.NoRequest);
    }
    
    public bool DoesAnyFieldHaveAValue
    {
        get
        {
            return  !((DateOfBirth == null)
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
                return DotNetOpenId.RegistrationExtension.Gender.Male;
            }
            if (this.genderDropdownList.SelectedValue == "Female")
            {
                return DotNetOpenId.RegistrationExtension.Gender.Female;
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
            fields.Email = emailTextBox.Text;
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
