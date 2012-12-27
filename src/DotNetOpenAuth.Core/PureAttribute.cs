//-----------------------------------------------------------------------
// <copyright file="PureAttribute.cs" company="Andrew Arnott">
//     Copyright (c) Andrew Arnott. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace System.Diagnostics.Contracts {
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Text;

#if !CLR4
	/// <summary>
	/// Designates a type or member as one that does not mutate any objects that were allocated
	/// before the invocation of the member.
	/// </summary>
	[AttributeUsage(AttributeTargets.All, Inherited = false, AllowMultiple = false)]
	internal sealed class PureAttribute : Attribute {
	}
#endif
}
