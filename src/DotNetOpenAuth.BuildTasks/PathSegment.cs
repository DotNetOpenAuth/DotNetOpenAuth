//-----------------------------------------------------------------------
// <copyright file="PathSegment.cs" company="Andrew Arnott">
//     Copyright (c) Andrew Arnott. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace DotNetOpenAuth.BuildTasks {
	using System;
	using System.Collections.Generic;
	using System.Collections.ObjectModel;
	using System.Diagnostics;
	using System.Diagnostics.Contracts;
	using System.IO;
	using System.Linq;
	using System.Text;

	internal class PathSegment {
		private const float ParentChildResizeThreshold = 0.30f;
		private readonly PathSegment parent;
		private readonly string originalName;
		private string currentName;

		internal PathSegment() {
			this.currentName = string.Empty;
			this.originalName = string.Empty;
			this.Children = new Collection<PathSegment>();
		}

		private PathSegment(string originalName, PathSegment parent)
			: this() {
			Contract.Requires<ArgumentException>(!string.IsNullOrEmpty(originalName));
			Contract.Requires<ArgumentNullException>(parent != null);
			this.currentName = this.originalName = originalName;
			this.parent = parent;
		}

		internal string OriginalPath {
			get {
				if (this.parent != null) {
					return Path.Combine(this.parent.OriginalPath, this.originalName);
				} else {
					return this.originalName;
				}
			}
		}

		internal string CurrentPath {
			get {
				if (this.parent != null) {
					return Path.Combine(this.parent.CurrentPath, this.currentName);
				} else {
					return this.currentName;
				}
			}
		}

		internal string CurrentName {
			get { return this.currentName; }
		}

		internal string OriginalName {
			get { return this.originalName; }
		}

		private int SegmentCount {
			get {
				int parents = this.parent != null ? this.parent.SegmentCount : 0;
				return parents + 1;
			}
		}

		internal int FullLength {
			get {
				if (this.parent != null) {
					return this.parent.FullLength + 1/*slash*/ + this.currentName.Length;
				} else {
					return this.currentName.Length;
				}
			}
		}

		internal bool NameChanged {
			get { return !string.Equals(this.currentName, this.originalName, StringComparison.OrdinalIgnoreCase); }
		}

		internal bool IsLeaf {
			get { return this.Children.Count == 0; }
		}

		internal IEnumerable<PathSegment> Descendents {
			get {
				IEnumerable<PathSegment> result = this.Children;
				foreach (PathSegment child in this.Children) {
					result = result.Concat(child.Descendents);
				}

				return result;
			}
		}

		internal IEnumerable<PathSegment> SelfAndDescendents {
			get {
				yield return this;
				foreach (var child in this.Descendents) {
					yield return child;
				}
			}
		}

		internal IEnumerable<PathSegment> LeafChildren {
			get { return this.Children.Where(child => child.IsLeaf); }
		}

		internal IEnumerable<PathSegment> LeafDescendents {
			get { return this.Descendents.Where(child => child.IsLeaf); }
		}

		internal IEnumerable<PathSegment> Siblings {
			get { return this.parent != null ? this.parent.Children : Enumerable.Empty<PathSegment>(); }
		}

		internal Collection<PathSegment> Children { get; private set; }

		public override string ToString() {
			string path;
			if (this.NameChanged) {
				path = "{" + this.originalName + " => " + this.currentName + "}";
			} else {
				path = this.currentName;
			}

			if (path.Length > 0 && !this.IsLeaf) {
				path += "\\";
			}

			if (this.parent != null) {
				path = parent.ToString() + path;
			}

			return path;
		}

		internal PathSegment Add(string originalPath) {
			Contract.Requires<ArgumentException>(!string.IsNullOrEmpty(originalPath));
			Contract.Ensures(Contract.Result<PathSegment>() != null);
			string[] segments = originalPath.Split(Path.DirectorySeparatorChar);
			return this.Add(segments, 0);
		}

		internal void Add(IEnumerable<string> originalPaths) {
			foreach (string path in originalPaths) {
				this.Add(path);
			}
		}

		internal int EnsureSelfAndChildrenNoLongerThan(int maxLength) {
			Contract.Requires<ArgumentOutOfRangeException>(maxLength > 0, "A path can only have a positive length.");
			Contract.Requires<ArgumentOutOfRangeException>(this.parent == null || maxLength > this.parent.FullLength + 1, "A child path cannot possibly be made shorter than its parent.");
			Contract.Ensures(Contract.Result<int>() <= maxLength);

			// First check whether this segment itself is too long.
			if (this.FullLength > maxLength) {
				int tooLongBy = this.FullLength - maxLength;
				this.currentName = CreateUniqueShortFileName(this.originalName, this.currentName.Length - tooLongBy);
			}

			// Now check whether children are too long.
			if (this.Children.Count > 0) {
				var longChildren = this.Children.Where(path => path.FullLength > maxLength).ToList();
				if (longChildren.Any()) {
					// If this segment's name is longer than the longest child that is too long, we need to 
					// shorten THIS segment's name.
					if (longChildren.Max(child => child.CurrentName.Length) < this.CurrentName.Length) {
						int tooLongBy = this.FullLength - maxLength;
						this.currentName = CreateUniqueShortFileName(this.originalName, this.currentName.Length - tooLongBy);
					} else {
						// The children need to be shortened.
						longChildren.ForEach(child => child.EnsureSelfAndChildrenNoLongerThan(maxLength));
					}
				}
			}

			// Give each child a chance to check on their own children.
			foreach (var child in this.Children) {
				child.EnsureSelfAndChildrenNoLongerThan(maxLength);
			}

			// Return the total length of self or longest child.
			return this.SelfAndDescendents.Max(c => c.FullLength);
		}

		internal PathSegment FindByOriginalPath(string originalPath) {
			Contract.Requires<ArgumentException>(!string.IsNullOrEmpty(originalPath));
			string[] segments = originalPath.Split(Path.DirectorySeparatorChar);
			return this.FindByOriginalPath(segments, 0);
		}

		private string GetUniqueShortName(string preferredPrefix, string preferredSuffix, int allowableLength) {
			Contract.Requires<ArgumentNullException>(preferredPrefix != null);
			Contract.Requires<ArgumentNullException>(preferredSuffix != null);
			Contract.Requires<ArgumentException>(allowableLength > 0);
			Contract.Ensures(!string.IsNullOrEmpty(Contract.Result<string>()));
			Contract.Ensures(Contract.Result<string>().Length <= allowableLength);
			string candidateName = string.Empty;
			int i;
			for (i = -1; i < 0 || this.Siblings.Any(child => string.Equals(child.CurrentName, candidateName, StringComparison.OrdinalIgnoreCase)); i++) {
				string unique = i < 0 ? string.Empty : i.ToString("x");
				if (allowableLength < unique.Length) {
					throw new InvalidOperationException("Unable to shorten path sufficiently to fit constraints.");
				}

				candidateName = unique;

				// Suffix gets higher priority than the prefix, but only if the entire suffix can be appended.
				if (candidateName.Length + preferredSuffix.Length <= allowableLength) {
					candidateName += preferredSuffix;
				}

				// Now prepend as much of the prefix as fits.
				candidateName = preferredPrefix.Substring(0, Math.Min(allowableLength - candidateName.Length, preferredPrefix.Length)) + candidateName;
			}

			return candidateName;
		}

		private string CreateUniqueShortFileName(string fileName, int targetLength) {
			Contract.Requires<ArgumentException>(!string.IsNullOrEmpty(fileName));
			Contract.Ensures(!string.IsNullOrEmpty(Contract.Result<string>()));
			Contract.Ensures(Contract.Result<string>().Length <= targetLength);

			// The filename may already full within the target length.
			if (fileName.Length <= targetLength) {
				return fileName;
			}

			string preferredPrefix = Path.GetFileNameWithoutExtension(fileName);
			string preferredSuffix = Path.GetExtension(fileName);

			string shortenedFileName = GetUniqueShortName(preferredPrefix, preferredSuffix, targetLength);
			return shortenedFileName;
		}

		private void ShortenThis(int targetLength) {
			this.currentName = CreateUniqueShortFileName(this.originalName, targetLength);
		}

		private PathSegment Add(string[] segments, int segmentIndex) {
			Contract.Requires<ArgumentNullException>(segments != null);
			Contract.Requires<ArgumentOutOfRangeException>(segmentIndex < segments.Length);
			Contract.Ensures(Contract.Result<PathSegment>() != null);
			var match = this.Children.SingleOrDefault(child => String.Equals(child.originalName, segments[segmentIndex]));
			if (match == null) {
				match = new PathSegment(segments[segmentIndex], this);
				this.Children.Add(match);
				if (segments.Length == segmentIndex + 1) {
					return match;
				}
			}

			return match.Add(segments, segmentIndex + 1);
		}

		private PathSegment FindByOriginalPath(string[] segments, int segmentIndex) {
			Contract.Requires<ArgumentNullException>(segments != null);
			Contract.Requires<ArgumentOutOfRangeException>(segmentIndex < segments.Length);
			if (string.Equals(this.originalName, segments[segmentIndex], StringComparison.OrdinalIgnoreCase)) {
				if (segmentIndex == segments.Length - 1) {
					return this;
				}

				foreach (var child in this.Children) {
					var match = child.FindByOriginalPath(segments, segmentIndex + 1);
					if (match != null) {
						return match;
					}
				}
			}

			return null;
		}
	}
}
