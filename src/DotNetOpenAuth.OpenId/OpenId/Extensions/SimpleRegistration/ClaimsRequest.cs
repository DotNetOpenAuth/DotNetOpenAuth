//-----------------------------------------------------------------------
// <copyright file="ClaimsRequest.cs" company="Outercurve Foundation">
//     Copyright (c) Outercurve Foundation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace DotNetOpenAuth.OpenId.Extensions.SimpleRegistration {
	using System;
	using System.Collections.Generic;
	using System.Diagnostics;
	using System.Diagnostics.CodeAnalysis;
	using System.Globalization;
	using System.Text;

	using DotNetOpenAuth.Logging;
	using DotNetOpenAuth.Messaging;
	using DotNetOpenAuth.OpenId.Messages;
	using Validation;

	/// <summary>
	/// Carries the request/require/none demand state of the simple registration fields.
	/// </summary>
	[Serializable]
	public sealed class ClaimsRequest : ExtensionBase {
		/// <summary>
		/// The factory method that may be used in deserialization of this message.
		/// </summary>
		internal static readonly StandardOpenIdExtensionFactory.CreateDelegate Factory = (typeUri, data, baseMessage, isProviderRole) => {
			if (typeUri == Constants.TypeUris.Standard && isProviderRole) {
				return new ClaimsRequest(typeUri);
			}

			return null;
		};

		/// <summary>
		/// The type URI that this particular (deserialized) extension was read in using,
		/// allowing a response to alter be crafted using the same type URI.
		/// </summary>
		private string typeUriDeserializedFrom;

		/// <summary>
		/// Initializes a new instance of the <see cref="ClaimsRequest"/> class.
		/// </summary>
		public ClaimsRequest()
			: base(new Version(1, 0), Constants.TypeUris.Standard, Constants.AdditionalTypeUris) {
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="ClaimsRequest"/> class
		/// by deserializing from a message.
		/// </summary>
		/// <param name="typeUri">The type URI this extension was recognized by in the OpenID message.</param>
		internal ClaimsRequest(string typeUri)
			: this() {
			Requires.NotNullOrEmpty(typeUri, "typeUri");

			this.typeUriDeserializedFrom = typeUri;
		}

		/// <summary>
		/// Gets or sets the URL the consumer site provides for the authenticating user to review
		/// for how his claims will be used by the consumer web site.
		/// </summary>
		[MessagePart(Constants.policy_url, IsRequired = false)]
		public Uri PolicyUrl { get; set; }

		/// <summary>
		/// Gets or sets the level of interest a relying party has in the nickname of the user.
		/// </summary>
		public DemandLevel Nickname { get; set; }

		/// <summary>
		/// Gets or sets the level of interest a relying party has in the email of the user.
		/// </summary>
		public DemandLevel Email { get; set; }

		/// <summary>
		/// Gets or sets the level of interest a relying party has in the full name of the user.
		/// </summary>
		public DemandLevel FullName { get; set; }

		/// <summary>
		/// Gets or sets the level of interest a relying party has in the birthdate of the user.
		/// </summary>
		public DemandLevel BirthDate { get; set; }

		/// <summary>
		/// Gets or sets the level of interest a relying party has in the gender of the user.
		/// </summary>
		public DemandLevel Gender { get; set; }

		/// <summary>
		/// Gets or sets the level of interest a relying party has in the postal code of the user.
		/// </summary>
		public DemandLevel PostalCode { get; set; }

		/// <summary>
		/// Gets or sets the level of interest a relying party has in the Country of the user.
		/// </summary>
		public DemandLevel Country { get; set; }

		/// <summary>
		/// Gets or sets the level of interest a relying party has in the language of the user.
		/// </summary>
		public DemandLevel Language { get; set; }

		/// <summary>
		/// Gets or sets the level of interest a relying party has in the time zone of the user.
		/// </summary>
		public DemandLevel TimeZone { get; set; }

		/// <summary>
		/// Gets or sets a value indicating whether this <see cref="ClaimsRequest"/> instance
		/// is synthesized from an AX request at the Provider.
		/// </summary>
		internal bool Synthesized { get; set; }

		/// <summary>
		/// Gets or sets the value of the sreg.required parameter.
		/// </summary>
		/// <value>A comma-delimited list of sreg fields.</value>
		[SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode", Justification = "Called by messaging framework via reflection.")]
		[MessagePart(Constants.required, AllowEmpty = true)]
		private string RequiredList {
			get { return string.Join(",", this.AssembleProfileFields(DemandLevel.Require)); }
			set { this.SetProfileRequestFromList(value.Split(','), DemandLevel.Require); }
		}

		/// <summary>
		/// Gets or sets the value of the sreg.optional parameter.
		/// </summary>
		/// <value>A comma-delimited list of sreg fields.</value>
		[SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode", Justification = "Called by messaging framework via reflection.")]
		[MessagePart(Constants.optional, AllowEmpty = true)]
		private string OptionalList {
			get { return string.Join(",", this.AssembleProfileFields(DemandLevel.Request)); }
			set { this.SetProfileRequestFromList(value.Split(','), DemandLevel.Request); }
		}

		/// <summary>
		/// Tests equality between two <see cref="ClaimsRequest"/> structs.
		/// </summary>
		/// <param name="one">One instance to compare.</param>
		/// <param name="other">Another instance to compare.</param>
		/// <returns>The result of the operator.</returns>
		public static bool operator ==(ClaimsRequest one, ClaimsRequest other) {
			return one.EqualsNullSafe(other);
		}

		/// <summary>
		/// Tests inequality between two <see cref="ClaimsRequest"/> structs.
		/// </summary>
		/// <param name="one">One instance to compare.</param>
		/// <param name="other">Another instance to compare.</param>
		/// <returns>The result of the operator.</returns>
		public static bool operator !=(ClaimsRequest one, ClaimsRequest other) {
			return !(one == other);
		}

		/// <summary>
		/// Tests equality between two <see cref="ClaimsRequest"/> structs.
		/// </summary>
		/// <param name="obj">The <see cref="T:System.Object"/> to compare with the current <see cref="T:System.Object"/>.</param>
		/// <returns>
		/// true if the specified <see cref="T:System.Object"/> is equal to the current <see cref="T:System.Object"/>; otherwise, false.
		/// </returns>
		/// <exception cref="T:System.NullReferenceException">
		/// The <paramref name="obj"/> parameter is null.
		/// </exception>
		public override bool Equals(object obj) {
			ClaimsRequest other = obj as ClaimsRequest;
			if (other == null) {
				return false;
			}

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
		/// Serves as a hash function for a particular type.
		/// </summary>
		/// <returns>
		/// A hash code for the current <see cref="T:System.Object"/>.
		/// </returns>
		public override int GetHashCode() {
			// It's important that if Equals returns true that the hash code also equals,
			// so returning base.GetHashCode() is a BAD option.
			// Return 1 is simple and poor for dictionary storage, but considering that every
			// ClaimsRequest formulated at a single RP will likely have all the same fields,
			// even a good hash code function will likely generate the same hash code.  So
			// we just cut to the chase and return a simple one.
			return 1;
		}

		/// <summary>
		/// Renders the requested information as a string.
		/// </summary>
		/// <returns>
		/// A <see cref="T:System.String"/> that represents the current <see cref="T:System.Object"/>.
		/// </returns>
		public override string ToString() {
			string format = @"Nickname = '{0}' 
Email = '{1}' 
FullName = '{2}' 
Birthdate = '{3}'
Gender = '{4}'
PostalCode = '{5}'
Country = '{6}'
Language = '{7}'
TimeZone = '{8}'";
			return string.Format(CultureInfo.CurrentCulture, format, this.Nickname, this.Email, this.FullName, this.BirthDate, this.Gender, this.PostalCode, this.Country, this.Language, this.TimeZone);
		}

		/// <summary>
		/// Prepares a Simple Registration response extension that is compatible with the
		/// version of Simple Registration used in the request message.
		/// </summary>
		/// <returns>The newly created <see cref="ClaimsResponse"/> instance.</returns>
		public ClaimsResponse CreateResponse() {
			if (this.typeUriDeserializedFrom == null) {
				throw new InvalidOperationException(OpenIdStrings.CallDeserializeBeforeCreateResponse);
			}

			return new ClaimsResponse(this.typeUriDeserializedFrom);
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
		internal void SetProfileRequestFromList(IEnumerable<string> fieldNames, DemandLevel requestLevel) {
			foreach (string field in fieldNames) {
				switch (field) {
					case "": // this occurs for empty lists
						break;
					case Constants.nickname:
						this.Nickname = requestLevel;
						break;
					case Constants.email:
						this.Email = requestLevel;
						break;
					case Constants.fullname:
						this.FullName = requestLevel;
						break;
					case Constants.dob:
						this.BirthDate = requestLevel;
						break;
					case Constants.gender:
						this.Gender = requestLevel;
						break;
					case Constants.postcode:
						this.PostalCode = requestLevel;
						break;
					case Constants.country:
						this.Country = requestLevel;
						break;
					case Constants.language:
						this.Language = requestLevel;
						break;
					case Constants.timezone:
						this.TimeZone = requestLevel;
						break;
					default:
						Logger.OpenId.WarnFormat("ClaimsRequest.SetProfileRequestFromList: Unrecognized field name '{0}'.", field);
						break;
				}
			}
		}

		/// <summary>
		/// Assembles the profile parameter names that have a given <see cref="DemandLevel"/>.
		/// </summary>
		/// <param name="level">The demand level (request, require, none).</param>
		/// <returns>An array of the profile parameter names that meet the criteria.</returns>
		[SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode", Justification = "Called by messaging framework via reflection.")]
		private string[] AssembleProfileFields(DemandLevel level) {
			List<string> fields = new List<string>(10);
			if (this.Nickname == level) {
				fields.Add(Constants.nickname);
			} if (this.Email == level) {
				fields.Add(Constants.email);
			} if (this.FullName == level) {
				fields.Add(Constants.fullname);
			} if (this.BirthDate == level) {
				fields.Add(Constants.dob);
			} if (this.Gender == level) {
				fields.Add(Constants.gender);
			} if (this.PostalCode == level) {
				fields.Add(Constants.postcode);
			} if (this.Country == level) {
				fields.Add(Constants.country);
			} if (this.Language == level) {
				fields.Add(Constants.language);
			} if (this.TimeZone == level) {
				fields.Add(Constants.timezone);
			}

			return fields.ToArray();
		}
	}
}
