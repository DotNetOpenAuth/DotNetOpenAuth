//-----------------------------------------------------------------------
// <copyright file="Model.Consumer.cs" company="Andrew Arnott">
//     Copyright (c) Andrew Arnott. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace WebFormsRelyingParty {
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Security.Cryptography.X509Certificates;
	using System.Web;
	using DotNetOpenAuth.OAuth;
	using DotNetOpenAuth.OAuth.ChannelElements;

	public partial class Consumer : IConsumerDescription {
		public VerificationCodeFormat VerificationCodeFormat {
			get { return (VerificationCodeFormat)this.VerificationCodeFormatAsInt; }
			set { this.VerificationCodeFormatAsInt = (int)value; }
		}

		public X509Certificate2 Certificate {
			get { return this.X509CertificateAsBinary != null ? new X509Certificate2(this.X509CertificateAsBinary) : null; }
			set { this.X509CertificateAsBinary = value != null ? value.RawData : null; }
		}

		public Uri Callback {
			get { return this.CallbackAsString != null ? new Uri(this.CallbackAsString) : null; }
			set { this.CallbackAsString = value != null ? value.AbsoluteUri : null; }
		}

		string IConsumerDescription.Secret {
			get { return this.ConsumerSecret; }
		}

		string IConsumerDescription.Key {
			get { return this.ConsumerKey; }
		}
	}
}
