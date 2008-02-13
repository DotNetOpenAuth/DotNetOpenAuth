namespace DotNetOpenId.Consumer
{
	using System;
	using System.Collections;
	using System.Collections.Specialized;
	using System.Collections.Generic;

	public class ConsumerResponse
	{
		Uri identity_url;
		public Uri IdentityUrl
		{
			get { return identity_url; }
		}

		IDictionary<string, string> signed_args;

		public Uri ReturnTo
		{
			get { return new Uri(this.signed_args[QueryStringArgs.openid.return_to], true); }
		}

		public ConsumerResponse(Uri identity_url, IDictionary<string, string> query, string signed)
		{
			this.identity_url = identity_url;
			this.signed_args = new Dictionary<string, string>();
			foreach (string field_name in signed.Split(','))
			{
				string field_name2 = QueryStringArgs.openid.Prefix + field_name;
				string val = query[field_name2];
				if (val == null)
					val = String.Empty;
				this.signed_args[field_name2] = val;
			}
		}
		public Dictionary<string, string> ExtensionResponse(string prefix)
		{
			var response = new Dictionary<string, string>();
			prefix = QueryStringArgs.openid.Prefix + prefix + ".";
			int prefix_len = prefix.Length;
			foreach (var pair in this.signed_args)
			{
				string k = pair.Key;
				if (k.StartsWith(prefix))
				{
					string response_key = k.Substring(prefix_len);
					response[response_key] = pair.Value;
				}
			}

			return response;
		}

	}
}
