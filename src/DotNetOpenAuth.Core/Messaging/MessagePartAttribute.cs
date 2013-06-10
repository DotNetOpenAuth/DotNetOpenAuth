//-----------------------------------------------------------------------
// <copyright file="MessagePartAttribute.cs" company="Outercurve Foundation">
//     Copyright (c) Outercurve Foundation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace DotNetOpenAuth.Messaging {
	using System;
	using System.Diagnostics;
	using System.Net.Security;
	using System.Reflection;

	/// <summary>
	/// Applied to fields and properties that form a key/value in a protocol message.
	/// </summary>
	[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, Inherited = true, AllowMultiple = true)]
	[DebuggerDisplay("MessagePartAttribute {Name}")]
	public sealed class MessagePartAttribute : Attribute {
		/// <summary>
		/// The overridden name to use as the serialized name for the property.
		/// </summary>
		private string name;

		/// <summary>
		/// Initializes a new instance of the <see cref="MessagePartAttribute"/> class.
		/// </summary>
		public MessagePartAttribute() {
			this.AllowEmpty = true;
			this.MinVersionValue = new Version(0, 0);
			this.MaxVersionValue = new Version(int.MaxValue, 0);
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="MessagePartAttribute"/> class.
		/// </summary>
		/// <param name="name">
		/// A special name to give the value of this member in the serialized message.
		/// When null or empty, the name of the member will be used in the serialized message.
		/// </param>
		public MessagePartAttribute(string name)
			: this() {
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
		/// <remarks>
		/// Message part protection must be provided and verified by the channel binding element(s)
		/// that provide security.
		/// </remarks>
		public ProtectionLevel RequiredProtection { get; set; }

		/// <summary>
		/// Gets or sets a value indicating whether this member is a required part of the serialized message.
		/// </summary>
		public bool IsRequired { get; set; }

		/// <summary>
		/// Gets or sets a value indicating whether the string value is allowed to be empty in the serialized message.
		/// </summary>
		/// <value>Default is true.</value>
		public bool AllowEmpty { get; set; }

		/// <summary>
		/// Gets or sets an IMessagePartEncoder custom encoder to use
		/// to translate the applied member to and from a string.
		/// </summary>
		public Type Encoder { get; set; }

		/// <summary>
		/// Gets or sets the minimum version of the protocol this attribute applies to
		/// and overrides any attributes with lower values for this property.
		/// </summary>
		/// <value>Defaults to 0.0.</value>
		public string MinVersion {
			get { return this.MinVersionValue.ToString(); }
			set { this.MinVersionValue = new Version(value); }
		}

		/// <summary>
		/// Gets or sets the maximum version of the protocol this attribute applies to.
		/// </summary>
		/// <value>Defaults to int.MaxValue for the major version number.</value>
		/// <remarks>
		/// Specifying <see cref="MinVersion"/> on another attribute on the same member
		/// automatically turns this attribute off.  This property should only be set when
		/// a property is totally dropped from a newer version of the protocol.
		/// </remarks>
		public string MaxVersion {
			get { return this.MaxVersionValue.ToString(); }
			set { this.MaxVersionValue = new Version(value); }
		}

		/// <summary>
		/// Gets or sets a value indicating whether the value contained by this property contains
		/// sensitive information that should generally not be logged.
		/// </summary>
		/// <value>
		/// <c>true</c> if this instance is security sensitive; otherwise, <c>false</c>.
		/// </value>
		public bool IsSecuritySensitive { get; set; }

		/// <summary>
		/// Gets or sets the minimum version of the protocol this attribute applies to
		/// and overrides any attributes with lower values for this property.
		/// </summary>
		/// <value>Defaults to 0.0.</value>
		internal Version MinVersionValue { get; set; }

		/// <summary>
		/// Gets or sets the maximum version of the protocol this attribute applies to.
		/// </summary>
		/// <value>Defaults to int.MaxValue for the major version number.</value>
		/// <remarks>
		/// Specifying <see cref="MinVersion"/> on another attribute on the same member
		/// automatically turns this attribute off.  This property should only be set when
		/// a property is totally dropped from a newer version of the protocol.
		/// </remarks>
		internal Version MaxVersionValue { get; set; }
	}
}
