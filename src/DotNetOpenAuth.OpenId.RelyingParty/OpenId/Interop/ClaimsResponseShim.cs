//-----------------------------------------------------------------------
// <copyright file="ClaimsResponseShim.cs" company="Outercurve Foundation">
//     Copyright (c) Outercurve Foundation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace DotNetOpenAuth.OpenId.Interop {
	using System;
	using System.Diagnostics.CodeAnalysis;
	using System.Diagnostics.Contracts;
	using System.Runtime.InteropServices;
	using DotNetOpenAuth.Messaging;
	using DotNetOpenAuth.OpenId.Extensions.SimpleRegistration;

	/// <summary>
	/// A struct storing Simple Registration field values describing an
	/// authenticating user.
	/// </summary>
	[SuppressMessage("Microsoft.Interoperability", "CA1409:ComVisibleTypesShouldBeCreatable", Justification = "It's only creatable on the inside.  It must be ComVisible for ASP to see it.")]
	[ComVisible(true), Obsolete("This class acts as a COM Server and should not be called directly from .NET code.")]
	[ContractVerification(true)]
	public sealed class ClaimsResponseShim {
		/// <summary>
		/// The Simple Registration claims response message that this shim wraps.
		/// </summary>
		private readonly ClaimsResponse response;

		/// <summary>
		/// Initializes a new instance of the <see cref="ClaimsResponseShim"/> class.
		/// </summary>
		/// <param name="response">The Simple Registration response to wrap.</param>
		internal ClaimsResponseShim(ClaimsResponse response)
		{
			Requires.NotNull(response, "response");

			this.response = response;
		}

		/// <summary>
		/// Gets the nickname the user goes by.
		/// </summary>
		public string Nickname {
			get { return this.response.Nickname; }
		}

		/// <summary>
		/// Gets the user's email address.
		/// </summary>
		public string Email {
			get { return this.response.Email; }
		}

		/// <summary>
		/// Gets the full name of a user as a single string.
		/// </summary>
		public string FullName {
			get { return this.response.FullName; }
		}

		/// <summary>
		/// Gets the raw birth date string given by the extension.
		/// </summary>
		/// <value>A string in the format yyyy-MM-dd.</value>
		public string BirthDate {
			get { return this.response.BirthDateRaw; }
		}

		/// <summary>
		/// Gets the gender of the user.
		/// </summary>
		public string Gender {
			get {
				if (this.response.Gender.HasValue) {
					return this.response.Gender.Value == Extensions.SimpleRegistration.Gender.Male ? Constants.Genders.Male : Constants.Genders.Female;
				}
				return null;
			}
		}

		/// <summary>
		/// Gets the zip code / postal code of the user.
		/// </summary>
		public string PostalCode {
			get { return this.response.PostalCode; }
		}

		/// <summary>
		/// Gets the country of the user.
		/// </summary>
		public string Country {
			get { return this.response.Country; }
		}

		/// <summary>
		/// Gets the primary/preferred language of the user.
		/// </summary>
		public string Language {
			get { return this.response.Language; }
		}

		/// <summary>
		/// Gets the user's timezone.
		/// </summary>
		public string TimeZone {
			get { return this.response.TimeZone; }
		}
	}
}