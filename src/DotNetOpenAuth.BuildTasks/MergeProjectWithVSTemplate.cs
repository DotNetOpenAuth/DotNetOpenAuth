//-----------------------------------------------------------------------
// <copyright file="MergeProjectWithVSTemplate.cs" company="Outercurve Foundation">
//     Copyright (c) Outercurve Foundation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace DotNetOpenAuth.BuildTasks {
	using System;
	using System.Collections.Generic;
	using System.Collections.ObjectModel;
	using System.Diagnostics.Contracts;
	using System.Globalization;
	using System.IO;
	using System.Linq;
	using System.Text;
	using System.Xml.Linq;
	using Microsoft.Build.BuildEngine;
	using Microsoft.Build.Framework;
	using Microsoft.Build.Utilities;

	public class MergeProjectWithVSTemplate : Task {
		internal const string VSTemplateNamespace = "http://schemas.microsoft.com/developer/vstemplate/2005";

		internal const string VsixNamespace = "http://schemas.microsoft.com/developer/vsx-schema/2010";

		/// <summary>
		/// A dictionary where the key is the project name and the value is the path contribution.
		/// </summary>
		private Dictionary<string, string> vsixContributionToPath = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

		[Required]
		public string[] ProjectItemTypes { get; set; }

		[Required]
		public string[] ReplaceParametersExtensions { get; set; }

		[Required]
		public ITaskItem[] SourceTemplates { get; set; }

		[Required]
		public ITaskItem[] SourceProjects { get; set; }

		[Required]
		public ITaskItem[] DestinationTemplates { get; set; }

		public ITaskItem[] SourcePathExceptions { get; set; }

		/// <summary>
		/// Gets or sets the maximum length a project item's relative path should
		/// be limited to, artificially renaming them as necessary.
		/// </summary>
		/// <value>Use 0 to disable the renaming feature.</value>
		public int MaximumRelativePathLength { get; set; }

		/// <summary>
		/// Gets or sets the project item paths from the source project to copy to the destination location.
		/// </summary>
		[Output]
		public ITaskItem[] ProjectItems { get; set; }

		/// <summary>
		/// Executes this instance.
		/// </summary>
		public override bool Execute() {
			if (this.DestinationTemplates.Length != this.SourceTemplates.Length) {
				this.Log.LogError("SourceTemplates array has length {0} while DestinationTemplates array has length {1}, but must equal.", this.SourceTemplates.Length, this.DestinationTemplates.Length);
			}
			if (this.SourceProjects.Length != this.SourceTemplates.Length) {
				this.Log.LogError("SourceTemplates array has length {0} while SourceProjects array has length {1}, but must equal.", this.SourceTemplates.Length, this.SourceProjects.Length);
			}

			var projectItemsToCopy = new List<ITaskItem>();

			for (int iTemplate = 0; iTemplate < this.SourceTemplates.Length; iTemplate++) {
				ITaskItem sourceTemplateTaskItem = this.SourceTemplates[iTemplate];
				var template = XElement.Load(sourceTemplateTaskItem.ItemSpec);
				var templateContentElement = template.Element(XName.Get("TemplateContent", VSTemplateNamespace));
				var projectElement = templateContentElement.Element(XName.Get("Project", VSTemplateNamespace));
				if (projectElement == null) {
					Log.LogMessage("Skipping merge of \"{0}\" with a project because no project was referenced from the template.", sourceTemplateTaskItem.ItemSpec);
					continue;
				}

				var projectPath = this.SourceProjects[iTemplate].ItemSpec;
				var projectDirectory = Path.GetDirectoryName(Path.Combine(Path.GetDirectoryName(sourceTemplateTaskItem.GetMetadata("FullPath")), projectElement.Attribute("File").Value));
				Log.LogMessage("Merging project \"{0}\" with \"{1}\".", projectPath, sourceTemplateTaskItem.ItemSpec);
				var sourceProject = new Project();
				sourceProject.Load(projectPath);
				var projectItems = sourceProject.EvaluatedItems.Cast<BuildItem>().Where(item => this.ProjectItemTypes.Contains(item.Name));

				// Figure out where every project item is in source, and where it will go in the destination,
				// taking into account a given maximum path length that may require that we shorten the path.
				PathSegment root = new PathSegment();
				root.Add(projectItems.Select(item => item.Include));
				root.EnsureSelfAndChildrenNoLongerThan(this.MaximumRelativePathLength);

				// Collect the project items from the project that are appropriate
				// to include in the .vstemplate file.
				foreach (var folder in root.SelfAndDescendents.Where(path => !path.IsLeaf && path.LeafChildren.Any())) {
					XElement parentNode = projectElement;
					parentNode = FindOrCreateParent(folder.CurrentPath, projectElement);
					if (folder.NameChanged) {
						parentNode.SetAttributeValue("TargetFolderName", folder.OriginalName);
					}

					foreach (var item in folder.LeafChildren) {
						var itemName = XName.Get("ProjectItem", VSTemplateNamespace);
						// The project item MAY be hard-coded in the .vstemplate file, under the original name.
						var projectItem = parentNode.Elements(itemName).FirstOrDefault(el => string.Equals(el.Value, Path.GetFileName(item.OriginalName), StringComparison.OrdinalIgnoreCase));
						if (projectItem == null) {
							projectItem = new XElement(itemName, item.CurrentName);
							parentNode.Add(projectItem);
						}
						if (item.NameChanged) {
							projectItem.Value = item.CurrentName; // set Value in case it was a hard-coded item in the .vstemplate file.
							projectItem.SetAttributeValue("TargetFileName", item.OriginalName);
						}
						if (this.ReplaceParametersExtensions.Contains(Path.GetExtension(item.OriginalPath))) {
							projectItem.SetAttributeValue("ReplaceParameters", "true");
						}
					}
				}

				template.Save(this.DestinationTemplates[iTemplate].ItemSpec);
				foreach (var pair in root.LeafDescendents) {
					TaskItem item = new TaskItem(Path.Combine(Path.GetDirectoryName(this.SourceTemplates[iTemplate].ItemSpec), pair.OriginalPath));
					string apparentSource = Path.Combine(Path.GetDirectoryName(this.SourceTemplates[iTemplate].ItemSpec), pair.OriginalPath);
					var sourcePathException = this.SourcePathExceptions.FirstOrDefault(ex => string.Equals(ex.ItemSpec, apparentSource));
					if (sourcePathException != null) {
						item.SetMetadata("SourceFullPath", sourcePathException.GetMetadata("ActualSource"));
					} else {
						item.SetMetadata("SourceFullPath", Path.GetFullPath(apparentSource));
					}
					item.SetMetadata("DestinationFullPath", Path.GetFullPath(Path.Combine(Path.GetDirectoryName(this.DestinationTemplates[iTemplate].ItemSpec), pair.CurrentPath)));
					item.SetMetadata("RecursiveDir", Path.GetDirectoryName(this.SourceTemplates[iTemplate].ItemSpec));
					item.SetMetadata("Transform", this.ReplaceParametersExtensions.Contains(Path.GetExtension(pair.OriginalName)) ? "true" : "false");
					projectItemsToCopy.Add(item);
				}
			}

			this.ProjectItems = projectItemsToCopy.ToArray();

			return !Log.HasLoggedErrors;
		}

		private static XElement FindOrCreateParent(string directoryName, XElement projectElement) {
			Contract.Requires<ArgumentNullException>(projectElement != null);

			if (string.IsNullOrEmpty(directoryName)) {
				return projectElement;
			}

			string[] segments = directoryName.Split(Path.DirectorySeparatorChar);
			XElement parent = projectElement;
			for (int i = 0; i < segments.Length; i++) {
				var candidateName = XName.Get("Folder", VSTemplateNamespace);
				var candidate = parent.Elements(XName.Get("Folder", VSTemplateNamespace)).FirstOrDefault(n => n.Attribute("Name").Value == segments[i]);
				if (candidate == null) {
					candidate = new XElement(
						candidateName,
						new XAttribute("Name", segments[i]));
					parent.Add(candidate);
				}

				parent = candidate;
			}

			return parent;
		}
	}
}
