//-----------------------------------------------------------------------
// <copyright file="CodeTimers.cs" company="Microsoft Corporation">
//     Copyright (c) Microsoft Corporation. All rights reserved.
// </copyright>
// <author>Vance Morrison</author>
//-----------------------------------------------------------------------

namespace DotNetOpenAuth.Test.Performance {
	using System;
	using System.Collections.Generic;
	using System.Text;
	using System.IO;
	using System.Web;               // for HttpUtility.HtmlEncode
	using System.Reflection;        // for Assembly 
	using System.Diagnostics;       // for Process, DebuggableAttribute

	/// <summary>
	/// A code:StatsLogger is something that remembers a set of common
	/// atrributes (verison of the code, Machine used, NGENed or JITed ...) as
	/// well as a set of performance results from mulitple benchmarks.
	/// (represented by a code:StatsCollection)
	/// 
	/// The primary value of a StatsLogger is the
	/// code:StatsLogger.DisplayHtmlReport which displayes the data in a
	/// user-friendly way.
	/// </summary>
	internal class StatsLogger {
		public StatsLogger(StatsCollection dataSet) {
			this.dataSet = dataSet;
			attributes = new Dictionary<string, object>();
		}
		StatsCollection DataSet { get { return dataSet; } }
		public object this[string key] {
			get { return attributes[key]; }
			set { attributes[key] = value; }
		}
		public void Add(string name, Stats sample) { dataSet.Add(name, sample); }
		public string Category {
			get { return category; }
			set { category = value; }
		}
		public void AddWithCount(string name, int iterationCount, float scale, Stats sample) {
			if (!string.IsNullOrEmpty(category))
				name = category + ": " + name;
			if (iterationCount != 1 || scale != 1) {
				name += "  [";
				if (iterationCount != 1) {
					name = name + "count=" + iterationCount.ToString();
					if (scale != 1)
						name += " ";
				}
				if (scale != 1)
					name = name + " scale=" + scale.ToString("f1");
				name += "]";
			}

			Add(name, sample);
		}
		public void DisplayHtmlReport(string reportFileName) {
			TextWriter writer = File.CreateText(reportFileName);

			writer.WriteLine("<html>");
			writer.WriteLine("<h1> MeasureIt Performance Results </h1>");
			object optimizationValue;
			if (attributes.TryGetValue("CodeOptimization", out optimizationValue) && ((string)optimizationValue) == "Unoptimized") {
				writer.WriteLine("<font color=red><p>");
				writer.WriteLine("Warning: the MeasureIt code was not optimized.  The results are likely invalid.");
				writer.WriteLine("</p></font>");
			}
			if (Environment.OSVersion.Version.Major < 6) {
				writer.WriteLine("<b><p>");
				writer.WriteLine("Data was collected on a Pre-Vista machine.   MeasureIt does NOT automatically");
				writer.WriteLine("set the CPU to a high performance power policy.  This means the CPU might");
				writer.WriteLine("be throttled to save power and can lead to");
				writer.WriteLine("incorrect and inconsistant benchmark measurements.");
				writer.WriteLine("</p></b>");
			} else if (PowerManagment.CurrentPolicy != PowerManagment.PowerProfiles.HighPerformance) {
				writer.WriteLine("<font color=red><p>");
				writer.WriteLine("Warning: The power policy settings were not set at 'High Performance' during this run.");
				writer.WriteLine("This means that the CPU could be throttled to lower frequency resulting in");
				writer.WriteLine("incorrect and inconsistant benchmark measurements.");
				writer.WriteLine("To correct go to Start Menu -> Contol Panel -> System and Maintance -> Power Options");
				writer.WriteLine("and set the power policy to 'High Performance' for the duration of the tests.");
				writer.WriteLine("</p></font>");
			}
			writer.WriteLine("<p>");
			writer.WriteLine("Below are the results of running a series of benchmarks.  Use the");
			writer.WriteLine("<b>MeasureIt /usersGuide</b> for more details on exactly what the benchmarks do.");
			writer.WriteLine("</p><p>");
			writer.WriteLine("It is very easy for benchmark results to be wrong or misleading.  You should read the guidance");
			writer.WriteLine("in the <b>MeasureIt /usersGuide</b> before making important decisions based on this data.");
			writer.WriteLine("</p><p>");
			writer.WriteLine("To improve the stability of the measurements, a may be cloned several times");
			writer.WriteLine("and this cloned code is then run in a loop.");
			writer.WriteLine("If the benchmark was cloned the 'scale' attribute represents the number of times");
			writer.WriteLine("it was cloned, and the count represents the number of times the cloned code was run in a loop");
			writer.WriteLine("before the measurement was made.    The reported number divides by both");
			writer.WriteLine("of these values, so it represents a single instance of the operation being measured.");
			writer.WriteLine("</p>");
			writer.WriteLine("<p>");
			writer.WriteLine("The benchmarks data can vary from run to run, so the benchmark is run several times and");
			writer.WriteLine("the statistics are displayed.  If we assume a normal distribution, you can expect 68% of all measureuments");
			writer.WriteLine("to fall within 1 StdDev of the Mean.   You can expect over 95% of all measurements");
			writer.WriteLine("to fall witin 2 StdDev of the Mean.   Thus 2 StdDev is a good error bound.");
			writer.WriteLine("Keep in mind, however that it is not uncommon for the statistics to be quite stable");
			writer.WriteLine("during a run and yet very widely across different runs.  See the users guide for more.");
			writer.WriteLine("</p>");
			writer.WriteLine("<p>");
			writer.WriteLine("Generally the mean is a better measurment if you use the number to compute an");
			writer.WriteLine("aggregate throughput for a large number of items.  The median is a better");
			writer.WriteLine("guess if you want to best guess of a typical sample.   The median is also");
			writer.WriteLine("more stable if the sample is noisy (eg has outliers).");
			writer.WriteLine("</p>");
			writer.WriteLine("<h3>Data collected</h3>");
			{
				writer.WriteLine("<p>");
				writer.WriteLine(UnitsDescription);
				writer.WriteLine("</p>");

				dataSet.WriteReportTable(writer, Scale);
			}

			writer.WriteLine("<p>");
			{
				writer.WriteLine("<h2>Attributes of the machine used to collect the data</h2>");
				writer.WriteLine("<table border>");
				writer.WriteLine("<tr><th>Attribute</th><th>Value</th></tr>");
				foreach (string key in attributes.Keys) {
					object valueObj = this[key];
					writer.Write("<tr>");
					writer.Write("<td>" + HttpUtility.HtmlEncode(key) + "</td>");

					string valueStr = HttpUtility.HtmlEncode(valueObj.ToString());
					writer.Write("<td>" + valueStr + "</td>");
					writer.WriteLine("<tr>");
				}
				writer.WriteLine("</table>");
			}
			writer.WriteLine("</p>");

			writer.WriteLine("</html>");
			writer.Close();
		}
		static public void LaunchIE(string fileName) {
			Process process = new Process();
			process.StartInfo = new ProcessStartInfo(fileName);
			process.Start();
		}

		public float Scale = 1.0F;
		public string UnitsDescription = "Scale in usec";

		// TODO: probabably does not belong in this class. 
		public void CaptureCurrentMachineInfo(Assembly assemblyWithCode, bool skipMachineStats) {
			this["Computer Name"] = Environment.MachineName;
			if (!skipMachineStats) {
				////ComputerSpecs specs = new ComputerSpecs();
				////this["Number of Processors"] = specs.NumberOfProcessors;
				////this["Processor Name "] = specs.ProcessorName;
				////this["Processor Mhz"] = specs.ProcessorClockSpeedMhz;
				////this["Memory MBytes"] = specs.MemoryMBytes;
				////this["L1 Cache KBytes"] = specs.L1KBytes;
				////this["L2 Cache KBytes"] = specs.L2KBytes;
				////this["Operating System"] = specs.OperatingSystem;
				////this["Operating System Version"] = specs.OperatingSystemVersion;
				////this["Stopwatch resolution (nsec)"] = (CodeTimer.ResolutionUsec * 1000.0).ToString("f3");
			}

			this["Machine Word Size (Bits)"] = (System.Runtime.InteropServices.Marshal.SizeOf(typeof(IntPtr)) * 8).ToString();

			this["CLR Version"] = Environment.Version.ToString();
			this["CLR Directory"] = System.Runtime.InteropServices.RuntimeEnvironment.GetRuntimeDirectory();

			// Are we NGENed or JITTed?
			if (IsNGenedCodeLoaded(assemblyWithCode))
				this["CompileType"] = "NGEN";
			else
				this["CompileType"] = "JIT";

			// Are we Appdomain Shared, or not?
			MethodInfo currentMethod = Assembly.GetEntryAssembly().EntryPoint;
			LoaderOptimizationAttribute loaderAttribute = (LoaderOptimizationAttribute)System.Attribute.GetCustomAttribute(currentMethod, typeof(LoaderOptimizationAttribute));
			if (loaderAttribute != null && loaderAttribute.Value == LoaderOptimization.MultiDomain)
				this["CodeSharing"] = "AppDomainShared";
			else
				this["CodeSharing"] = "AppDomainSpecific";

			this["CodeOptimization"] = IsOptimized(assemblyWithCode) ? "Optimized" : "Unoptimized";
		}

		internal static bool IsOptimized(Assembly assembly) {
			DebuggableAttribute debugAttribute = (DebuggableAttribute)System.Attribute.GetCustomAttribute(assembly, typeof(System.Diagnostics.DebuggableAttribute));
			if (debugAttribute != null && debugAttribute.IsJITOptimizerDisabled)
				return false;
			else
				return true;
		}

		static bool IsNGenedCodeLoaded(Assembly assembly) {
			// This is a bit of a hack, basically I find the assemblies file name, then
			// look for a module loaded called '<filename>.ni.<ext>'.   It is not foolproof,
			// but it more than good enough for most purposes. 
			string assemblyFileName = Path.GetFileName(assembly.ManifestModule.FullyQualifiedName);
			string nativeImageExt = "ni" + Path.GetExtension(assemblyFileName);
			string nativeImageSuffix = @"\" + Path.ChangeExtension(assemblyFileName, nativeImageExt);

			System.Diagnostics.Process myProcess = System.Diagnostics.Process.GetCurrentProcess();
			foreach (System.Diagnostics.ProcessModule module in myProcess.Modules) {
				if (module.FileName.EndsWith(nativeImageSuffix, StringComparison.OrdinalIgnoreCase))
					return true;
			}
			return false;
		}

		#region privates
		Dictionary<string, object> attributes;
		StatsCollection dataSet;
		string category;

		#endregion
	}

	/// <summary>
	/// StatsCollection represents a collecton of named of samples (class
	/// Stats) that have have been given string names.   The data can be
	/// looked up by name, but is also collection also remembers the order in
	/// which the samples were added, and the names can be enumerated in that
	/// order.  
	/// </summary>
	internal class StatsCollection {
		public StatsCollection() {
			dict = new Dictionary<string, Stats>();
			order = new List<string>();
		}

		public Stats this[string key] { get { return dict[key]; } }
		public bool ContainsKey(string key) { return dict.ContainsKey(key); }
		public IEnumerable<string> Keys { get { return order; } }
		public void Add(string key, Stats value) {
			dict.Add(key, value);
			order.Add(key);
		}
		public void WriteReportTable(TextWriter writer, float scale) {
			writer.WriteLine("<table border>");
			writer.WriteLine("<tr><th>Name</th><th>Median</th><th>Mean</th><th>StdDev</th><th>Min</th><th>Max</th><th>Samples</th></tr>");

			foreach (string key in this.Keys) {
				Stats value = this[key];
				writer.Write("<tr>");
				writer.Write("<td>" + HttpUtility.HtmlEncode(key) + "</td>");
				writer.Write("<td>" + (value.Median / scale).ToString("f3") + "</td>");
				writer.Write("<td>" + (value.Mean / scale).ToString("f3") + "</td>");
				writer.Write("<td>" + (value.StandardDeviation / scale).ToString("f3") + "</td>");
				writer.Write("<td>" + (value.Minimum / scale).ToString("f3") + "</td>");
				writer.Write("<td>" + (value.Maximum / scale).ToString("f3") + "</td>");
				writer.Write("<td>" + value.Count + "</td>");
				writer.WriteLine("</tr>");
			}
			writer.WriteLine("</table>");
		}

		#region privates
		Dictionary<string, Stats> dict;
		List<string> order;
		#endregion
	}
}

