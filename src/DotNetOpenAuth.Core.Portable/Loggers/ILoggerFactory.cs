//-----------------------------------------------------------------------
// <copyright file="ILoggerFactory.cs" company="Andrew Arnott">
//     Copyright (c) Andrew Arnott. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace DotNetOpenAuth.Loggers {
	/// <summary>
	/// A factory for named loggers.
	/// </summary>
	public interface ILoggerFactory {
		/// <summary>
		/// Creates a logger.
		/// </summary>
		/// <param name="name">The name for the logger.</param>
		/// <returns>The instantiated logger.</returns>
		ILog CreateLogger(string name);
	}
}
