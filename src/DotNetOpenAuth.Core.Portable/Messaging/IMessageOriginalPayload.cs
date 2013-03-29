//-----------------------------------------------------------------------
// <copyright file="IMessageOriginalPayload.cs" company="Outercurve Foundation">
//     Copyright (c) Outercurve Foundation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace DotNetOpenAuth.Messaging {
	using System;
	using System.Collections.Generic;
	using System.Diagnostics.CodeAnalysis;
	using System.Text;

	/// <summary>
	/// An interface that appears on messages that need to retain a description of
	/// what their literal payload was when they were deserialized.
	/// </summary>
	public interface IMessageOriginalPayload {
		/// <summary>
		/// Gets or sets the original message parts, before any normalization or default values were assigned.
		/// </summary>
		[SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly", Justification = "By design")]
		IDictionary<string, string> OriginalPayload { get; set; }
	}
}
