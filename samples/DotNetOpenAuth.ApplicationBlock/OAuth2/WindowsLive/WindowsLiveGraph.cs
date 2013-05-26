//-----------------------------------------------------------------------
// <copyright file="WindowsLiveGraph.cs" company="Andrew Arnott">
//     Copyright (c) Andrew Arnott. All rights reserved.
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

	//// Documentation: http://msdn.microsoft.com/en-us/library/live/hh243648.aspx#user

	[DataContract]
	public class WindowsLiveGraph : IOAuth2Graph {
		private static DataContractJsonSerializer jsonSerializer = new DataContractJsonSerializer(typeof(WindowsLiveGraph));

		/// <summary>
		/// The user's ID.
		/// </summary>
		[DataMember(Name = "id", IsRequired = true)]
		public string Id { get; set; }

		/// <summary>
		/// The user's full name.
		/// </summary>
		[DataMember(Name = "name", IsRequired = true)]
		public string Name { get; set; }

		/// <summary>
		/// The user's first name.
		/// </summary>
		[DataMember(Name = "first_name")]
		public string FirstName { get; set; }

		/// <summary>
		/// The user's last name.
		/// </summary>
		[DataMember(Name = "last_name")]
		public string LastName { get; set; }

		/// <summary>
		/// The URL of the user's profile page.
		/// </summary>
		[DataMember(Name = "link")]
		public Uri Link { get; set; }

		/// <summary>
		/// The day of the user's birth date, or null if no birth date is specified.
		/// </summary>
		[DataMember(Name = "birth_day")]
		public int? BirthDay { get; set; }

		/// <summary>
		/// The month of the user's birth date, or null if no birth date is specified.
		/// </summary>
		[DataMember(Name = "birth_month")]
		public int? BirthMonth { get; set; }

		/// <summary>
		/// The year of the user's birth date, or null if no birth date is specified.
		/// </summary>
		[DataMember(Name = "birth_year")]
		public int? BirthYear { get; set; }

		/// <summary>
		/// An array that contains the user's work info.
		/// </summary>
		[DataMember(Name = "work")]
		public WindowsLiveWorkProfile[] Work { get; set; }

		/// <summary>
		/// The user's gender. Valid values are "male", "female", or null if the user's gender is not specified.
		/// </summary>
		[DataMember(Name = "gender")]
		public string Gender { get; set; }

		/// <summary>
		/// The user's email addresses.
		/// </summary>
		[DataMember(Name = "emails")]
		public WindowsLiveEmails Emails { get; set; }

		/// <summary>
		/// The user's postal addresses.
		/// </summary>
		[DataMember(Name = "addresses")]
		public WindowsLiveAddresses Addresses { get; set; }

		/// <summary>
		/// The user's phone numbers.
		/// </summary>
		[DataMember(Name = "phones")]
		public WindowsLivePhones Phones { get; set; }

		/// <summary>
		/// The user's locale code.
		/// </summary>
		[DataMember(Name = "locale", IsRequired = true)]
		public string Locale { get; set; }

		/// <summary>
		/// The time, in ISO 8601 format, at which the user last updated the object.
		/// </summary>
		[DataMember(Name = "updated_time")]
		public string UpdatedTime { get; set; }

		public string Email {
			get {
				return this.Emails.Account;
			}
		}

		public Uri AvatarUrl { get; set; }

		public DateTime? BirthdayDT {
			get {
				if (this.BirthYear.HasValue && this.BirthMonth.HasValue && this.BirthDay.HasValue) {
					return new DateTime(this.BirthYear.Value, this.BirthMonth.Value, this.BirthDay.Value);
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
				}

				return HumanGender.Unknown;
			}
		}

		public static WindowsLiveGraph Deserialize(string json) {
			if (string.IsNullOrEmpty(json)) {
				throw new ArgumentNullException("json");
			}

			return Deserialize(new MemoryStream(Encoding.UTF8.GetBytes(json)));
		}

		public static WindowsLiveGraph Deserialize(Stream jsonStream) {
			if (jsonStream == null) {
				throw new ArgumentNullException("jsonStream");
			}

			return (WindowsLiveGraph)jsonSerializer.ReadObject(jsonStream);
		}

		[DataContract]
		public class WindowsLiveEmails {
			/// <summary>
			/// The user's preferred email address, or null if one is not specified.
			/// </summary>
			[DataMember(Name = "preferred")]
			public string Preferred { get; set; }

			/// <summary>
			/// The email address that is associated with the account.
			/// </summary>
			[DataMember(Name = "account", IsRequired = true)]
			public string Account { get; set; }

			/// <summary>
			/// The user's personal email address, or null if one is not specified.
			/// </summary>
			[DataMember(Name = "personal")]
			public string Personal { get; set; }

			/// <summary>
			/// The user's business email address, or null if one is not specified.
			/// </summary>
			[DataMember(Name = "business")]
			public string Business { get; set; }

			/// <summary>
			/// The user's "alternate" email address, or null if one is not specified.
			/// </summary>
			[DataMember(Name = "other")]
			public string Other { get; set; }
		}

		[DataContract]
		public class WindowsLivePhones {
			/// <summary>
			/// The user's personal phone number, or null if one is not specified.
			/// </summary>
			[DataMember(Name = "personal")]
			public string Personal { get; set; }

			/// <summary>
			/// The user's business phone number, or null if one is not specified.
			/// </summary>
			[DataMember(Name = "business")]
			public string Business { get; set; }

			/// <summary>
			/// The user's mobile phone number, or null if one is not specified.
			/// </summary>
			[DataMember(Name = "mobile")]
			public string Mobile { get; set; }
		}

		[DataContract]
		public class WindowsLiveAddress {
			/// <summary>
			/// The street address, or null if one is not specified.
			/// </summary>
			[DataMember(Name = "street")]
			public string Street { get; set; }

			/// <summary>
			/// The second line of the street address, or null if one is not specified.
			/// </summary>
			[DataMember(Name = "street_2")]
			public string Street2 { get; set; }

			/// <summary>
			/// The city of the address, or null if one is not specified.
			/// </summary>
			[DataMember(Name = "city")]
			public string City { get; set; }

			/// <summary>
			/// The state of the address, or null if one is not specified.
			/// </summary>
			[DataMember(Name = "state")]
			public string State { get; set; }

			/// <summary>
			/// The postal code of the address, or null if one is not specified.
			/// </summary>
			[DataMember(Name = "postal_code")]
			public string PostalCode { get; set; }

			/// <summary>
			/// The region of the address, or null if one is not specified.
			/// </summary>
			[DataMember(Name = "region")]
			public string Region { get; set; }
		}

		[DataContract]
		public class WindowsLiveAddresses {
			/// <summary>
			/// The user's personal postal address.
			/// </summary>
			[DataMember(Name = "personal")]
			public WindowsLiveAddress Personal { get; set; }

			/// <summary>
			/// The user's business postal address.
			/// </summary>
			[DataMember(Name = "business")]
			public WindowsLiveAddress Business { get; set; }
		}

		[DataContract]
		public class WindowsLiveWorkProfile {
			/// <summary>
			/// Info about the user's employer.
			/// </summary>
			[DataMember(Name = "employer")]
			public WindowsLiveEmployer Employer { get; set; }

			/// <summary>
			/// Info about the user's employer.
			/// </summary>
			[DataMember(Name = "position")]
			public WindowsLivePosition Position { get; set; }
		}

		[DataContract]
		public class WindowsLiveEmployer {
			/// <summary>
			/// The name of the user's employer, or null if the employer's name is not specified.
			/// </summary>
			[DataMember(Name = "name")]
			public string Name { get; set; }
		}

		[DataContract]
		public class WindowsLivePosition {
			/// <summary>
			/// The name of the user's work position, or null if the name of the work position is not specified.
			/// </summary>
			[DataMember(Name = "name")]
			public string Name { get; set; }
		}
	}
}
