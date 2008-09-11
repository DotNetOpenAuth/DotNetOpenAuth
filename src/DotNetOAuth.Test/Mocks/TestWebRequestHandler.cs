//-----------------------------------------------------------------------
// <copyright file="TestWebRequestHandler.cs" company="Andrew Arnott">
//     Copyright (c) Andrew Arnott. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace DotNetOAuth.Test.Mocks {
	using System;
	using System.IO;
	using System.Net;
	using System.Text;
	using DotNetOAuth.Messaging;

	internal class TestWebRequestHandler : IWebRequestHandler {
		private StringBuilder postEntity;

		internal Func<HttpWebRequest, Response> Callback { get; set; }
		
		internal Stream RequestEntityStream {
			get {
				if (this.postEntity == null) {
					return null;
				}
				return new MemoryStream(Encoding.UTF8.GetBytes(this.postEntity.ToString()));
			}
		}

		#region IWebRequestHandler Members

		public TextWriter GetRequestStream(HttpWebRequest request) {
			this.postEntity = new StringBuilder();
			return new StringWriter(this.postEntity);
		}

		public Response GetResponse(HttpWebRequest request) {
			if (this.Callback == null) {
				throw new InvalidOperationException("Set the Callback property first.");
			}

			return this.Callback(request);
		}

		#endregion
	}
}
