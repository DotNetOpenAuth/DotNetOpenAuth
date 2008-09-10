//-----------------------------------------------------------------------
// <copyright file="TestBadChannel.cs" company="Andrew Arnott">
//     Copyright (c) Andrew Arnott. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace DotNetOAuth.Test.Mocks {
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Text;
	using DotNetOAuth.Messaging;

	/// <summary>
	/// A Channel derived type that passes null to the protected constructor.
	/// </summary>
	internal class TestBadChannel : Channel {
		internal TestBadChannel()
			: base(null) {
		}

		protected internal override IProtocolMessage Request(IDirectedProtocolMessage request) {
			throw new NotImplementedException();
		}

		protected internal override IProtocolMessage ReadFromResponse(System.IO.Stream responseStream) {
			throw new NotImplementedException();
		}

		protected override void SendDirectMessageResponse(IProtocolMessage response) {
			throw new NotImplementedException();
		}
	}
}
