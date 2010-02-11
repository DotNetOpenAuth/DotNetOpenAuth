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
		public ITaskItem[] Templates { get; set; }

		/// <summary>
		/// Gets or sets the path to the .vsixmanifest file that will be used to assist
		/// in calculating the actual full path of project items.
		/// </summary>
		/// <remarks>
		/// This property is required if <see cref="EnsureMaxPath"/> &gt; 0;
		/// </remarks>
		public ITaskItem VsixManifest { get; set; }

		/// <summary>
		/// Gets or sets the maximum length a project item's relative path should
		/// be limited to, artificially renaming them as necessary.
		/// </summary>
		/// <value>Use 0 to disable the renaming feature.</value>
		public int EnsureMaxPath { get; set; }

		/// <summary>
		/// Gets or sets the project items that had to be renamed to comply with maximum path length requirements.
		/// </summary>
		/// <remarks>
		/// The item name is the original full path.  The ShortPath metadata contains the shortened full path.
		/// </remarks>
		[Output]
		public ITaskItem[] MaxPathAdjustedPaths { get; set; }

		/// <summary>
		/// Executes this instance.
		/// </summary>
		public override bool Execute() {
			var shortenedItems = new List<ITaskItem>();
			int uniqueItemCounter = 0;

			foreach (ITaskItem sourceTemplateTaskItem in this.Templates) {
				var template = XElement.Load(sourceTemplateTaskItem.ItemSpec);
				var templateContentElement = template.Element(XName.Get("TemplateContent", VSTemplateNamespace));
				var projectElement = templateContentElement.Element(XName.Get("Project", VSTemplateNamespace));
				if (projectElement == null) {
					Log.LogMessage("Skipping merge of \"{0}\" with a project because no project was referenced from the template.", sourceTemplateTaskItem.ItemSpec);
					continue;
				}

				var projectPath = Path.Combine(Path.GetDirectoryName(sourceTemplateTaskItem.ItemSpec), projectElement.Attribute("File").Value);
				var projectDirectory = Path.GetDirectoryName(Path.Combine(Path.GetDirectoryName(sourceTemplateTaskItem.GetMetadata("FullPath")), projectElement.Attribute("File").Value));
				Log.LogMessage("Merging project \"{0}\" with \"{1}\".", projectPath, sourceTemplateTaskItem.ItemSpec);
				var sourceProject = new Project();
				sourceProject.Load(projectPath);

				// Collect the project items from the project that are appropriate
				// to include in the .vstemplate file.
				var itemsByFolder = from item in sourceProject.EvaluatedItems.Cast<BuildItem>()
									where this.ProjectItemTypes.Contains(item.Name)
									orderby item.Include
									group item by Path.GetDirectoryName(item.Include);
				foreach (var folder in itemsByFolder) {
					XElement parentNode = projectElement;
					if (folder.Key.Length > 0) {
						parentNode = FindOrCreateParent((uniqueItemCounter++).ToString("x"), projectElement);
						parentNode.SetAttributeValue("TargetFolderName", folder.Key);
					}

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

						if (this.EnsureMaxPath > 0) {
							string estimatedFullPath = EstimateFullPathInProjectCache(Path.GetFileNameWithoutExtension(sourceProject.FullFileName), item.Include);
							if (estimatedFullPath.Length > this.EnsureMaxPath) {
								string leafName = Path.GetFileName(item.Include);
								int targetLeafLength = leafName.Length - (estimatedFullPath.Length - this.EnsureMaxPath);
								string shortenedFileName = CreateUniqueShortFileName(leafName, targetLeafLength);
								string shortenedRelativePath = Path.Combine(Path.GetDirectoryName(item.Include), shortenedFileName);
								string shortenedEstimatedFullPath = EstimateFullPathInProjectCache(Path.GetFileNameWithoutExtension(sourceProject.FullFileName), shortenedRelativePath);
								if (shortenedEstimatedFullPath.Length <= this.EnsureMaxPath) {
									this.Log.LogMessage(
										"Renaming long project item '{0}' to '{1}' within project template to avoid MAX_PATH issues.  The instantiated project will remain unchanged.",
										item.Include,
										shortenedRelativePath);
									projectItem.SetAttributeValue("TargetFileName", Path.GetFileName(item.Include));
									projectItem.Value = shortenedFileName;
									string originalFullPath = Path.Combine(projectDirectory, item.Include);
									string shortenedFullPath = Path.Combine(projectDirectory, shortenedRelativePath);
									if (File.Exists(shortenedFullPath)) {
										File.Delete(shortenedFullPath); // File.Move can't overwrite files
									}
									File.Move(originalFullPath, shortenedFullPath);

									// Document the change so the build system can account for it.
									TaskItem shortChange = new TaskItem(originalFullPath);
									shortChange.SetMetadata("ShortPath", shortenedFullPath);
									shortenedItems.Add(shortChange);
								} else {
									this.Log.LogError(
										"Project item '{0}' exceeds maximum allowable length {1} by {2} characters and it cannot be sufficiently shortened.  Estimated full path is: '{3}'.",
										item.Include,
										this.EnsureMaxPath,
										estimatedFullPath.Length - this.EnsureMaxPath,
										estimatedFullPath);
								}
							}
						}
					}
				}

				template.Save(sourceTemplateTaskItem.ItemSpec);
			}

			this.MaxPathAdjustedPaths = shortenedItems.ToArray();
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

		private string EstimateFullPathInProjectCache(string projectName, string itemRelativePath) {
			Contract.Requires<ArgumentException>(!string.IsNullOrEmpty(projectName));
			Contract.Requires<ArgumentException>(!string.IsNullOrEmpty(itemRelativePath));

			const string PathRoot = @"c:\documents and settings\usernameZZZ\AppData\Local\Microsoft\VisualStudio\10.0\Extensions\";
			string subPath;
			if (!vsixContributionToPath.TryGetValue(projectName, out subPath)) {
				if (this.VsixManifest == null) {
					this.Log.LogError("The task parameter VsixManifests is required but missing.");
				}
				var vsixDocument = XDocument.Load(this.VsixManifest.ItemSpec);
				XElement vsix = vsixDocument.Element(XName.Get("Vsix", VsixNamespace));
				XElement identifier = vsix.Element(XName.Get("Identifier", VsixNamespace));
				XElement content = vsix.Element(XName.Get("Content", VsixNamespace));
				string author = identifier.Element(XName.Get("Author", VsixNamespace)).Value;
				string name = identifier.Element(XName.Get("Name", VsixNamespace)).Value;
				string version = identifier.Element(XName.Get("Version", VsixNamespace)).Value;
				string pt = content.Element(XName.Get("ProjectTemplate", VsixNamespace)).Value;
				vsixContributionToPath[projectName] = subPath = string.Format(
					CultureInfo.InvariantCulture,
					@"{0}\{1}\{2}\~PC\{3}\CSharp\Web\{4}.zip\",
					author,
					name,
					version,
					pt,
					projectName
					);
			}
			return Path.Combine(PathRoot + subPath + Path.GetFileNameWithoutExtension(projectName), itemRelativePath);
		}
	}
}
