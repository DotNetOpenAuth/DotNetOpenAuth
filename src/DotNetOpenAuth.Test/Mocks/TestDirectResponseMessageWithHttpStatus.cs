//-----------------------------------------------------------------------
// <copyright file="TestDirectResponseMessageWithHttpStatus.cs" company="Outercurve Foundation">
//     Copyright (c) Outercurve Foundation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace DotNetOpenAuth.Test.Mocks {
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Net;
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
		public HttpStatusCode HttpStatusCode { get; set;  }

		/// <summary>
		/// Gets the HTTP headers to add to the response.
		/// </summary>
		/// <value>May be an empty collection, but must not be <c>null</c>.</value>
		public WebHeaderCollection Headers {
			get { return new WebHeaderCollection(); }
		}

		#endregion
	}
}
