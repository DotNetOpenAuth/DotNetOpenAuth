using System;
using System.Collections.Generic;
using System.Text;
using System.Web;
using System.Web.SessionState;

namespace DotNetOpenId {
	internal class SystemHttpSessionState : ISessionState {
		HttpSessionState sessionState;
		string prefix;

		/// <summary>
		/// Wraps an <see cref="ISessionState"/> around an ASP.NET session state bag.
		/// </summary>
		/// <param name="sessionState">The ASP.NET managed session.</param>
		/// <param name="prefix">
		/// An optional prefix to be used when storing keys in the session state
		/// so that OpenID doesn't use keys that the web site already uses elsewhere.
		/// </param>
		public SystemHttpSessionState(HttpSessionState sessionState, string prefix) {
			this.sessionState = sessionState;
			this.prefix = prefix;
		}

		public object this[string key] {
			get { return sessionState[prefix + key]; }
			set { sessionState[prefix + key] = value; }
		}
		public void Add(string key, object value) {
			sessionState.Add(prefix + key, value);
		}
		public void Remove(string key) {
			sessionState.Remove(prefix + key);
		}
	}
}
