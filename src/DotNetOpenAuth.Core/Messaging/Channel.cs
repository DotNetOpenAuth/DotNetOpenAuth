//-----------------------------------------------------------------------
// <copyright file="Channel.cs" company="Andrew Arnott">
//     Copyright (c) Andrew Arnott. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace DotNetOpenAuth.Messaging {
	using System;
	using System.Collections.Generic;
	using System.Diagnostics.CodeAnalysis;
	using System.Linq;
	using System.Runtime.Serialization.Json;
	using System.Text;
	using System.Threading.Tasks;
	using System.Web;
	using System.Xml;
	using DotNetOpenAuth.Configuration;
	using Validation;

	/// <summary>
	/// Derivation of ChannelBase that adds desktop-specific behavior.
	/// </summary>
	public abstract class Channel : ChannelBase {
		/// <summary>
		/// A default set of XML dictionary reader quotas that are relatively safe from causing unbounded memory consumption.
		/// </summary>
		internal static readonly XmlDictionaryReaderQuotas DefaultUntrustedXmlDictionaryReaderQuotas = new XmlDictionaryReaderQuotas {
			MaxArrayLength = 1,
			MaxDepth = 2,
			MaxBytesPerRead = 8 * 1024,
			MaxStringContentLength = 16 * 1024,
		};

		/// <summary>
		/// Initializes a new instance of the <see cref="Channel"/> class.
		/// </summary>
		/// <param name="messageTypeProvider">A class prepared to analyze incoming messages and indicate what concrete
		/// message types can deserialize from it.</param>
		/// <param name="bindingElements">The binding elements to use in sending and receiving messages.
		/// The order they are provided is used for outgoing messgaes, and reversed for incoming messages.</param>
		/// <param name="hostFactories">The host factories.</param>
		protected Channel(IMessageFactory messageTypeProvider, IChannelBindingElement[] bindingElements, IHostFactories hostFactories)
			: base(messageTypeProvider, bindingElements, hostFactories) {
			this.XmlDictionaryReaderQuotas = DefaultUntrustedXmlDictionaryReaderQuotas;
			this.MaximumIndirectMessageUrlLength = Configuration.DotNetOpenAuthSection.Messaging.MaximumIndirectMessageUrlLength;
			this.MaximumClockSkew = DotNetOpenAuthSection.Messaging.MaximumClockSkew;
			this.MaximumMessageLifetimeNoSkew = Configuration.DotNetOpenAuthSection.Messaging.MaximumMessageLifetime;
		}

		/// <summary>
		/// Gets the HTTP context for the current HTTP request.
		/// </summary>
		/// <returns>An HttpContextBase instance.</returns>
		[SuppressMessage("Microsoft.Design", "CA1024:UsePropertiesWhereAppropriate", Justification = "Allocates memory")]
		protected internal virtual HttpContextBase GetHttpContext() {
			RequiresEx.ValidState(HttpContext.Current != null, MessagingStrings.HttpContextRequired);
			return new HttpContextWrapper(HttpContext.Current);
		}

		/// <summary>
		/// Gets the current HTTP request being processed.
		/// </summary>
		/// <returns>The HttpRequestInfo for the current request.</returns>
		/// <remarks>
		/// Requires an <see cref="HttpContext.Current"/> context.
		/// </remarks>
		/// <exception cref="InvalidOperationException">Thrown if <see cref="HttpContext.Current">HttpContext.Current</see> == <c>null</c>.</exception>
		[SuppressMessage("Microsoft.Design", "CA1024:UsePropertiesWhereAppropriate", Justification = "Costly call should not be a property.")]
		protected internal virtual HttpRequestBase GetRequestFromContext() {
			RequiresEx.ValidState(HttpContext.Current != null && HttpContext.Current.Request != null, MessagingStrings.HttpContextRequired);

			Assumes.True(HttpContext.Current.Request.Url != null);
			Assumes.True(HttpContext.Current.Request.RawUrl != null);
			return new HttpRequestWrapper(HttpContext.Current.Request);
		}

		/// <summary>
		/// Serializes the given message as a JSON string.
		/// </summary>
		/// <param name="message">The message to serialize.</param>
		/// <returns>A JSON string.</returns>
		[SuppressMessage("Microsoft.Usage", "CA2202:Do not dispose objects multiple times", Justification = "This Dispose is safe.")]
		[SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope", Justification = "No apparent problem.  False positive?")]
		protected virtual string SerializeAsJson(IMessage message) {
			Requires.NotNull(message, "message");
			return MessagingUtilities.SerializeAsJson(message, this.MessageDescriptions);
		}

		/// <summary>
		/// Deserializes from flat data from a JSON object.
		/// </summary>
		/// <param name="json">A JSON string.</param>
		/// <returns>The simple "key":"value" pairs from a JSON-encoded object.</returns>
		[SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope", Justification = "No apparent problem.  False positive?")]
		protected virtual IDictionary<string, string> DeserializeFromJson(string json) {
			Requires.NotNullOrEmpty(json, "json");

			var dictionary = new Dictionary<string, string>();
			using (var jsonReader = JsonReaderWriterFactory.CreateJsonReader(Encoding.UTF8.GetBytes(json), this.XmlDictionaryReaderQuotas)) {
				MessageSerializer.DeserializeJsonAsFlatDictionary(dictionary, jsonReader);
			}
			return dictionary;
		}
	}
}
