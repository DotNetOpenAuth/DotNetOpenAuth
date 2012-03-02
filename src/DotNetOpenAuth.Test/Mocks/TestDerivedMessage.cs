//-----------------------------------------------------------------------
// <copyright file="TestDerivedMessage.cs" company="Outercurve Foundation">
//     Copyright (c) Outercurve Foundation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace DotNetOpenAuth.Test.Mocks {
	using System.Runtime.Serialization;
	using DotNetOpenAuth.Messaging;

	internal class TestDerivedMessage : TestBaseMessage {
		/// <summary>
		/// Gets or sets the first value.
		/// </summary>
		/// <remarks>
		/// This element should appear AFTER <see cref="SecondDerivedElement"/>
		/// due to alphabetical ordering rules, but after all the elements in the
		/// base class due to inheritance rules.
		/// </remarks>
		[MessagePart]
		public string TheFirstDerivedElement { get; set; }

		/// <summary>
		/// Gets or sets the second value.
		/// </summary>
		/// <remarks>
		/// This element should appear BEFORE <see cref="TheFirstDerivedElement"/>,
		/// but after all the elements in the base class.
		/// </remarks>
		[MessagePart]
		public string SecondDerivedElement { get; set; }
	}
}
