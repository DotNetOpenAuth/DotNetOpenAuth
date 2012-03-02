//-----------------------------------------------------------------------
// <copyright file="OAuthToken.cs" company="Outercurve Foundation">
//     Copyright (c) Outercurve Foundation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace OAuthServiceProvider.Code {
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Web;
	using DotNetOpenAuth.OAuth.ChannelElements;

	public partial class OAuthToken : IServiceProviderRequestToken, IServiceProviderAccessToken {
		#region IServiceProviderRequestToken Members

		string IServiceProviderRequestToken.Token {
			get { return this.Token; }
		}

		string IServiceProviderRequestToken.ConsumerKey {
			get { return this.OAuthConsumer.ConsumerKey; }
		}

		DateTime IServiceProviderRequestToken.CreatedOn {
			get { return this.IssueDate; }
		}

		Uri IServiceProviderRequestToken.Callback {
			get { return string.IsNullOrEmpty(this.RequestTokenCallback) ? null : new Uri(this.RequestTokenCallback); }
			set { this.RequestTokenCallback = value.AbsoluteUri; }
		}

		string IServiceProviderRequestToken.VerificationCode {
			get { return this.RequestTokenVerifier; }
			set { this.RequestTokenVerifier = value; }
		}

		Version IServiceProviderRequestToken.ConsumerVersion {
			get { return new Version(this.ConsumerVersion); }
			set { this.ConsumerVersion = value.ToString(); }
		}

		#endregion

		#region IServiceProviderAccessToken Members

		string IServiceProviderAccessToken.Token {
			get { return this.Token; }
		}

		DateTime? IServiceProviderAccessToken.ExpirationDate {
			get { return null; }
		}

		string IServiceProviderAccessToken.Username {
			get { return this.User.OpenIDClaimedIdentifier; }
		}

		string[] IServiceProviderAccessToken.Roles {
			get { return this.Scope.Split('|'); }
		}

		#endregion

		/// <summary>
		/// Called by LinqToSql when the <see cref="IssueDate"/> property is about to change.
		/// </summary>
		/// <param name="value">The value.</param>
		partial void OnIssueDateChanging(DateTime value) {
			if (value.Kind == DateTimeKind.Unspecified) {
				throw new ArgumentException("The DateTime.Kind cannot be Unspecified to ensure accurate timestamp checks.");
			}
		}

		/// <summary>
		/// Called by LinqToSql when <see cref="IssueDate"/> has changed.
		/// </summary>
		partial void OnIssueDateChanged() {
			if (this.IssueDate.Kind == DateTimeKind.Local) {
				this._IssueDate = this.IssueDate.ToUniversalTime();
			}
		}

		/// <summary>
		/// Called by LinqToSql when a token instance is deserialized.
		/// </summary>
		partial void OnLoaded() {
			if (this.IssueDate.Kind == DateTimeKind.Unspecified) {
				// this detail gets lost in db storage, but must be reaffirmed so that expiratoin checks succeed.
				this._IssueDate = DateTime.SpecifyKind(this.IssueDate, DateTimeKind.Utc);
			}
		}
	}
}