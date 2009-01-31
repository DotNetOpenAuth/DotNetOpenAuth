//-----------------------------------------------------------------------
// <copyright file="TestDirectResponseMessageWithHttpStatus.cs" company="Andrew Arnott">
//     Copyright (c) Andrew Arnott. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace DotNetOpenAuth.Test.Mocks {
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Text;
	using DotNetOpenAuth.Messaging;

	internal class TestDirectResponseMessageWithHttpStatus : TestMessage, IHttpDirectResponse {
		/// <summary>
		/// Initializes a new instance of the <see cref="TestDirectResponseMessageWithHttpStatus"/> class.
		/// </summary>
		internal TestDirectResponseMessageWithHttpStatus() {
		}

		#region IHttpDirectResponse Members

		/// <summary>
		/// Gets or sets the HTTP status code that the direct respones should be sent with.
		/// </summary>
		/// <value></value>
		public System.Net.HttpStatusCode HttpStatusCode { get; set;  }

		#endregion
	}
}
