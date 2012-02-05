//-----------------------------------------------------------------------
// <copyright file="CheckAdminRights.cs" company="Outercurve Foundation">
//     Copyright (c) Outercurve Foundation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace DotNetOpenAuth.BuildTasks {
	using System.Security.Principal;
	using Microsoft.Build.Framework;
	using Microsoft.Build.Utilities;

	public class CheckAdminRights : Task {
		/// <summary>
		/// Gets or sets a value indicating whether this process has elevated permissions.
		/// </summary>
		[Output]
		public bool IsElevated { get; set; }

		/// <summary>
		/// Executes this instance.
		/// </summary>
		public override bool Execute() {
			WindowsIdentity id = WindowsIdentity.GetCurrent();
			WindowsPrincipal p = new WindowsPrincipal(id);
			this.IsElevated = p.IsInRole(WindowsBuiltInRole.Administrator);

			return true;
		}
	}
}
