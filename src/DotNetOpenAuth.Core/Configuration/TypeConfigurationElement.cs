//-----------------------------------------------------------------------
// <copyright file="TypeConfigurationElement.cs" company="Outercurve Foundation">
//     Copyright (c) Outercurve Foundation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace DotNetOpenAuth.Configuration {
	using System;
	using System.Configuration;
	using System.Diagnostics.CodeAnalysis;
	using System.IO;
	using System.Reflection;
	using System.Web;
	using System.Xaml;
	using DotNetOpenAuth.Messaging;

	/// <summary>
	/// Represents an element in a .config file that allows the user to provide a @type attribute specifying
	/// the full type that provides some service used by this library.
	/// </summary>
	/// <typeparam name="T">A constraint on the type the user may provide.</typeparam>
	internal class TypeConfigurationElement<T> : ConfigurationElement
		where T : class {
		/// <summary>
		/// The name of the attribute whose value is the full name of the type the user is specifying.
		/// </summary>
		private const string CustomTypeConfigName = "type";

		/// <summary>
		/// The name of the attribute whose value is the path to the XAML file to deserialize to obtain the type.
		/// </summary>
		private const string XamlReaderSourceConfigName = "xaml";

		/// <summary>
		/// Initializes a new instance of the TypeConfigurationElement class.
		/// </summary>
		public TypeConfigurationElement() {
		}

		/// <summary>
		/// Gets or sets the full name of the type.
		/// </summary>
		/// <value>The full name of the type, such as: "ConsumerPortal.Code.CustomStore, ConsumerPortal".</value>
		[ConfigurationProperty(CustomTypeConfigName)]
		////[SubclassTypeValidator(typeof(T))] // this attribute is broken in .NET, I think.
		public string TypeName {
			get { return (string)this[CustomTypeConfigName]; }
			set { this[CustomTypeConfigName] = value; }
		}

		/// <summary>
		/// Gets or sets the path to the XAML file to deserialize to obtain the instance.
		/// </summary>
		[ConfigurationProperty(XamlReaderSourceConfigName)]
		public string XamlSource {
			get { return (string)this[XamlReaderSourceConfigName]; }
			set { this[XamlReaderSourceConfigName] = value; }
		}

		/// <summary>
		/// Gets the type described in the .config file.
		/// </summary>
		public Type CustomType {
			get { return string.IsNullOrEmpty(this.TypeName) ? null : Type.GetType(this.TypeName); }
		}

		/// <summary>
		/// Gets a value indicating whether this type has no meaningful type to instantiate.
		/// </summary>
		public bool IsEmpty {
			get { return this.CustomType == null && string.IsNullOrEmpty(this.XamlSource); }
		}

		/// <summary>
		/// Creates an instance of the type described in the .config file.
		/// </summary>
		/// <param name="defaultValue">The value to return if no type is given in the .config file.</param>
		/// <param name="hostFactories">The host factories.</param>
		/// <returns>
		/// The newly instantiated type.
		/// </returns>
		public T CreateInstance(T defaultValue, IHostFactories hostFactories) {
			return this.CreateInstance(defaultValue, false, hostFactories);
		}

		/// <summary>
		/// Creates an instance of the type described in the .config file.
		/// </summary>
		/// <param name="defaultValue">The value to return if no type is given in the .config file.</param>
		/// <param name="allowInternals">if set to <c>true</c> then internal types may be instantiated.</param>
		/// <param name="hostFactories">The host factories.</param>
		/// <returns>
		/// The newly instantiated type.
		/// </returns>
		[SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope", Justification = "No apparent problem.  False positive?")]
		public T CreateInstance(T defaultValue, bool allowInternals, IHostFactories hostFactories) {
			T instance;
			if (this.CustomType != null) {
				if (!allowInternals) {
					// Although .NET will usually prevent our instantiating non-public types,
					// it will allow our instantiation of internal types within this same assembly.
					// But we don't want the host site to be able to do this, so we check ourselves.
					ErrorUtilities.VerifyArgument((this.CustomType.Attributes & TypeAttributes.Public) != 0, Strings.ConfigurationTypeMustBePublic, this.CustomType.FullName);
				}
				instance = (T)Activator.CreateInstance(this.CustomType);
			} else if (!string.IsNullOrEmpty(this.XamlSource)) {
				string source = this.XamlSource;
				if (source.StartsWith("~/", StringComparison.Ordinal)) {
					ErrorUtilities.VerifyHost(HttpContext.Current != null, Strings.ConfigurationXamlReferenceRequiresHttpContext, this.XamlSource);
					source = HttpContext.Current.Server.MapPath(source);
				}
				using (Stream xamlFile = File.OpenRead(source)) {
					instance = CreateInstanceFromXaml(xamlFile);
				}
			} else {
				instance = defaultValue;
			}

			var requiresHostFactories = instance as IRequireHostFactories;
			if (requiresHostFactories != null) {
				requiresHostFactories.HostFactories = hostFactories;
			}

			return instance;
		}

		/// <summary>
		/// Creates the instance from xaml.
		/// </summary>
		/// <param name="xaml">The stream of xaml to deserialize.</param>
		/// <returns>The deserialized object.</returns>
		/// <remarks>
		/// This exists as its own method to prevent the CLR's JIT compiler from failing
		/// to compile the CreateInstance method just because the PresentationFramework.dll
		/// may be missing (which it is on some shared web hosts).  This way, if the
		/// XamlSource attribute is never used, the PresentationFramework.dll never need
		/// be present.
		/// </remarks>
		private static T CreateInstanceFromXaml(Stream xaml) {
			return (T)XamlServices.Load(xaml);
		}
	}
}
