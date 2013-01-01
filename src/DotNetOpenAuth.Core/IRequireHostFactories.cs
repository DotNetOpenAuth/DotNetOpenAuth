//-----------------------------------------------------------------------
// <copyright file="IRequireHostFactories.cs" company="Andrew Arnott">
//     Copyright (c) Andrew Arnott. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace DotNetOpenAuth {
	public interface IRequireHostFactories {
		IHostFactories HostFactories { get; set; }
	}
}
