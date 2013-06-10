//-----------------------------------------------------------------------
// <copyright file="DefaultEncoderAttribute.cs" company="Andrew Arnott">
//     Copyright (c) Andrew Arnott. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace DotNetOpenAuth.Messaging.Reflection {
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Text;
	using Validation;

	/// <summary>
	/// Allows a custom class or struct to be serializable between itself and a string representation.
	/// </summary>
	[AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct, Inherited = false, AllowMultiple = false)]
	internal sealed class DefaultEncoderAttribute : Attribute {
		/// <summary>
		/// Initializes a new instance of the <see cref="DefaultEncoderAttribute"/> class.
		/// </summary>
		/// <param name="converterType">The <see cref="IMessagePartEncoder"/> implementing type to use for serializing this type.</param>
		public DefaultEncoderAttribute(Type converterType) {
			Requires.NotNull(converterType, "converterType");
			Requires.That(typeof(IMessagePartEncoder).IsAssignableFrom(converterType), "Argument must be a type that implements {0}.", typeof(IMessagePartEncoder).Name);
			this.Encoder = (IMessagePartEncoder)Activator.CreateInstance(converterType);
		}

		/// <summary>
		/// Gets the default encoder to use for the declaring class.
		/// </summary>
		public IMessagePartEncoder Encoder { get; private set; }
	}
}
