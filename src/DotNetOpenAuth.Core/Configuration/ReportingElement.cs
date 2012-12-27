//-----------------------------------------------------------------------
// <copyright file="ReportingElement.cs" company="Outercurve Foundation">
//     Copyright (c) Outercurve Foundation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace DotNetOpenAuth.Configuration {
	using System;
	using System.Collections.Generic;
	using System.Configuration;
	using System.Linq;
	using System.Text;

	/// <summary>
	/// Represents the &lt;reporting&gt; element in the host's .config file.
	/// </summary>
	internal class ReportingElement : ConfigurationSection {
		/// <summary>
		/// The name of the @enabled attribute.
		/// </summary>
		private const string EnabledAttributeName = "enabled";

		/// <summary>
		/// The name of the @minimumReportingInterval attribute.
		/// </summary>
		private const string MinimumReportingIntervalAttributeName = "minimumReportingInterval";

		/// <summary>
		/// The name of the @minimumFlushInterval attribute.
		/// </summary>
		private const string MinimumFlushIntervalAttributeName = "minimumFlushInterval";

		/// <summary>
		/// The name of the @includeFeatureUsage attribute.
		/// </summary>
		private const string IncludeFeatureUsageAttributeName = "includeFeatureUsage";

		/// <summary>
		/// The name of the @includeEventStatistics attribute.
		/// </summary>
		private const string IncludeEventStatisticsAttributeName = "includeEventStatistics";

		/// <summary>
		/// The name of the @includeLocalRequestUris attribute.
		/// </summary>
		private const string IncludeLocalRequestUrisAttributeName = "includeLocalRequestUris";

		/// <summary>
		/// The name of the @includeCultures attribute.
		/// </summary>
		private const string IncludeCulturesAttributeName = "includeCultures";

		/// <summary>
		/// The name of the &lt;reporting&gt; sub-element.
		/// </summary>
		private const string ReportingElementName = DotNetOpenAuthSection.SectionName + "/reporting";

		/// <summary>
		/// The default value for the @minimumFlushInterval attribute.
		/// </summary>
#if DEBUG
		private const string MinimumFlushIntervalDefault = "0";
#else
		private const string MinimumFlushIntervalDefault = "0:15";
#endif

		/// <summary>
		/// Initializes a new instance of the <see cref="ReportingElement"/> class.
		/// </summary>
		internal ReportingElement() {
		}

		/// <summary>
		/// Gets the configuration section from the .config file.
		/// </summary>
		public static ReportingElement Configuration {
			get {
				return (ReportingElement)ConfigurationManager.GetSection(ReportingElementName) ?? new ReportingElement();
			}
		}

		/// <summary>
		/// Gets or sets a value indicating whether this reporting is enabled.
		/// </summary>
		/// <value><c>true</c> if enabled; otherwise, <c>false</c>.</value>
		[ConfigurationProperty(EnabledAttributeName, DefaultValue = true)]
		internal bool Enabled {
			get { return (bool)this[EnabledAttributeName]; }
			set { this[EnabledAttributeName] = value; }
		}

		/// <summary>
		/// Gets or sets the maximum frequency that reports will be published.
		/// </summary>
		[ConfigurationProperty(MinimumReportingIntervalAttributeName, DefaultValue = "1")] // 1 day default
		internal TimeSpan MinimumReportingInterval {
			get { return (TimeSpan)this[MinimumReportingIntervalAttributeName]; }
			set { this[MinimumReportingIntervalAttributeName] = value; }
		}

		/// <summary>
		/// Gets or sets the maximum frequency the set can be flushed to disk.
		/// </summary>
		[ConfigurationProperty(MinimumFlushIntervalAttributeName, DefaultValue = MinimumFlushIntervalDefault)]
		internal TimeSpan MinimumFlushInterval {
			get { return (TimeSpan)this[MinimumFlushIntervalAttributeName]; }
			set { this[MinimumFlushIntervalAttributeName] = value; }
		}

		/// <summary>
		/// Gets or sets a value indicating whether to include a list of library features used in the report.
		/// </summary>
		/// <value><c>true</c> to include a report of features used; otherwise, <c>false</c>.</value>
		[ConfigurationProperty(IncludeFeatureUsageAttributeName, DefaultValue = true)]
		internal bool IncludeFeatureUsage {
			get { return (bool)this[IncludeFeatureUsageAttributeName]; }
			set { this[IncludeFeatureUsageAttributeName] = value; }
		}

		/// <summary>
		/// Gets or sets a value indicating whether to include statistics of certain events such as
		/// authentication success and failure counting, and can include remote endpoint URIs.
		/// </summary>
		/// <value>
		/// 	<c>true</c> to include event counters in the report; otherwise, <c>false</c>.
		/// </value>
		[ConfigurationProperty(IncludeEventStatisticsAttributeName, DefaultValue = true)]
		internal bool IncludeEventStatistics {
			get { return (bool)this[IncludeEventStatisticsAttributeName]; }
			set { this[IncludeEventStatisticsAttributeName] = value; }
		}

		/// <summary>
		/// Gets or sets a value indicating whether to include a few URLs to pages on the hosting
		/// web site that host DotNetOpenAuth components.
		/// </summary>
		[ConfigurationProperty(IncludeLocalRequestUrisAttributeName, DefaultValue = true)]
		internal bool IncludeLocalRequestUris {
			get { return (bool)this[IncludeLocalRequestUrisAttributeName]; }
			set { this[IncludeLocalRequestUrisAttributeName] = value; }
		}

		/// <summary>
		/// Gets or sets a value indicating whether to include the cultures requested by the user agent
		/// on pages that host DotNetOpenAuth components.
		/// </summary>
		[ConfigurationProperty(IncludeCulturesAttributeName, DefaultValue = true)]
		internal bool IncludeCultures {
			get { return (bool)this[IncludeCulturesAttributeName]; }
			set { this[IncludeCulturesAttributeName] = value; }
		}
	}
}
