//-----------------------------------------------------------------------
// <copyright file="OAuthToken.cs" company="Andrew Arnott">
//     Copyright (c) Andrew Arnott. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

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
		get { return new Uri(this.RequestTokenCallback); }
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
}
