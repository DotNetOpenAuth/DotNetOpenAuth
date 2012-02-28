//-----------------------------------------------------------------------
// <copyright file="PathSegment.cs" company="Outercurve Foundation">
//     Copyright (c) Outercurve Foundation. All rights reserved.
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
		private readonly string originalName;
		private string currentName;
		private bool minimized;
		private static readonly string[] ReservedFileNames = "CON PRN AUX CLOCK$ NUL COM0 COM1 COM2 COM3 COM4 COM5 COM6 COM7 COM8 COM9 LPT0 LPT1 LPT2 LPT3 LPT4 LPT5 LPT6 LPT7 LPT8 LPT9".Split(' ');

		internal PathSegment() {
			this.currentName = string.Empty;
			this.originalName = string.Empty;
			this.minimized = true;
			this.Children = new Collection<PathSegment>();
		}

		private PathSegment(string originalName, PathSegment parent)
			: this() {
			Contract.Requires<ArgumentException>(!string.IsNullOrEmpty(originalName));
			Contract.Requires<ArgumentNullException>(parent != null);
			this.currentName = this.originalName = originalName;
			this.Parent = parent;
			this.minimized = false;
		}

		internal PathSegment Parent { get; private set; }

		internal string OriginalPath {
			get {
				if (this.Parent != null) {
					return Path.Combine(this.Parent.OriginalPath, this.originalName);
				} else {
					return this.originalName;
				}
			}
		}

		internal string CurrentPath {
			get {
				if (this.Parent != null) {
					return Path.Combine(this.Parent.CurrentPath, this.currentName);
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
				int parents = this.Parent != null ? this.Parent.SegmentCount : 0;
				return parents + 1;
			}
		}

		internal int FullLength {
			get {
				if (this.Parent != null) {
					int parentLength = this.Parent.FullLength;
					if (parentLength > 0) {
						parentLength++; // allow for an in between slash
					}
					return parentLength + this.currentName.Length;
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

		internal IEnumerable<PathSegment> Ancestors {
			get {
				PathSegment parent = this.Parent;
				while (parent != null) {
					yield return parent;
					parent = parent.Parent;
				}
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

		internal IEnumerable<PathSegment> SelfAndAncestors {
			get {
				yield return this;
				foreach (var parent in this.Ancestors) {
					yield return parent;
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
			get { return this.Parent != null ? this.Parent.Children : Enumerable.Empty<PathSegment>(); }
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

			if (this.Parent != null) {
				path = Parent.ToString() + path;
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
			Contract.Requires<ArgumentOutOfRangeException>(this.Parent == null || maxLength > this.Parent.FullLength + 1, "A child path cannot possibly be made shorter than its parent.");
			Contract.Ensures(Contract.Result<int>() <= maxLength);
			const int uniqueBase = 16;

			// Find the items that are too long, and always work on the longest one
			var longPaths = this.SelfAndDescendents.Where(path => path.FullLength > maxLength).OrderByDescending(path => path.FullLength);
			PathSegment longPath;
			while ((longPath = longPaths.FirstOrDefault()) != null) {
				// Keep working on this one until it's short enough.
				do {
					int tooLongBy = longPath.FullLength - maxLength;
					var longSegments = longPath.SelfAndAncestors.Where(segment => !segment.minimized).OrderByDescending(segment => segment.CurrentName.Length);
					PathSegment longestSegment = longSegments.FirstOrDefault();
					if (longestSegment == null) {
						throw new InvalidOperationException("Unable to shrink path length sufficiently.");
					}
					PathSegment secondLongestSegment = longSegments.Skip(1).FirstOrDefault();
					int shortenByUpTo;
					if (secondLongestSegment != null) {
						shortenByUpTo = Math.Min(tooLongBy, Math.Max(1, longestSegment.CurrentName.Length - secondLongestSegment.CurrentName.Length));
					} else {
						shortenByUpTo = tooLongBy;
					}
					int minimumGuaranteedUniqueLength = Math.Max(1, RoundUp(Math.Log(longestSegment.Siblings.Count(), uniqueBase)));
					int allowableSegmentLength = Math.Max(minimumGuaranteedUniqueLength, longestSegment.CurrentName.Length - shortenByUpTo);
					if (allowableSegmentLength >= longestSegment.CurrentName.Length) {
						// We can't make this segment any shorter.
						longestSegment.minimized = true;
					}
					longestSegment.currentName = longestSegment.CreateUniqueShortFileName(longestSegment.CurrentName, allowableSegmentLength);
				} while (longPath.FullLength > maxLength);
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
			for (i = -1; candidateName.Length == 0 || ReservedFileNames.Contains(candidateName, StringComparer.OrdinalIgnoreCase) || this.Siblings.Any(child => string.Equals(child.CurrentName, candidateName, StringComparison.OrdinalIgnoreCase)); i++) {
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

		private static int RoundUp(double value) {
			int roundedValue = (int)value;
			if (roundedValue < value) {
				roundedValue++;
			}

			return roundedValue;
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
			}

			return segments.Length == segmentIndex + 1 ? match : match.Add(segments, segmentIndex + 1);
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
