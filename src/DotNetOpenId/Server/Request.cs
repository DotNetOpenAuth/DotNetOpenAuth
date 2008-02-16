using System;
using System.Collections.Generic;
using System.Text;
using System.Collections.Specialized;

namespace DotNetOpenId.Server
{
	public enum RequestType {
		CheckIdRequest,
		CheckAuthRequest,
		AssociateRequest,
	}

	public abstract class Request {

		internal abstract string Mode { get; }
		public abstract RequestType RequestType { get; }

		public override string ToString() {
			string returnString = @"Request.Mode = {0}";
			return String.Format(returnString, Mode);
		}
	}
}
