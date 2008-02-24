using System;
using System.Collections.Generic;
using System.Text;
using System.Globalization;

namespace DotNetOpenId.Provider {

	public abstract class AssociatedRequest : Request {
		protected AssociatedRequest(Provider server) : base(server) { }

		internal string AssociationHandle { get; set; }

		public override string ToString() {
			string returnString = "AssociatedRequest.AssocHandle = {0}";
			return base.ToString() + Environment.NewLine + String.Format(CultureInfo.CurrentUICulture,
				returnString, AssociationHandle);
		}

	}
}