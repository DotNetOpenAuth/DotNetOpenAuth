//-----------------------------------------------------------------------
// <copyright file="FixupReferenceHintPaths.cs" company="Outercurve Foundation">
//     Copyright (c) Outercurve Foundation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace DotNetOpenAuth.BuildTasks {
	using System;
	using System.Collections.Generic;
	using System.IO;
	using System.Linq;
	using System.Reflection;
	using System.Text;
	using Microsoft.Build.BuildEngine;
	using Microsoft.Build.Framework;
	using Microsoft.Build.Utilities;

	public class FixupReferenceHintPaths : Task {
		/// <summary>
		/// Gets or sets the projects to fixup references for.
		/// </summary>
		[Required]
		public ITaskItem[] Projects { get; set; }

		/// <summary>
		/// Gets or sets the set of full paths to assemblies that may be found in any of the <see cref="Projects"/>.
		/// </summary>
		[Required]
		public ITaskItem[] References { get; set; }

		/// <summary>
		/// Executes this instance.
		/// </summary>
		public override bool Execute() {
			if (this.References.Length == 0 || this.Projects.Length == 0) {
				this.Log.LogMessage(MessageImportance.Low, "Skipping reference hintpath fixup because no projects or no references were supplied.");
				return !this.Log.HasLoggedErrors;
			}

			// Figure out what the assembly names are of the references that are available.
			AssemblyName[] availableReferences = new AssemblyName[this.References.Length];
			for (int i = 0; i < this.References.Length; i++) {
				if (File.Exists(this.References[i].ItemSpec)) {
					availableReferences[i] = AssemblyName.GetAssemblyName(this.References[i].ItemSpec);
				} else {
					availableReferences[i] = new AssemblyName(Path.GetFileNameWithoutExtension(this.References[i].ItemSpec)) {
						CodeBase = this.References[i].GetMetadata("FullPath"),
					};
				}
			}

			foreach (var projectTaskItem in this.Projects) {
				var project = new Project();
				Uri projectUri = new Uri(projectTaskItem.GetMetadata("FullPath"));
				project.Load(projectTaskItem.ItemSpec);

				foreach (BuildItem referenceItem in project.GetEvaluatedItemsByName("Reference")) {
					var referenceAssemblyName = new AssemblyName(referenceItem.Include);
					var matchingReference = availableReferences.FirstOrDefault(r => string.Equals(r.Name, referenceAssemblyName.Name, StringComparison.OrdinalIgnoreCase));
					if (matchingReference != null) {
						var originalSuppliedReferenceItem = this.References[Array.IndexOf(availableReferences, matchingReference)];
						string hintPath = originalSuppliedReferenceItem.GetMetadata("HintPath");
						if (string.IsNullOrEmpty(hintPath)) {
							hintPath = projectUri.MakeRelativeUri(new Uri(matchingReference.CodeBase)).OriginalString.Replace(Path.AltDirectorySeparatorChar, Path.DirectorySeparatorChar);
						}
						this.Log.LogMessage("Fixing up HintPath to \"{0}\" in project \"{1}\".", referenceAssemblyName.Name, projectTaskItem.ItemSpec);
						referenceItem.SetMetadata("HintPath", hintPath);
					}
				}

				project.Save(projectTaskItem.ItemSpec);
			}

			return !this.Log.HasLoggedErrors;
		}
	}
}
