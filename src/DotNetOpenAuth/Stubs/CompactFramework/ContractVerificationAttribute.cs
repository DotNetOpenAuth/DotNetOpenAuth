//-----------------------------------------------------------------------
// <copyright file="ContractVerificationAttribute.cs" company="Andrew Arnott">
//     Copyright (c) Andrew Arnott. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace System.Diagnostics.Contracts {
	using System;
	using System.Linq;
	using System.Collections.Generic;
	using System.Text;
	using System.Diagnostics;

	[global::System.AttributeUsage(AttributeTargets.All, Inherited = false, AllowMultiple = false)]
	[Conditional("StubbedAttributes")] // we never want these to actually compile in
	internal sealed class ContractVerificationAttribute : Attribute {
		public ContractVerificationAttribute(bool verification) {
		}
	}
}
