//-----------------------------------------------------------------------
// <copyright file="ClaimsRequest.cs" company="Andrew Arnott">
//     Copyright (c) Andrew Arnott. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace DotNetOpenAuth.OpenId.Extensions.SimpleRegistration {
	using System;
	using System.Collections.Generic;
	using System.Text;
	using System.Diagnostics;
	using System.Globalization;
	using DotNetOpenAuth.OpenId.Messages;
	using System.Diagnostics.CodeAnalysis;
	using DotNetOpenAuth.Messaging;

	/// <summary>
	/// Carries the request/require/none demand state of the simple registration fields.
	/// </summary>
#pragma warning disable 0659, 0661
	[SuppressMessage("Microsoft.Usage", "CA2218:OverrideGetHashCodeOnOverridingEquals")]
	public sealed class ClaimsRequest : ExtensionBase {
		private static readonly string[] additionalTypeUris = new string[] {
			Constants.sreg_ns10,
			Constants.sreg_ns11other,
		};

		/// <summary>
		/// Initializes a new instance of the <see cref="ClaimsRequest"/> class.
		/// </summary>
		public ClaimsRequest()
			: base(new Version(1, 0), Constants.sreg_ns, additionalTypeUris) {
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="ClaimsRequest"/> class
		/// by deserializing from a message.
		/// </summary>
		/// <param name="typeUri">The type URI this extension was recognized by in the OpenID message.</param>
		internal ClaimsRequest(string typeUri) : this() {
			ErrorUtilities.VerifyNonZeroLength(typeUri, "typeUri");

			this.typeUriDeserializedFrom = typeUri;
		}

		/// <summary>
		/// The URL the consumer site provides for the authenticating user to review
		/// for how his claims will be used by the consumer web site.
		/// </summary>
		[MessagePart(Constants.policy_url, IsRequired = false)]
		public Uri PolicyUrl { get; set; }

		/// <summary>
		/// The level of interest a relying party has in the nickname of the user.
		/// </summary>
		public DemandLevel Nickname { get; set; }

		/// <summary>
		/// The level of interest a relying party has in the email of the user.
		/// </summary>
		public DemandLevel Email { get; set; }
	
		/// <summary>
		/// The level of interest a relying party has in the full name of the user.
		/// </summary>
		public DemandLevel FullName { get; set; }
		
		/// <summary>
		/// The level of interest a relying party has in the birthdate of the user.
		/// </summary>
		public DemandLevel BirthDate { get; set; }
		
		/// <summary>
		/// The level of interest a relying party has in the gender of the user.
		/// </summary>
		public DemandLevel Gender { get; set; }
		
		/// <summary>
		/// The level of interest a relying party has in the postal code of the user.
		/// </summary>
		public DemandLevel PostalCode { get; set; }
		
		/// <summary>
		/// The level of interest a relying party has in the Country of the user.
		/// </summary>
		public DemandLevel Country { get; set; }
		
		/// <summary>
		/// The level of interest a relying party has in the language of the user.
		/// </summary>
		public DemandLevel Language { get; set; }
		
		/// <summary>
		/// The level of interest a relying party has in the time zone of the user.
		/// </summary>
		public DemandLevel TimeZone { get; set; }

		[MessagePart(Constants.required, AllowEmpty = true)]
		private string RequiredList {
			get { return string.Join(",", assembleProfileFields(DemandLevel.Require)); }
			set { this.SetProfileRequestFromList(value.Split(','), DemandLevel.Require); }
		}

		[MessagePart(Constants.optional, AllowEmpty = true)]
		private string OptionalList {
			get { return string.Join(",", assembleProfileFields(DemandLevel.Request)); }
			set { this.SetProfileRequestFromList(value.Split(','), DemandLevel.Request); }
		}

		private string typeUriDeserializedFrom;

		/// <summary>
		/// Prepares a Simple Registration response extension that is compatible with the
		/// version of Simple Registration used in the request message.
		/// </summary>
		public ClaimsResponse CreateResponse() {
			if (typeUriDeserializedFrom == null) {
				throw new InvalidOperationException(OpenIdStrings.CallDeserializeBeforeCreateResponse);
			}
			return new ClaimsResponse(typeUriDeserializedFrom);
		}

		/// <summary>
		/// Tests equality between two <see cref="ClaimsRequest"/> structs.
		/// </summary>
		public static bool operator ==(ClaimsRequest one, ClaimsRequest other) {
			if ((object)one == null && (object)other == null) return true;
			if ((object)one == null ^ (object)other == null) return false;
			return one.Equals(other);
		}
	
		/// <summary>
		/// Tests inequality between two <see cref="ClaimsRequest"/> structs.
		/// </summary>
		public static bool operator !=(ClaimsRequest one, ClaimsRequest other) {
			return !(one == other);
		}
		
		/// <summary>
		/// Tests equality between two <see cref="ClaimsRequest"/> structs.
		/// </summary>
		public override bool Equals(object obj) {
			ClaimsRequest other = obj as ClaimsRequest;
			if (other == null) return false;

			return
				this.BirthDate.Equals(other.BirthDate) &&
				this.Country.Equals(other.Country) &&
				this.Language.Equals(other.Language) &&
				this.Email.Equals(other.Email) &&
				this.FullName.Equals(other.FullName) &&
				this.Gender.Equals(other.Gender) &&
				this.Nickname.Equals(other.Nickname) &&
				this.PostalCode.Equals(other.PostalCode) &&
				this.TimeZone.Equals(other.TimeZone) &&
				this.PolicyUrl.EqualsNullSafe(other.PolicyUrl);
		}

		/// <summary>
		/// Renders the requested information as a string.
		/// </summary>
		public override string ToString() {
			return string.Format(CultureInfo.CurrentCulture, @"Nickname = '{0}' 
Email = '{1}' 
FullName = '{2}' 
Birthdate = '{3}'
Gender = '{4}'
PostalCode = '{5}'
Country = '{6}'
Language = '{7}'
TimeZone = '{8}'", Nickname, Email, FullName, BirthDate, Gender, PostalCode, Country, Language, TimeZone);
		}
		
		/// <summary>
		/// Sets the profile request properties according to a list of
		/// field names that might have been passed in the OpenId query dictionary.
		/// </summary>
		/// <param name="fieldNames">
		/// The list of field names that should receive a given 
		/// <paramref name="requestLevel"/>.  These field names should match 
		/// the OpenId specification for field names, omitting the 'openid.sreg' prefix.
		/// </param>
		/// <param name="requestLevel">The none/request/require state of the listed fields.</param>
		internal void SetProfileRequestFromList(ICollection<string> fieldNames, DemandLevel requestLevel) {
			foreach (string field in fieldNames) {
				switch (field) {
					case Constants.nickname:
						Nickname = requestLevel;
						break;
					case Constants.email:
						Email = requestLevel;
						break;
					case Constants.fullname:
						FullName = requestLevel;
						break;
					case Constants.dob:
						BirthDate = requestLevel;
						break;
					case Constants.gender:
						Gender = requestLevel;
						break;
					case Constants.postcode:
						PostalCode = requestLevel;
						break;
					case Constants.country:
						Country = requestLevel;
						break;
					case Constants.language:
						Language = requestLevel;
						break;
					case Constants.timezone:
						TimeZone = requestLevel;
						break;
					default:
						Logger.WarnFormat("OpenIdProfileRequest.SetProfileRequestFromList: Unrecognized field name '{0}'.", field);
						break;
				}
			}
		}

		private string[] assembleProfileFields(DemandLevel level) {
			List<string> fields = new List<string>(10);
			if (Nickname == level)
				fields.Add(Constants.nickname);
			if (Email == level)
				fields.Add(Constants.email);
			if (FullName == level)
				fields.Add(Constants.fullname);
			if (BirthDate == level)
				fields.Add(Constants.dob);
			if (Gender == level)
				fields.Add(Constants.gender);
			if (PostalCode == level)
				fields.Add(Constants.postcode);
			if (Country == level)
				fields.Add(Constants.country);
			if (Language == level)
				fields.Add(Constants.language);
			if (TimeZone == level)
				fields.Add(Constants.timezone);

			return fields.ToArray();
		}
	}
}
