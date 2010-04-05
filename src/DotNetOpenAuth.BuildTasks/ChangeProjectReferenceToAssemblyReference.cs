namespace DotNetOpenAuth.BuildTasks {
	using System;
	using System.IO;
	using System.Linq;
	using System.Xml;
	using Microsoft.Build.BuildEngine;
	using Microsoft.Build.Framework;
	using Microsoft.Build.Utilities;

	/// <summary>
	/// Replaces ProjectReference items in a set of projects with Reference items.
	/// </summary>
	public class ChangeProjectReferenceToAssemblyReference : Task {
		private const string msbuildNamespace = "http://schemas.microsoft.com/developer/msbuild/2003";

		/// <summary>
		/// The projects to alter.
		/// </summary>
		[Required]
		public ITaskItem[] Projects { get; set; }

		/// <summary>
		/// The project references to remove.
		/// </summary>
		[Required]
		public ITaskItem[] ProjectReferences { get; set; }

		/// <summary>
		/// The assembly references to add.
		/// </summary>
		[Required]
		public ITaskItem[] References { get; set; }

		public override bool Execute() {
			if (this.ProjectReferences.Length != this.References.Length) {
				this.Log.LogError("ProjectReferences and References arrays do not have matching lengths.");
			}

			foreach (var project in Projects) {
				Project doc = new Project();
				doc.Load(project.ItemSpec);

				var projectReferences = doc.EvaluatedItems.OfType<BuildItem>().Where(item => item.Name == "ProjectReference");
				var matchingReferences = from reference in projectReferences
										 join refToRemove in this.ProjectReferences on reference.Include equals refToRemove.ItemSpec
										 let addIndex = Array.IndexOf(this.ProjectReferences, refToRemove)
										 select new { Remove = reference, Add = this.References[addIndex] };
				foreach (var matchingReference in matchingReferences) {
					this.Log.LogMessage("Removing project reference to \"{0}\" from \"{1}\".", matchingReference.Remove.Include, project.ItemSpec);
					doc.RemoveItem(matchingReference.Remove);
					if (matchingReference.Add.ItemSpec != "REMOVE") {
						this.Log.LogMessage("Adding assembly reference to \"{0}\" to \"{1}\".", matchingReference.Add.ItemSpec, project.ItemSpec);
						var newReference = doc.AddNewItem("Reference", Path.GetFileNameWithoutExtension(matchingReference.Add.ItemSpec), true);
						newReference.SetMetadata("HintPath", matchingReference.Add.ItemSpec);
					}
				}

				doc.Save(project.ItemSpec);
			}

			return true;
		}
	}
}
