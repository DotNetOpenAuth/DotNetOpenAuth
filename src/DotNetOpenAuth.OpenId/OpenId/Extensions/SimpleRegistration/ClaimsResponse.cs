//-----------------------------------------------------------------------
// <copyright file="ClaimsResponse.cs" company="Outercurve Foundation">
//     Copyright (c) Outercurve Foundation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace DotNetOpenAuth.OpenId.Extensions.SimpleRegistration {
	using System;
	using System.Collections.Generic;
	using System.Diagnostics.CodeAnalysis;
	using System.Globalization;
	using System.Net.Mail;
	using System.Text;
	using System.Text.RegularExpressions;
	using System.Xml.Serialization;

	using DotNetOpenAuth.Logging;
	using DotNetOpenAuth.Messaging;
	using DotNetOpenAuth.OpenId.Messages;
	using Validation;

	/// <summary>
	/// A struct storing Simple Registration field values describing an
	/// authenticating user.
	/// </summary>
	[Serializable]
	public sealed class ClaimsResponse : ExtensionBase, IClientScriptExtensionResponse, IMessageWithEvents {
		/// <summary>
		/// The factory method that may be used in deserialization of this message.
		/// </summary>
		internal static readonly StandardOpenIdExtensionFactory.CreateDelegate Factory = (typeUri, data, baseMessage, isProviderRole) => {
			if ((typeUri == Constants.TypeUris.Standard || Array.IndexOf(Constants.AdditionalTypeUris, typeUri) >= 0) && !isProviderRole) {
				return new ClaimsResponse(typeUri);
			}

			return null;
		};

		/// <summary>
		/// The allowed format for birthdates.
		/// </summary>
		private static readonly Regex birthDateValidator = new Regex(@"^\d\d\d\d-\d\d-\d\d$");

		/// <summary>
		/// Storage for the raw string birthdate value.
		/// </summary>
		private string birthDateRaw;

		/// <summary>
		/// Backing field for the <see cref="BirthDate"/> property.
		/// </summary>
		private DateTime? birthDate;

		/// <summary>
		/// Backing field for the <see cref="Culture"/> property.
		/// </summary>
		private CultureInfo culture;

		/// <summary>
		/// Initializes a new instance of the <see cref="ClaimsResponse"/> class
		/// using the most common, and spec prescribed type URI.
		/// </summary>
		public ClaimsResponse()
			: this(Constants.TypeUris.Standard) {
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="ClaimsResponse"/> class.
		/// </summary>
		/// <param name="typeUriToUse">
		/// The type URI that must be used to identify this extension in the response message.
		/// This value should be the same one the relying party used to send the extension request.
		/// Commonly used type URIs supported by relying parties are defined in the
		/// <see cref="Constants.TypeUris"/> class.
		/// </param>
		public ClaimsResponse(string typeUriToUse = Constants.TypeUris.Standard)
			: base(new Version(1, 0), typeUriToUse, Constants.AdditionalTypeUris) {
			Requires.NotNullOrEmpty(typeUriToUse, "typeUriToUse");
		}

		/// <summary>
		/// Gets or sets the nickname the user goes by.
		/// </summary>
		[MessagePart(Constants.nickname)]
		public string Nickname { get; set; }

		/// <summary>
		/// Gets or sets the user's email address.
		/// </summary>
		[MessagePart(Constants.email)]
		public string Email { get; set; }

		/// <summary>
		/// Gets or sets the full name of a user as a single string.
		/// </summary>
		[MessagePart(Constants.fullname)]
		public string FullName { get; set; }

		/// <summary>
		/// Gets or sets the user's birthdate.
		/// </summary>
		public DateTime? BirthDate {
			get {
				return this.birthDate;
			}

			set {
				this.birthDate = value;

				// Don't use property accessor for peer property to avoid infinite loop between the two proeprty accessors.
				if (value.HasValue) {
					this.birthDateRaw = value.Value.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);
				} else {
					this.birthDateRaw = null;
				}
			}
		}

		/// <summary>
		/// Gets or sets the raw birth date string given by the extension.
		/// </summary>
		/// <value>A string in the format yyyy-MM-dd.</value>
		[MessagePart(Constants.dob)]
		public string BirthDateRaw {
			get {
				return this.birthDateRaw;
			}

			set {
				ErrorUtilities.VerifyArgument(value == null || birthDateValidator.IsMatch(value), OpenIdStrings.SregInvalidBirthdate);
				if (value != null) {
					// Update the BirthDate property, if possible. 
					// Don't use property accessor for peer property to avoid infinite loop between the two proeprty accessors.
					// Some valid sreg dob values like "2000-00-00" will not work as a DateTime struct, 
					// in which case we null it out, but don't show any error.
					DateTime newBirthDate;
					if (DateTime.TryParse(value, out newBirthDate)) {
						this.birthDate = newBirthDate;
					} else {
						Logger.OpenId.WarnFormat("Simple Registration birthdate '{0}' could not be parsed into a DateTime and may not include month and/or day information.  Setting BirthDate property to null.", value);
						this.birthDate = null;
					}
				} else {
					this.birthDate = null;
				}

				this.birthDateRaw = value;
			}
		}

		/// <summary>
		/// Gets or sets the gender of the user.
		/// </summary>
		[MessagePart(Constants.gender, Encoder = typeof(GenderEncoder))]
		public Gender? Gender { get; set; }

		/// <summary>
		/// Gets or sets the zip code / postal code of the user.
		/// </summary>
		[MessagePart(Constants.postcode)]
		public string PostalCode { get; set; }

		/// <summary>
		/// Gets or sets the country of the user.
		/// </summary>
		[MessagePart(Constants.country)]
		public string Country { get; set; }

		/// <summary>
		/// Gets or sets the primary/preferred language of the user.
		/// </summary>
		[MessagePart(Constants.language)]
		public string Language { get; set; }

		/// <summary>
		/// Gets or sets the user's timezone.
		/// </summary>
		[MessagePart(Constants.timezone)]
		public string TimeZone { get; set; }

		/// <summary>
		/// Gets a combination of the user's full name and email address.
		/// </summary>
		public MailAddress MailAddress {
			get {
				if (string.IsNullOrEmpty(this.Email)) {
					return null;
				} else if (string.IsNullOrEmpty(this.FullName)) {
					return new MailAddress(this.Email);
				} else {
					return new MailAddress(this.Email, this.FullName);
				}
			}
		}

		/// <summary>
		/// Gets or sets a combination of the language and country of the user.
		/// </summary>
		[XmlIgnore]
		public CultureInfo Culture {
			get {
				if (this.culture == null && !string.IsNullOrEmpty(this.Language)) {
					string cultureString = string.Empty;
					cultureString = this.Language;
					if (!string.IsNullOrEmpty(this.Country)) {
						cultureString += "-" + this.Country;
					}

					// language-country may not always form a recongized valid culture.
					// For instance, a Google OpenID Provider can return a random combination
					// of language and country based on user settings.
					try {
						this.culture = CultureInfo.GetCultureInfo(cultureString);
					} catch (ArgumentException) { // CultureNotFoundException derives from this, and .NET 3.5 throws the base type
						// Fallback to just reporting a culture based on language.
						this.culture = CultureInfo.GetCultureInfo(this.Language);
					}
				}

				return this.culture;
			}

			set {
				this.culture = value;
				this.Language = (value != null) ? value.TwoLetterISOLanguageName : null;
				int indexOfHyphen = (value != null) ? value.Name.IndexOf('-') : -1;
				this.Country = indexOfHyphen > 0 ? value.Name.Substring(indexOfHyphen + 1) : null;
			}
		}

		/// <summary>
		/// Gets a value indicating whether this extension is signed by the Provider.
		/// </summary>
		/// <value>
		/// 	<c>true</c> if this instance is signed by the Provider; otherwise, <c>false</c>.
		/// </value>
		public bool IsSignedByProvider {
			get { return this.IsSignedByRemoteParty; }
		}

		/// <summary>
		/// Tests equality of two <see cref="ClaimsResponse"/> objects.
		/// </summary>
		/// <param name="one">One instance to compare.</param>
		/// <param name="other">Another instance to compare.</param>
		/// <returns>The result of the operator.</returns>
		public static bool operator ==(ClaimsResponse one, ClaimsResponse other) {
			return one.EqualsNullSafe(other);
		}

		/// <summary>
		/// Tests inequality of two <see cref="ClaimsResponse"/> objects.
		/// </summary>
		/// <param name="one">One instance to compare.</param>
		/// <param name="other">Another instance to compare.</param>
		/// <returns>The result of the operator.</returns>
		public static bool operator !=(ClaimsResponse one, ClaimsResponse other) {
			return !(one == other);
		}

		/// <summary>
		/// Tests equality of two <see cref="ClaimsResponse"/> objects.
		/// </summary>
		/// <param name="obj">The <see cref="T:System.Object"/> to compare with the current <see cref="T:System.Object"/>.</param>
		/// <returns>
		/// true if the specified <see cref="T:System.Object"/> is equal to the current <see cref="T:System.Object"/>; otherwise, false.
		/// </returns>
		/// <exception cref="T:System.NullReferenceException">
		/// The <paramref name="obj"/> parameter is null.
		/// </exception>
		public override bool Equals(object obj) {
			ClaimsResponse other = obj as ClaimsResponse;
			if (other == null) {
				return false;
			}

			return
				this.BirthDateRaw.EqualsNullSafe(other.BirthDateRaw) &&
				this.Country.EqualsNullSafe(other.Country) &&
				this.Language.EqualsNullSafe(other.Language) &&
				this.Email.EqualsNullSafe(other.Email) &&
				this.FullName.EqualsNullSafe(other.FullName) &&
				this.Gender.Equals(other.Gender) &&
				this.Nickname.EqualsNullSafe(other.Nickname) &&
				this.PostalCode.EqualsNullSafe(other.PostalCode) &&
				this.TimeZone.EqualsNullSafe(other.TimeZone);
		}

		/// <summary>
		/// Serves as a hash function for a particular type.
		/// </summary>
		/// <returns>
		/// A hash code for the current <see cref="T:System.Object"/>.
		/// </returns>
		public override int GetHashCode() {
			return (this.Nickname != null) ? this.Nickname.GetHashCode() : base.GetHashCode();
		}

		#region IClientScriptExtension Members

		/// <summary>
		/// Reads the extension information on an authentication response from the provider.
		/// </summary>
		/// <param name="response">The incoming OpenID response carrying the extension.</param>
		/// <returns>
		/// A Javascript snippet that when executed on the user agent returns an object with
		/// the information deserialized from the extension response.
		/// </returns>
		/// <remarks>
		/// This method is called <b>before</b> the signature on the assertion response has been
		/// verified.  Therefore all information in these fields should be assumed unreliable
		/// and potentially falsified.
		/// </remarks>
		string IClientScriptExtensionResponse.InitializeJavaScriptData(IProtocolMessageWithExtensions response) {
			var sreg = new Dictionary<string, string>(15);

			// Although we could probably whip up a trip with MessageDictionary
			// to avoid explicitly setting each field, doing so would likely
			// open ourselves up to security exploits from the OP as it would
			// make possible sending arbitrary javascript in arbitrary field names.
			sreg[Constants.nickname] = this.Nickname;
			sreg[Constants.email] = this.Email;
			sreg[Constants.fullname] = this.FullName;
			sreg[Constants.dob] = this.BirthDateRaw;
			sreg[Constants.gender] = this.Gender.HasValue ? this.Gender.Value.ToString() : null;
			sreg[Constants.postcode] = this.PostalCode;
			sreg[Constants.country] = this.Country;
			sreg[Constants.language] = this.Language;
			sreg[Constants.timezone] = this.TimeZone;

			return MessagingUtilities.CreateJsonObject(sreg, false);
		}

		#endregion

		#region IMessageWithEvents Members

		/// <summary>
		/// Called when the message is about to be transmitted,
		/// before it passes through the channel binding elements.
		/// </summary>
		void IMessageWithEvents.OnSending() {
			// Null out empty values so we don't send out a lot of empty parameters.
			this.Country = EmptyToNull(this.Country);
			this.Email = EmptyToNull(this.Email);
			this.FullName = EmptyToNull(this.FullName);
			this.Language = EmptyToNull(this.Language);
			this.Nickname = EmptyToNull(this.Nickname);
			this.PostalCode = EmptyToNull(this.PostalCode);
			this.TimeZone = EmptyToNull(this.TimeZone);
		}

		/// <summary>
		/// Called when the message has been received,
		/// after it passes through the channel binding elements.
		/// </summary>
		void IMessageWithEvents.OnReceiving() {
		}

		#endregion

		/// <summary>
		/// Translates an empty string value to null, or passes through non-empty values.
		/// </summary>
		/// <param name="value">The value to consider changing to null.</param>
		/// <returns>Either null or a non-empty string.</returns>
		private static string EmptyToNull(string value) {
			return string.IsNullOrEmpty(value) ? null : value;
		}
	}
}