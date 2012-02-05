//-----------------------------------------------------------------------
// <copyright file="DeleteWebApplication.cs" company="Outercurve Foundation">
//     Copyright (c) Outercurve Foundation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace DotNetOpenAuth.BuildTasks {
	using System;
	using System.Linq;
	using Microsoft.Build.Framework;
	using Microsoft.Build.Utilities;
	using Microsoft.Web.Administration;

	/// <summary>
	/// Deletes a web application from IIS.
	/// </summary>
	public class DeleteWebApplication : Task {
		/// <summary>
		/// Gets or sets the name of the web site under which to create the web application.
		/// </summary>
		/// <value>The name of the existing web site.</value>
		[Required]
		public string WebSiteName { get; set; }

		/// <summary>
		/// Gets or sets the virtual paths within the web site that will access these applications.
		/// </summary>
		/// <value>The virtual path, which must start with '/'.</value>
		[Required]
		public ITaskItem[] VirtualPaths { get; set; }

		/// <summary>
		/// Executes this instance.
		/// </summary>
		/// <returns>A value indicating whether the task completed successfully.</returns>
		public override bool Execute() {
			var serverManager = new ServerManager();

			// Find the root web site that this web application will be created under.
			var site = serverManager.Sites.FirstOrDefault(s => string.Equals(s.Name, this.WebSiteName, StringComparison.OrdinalIgnoreCase));
			if (site == null) {
				Log.LogMessage(MessageImportance.Low, TaskStrings.NoMatchingWebSiteFound, this.WebSiteName);
				return true;
			}

			if (this.VirtualPaths.Length == 0) {
				// Nothing to do.
				return true;
			}

			foreach (ITaskItem path in this.VirtualPaths) {
				var app = site.Applications.FirstOrDefault(a => string.Equals(a.Path, path.ItemSpec, StringComparison.OrdinalIgnoreCase));
				if (app != null) {
					site.Applications.Remove(app);
					Log.LogMessage(MessageImportance.Normal, TaskStrings.DeletedWebApplication, app.Path);
				} else {
					Log.LogMessage(MessageImportance.Low, TaskStrings.WebApplicationNotFoundSoNotDeleted, path.ItemSpec);
				}
			}

			serverManager.CommitChanges();

			return true;
		}
	}
}
