//-----------------------------------------------------------------------
// <copyright file="ValueMapping.cs" company="Andrew Arnott">
//     Copyright (c) Andrew Arnott. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace DotNetOAuth.Messaging.Reflection {
	using System;

	internal struct ValueMapping {
		internal Func<object, string> ValueToString;
		internal Func<string, object> StringToValue;

		internal ValueMapping(Func<object, string> toString, Func<string, object> toValue) {
			if (toString == null) {
				throw new ArgumentNullException("toString");
			}

			if (toValue == null) {
				throw new ArgumentNullException("toValue");
			}

			this.ValueToString = toString;
			this.StringToValue = toValue;
		}
	}
}
