//-----------------------------------------------------------------------
// <copyright file="TestUtilities.cs" company="Andrew Arnott">
//     Copyright (c) Andrew Arnott. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace DotNetOpenAuth.Test {
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using log4net;

	/// <summary>
	/// An assortment of methods useful for testing.
	/// </summary>
	internal class TestUtilities {
		/// <summary>
		/// The logger that tests should use.
		/// </summary>
		internal static readonly ILog TestLogger = LogManager.GetLogger("DotNetOpenAuth.Test");
	}
}
