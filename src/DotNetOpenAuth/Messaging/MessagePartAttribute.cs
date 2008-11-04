//-----------------------------------------------------------------------
// <copyright file="MessagePartAttribute.cs" company="Andrew Arnott">
//     Copyright (c) Andrew Arnott. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace DotNetOpenAuth.Messaging {
	using System;
	using System.Net.Security;
	using System.Reflection;

	/// <summary>
	/// Applied to fields and properties that form a key/value in a protocol message.
	/// </summary>
	[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, Inherited = true, AllowMultiple = false)]
	public sealed class MessagePartAttribute : Attribute {
		/// <summary>
		/// The overridden name to use as the serialized name for the property.
		/// </summary>
		private string name;

		/// <summary>
		/// Initializes a new instance of the <see cref="MessagePartAttribute"/> class.
		/// </summary>
		public MessagePartAttribute() {
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="MessagePartAttribute"/> class.
		/// </summary>
		/// <param name="name">
		/// A special name to give the value of this member in the serialized message.
		/// When null or empty, the name of the member will be used in the serialized message.
		/// </param>
		public MessagePartAttribute(string name) {
			this.Name = name;
		}

		/// <summary>
		/// Gets the name of the serialized form of this member in the message.
		/// </summary>
		public string Name {
			get { return this.name; }
			private set { this.name = string.IsNullOrEmpty(value) ? null : value; }
		}

		/// <summary>
		/// Gets or sets the level of protection required by this member in the serialized message.
		/// </summary>
		public ProtectionLevel RequiredProtection { get; set; }

		/// <summary>
		/// Gets or sets a value indicating whether this member is a required part of the serialized message.
		/// </summary>
		public bool IsRequired { get; set; }
	}
}
