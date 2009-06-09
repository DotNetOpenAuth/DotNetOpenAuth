//-----------------------------------------------------------------------
// <copyright file="OAuthConsumer.cs" company="Andrew Arnott">
//     Copyright (c) Andrew Arnott. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using DotNetOpenAuth.OAuth.ChannelElements;

public partial class OAuthConsumer : IConsumerDescription {
	#region IConsumerDescription Members

	string IConsumerDescription.Key {
		get { return this.ConsumerKey; }
	}

	string IConsumerDescription.Secret {
		get { return this.ConsumerSecret; }
	}

	System.Security.Cryptography.X509Certificates.X509Certificate2 IConsumerDescription.Certificate {
		get { return null; }
	}

	Uri IConsumerDescription.Callback {
		get { return this.Callback != null ? new Uri(this.Callback) : null; }
	}

	DotNetOpenAuth.OAuth.VerificationCodeFormat IConsumerDescription.VerificationCodeFormat {
		get { return this.VerificationCodeFormat; }
	}

	int IConsumerDescription.VerificationCodeLength {
		get { return this.VerificationCodeLength; }
	}

	#endregion
}
