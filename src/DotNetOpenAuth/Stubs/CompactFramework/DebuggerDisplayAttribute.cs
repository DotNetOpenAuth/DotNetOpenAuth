//-----------------------------------------------------------------------
// <copyright file="DebuggerDisplayAttribute.cs" company="Andrew Arnott">
//     Copyright (c) Andrew Arnott. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace System.Diagnostics {
	[global::System.AttributeUsage(System.AttributeTargets.Class, Inherited = false, AllowMultiple = false)]
	[Conditional("StubbedAttributes")] // we never want these to actually compile in
	sealed class DebuggerDisplayAttribute : System.Attribute {
		public DebuggerDisplayAttribute(string display) {
		}
	}
}