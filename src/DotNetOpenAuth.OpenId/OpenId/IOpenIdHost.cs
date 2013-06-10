//-----------------------------------------------------------------------
// <copyright file="IOpenIdHost.cs" company="Outercurve Foundation">
//     Copyright (c) Outercurve Foundation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace DotNetOpenAuth.OpenId {
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Net.Http;
	using System.Text;
	using DotNetOpenAuth.Messaging;

	/// <summary>
	/// An interface implemented by both providers and relying parties.
	/// </summary>
	internal interface IOpenIdHost {
		/// <summary>
		/// Gets the security settings.
		/// </summary>
		SecuritySettings SecuritySettings { get; }

		/// <summary>
		/// Gets the factory for various dependencies.
		/// </summary>
		IHostFactories HostFactories { get; }
	}
}
