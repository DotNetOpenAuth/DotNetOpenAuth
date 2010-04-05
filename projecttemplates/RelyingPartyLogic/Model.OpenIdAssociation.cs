//-----------------------------------------------------------------------
// <copyright file="Model.OpenIdAssociation.cs" company="Andrew Arnott">
//     Copyright (c) Andrew Arnott. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace RelyingPartyLogic {
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Text;

	public partial class OpenIdAssociation {
		partial void OnPrivateDataChanged() {
			this.PrivateDataLength = this.PrivateData.Length;
		}
	}
}
