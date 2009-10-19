//-----------------------------------------------------------------------
// <copyright file="Contract.cs" company="Andrew Arnott">
//     Copyright (c) Andrew Arnott. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace System.Diagnostics.Contracts {
	using System;
	using System.Linq;
	using System.Collections.Generic;
	using System.Text;
	using DotNetOpenAuth.Messaging;

	internal class Contract {
		internal static void Requires(bool condition) {
			ErrorUtilities.VerifyArgument(condition, "invalid argument");
		}

		internal static void Assume(bool condition) {
			Debug.Assert(condition, "invalid state");
		}

		[Conditional("StubbedOut")]
		internal static void Ensures(bool condition) {
		}

		[Conditional("StubbedOut")]
		internal static void EnsuresOnThrow<T>(bool condition) {
		}

		internal static T Result<T>() {
			throw new NotImplementedException();
		}
	}
}
