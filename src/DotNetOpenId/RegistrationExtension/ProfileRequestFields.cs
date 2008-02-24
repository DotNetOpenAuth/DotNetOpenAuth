using System;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;
using System.Globalization;

namespace DotNetOpenId.RegistrationExtension {
	/// <summary>
	/// Carries the request/require/none demand state of the simple registration fields.
	/// </summary>
	public class ProfileRequestFields {
		public ProfileRequest Nickname { get; private set; }
		public ProfileRequest Email { get; private set; }
		public ProfileRequest FullName { get; private set; }
		public ProfileRequest BirthDate { get; private set; }
		public ProfileRequest Gender { get; private set; }
		public ProfileRequest PostalCode { get; private set; }
		public ProfileRequest Country { get; private set; }
		public ProfileRequest Language { get; private set; }
		public ProfileRequest TimeZone { get; private set; }

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
		internal void SetProfileRequestFromList(ICollection<string> fieldNames, ProfileRequest requestLevel) {
			foreach (string field in fieldNames) {
				switch (field) {
					case QueryStringArgs.openidnp.sregnp.nickname:
						Nickname = requestLevel;
						break;
					case QueryStringArgs.openidnp.sregnp.email:
						Email = requestLevel;
						break;
					case QueryStringArgs.openidnp.sregnp.fullname:
						FullName = requestLevel;
						break;
					case QueryStringArgs.openidnp.sregnp.dob:
						BirthDate = requestLevel;
						break;
					case QueryStringArgs.openidnp.sregnp.gender:
						Gender = requestLevel;
						break;
					case QueryStringArgs.openidnp.sregnp.postcode:
						PostalCode = requestLevel;
						break;
					case QueryStringArgs.openidnp.sregnp.country:
						Country = requestLevel;
						break;
					case QueryStringArgs.openidnp.sregnp.language:
						Language = requestLevel;
						break;
					case QueryStringArgs.openidnp.sregnp.timezone:
						TimeZone = requestLevel;
						break;
					default:
						Trace.TraceWarning("OpenIdProfileRequest.SetProfileRequestFromList: Unrecognized field name '{0}'.", field);
						break;
				}
			}
		}
		
		/// <summary>
		/// Gets whether any of the profile fields have been requested or required by
		/// the consumer.
		/// </summary>
		public bool AnyRequestedOrRequired {
			get {
				return (!(BirthDate == ProfileRequest.NoRequest
						  && Country == ProfileRequest.NoRequest
						  && Email == ProfileRequest.NoRequest
						  && FullName == ProfileRequest.NoRequest
						  && Gender == ProfileRequest.NoRequest
						  && Language == ProfileRequest.NoRequest
						  && Nickname == ProfileRequest.NoRequest
						  && PostalCode == ProfileRequest.NoRequest
						  && TimeZone == ProfileRequest.NoRequest));
			}
		}

		public override string ToString() {
			return string.Format(CultureInfo.CurrentUICulture, @"Nickname = '{0}' 
Email = '{1}' 
FullName = '{2}' 
Birthdate = '{3}'
Gender = '{4}'
PostalCode = '{5}'
Country = '{6}'
Language = '{7}'
TimeZone = '{8}'", Nickname, Email, FullName, BirthDate, Gender, PostalCode, Country, Language, TimeZone);
		}
	}
}
