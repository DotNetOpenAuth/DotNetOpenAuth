//-----------------------------------------------------------------------
// <copyright file="TestMessageWithDate.cs" company="Andrew Arnott">
//     Copyright (c) Andrew Arnott. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace DotNetOpenAuth.Test.Mocks {
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Text;
	using DotNetOpenAuth.Messaging;

	internal class TestMessageWithDate : TestBaseMessage {
		[MessagePart("ts", IsRequired = true)]
		internal DateTime Timestamp { get; set; }
	}
}
