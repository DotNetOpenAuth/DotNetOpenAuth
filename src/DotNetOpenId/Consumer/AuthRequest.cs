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

		Dictionary<string, string> extraArgs;
		Dictionary<string, string> returnToArgs;
		Association assoc;
		ServiceEndpoint endpoint;

		public AuthRequest(string token, Association assoc, ServiceEndpoint endpoint) {
			Token = token;
			this.assoc = assoc;
			this.endpoint = endpoint;

			this.extraArgs = new Dictionary<string, string>();
			this.returnToArgs = new Dictionary<string, string>();
		}

		public string Token { get; set; }

		public IDictionary<string, string> ExtraArgs {
			get { return this.extraArgs; }
		}

		public IDictionary<string, string> ReturnToArgs {
			get { return this.returnToArgs; }
		}

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
			UriUtil.AppendQueryArgs(redir, this.ExtraArgs);

			return redir.Uri;
		}

	}
}
