using System;
using System.Collections.Generic;
using System.Text;
using DotNetOpenId;
using System.Collections.Specialized;

namespace DotNetOpenId.Consumer {
	public class AuthRequest {
		public enum Mode {
			Immediate,
			Setup
		}

		Association assoc;
		ServiceEndpoint endpoint;

		internal AuthRequest(string token, Association assoc, ServiceEndpoint endpoint) {
			Token = token;
			this.assoc = assoc;
			this.endpoint = endpoint;

			ExtraArgs = new Dictionary<string, string>();
			ReturnToArgs = new Dictionary<string, string>();
			ReturnToArgs.Add(DotNetOpenId.Consumer.Token.TokenKey, Token);
		}

		public string Token { get; set; }
		/// <summary>
		/// Arguments to add to the query string to be sent to the provider.
		/// </summary>
		public IDictionary<string, string> ExtraArgs { get; private set; }
		/// <summary>
		/// Arguments to add to the return_to part of the query string, so that
		/// these values come back to the consumer when the user agent returns.
		/// </summary>
		public IDictionary<string, string> ReturnToArgs { get; private set; }

		public Uri CreateRedirect(string trustRoot, Uri returnTo, Mode mode) {
			UriBuilder returnToBuilder = new UriBuilder(returnTo);
			UriUtil.AppendQueryArgs(returnToBuilder, this.ReturnToArgs);

			var qsArgs = new Dictionary<string, string>();

			qsArgs.Add(QueryStringArgs.openid.mode, (mode == Mode.Immediate) ?
				QueryStringArgs.Modes.checkid_immediate : QueryStringArgs.Modes.checkid_setup);
			qsArgs.Add(QueryStringArgs.openid.identity, this.endpoint.ServerId.AbsoluteUri); //TODO: breaks the Law of Demeter
			qsArgs.Add(QueryStringArgs.openid.return_to, returnToBuilder.ToString());
			qsArgs.Add(QueryStringArgs.openid.trust_root, trustRoot);

			if (this.assoc != null)
				qsArgs.Add(QueryStringArgs.openid.assoc_handle, this.assoc.Handle); // !!!!

			UriBuilder redir = new UriBuilder(this.endpoint.ServerUrl);

			UriUtil.AppendQueryArgs(redir, qsArgs);
			UriUtil.AppendQueryArgs(redir, ExtraArgs);

			return redir.Uri;
		}

	}
}
