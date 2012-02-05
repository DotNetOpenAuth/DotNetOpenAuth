//-----------------------------------------------------------------------
// <copyright file="CreateWebApplication.cs" company="Outercurve Foundation">
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
	/// Creates or updates web applications within an existing web site in IIS.
	/// </summary>
	public class CreateWebApplication : Task {
		/// <summary>
		/// Gets or sets the name of the application pool that should host the web application.
		/// </summary>
		/// <value>The name of an existing application pool.</value>
		public string ApplicationPoolName { get; set; }

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
		/// Gets or sets the full file system paths to the web applications.
		/// </summary>
		/// <value>The physical path.</value>
		[Required]
		public ITaskItem[] PhysicalPaths { get; set; }

		/// <summary>
		/// Executes this instance.
		/// </summary>
		/// <returns>A value indicating whether the task completed successfully.</returns>
		public override bool Execute() {
			var serverManager = new ServerManager();

			if (this.PhysicalPaths.Length != this.VirtualPaths.Length) {
				Log.LogError(TaskStrings.MismatchingArrayLengths, "PhysicalPath", "VirtualPath");
				return false;
			}

			if (this.VirtualPaths.Length == 0) {
				// Nothing to do.
				return true;
			}

			// Find the root web site that this web application will be created under.
			var site = serverManager.Sites.FirstOrDefault(s => string.Equals(s.Name, this.WebSiteName, StringComparison.OrdinalIgnoreCase));
			if (site == null) {
				Log.LogError(TaskStrings.NoMatchingWebSiteFound, this.WebSiteName);
				return false;
			}

			Log.LogMessage(MessageImportance.Normal, "Creating web applications under web site: {0}", site.Name);

			for (int i = 0; i < this.PhysicalPaths.Length; i++) {
				string physicalPath = this.PhysicalPaths[i].ItemSpec;
				string virtualPath = this.VirtualPaths[i].ItemSpec;

				Log.LogMessage(MessageImportance.Normal, "\t{0} -> {1}", virtualPath, physicalPath);

				var app = site.Applications.FirstOrDefault(a => string.Equals(a.Path, virtualPath, StringComparison.OrdinalIgnoreCase));
				if (app == null) {
					app = site.Applications.Add(virtualPath, physicalPath);
				} else {
					// Ensure physical path is set correctly.
					var appRoot = app.VirtualDirectories.First(vd => vd.Path == "/");
					appRoot.PhysicalPath = physicalPath;
				}

				if (!string.IsNullOrEmpty(this.ApplicationPoolName)) {
					app.ApplicationPoolName = this.ApplicationPoolName;
				}
			}

			serverManager.CommitChanges();
			return true;
		}
	}
}
