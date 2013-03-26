//-----------------------------------------------------------------------
// <copyright file="ExtensionTestUtilities.cs" company="Outercurve Foundation">
//     Copyright (c) Outercurve Foundation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace DotNetOpenAuth.Test.OpenId.Extensions {
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Net.Http;
	using System.Threading;
	using System.Threading.Tasks;

	using DotNetOpenAuth.Messaging;
	using DotNetOpenAuth.Messaging.Bindings;
	using DotNetOpenAuth.OpenId;
	using DotNetOpenAuth.OpenId.ChannelElements;
	using DotNetOpenAuth.OpenId.Extensions;
	using DotNetOpenAuth.OpenId.Messages;
	using DotNetOpenAuth.OpenId.Provider;
	using DotNetOpenAuth.OpenId.RelyingParty;
	using DotNetOpenAuth.Test.Messaging;
	using Validation;

	public static class ExtensionTestUtilities {
		internal static void RegisterExtension(Channel channel, StandardOpenIdExtensionFactory.CreateDelegate extensionFactory) {
			Requires.NotNull(channel, "channel");

			var factory = (OpenIdExtensionFactoryAggregator)channel.BindingElements.OfType<ExtensionsBindingElement>().Single().ExtensionFactory;
			factory.Factories.OfType<StandardOpenIdExtensionFactory>().Single().RegisterExtension(extensionFactory);
		}
	}
}
