//-----------------------------------------------------------------------
// <copyright file="MockOpenIdExtension.cs" company="Outercurve Foundation">
//     Copyright (c) Outercurve Foundation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace DotNetOpenAuth.Test.Mocks {
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using DotNetOpenAuth.Messaging;
	using DotNetOpenAuth.OpenId.Extensions;
	using DotNetOpenAuth.OpenId.Messages;

	internal class MockOpenIdExtension : IOpenIdMessageExtension {
		internal const string MockTypeUri = "http://mockextension";

		internal static readonly StandardOpenIdExtensionFactory.CreateDelegate Factory = (typeUri, data, baseMessage, isProviderRole) => {
			if (typeUri == MockTypeUri) {
				return new MockOpenIdExtension();
			}

			return null;
		};

		private IDictionary<string, string> extraData = new Dictionary<string, string>();

		internal MockOpenIdExtension() {
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="MockOpenIdExtension"/> class.
		/// </summary>
		/// <param name="partValue">The value of the 'Part' parameter.</param>
		/// <param name="extraValue">The value of the 'data' parameter.</param>
		internal MockOpenIdExtension(string partValue, string extraValue) {
			this.Part = partValue;
			this.Data = extraValue;
		}

		#region IOpenIdMessageExtension Members

		public string TypeUri {
			get { return MockTypeUri; }
		}

		public IEnumerable<string> AdditionalSupportedTypeUris {
			get { return Enumerable.Empty<string>(); }
		}

		/// <summary>
		/// Gets or sets a value indicating whether this extension was
		/// signed by the OpenID Provider.
		/// </summary>
		/// <value>
		/// 	<c>true</c> if this instance is signed by the provider; otherwise, <c>false</c>.
		/// </value>
		public bool IsSignedByRemoteParty { get; set; }

		#endregion

		#region IMessage Properties

		public Version Version {
			get { return new Version(1, 0); }
		}

		public IDictionary<string, string> ExtraData {
			get { return this.extraData; }
		}

		#endregion

		[MessagePart]
		internal string Part { get; set; }

		internal string Data {
			get {
				string data;
				this.extraData.TryGetValue("data", out data);
				return data;
			}

			set {
				this.extraData["data"] = value;
			}
		}

		public override bool Equals(object obj) {
			MockOpenIdExtension other = obj as MockOpenIdExtension;
			if (other == null) {
				return false;
			}

			return this.Part.EqualsNullSafe(other.Part) &&
				this.Data.EqualsNullSafe(other.Data);
		}

		public override int GetHashCode() {
			return 1; // mock doesn't need a good hash code algorithm
		}

		#region IMessage methods

		public void EnsureValidMessage() {
		}

		#endregion
	}
}
