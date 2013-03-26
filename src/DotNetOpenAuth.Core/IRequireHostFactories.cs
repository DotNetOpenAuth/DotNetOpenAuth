//-----------------------------------------------------------------------
// <copyright file="IRequireHostFactories.cs" company="Andrew Arnott">
//     Copyright (c) Andrew Arnott. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace DotNetOpenAuth {
	/// <summary>
	/// An interface implemented by DotNetOpenAuth extensions that need to create host-specific instances of specific interfaces.
	/// </summary>
	public interface IRequireHostFactories {
		/// <summary>
		/// Gets or sets the host factories used by this instance.
		/// </summary>
		/// <value>
		/// The host factories.
		/// </value>
		IHostFactories HostFactories { get; set; }
	}
}
