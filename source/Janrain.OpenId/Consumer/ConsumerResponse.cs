namespace Janrain.OpenId.Consumer
{
	using System;
	using System.Collections;
	using System.Collections.Specialized;

	public class ConsumerResponse
	{
		Uri identity_url;
		public Uri IdentityUrl
		{
			get { return identity_url; }
		}

		IDictionary signed_args;

		public Uri ReturnTo
		{
			get { return new Uri((string)this.signed_args[QueryStringArgs.openid.return_to], true); }
		}

		public ConsumerResponse(Uri identity_url, NameValueCollection query, string signed)
		{
			this.identity_url = identity_url;
			this.signed_args = new Hashtable();
			foreach (string field_name in signed.Split(','))
			{
				string field_name2 = QueryStringArgs.openid.Prefix + field_name;
				string val = query[field_name2];
				if (val == null)
					val = String.Empty;
				this.signed_args[field_name2] = val;
			}
		}
		public IDictionary ExtensionResponse(string prefix)
		{
			Hashtable response = new Hashtable();
			prefix = QueryStringArgs.openid.Prefix + prefix + ".";
			int prefix_len = prefix.Length;
			foreach (DictionaryEntry pair in this.signed_args)
			{
				string k = (string)pair.Key;
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
