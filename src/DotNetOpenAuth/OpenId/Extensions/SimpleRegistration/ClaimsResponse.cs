//-----------------------------------------------------------------------
// <copyright file="ClaimsResponse.cs" company="Andrew Arnott">
//     Copyright (c) Andrew Arnott. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace DotNetOpenAuth.OpenId.Extensions.SimpleRegistration {
	using System;
	using System.Collections.Generic;
	using System.Globalization;
	using System.Net.Mail;
	using System.Text;
	using System.Xml.Serialization;
	using DotNetOpenAuth.OpenId.Messages;
	using System.Diagnostics.CodeAnalysis;
	using DotNetOpenAuth.Messaging;

#pragma warning disable 0659, 0661
	/// <summary>
	/// A struct storing Simple Registration field values describing an
	/// authenticating user.
	/// </summary>
	[SuppressMessage("Microsoft.Usage", "CA2218:OverrideGetHashCodeOnOverridingEquals"), Serializable()]
	public sealed class ClaimsResponse : ExtensionBase {
		/// <summary>
		/// The TypeURI that must be used in the response, based on the one used in the request.
		/// </summary>
		private string typeUriToUse;

		/// <summary>
		/// Backing field for the <see cref="Culture"/> property.
		/// </summary>
		private CultureInfo culture;

		/// <summary>
		/// Creates an instance of the <see cref="ClaimsResponse"/> class.
		/// </summary>
		[Obsolete("Use ClaimsRequest.CreateResponse() instead.")]
		public ClaimsResponse()
			: this(Constants.sreg_ns) {
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="ClaimsResponse"/> class.
		/// </summary>
		/// <param name="typeUriToUse">
		/// The type URI that must be used to identify this extension in the response message.
		/// This value should be the same one the relying party used to send the extension request.
		/// </param>
		internal ClaimsResponse(string typeUriToUse)
			: base(new Version(1, 0), typeUriToUse, EmptyList<string>.Instance) {
			ErrorUtilities.VerifyNonZeroLength(typeUriToUse, "typeUriToUse");
			this.typeUriToUse = typeUriToUse;
		}

		/// <summary>
		/// The nickname the user goes by.
		/// </summary>
		[MessagePart(Constants.nickname)]
		public string Nickname { get; set; }

		/// <summary>
		/// The user's email address.
		/// </summary>
		[MessagePart(Constants.email)]
		public string Email { get; set; }

		/// <summary>
		/// The full name of a user as a single string.
		/// </summary>
		[MessagePart(Constants.fullname)]
		public string FullName { get; set; }

		/// <summary>
		/// The user's birthdate.
		/// </summary>
		[MessagePart(Constants.dob)]
		public DateTime? BirthDate { get; set; }

		/// <summary>
		/// The gender of the user.
		/// </summary>
		[MessagePart(Constants.gender, Encoder = typeof(GenderEncoder))]
		public Gender? Gender { get; set; }

		/// <summary>
		/// The zip code / postal code of the user.
		/// </summary>
		[MessagePart(Constants.postcode)]
		public string PostalCode { get; set; }

		/// <summary>
		/// The country of the user.
		/// </summary>
		[MessagePart(Constants.country)]
		public string Country { get; set; }

		/// <summary>
		/// The primary/preferred language of the user.
		/// </summary>
		[MessagePart(Constants.language)]
		public string Language { get; set; }

		/// <summary>
		/// The user's timezone.
		/// </summary>
		[MessagePart(Constants.timezone)]
		public string TimeZone { get; set; }

		/// <summary>
		/// A combination of the user's full name and email address.
		/// </summary>
		public MailAddress MailAddress {
			get {
				if (string.IsNullOrEmpty(Email)) return null;
				if (string.IsNullOrEmpty(FullName))
					return new MailAddress(Email);
				else
					return new MailAddress(Email, FullName);
			}
		}

		/// <summary>
		/// A combination o the language and country of the user.
		/// </summary>
		[XmlIgnore]
		public CultureInfo Culture {
			get {
				if (culture == null && !string.IsNullOrEmpty(Language)) {
					string cultureString = "";
					cultureString = Language;
					if (!string.IsNullOrEmpty(Country))
						cultureString += "-" + Country;
					culture = CultureInfo.GetCultureInfo(cultureString);
				}

				return culture;
			}
			set {
				culture = value;
				Language = (value != null) ? value.TwoLetterISOLanguageName : null;
				int indexOfHyphen = (value != null) ? value.Name.IndexOf('-') : -1;
				Country = indexOfHyphen > 0 ? value.Name.Substring(indexOfHyphen + 1) : null;
			}
		}

		#region IClientScriptExtension Members

		// TODO: re-enable this
		////string IClientScriptExtensionResponse.InitializeJavaScriptData(IDictionary<string, string> sreg, IAuthenticationResponse response, string typeUri) {
		////    StringBuilder builder = new StringBuilder();
		////    builder.Append("{ ");

		////    string nickname, email, fullName, dob, genderString, postalCode, country, language, timeZone;
		////    if (sreg.TryGetValue(Constants.nickname, out nickname)) {
		////        builder.Append(createAddFieldJS(Constants.nickname, nickname));
		////    }
		////    if (sreg.TryGetValue(Constants.email, out email)) {
		////        builder.Append(createAddFieldJS(Constants.email, email));
		////    }
		////    if (sreg.TryGetValue(Constants.fullname, out fullName)) {
		////        builder.Append(createAddFieldJS(Constants.fullname, fullName));
		////    }
		////    if (sreg.TryGetValue(Constants.dob, out dob)) {
		////        builder.Append(createAddFieldJS(Constants.dob, dob));
		////    }
		////    if (sreg.TryGetValue(Constants.gender, out genderString)) {
		////        builder.Append(createAddFieldJS(Constants.gender, genderString));
		////    }
		////    if (sreg.TryGetValue(Constants.postcode, out postalCode)) {
		////        builder.Append(createAddFieldJS(Constants.postcode, postalCode));
		////    }
		////    if (sreg.TryGetValue(Constants.country, out country)) {
		////        builder.Append(createAddFieldJS(Constants.country, country));
		////    }
		////    if (sreg.TryGetValue(Constants.language, out language)) {
		////        builder.Append(createAddFieldJS(Constants.language, language));
		////    }
		////    if (sreg.TryGetValue(Constants.timezone, out timeZone)) {
		////        builder.Append(createAddFieldJS(Constants.timezone, timeZone));
		////    }
		////    if (builder[builder.Length - 1] == ',') builder.Length -= 1;
		////    builder.Append("}");
		////    return builder.ToString();
		////}

		#endregion

		/// <summary>
		/// Tests equality of two <see cref="ClaimsResponse"/> objects.
		/// </summary>
		public static bool operator ==(ClaimsResponse one, ClaimsResponse other) {
			return one.EqualsNullSafe(other);
		}

		/// <summary>
		/// Tests inequality of two <see cref="ClaimsResponse"/> objects.
		/// </summary>
		public static bool operator !=(ClaimsResponse one, ClaimsResponse other) {
			return !(one == other);
		}

		/// <summary>
		/// Tests equality of two <see cref="ClaimsResponse"/> objects.
		/// </summary>
		public override bool Equals(object obj) {
			ClaimsResponse other = obj as ClaimsResponse;
			if (other == null) return false;

			return
				this.BirthDate.Equals(other.BirthDate) &&
				this.Country.EqualsNullSafe(other.Country) &&
				this.Language.EqualsNullSafe(other.Language) &&
				this.Email.EqualsNullSafe(other.Email) &&
				this.FullName.EqualsNullSafe(other.FullName) &&
				this.Gender.Equals(other.Gender) &&
				this.Nickname.EqualsNullSafe(other.Nickname) &&
				this.PostalCode.EqualsNullSafe(other.PostalCode) &&
				this.TimeZone.EqualsNullSafe(other.TimeZone);
		}

		private static string createAddFieldJS(string propertyName, string value) {
			return string.Format(CultureInfo.InvariantCulture, "{0}: {1},", propertyName, Util.GetSafeJavascriptValue(value));
		}
	}
}