//-----------------------------------------------------------------------
// <copyright file="OpenIdExtensionFactoryAggregator.cs" company="Outercurve Foundation">
//     Copyright (c) Outercurve Foundation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace DotNetOpenAuth.OpenId.Extensions {
	using System.Collections.Generic;
	using DotNetOpenAuth.Messaging;
	using DotNetOpenAuth.OpenId.ChannelElements;
	using DotNetOpenAuth.OpenId.Messages;

	/// <summary>
	/// An OpenID extension factory that only delegates extension
	/// instantiation requests to other factories.
	/// </summary>
	internal class OpenIdExtensionFactoryAggregator : IOpenIdExtensionFactory {
		/// <summary>
		/// The list of factories this factory delegates to.
		/// </summary>
		private List<IOpenIdExtensionFactory> factories = new List<IOpenIdExtensionFactory>(2);

		/// <summary>
		/// Initializes a new instance of the <see cref="OpenIdExtensionFactoryAggregator"/> class.
		/// </summary>
		internal OpenIdExtensionFactoryAggregator() {
		}

		/// <summary>
		/// Gets the extension factories that this aggregating factory delegates to.
		/// </summary>
		/// <value>A list of factories.  May be empty, but never null.</value>
		internal IList<IOpenIdExtensionFactory> Factories {
			get { return this.factories; }
		}

		#region IOpenIdExtensionFactory Members

		/// <summary>
		/// Creates a new instance of some extension based on the received extension parameters.
		/// </summary>
		/// <param name="typeUri">The type URI of the extension.</param>
		/// <param name="data">The parameters associated specifically with this extension.</param>
		/// <param name="baseMessage">The OpenID message carrying this extension.</param>
		/// <param name="isProviderRole">A value indicating whether this extension is being received at the OpenID Provider.</param>
		/// <returns>
		/// An instance of <see cref="IOpenIdMessageExtension"/> if the factory recognizes
		/// the extension described in the input parameters; <c>null</c> otherwise.
		/// </returns>
		/// <remarks>
		/// This factory method need only initialize properties in the instantiated extension object
		/// that are not bound using <see cref="MessagePartAttribute"/>.
		/// </remarks>
		public IOpenIdMessageExtension Create(string typeUri, IDictionary<string, string> data, IProtocolMessageWithExtensions baseMessage, bool isProviderRole) {
			foreach (var factory in this.factories) {
				IOpenIdMessageExtension result = factory.Create(typeUri, data, baseMessage, isProviderRole);
				if (result != null) {
					return result;
				}
			}

			return null;
		}

		#endregion

		/// <summary>
		/// Loads the default factory and additional ones given by the configuration.
		/// </summary>
		/// <returns>A new instance of <see cref="OpenIdExtensionFactoryAggregator"/>.</returns>
		internal static OpenIdExtensionFactoryAggregator LoadFromConfiguration() {
			var factoriesElement = DotNetOpenAuth.Configuration.OpenIdElement.Configuration.ExtensionFactories;
			var aggregator = new OpenIdExtensionFactoryAggregator();
			aggregator.Factories.Add(new StandardOpenIdExtensionFactory());
			aggregator.factories.AddRange(factoriesElement.CreateInstances(false, null));
			return aggregator;
		}
	}
}
