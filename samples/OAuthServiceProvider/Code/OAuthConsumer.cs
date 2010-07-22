//-----------------------------------------------------------------------
// <copyright file="OAuthConsumer.cs" company="Andrew Arnott">
//     Copyright (c) Andrew Arnott. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace OAuthServiceProvider.Code {
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Web;
	using DotNetOpenAuth.OAuth2;

	public partial class Client : IConsumerDescription {
		#region IConsumerDescription Members

		string IConsumerDescription.Secret {
			get { return this.ClientSecret; }
		}

		Uri IConsumerDescription.Callback {
			get { return string.IsNullOrEmpty(this.Callback) ? null : new Uri(this.Callback); }
		}

		#endregion
	}
}