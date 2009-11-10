namespace WebFormsRelyingParty {
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Web;
	using DotNetOpenAuth.OAuth.ChannelElements;

	public partial class IssuedRequestToken : IServiceProviderRequestToken {
		public Uri Callback {
			get { return this.CallbackAsString != null ? new Uri(this.CallbackAsString) : null; }
			set { this.CallbackAsString = value != null ? value.AbsoluteUri : null; }
		}

		Version IServiceProviderRequestToken.ConsumerVersion {
			get { return this.ConsumerVersionAsString != null ? new Version(this.ConsumerVersionAsString) : null; }
			set { this.ConsumerVersionAsString = value != null ? value.ToString() : null; }
		}

		string IServiceProviderRequestToken.ConsumerKey {
			get { return this.Consumer.ConsumerKey; }
		}
	}
}
