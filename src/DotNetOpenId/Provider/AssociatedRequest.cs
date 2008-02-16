using System;
using System.Collections.Generic;
using System.Text;

namespace DotNetOpenId.Provider {

	public abstract class AssociatedRequest : Request {
		protected AssociatedRequest(Server server) : base(server) { }

		public string AssocHandle { get; set; }

		public override string ToString() {
			string returnString = "AssociatedRequest.AssocHandle = {0}";
			return base.ToString() + Environment.NewLine + String.Format(returnString, AssocHandle);
		}

	}
}