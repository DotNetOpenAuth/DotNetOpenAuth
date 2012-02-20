//-----------------------------------------------------------------------
// <copyright file="AcmeRequest.cs" company="Outercurve Foundation">
//     Copyright (c) Outercurve Foundation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace DotNetOpenAuth.ApplicationBlock.CustomExtensions {
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using DotNetOpenAuth.Messaging;
	using DotNetOpenAuth.OpenId.Messages;

	public class AcmeRequest : IOpenIdMessageExtension {
		private IDictionary<string, string> extraData = new Dictionary<string, string>();

		[MessagePart]
		public string FavoriteFlavor { get; set; }

		#region IOpenIdMessageExtension Members

		public string TypeUri {
			get { return Acme.CustomExtensionTypeUri; }
		}

		public IEnumerable<string> AdditionalSupportedTypeUris {
			get { return Enumerable.Empty<string>(); }
		}

		public bool IsSignedByRemoteParty { get; set; }

		#endregion

		#region IMessage Members

		public Version Version {
			get { return Acme.Version; }
		}

		public IDictionary<string, string> ExtraData {
			get { return this.extraData; }
		}

		public void EnsureValidMessage() {
		}

		#endregion
	}
}
