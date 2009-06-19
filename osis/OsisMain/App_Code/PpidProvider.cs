using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using DotNetOpenAuth.OpenId.Provider;
using DotNetOpenAuth.OpenId;

/// <summary>
/// Summary description for PpidProvider
/// </summary>
public class PpidProvider : PrivatePersonalIdentifierProviderBase {
	/// <summary>
	/// Initializes a new instance of the <see cref="PpidProvider"/> class.
	/// </summary>
	public PpidProvider() : base(Util.GetAppPathRootedUri("RP/GSALevel1Identity.aspx?id=")) {
	}

	protected override byte[] GetHashSaltForLocalIdentifier(Identifier localIdentifier) {
		return new byte[] { 0x55 };
	}
}
