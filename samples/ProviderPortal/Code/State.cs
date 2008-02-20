using System;
using System.Data;
using System.Configuration;
using System.Web;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.UI.WebControls.WebParts;
using System.Web.UI.HtmlControls;
using DotNetOpenId.Provider;

/// <summary>
/// Helps manage state across requests.
/// </summary>
public class State {
	const string sessionStateKey = "SessionState";
	public static SessionState Session {
		get {
			SessionState state = HttpContext.Current.Session[sessionStateKey] as SessionState;
			if (state == null) {
				HttpContext.Current.Session[sessionStateKey] = state = new SessionState();
			}
			return state;
		}
	}

	[Serializable()]
	public class SessionState {
		CheckIdRequest lastRequest;
		public CheckIdRequest LastRequest {
			get { return lastRequest; }
			set { lastRequest = value; }
		}

		/// <summary>
		/// Ensures that memory of a prior request as part of an OpenID authentication
		/// exists.  Throws an exception if this is not the case.
		/// </summary>
		public void CheckExpectedStateIsAvailable() {
			if (LastRequest == null) {
				throw new InvalidOperationException("The CheckIdRequest has not been set. This usually means that Http Session is not available and the OpenID request needs to be restarted.");
			}
		}

		/// <summary>
		/// Clears memory of a prior request.
		/// </summary>
		public void Reset() {
			LastRequest = null;
		}
	}
}