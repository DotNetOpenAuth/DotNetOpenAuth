//-----------------------------------------------------------------------
// <copyright file="MessageProtectionTasks.cs" company="Andrew Arnott">
//     Copyright (c) Andrew Arnott. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace DotNetOpenAuth.Messaging {
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Text;
	using System.Threading.Tasks;

	/// <summary>
	/// Reusable pre-completed tasks that may be returned multiple times to reduce GC pressure.
	/// </summary>
	internal static class MessageProtectionTasks {
		/// <summary>
		/// A task whose result is <c>null</c>
		/// </summary>
		internal static readonly Task<MessageProtections?> Null = Task.FromResult<MessageProtections?>(null);

		/// <summary>
		/// A task whose result is <see cref="MessageProtections.None"/>
		/// </summary>
		internal static readonly Task<MessageProtections?> None =
			Task.FromResult<MessageProtections?>(MessageProtections.None);

		/// <summary>
		/// A task whose result is <see cref="MessageProtections.TamperProtection"/>
		/// </summary>
		internal static readonly Task<MessageProtections?> TamperProtection =
			Task.FromResult<MessageProtections?>(MessageProtections.TamperProtection);

		/// <summary>
		/// A task whose result is <see cref="MessageProtections.ReplayProtection"/>
		/// </summary>
		internal static readonly Task<MessageProtections?> ReplayProtection =
			Task.FromResult<MessageProtections?>(MessageProtections.ReplayProtection);
	}
}
