using System;
using System.Linq;
using System.IO;
using System.Xml;

using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;
using Microsoft.Build.BuildEngine;

namespace DotNetOpenAuth.BuildTasks {
	/// <summary>
	/// Replaces Reference items HintPaths in a set of projects.
	/// </summary>
	public class ChangeAssemblyReference : Task {
		/// <summary>
		/// The projects to alter.
		/// </summary>
		[Required]
		public ITaskItem[] Projects { get; set; }
		/// <summary>
		/// The project reference to remove.
		/// </summary>
		[Required]
		public string OldReference { get; set; }
		/// <summary>
		/// The assembly reference to add.
		/// </summary>
		[Required]
		public string NewReference { get; set; }

		const string msbuildNamespace = "http://schemas.microsoft.com/developer/msbuild/2003";
		public override bool Execute() {
			foreach (var project in Projects) {
				Project doc = new Project();
				doc.Load(project.ItemSpec);
				
				var reference = doc.GetEvaluatedItemsByName("Reference").OfType<BuildItem>().
					Where(item => string.Equals(item.GetMetadata("HintPath"), OldReference, StringComparison.OrdinalIgnoreCase)).Single();
				reference.SetMetadata("HintPath", NewReference);

				doc.Save(project.ItemSpec);
			}

			return true;
		}
	}
}
