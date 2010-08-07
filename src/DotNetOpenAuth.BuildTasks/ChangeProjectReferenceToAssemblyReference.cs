using System;
using System.Linq;
using System.IO;
using System.Xml;

using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;
using Microsoft.Build.BuildEngine;

namespace DotNetOpenAuth.BuildTasks {
	/// <summary>
	/// Replaces ProjectReference items in a set of projects with Reference items.
	/// </summary>
	public class ChangeProjectReferenceToAssemblyReference : Task {
		/// <summary>
		/// The projects to alter.
		/// </summary>
		[Required]
		public ITaskItem[] Projects { get; set; }
		/// <summary>
		/// The project reference to remove.
		/// </summary>
		[Required]
		public string ProjectReference { get; set; }
		/// <summary>
		/// The assembly reference to add.
		/// </summary>
		[Required]
		public string Reference { get; set; }

		const string msbuildNamespace = "http://schemas.microsoft.com/developer/msbuild/2003";
		public override bool Execute() {
			foreach (var project in Projects) {
				Log.LogMessage(MessageImportance.Normal, "Changing P2P references to assembly references in \"{0}\".", project.ItemSpec);

				Project doc = new Project();
				doc.Load(project.ItemSpec, ProjectLoadSettings.IgnoreMissingImports);
				
				var projectReference = doc.EvaluatedItems.OfType<BuildItem>().Where(
					item => item.Name == "ProjectReference" && item.Include == ProjectReference).Single();
				doc.RemoveItem(projectReference);

				var newReference = doc.AddNewItem("Reference", Path.GetFileNameWithoutExtension(Reference), true);
				newReference.SetMetadata("HintPath", Reference);

				doc.Save(project.ItemSpec);
			}

			return true;
		}
	}
}
