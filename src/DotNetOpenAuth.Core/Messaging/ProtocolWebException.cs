// //-----------------------------------------------------------------------
// // <copyright file="ProtocolWebException.cs" company="Outercurve Foundation">
// //     Copyright (c) Outercurve Foundation. All rights reserved.
// // </copyright>
// //-----------------------------------------------------------------------
namespace DotNetOpenAuth.Messaging {
    using System;

    /// <summary>
    /// An exception with detailed web (http) response
    /// </summary>
    [Serializable]
    [System.Diagnostics.DebuggerDisplay("{WebResponse}")]
    public class ProtocolWebException : ProtocolException {
        private readonly string _webResponse;

        /// <summary>
        /// Initializes a new instance of the <see cref="ProtocolWebException"/> class.
        /// </summary>
        /// <param name="inner">The inner exception to include.</param>
        /// <param name="message">A message describing the specific error the occurred or was detected.</param>
        /// <param name="webResponse">The response string from <see cref="System.Net.WebException"/>.</param>
        public ProtocolWebException(Exception inner, string message, string webResponse)
            : base(message, inner) {
            _webResponse = webResponse;
        }

        /// <summary>
        /// The response string from <see cref="System.Net.WebException"/>.
        /// </summary>
        public string WebResponse {
            get { return this._webResponse; }
        }
    }
}