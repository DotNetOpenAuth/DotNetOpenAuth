//-----------------------------------------------------------------------
// <copyright file="GoogleGraph.cs" company="Andras Fuchs">
//   Copyright (c) Outercurve Foundation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace DotNetOpenAuth.ApplicationBlock {
	using System;
	using System.Collections.Generic;
	using System.IO;
	using System.Linq;
	using System.Runtime.Serialization;
	using System.Runtime.Serialization.Json;
	using System.Text;

	//// Documentation: https://developers.google.com/accounts/docs/OAuth2Login

	[DataContract]
	public class GoogleGraph : IOAuth2Graph {
		private static DataContractJsonSerializer jsonSerializer = new DataContractJsonSerializer(typeof(GoogleGraph));

		/// <summary>
		/// Gets or sets the value of this field is an immutable identifier for the logged-in user, and may be used when creating and managing user sessions in your application. This identifier is the same regardless of the client_id. This provides the ability to correlate profile information across multiple applications in the same organization. The value of this field is the same as the value of the userid field returned by the TokenInfo endpoint.
		/// </summary>
		[DataMember(Name = "id", IsRequired = true)]
		public string Id { get; set; }

		/// <summary>
		/// Gets or sets the email address of the logged in user
		/// </summary>
		[DataMember(Name = "email")]
		public string Email { get; set; }

		/// <summary>
		/// Gets or sets a flag that indicates whether or not Google has been able to verify the email address.
		/// </summary>
		[DataMember(Name = "verified_email")]
		public bool? VerifiedEmail { get; set; }

		/// <summary>
		/// Gets or sets the full name of the logged in user
		/// </summary>
		[DataMember(Name = "name", IsRequired = true)]
		public string Name { get; set; }

		/// <summary>
		/// Gets or sets the first name of the logged in user
		/// </summary>
		[DataMember(Name = "given_name")]
		public string GivenName { get; set; }

		/// <summary>
		/// Gets or sets the last name of the logged in user
		/// </summary>
		[DataMember(Name = "family_name")]
		public string FamilyName { get; set; }

		/// <summary>
		/// Gets or sets the URL to the user's profile picture. If the user has no public profile, this field is not included.
		/// </summary>
		[DataMember(Name = "picture")]
		public Uri Picture { get; set; }

		/// <summary>
		/// Gets or sets the user's registered locale. If the user has no public profile, this field is not included.
		/// </summary>
		[DataMember(Name = "locale")]
		public string Locale { get; set; }

		/// <summary>
		/// Gets or sets the default timezone of the logged in user
		/// </summary>
		[DataMember(Name = "timezone")]
		public string Timezone { get; set; }

		/// <summary>
		/// Gets or sets the gender of the logged in user (other|female|male)
		/// </summary>
		[DataMember(Name = "gender")]
		public string Gender { get; set; }

		[DataMember(Name = "birthday")]
		public string Birthday { get; set; }

		[DataMember(Name = "link")]
		public Uri Link { get; set; }

		public Uri AvatarUrl {
			get {
				return this.Picture;
			}
		}

		public DateTime? BirthdayDT {
			get {
				if (!string.IsNullOrEmpty(this.Birthday) && (!this.Birthday.StartsWith("0000"))) {
					return DateTime.ParseExact(this.Birthday, "yyyy-MM-dd", null);
				}

				return null;
			}
		}

		public HumanGender GenderEnum {
			get {
				if (this.Gender == "male") {
					return HumanGender.Male;
				} else if (this.Gender == "female") {
					return HumanGender.Female;
				} else if (this.Gender == "other") {
					return HumanGender.Other;
				}

				return HumanGender.Unknown;
			}
		}

		public string FirstName {
			get {
				return this.GivenName;
			}
		}

		public string LastName {
			get {
				return this.FamilyName;
			}
		}

		public string UpdatedTime {
			get {
				return null;
			}
		}

		public static GoogleGraph Deserialize(string json) {
			if (string.IsNullOrEmpty(json)) {
				throw new ArgumentNullException("json");
			}

			return Deserialize(new MemoryStream(Encoding.UTF8.GetBytes(json)));
		}

		public static GoogleGraph Deserialize(Stream jsonStream) {
			if (jsonStream == null) {
				throw new ArgumentNullException("jsonStream");
			}

			return (GoogleGraph)jsonSerializer.ReadObject(jsonStream);
		}
	}
}
