//-----------------------------------------------------------------------
// <copyright file="FacebookGraph.cs" company="Outercurve Foundation">
//     Copyright (c) Outercurve Foundation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace DotNetOpenAuth.ApplicationBlock {
	using System;
	using System.Collections.Generic;
	using System.Globalization;
	using System.IO;
	using System.Linq;
	using System.Runtime.Serialization;
	using System.Runtime.Serialization.Json;
	using System.Text;

	//// Documentation: https://developers.facebook.com/docs/reference/api/user/

	[DataContract]
	public class FacebookGraph : IOAuth2Graph {
		private static DataContractJsonSerializer jsonSerializer = new DataContractJsonSerializer(typeof(FacebookGraph));

		/// <summary>
		/// Gets or sets the user's Facebook ID
		/// </summary>
		[DataMember(Name = "id")]
		public string Id { get; set; }

		/// <summary>
		/// Gets or sets the user's full name
		/// </summary>
		[DataMember(Name = "name")]
		public string Name { get; set; }

		/// <summary>
		/// Gets or sets the user's first name
		/// </summary>
		[DataMember(Name = "first_name")]
		public string FirstName { get; set; }

		/// <summary>
		/// Gets or sets the user's middle name
		/// </summary>
		[DataMember(Name = "middle_name")]
		public string MiddleName { get; set; }

		/// <summary>
		/// Gets or sets the user's last name
		/// </summary>
		[DataMember(Name = "last_name")]
		public string LastName { get; set; }

		/// <summary>
		/// Gets or sets the user's gender: female or male
		/// </summary>
		[DataMember(Name = "gender")]
		public string Gender { get; set; }

		/// <summary>
		/// Gets or sets the user's locale
		/// </summary>
		[DataMember(Name = "locale")]
		public string Locale { get; set; }

		/// <summary>
		/// Gets or sets the user's languages
		/// </summary>
		[DataMember(Name = "languages")]
		public FacebookIdName[] Languages { get; set; }

		/// <summary>
		/// Gets or sets the URL of the profile for the user on Facebook
		/// </summary>
		[DataMember(Name = "link")]
		public Uri Link { get; set; }

		/// <summary>
		/// Gets or sets the user's Facebook username
		/// </summary>
		[DataMember(Name = "username")]
		public string Username { get; set; }

		// age_range

		// third_party_id

		// installed

		/// <summary>
		/// Gets or sets the user's timezone offset from UTC
		/// </summary>
		[DataMember(Name = "timezone")]
		public int? Timezone { get; set; }

		/// <summary>
		/// Gets or sets the last time the user's profile was updated; changes to the languages, link, timezone, verified, interested_in, favorite_athletes, favorite_teams, and video_upload_limits are not not reflected in this value
		/// string containing an ISO-8601 datetime
		/// </summary>
		[DataMember(Name = "updated_time")]
		public string UpdatedTime { get; set; }

		// verified

		// bio

		/// <summary>
		/// Gets or sets the user's birthday
		/// Date string in MM/DD/YYYY format
		/// </summary>
		[DataMember(Name = "birthday")]
		public string Birthday { get; set; }

		[Obsolete]
		[DataMember(Name = "birthday_date")]
		public string BirthdayDate { get; set; }

		// cover

		// currency

		// devices

		// education

		/// <summary>
		/// Gets or sets the proxied or contact email address granted by the user
		/// </summary>
		[DataMember(Name = "email")]
		public string Email { get; set; }

		/// <summary>
		/// Gets or sets the user's hometown
		/// </summary>
		[DataMember(Name = "hometown")]
		public FacebookIdName Hometown { get; set; }

		/// <summary>
		/// Gets or sets the genders the user is interested in
		/// </summary>
		[DataMember(Name = "interested_in")]
		public string[] InterestedIn { get; set; }

		/// <summary>
		/// Gets or sets the user's current city
		/// </summary>
		[DataMember(Name = "location")]
		public FacebookIdName Location { get; set; }

		/// <summary>
		/// Gets or sets the user's political view
		/// </summary>
		[DataMember(Name = "political")]
		public string Political { get; set; }

		// payment_pricepoints

		/// <summary>
		/// Gets or sets the user's favorite athletes; this field is deprecated and will be removed in the near future
		/// </summary>
		[Obsolete]
		[DataMember(Name = "favorite_athletes")]
		public FacebookIdName[] FavoriteAthletes { get; set; }

		/// <summary>
		/// Gets or sets the user's favorite teams; this field is deprecated and will be removed in the near future
		/// </summary>
		[Obsolete]
		[DataMember(Name = "favorite_teams")]
		public FacebookIdName[] FavoriteTeams { get; set; }

		/// <summary>
		/// Gets or sets the URL of the user's profile pic (only returned if you explicitly specify a 'fields=picture' param)
		/// If the "October 2012 Breaking Changes" migration setting is enabled for your app, this field will be an object with the url and is_silhouette fields; is_silhouette is true if the user has not uploaded a profile picture
		/// </summary>
		[DataMember(Name = "picture")]
		public FacebookPicture Picture { get; set; }

		/// <summary>
		/// Gets or sets the user's favorite quotes
		/// </summary>
		[DataMember(Name = "quotes")]
		public Uri Quotes { get; set; }

		/// <summary>
		/// Gets or sets the user's relationship status: Single, In a relationship, Engaged, Married, It's complicated, In an open relationship, Widowed, Separated, Divorced, In a civil union, In a domestic partnership
		/// </summary>
		[DataMember(Name = "relationship_status")]
		public string RelationshipStatus { get; set; }

		/// <summary>
		/// Gets or sets the user's religion
		/// </summary>
		[DataMember(Name = "religion")]
		public string Religion { get; set; }

		// security_settings

		/// <summary>
		/// Gets or sets the user's significant other
		/// </summary>
		[DataMember(Name = "significant_other")]
		public FacebookIdName SignificantOther { get; set; }

		// video_upload_limits

		/// <summary>
		/// Gets or sets the URL of the user's personal website
		/// </summary>
		[DataMember(Name = "website")]
		public Uri Website { get; set; }

		public DateTime? BirthdayDT {
			get {
				if (!string.IsNullOrEmpty(this.Birthday) && (this.Locale != null)) {
					CultureInfo ci = new CultureInfo(this.Locale.Replace('_', '-'));
					return DateTime.Parse(this.Birthday, ci);
				}

				return null;
			}
		}

		public Uri AvatarUrl {
			get {
				if ((this.Picture != null) && (this.Picture.Data != null) && (this.Picture.Data.Url != null)) {
					return this.Picture.Data.Url;
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

		public static FacebookGraph Deserialize(string json) {
			if (string.IsNullOrEmpty(json)) {
				throw new ArgumentNullException("json");
			}

			return Deserialize(new MemoryStream(Encoding.UTF8.GetBytes(json)));
		}

		public static FacebookGraph Deserialize(Stream jsonStream) {
			if (jsonStream == null) {
				throw new ArgumentNullException("jsonStream");
			}

			return (FacebookGraph)jsonSerializer.ReadObject(jsonStream);
		}

		public static class Fields {
			public const string Defaults = "id,name,first_name,middle_name,last_name,gender,locale,link,username";

			public const string Birthday = "locale,birthday";

			public const string Email = "email";

			public const string Picture = "picture";
		}

		/// <summary>
		/// Obsolete: used only before October 2012
		/// </summary>
		[Obsolete]
		[DataContract]
		public class FacebookPicture {
			[DataMember(Name = "data")]
			public FacebookPictureData Data { get; set; }
		}

		/// <summary>
		/// Obsolete: used only before October 2012
		/// </summary>
		[Obsolete]
		[DataContract]
		public class FacebookPictureData {
			[DataMember(Name = "url")]
			public Uri Url { get; set; }

			[DataMember(Name = "is_silhouette")]
			public bool IsSilhouette { get; set; }
		}

		[DataContract]
		public class FacebookIdName {
			[DataMember(Name = "id")]
			public string Id { get; set; }

			[DataMember(Name = "name")]
			public string Name { get; set; }
		}
	}
}
