//-----------------------------------------------------------------------
// <copyright file="MergeProjectWithVSTemplate.cs" company="Andrew Arnott">
//     Copyright (c) Andrew Arnott. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace DotNetOpenAuth.BuildTasks {
	using System;
	using System.Collections.Generic;
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
				var sourceToDestinationProjectItemMap = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
				//var oversizedItemPaths = projectItems.Where(item => item.Include.Length > this.MaximumRelativePathLength);
				foreach (var item in projectItems) {
					var source = item.Include;
					var dest = item.Include;

						//        if (this.MaximumRelativePathLength > 0) {
						//    if (item.Include.Length > this.MaximumRelativePathLength) {
						//        string leafName = Path.GetFileName(item.Include);
						//        int targetLeafLength = leafName.Length - (item.Include.Length - this.MaximumRelativePathLength);
						//        string shortenedFileName = CreateUniqueShortFileName(leafName, targetLeafLength);
						//        string shortenedRelativePath = Path.Combine(Path.GetDirectoryName(item.Include), shortenedFileName);
						//        if (shortenedRelativePath.Length <= this.MaximumRelativePathLength) {
						//            this.Log.LogMessage(
						//                "Renaming long project item '{0}' to '{1}' within project template to avoid MAX_PATH issues.  The instantiated project will remain unchanged.",
						//                item.Include,
						//                shortenedRelativePath);
						//            projectItem.SetAttributeValue("TargetFileName", Path.GetFileName(item.Include));
						//            projectItem.Value = shortenedFileName;
						//            string originalFullPath = Path.Combine(projectDirectory, item.Include);
						//            string shortenedFullPath = Path.Combine(projectDirectory, shortenedRelativePath);
						//            if (File.Exists(shortenedFullPath)) {
						//                File.Delete(shortenedFullPath); // File.Move can't overwrite files
						//            }
						//            File.Move(originalFullPath, shortenedFullPath);

						//            // Document the change so the build system can account for it.
						//            TaskItem shortChange = new TaskItem(originalFullPath);
						//            shortChange.SetMetadata("ShortPath", shortenedFullPath);
						//            shortenedItems.Add(shortChange);
						//        } else {
						//            this.Log.LogError(
						//                "Project item '{0}' exceeds maximum allowable length {1} by {2} characters and it cannot be sufficiently shortened.  Estimated full path is: '{3}'.",
						//                item.Include,
						//                this.MaximumRelativePathLength,
						//                item.Include.Length - this.MaximumRelativePathLength,
						//                item.Include);
						//        }
						//    }
						//}

					sourceToDestinationProjectItemMap[source] = dest;
				}

				// Collect the project items from the project that are appropriate
				// to include in the .vstemplate file.
				var itemsByFolder = from item in projectItems
									orderby item.Include
									group item by Path.GetDirectoryName(item.Include);

				foreach (var folder in itemsByFolder) {
					XElement parentNode = projectElement;
					parentNode = FindOrCreateParent(folder.Key, projectElement);
					//parentNode.SetAttributeValue("TargetFolderName", folder.Key);

					foreach (var item in folder) {
						bool replaceParameters = this.ReplaceParametersExtensions.Contains(Path.GetExtension(item.Include));
						var itemName = XName.Get("ProjectItem", VSTemplateNamespace);
						var projectItem = parentNode.Elements(itemName).FirstOrDefault(el => string.Equals(el.Value, Path.GetFileName(item.Include), StringComparison.OrdinalIgnoreCase));
						if (projectItem == null) {
							projectItem = new XElement(itemName, Path.GetFileName(item.Include));
							parentNode.Add(projectItem);
						}
						if (replaceParameters) {
							projectItem.SetAttributeValue("ReplaceParameters", "true");
						}
					}
				}

				template.Save(this.DestinationTemplates[iTemplate].ItemSpec);
				foreach (var pair in sourceToDestinationProjectItemMap) {
					TaskItem item = new TaskItem(Path.Combine(Path.GetDirectoryName(this.SourceTemplates[iTemplate].ItemSpec), pair.Key));
					item.SetMetadata("SourceFullPath", Path.GetFullPath(Path.Combine(Path.GetDirectoryName(this.SourceTemplates[iTemplate].ItemSpec), pair.Key)));
					item.SetMetadata("DestinationFullPath", Path.GetFullPath(Path.Combine(Path.GetDirectoryName(this.DestinationTemplates[iTemplate].ItemSpec), pair.Value)));
					item.SetMetadata("RecursiveDir", Path.GetDirectoryName(this.SourceTemplates[iTemplate].ItemSpec));
					projectItemsToCopy.Add(item);
				}
			}

			this.ProjectItems = projectItemsToCopy.ToArray();

			return !Log.HasLoggedErrors;
		}

		private static string CreateUniqueShortFileName(string fileName, int targetLength) {
			Contract.Requires<ArgumentException>(!string.IsNullOrEmpty(fileName));
			Contract.Ensures(!string.IsNullOrEmpty(Contract.Result<string>()));

			// The filename may already full within the target length.
			if (fileName.Length <= targetLength) {
				return fileName;
			}

			string hashSuffix = Utilities.SuppressCharacters(Math.Abs(fileName.GetHashCode() % 0xfff).ToString("x"), Path.GetInvalidFileNameChars(), '_');
			string fileNameWithoutExtension = Path.GetFileNameWithoutExtension(fileName);
			string extension = Path.GetExtension(fileName);
			string hashSuffixWithExtension = hashSuffix + extension;

			// If the target length is itself shorter than the hash code, then we won't meet our goal,
			// but at least put the hash in there so it's unique, and we'll return a string that's too long.
			string shortenedFileName = fileName.Substring(0, Math.Max(0, targetLength - hashSuffixWithExtension.Length)) + hashSuffixWithExtension;

			return shortenedFileName;
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
