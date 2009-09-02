//-----------------------------------------------------------------------
// <copyright file="Protocol.cs" company="Andrew Arnott">
//     Copyright (c) Andrew Arnott. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace DotNetOpenAuth.SimpleAuth {
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Text;

	/// <summary>
	/// Protocol constants for Simple Auth.
	/// </summary>
	internal class Protocol {
		/// <summary>
		/// The default (latest) version of the Simple Auth protocol.
		/// </summary>
		internal static readonly Version DefaultVersion = V10;

		/// <summary>
		/// The initial (1.0) version of Simple Auth.
		/// </summary>
		internal static readonly Version V10 = new Version(1, 0);
	}
}
