//-----------------------------------------------------------------------
// <copyright file="MockOpenIdExtension.cs" company="Andrew Arnott">
//     Copyright (c) Andrew Arnott. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace DotNetOpenAuth.Test.Mocks {
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using DotNetOpenAuth.Messaging;
	using DotNetOpenAuth.OpenId.Messages;
	using DotNetOpenAuth.OpenId.Extensions;

	internal class MockOpenIdExtension : IOpenIdMessageExtension {
		private IDictionary<string, string> extraData = new Dictionary<string, string>();
		private const string MockTypeUri = "http://mockextension";

		internal static readonly OpenIdExtensionFactory.CreateDelegate Factory = (typeUri, data, baseMessage) => {
			if (typeUri == MockTypeUri) {
				return new MockOpenIdExtension();
			}

			return null;
		};

		internal MockOpenIdExtension() {
		}

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

		#endregion

		#region IMessage Members

		public Version Version {
			get { return new Version(1, 0); }
		}

		public IDictionary<string, string> ExtraData {
			get { return this.extraData; }
		}

		public void EnsureValidMessage() {
		}

		#endregion

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
	}
}
