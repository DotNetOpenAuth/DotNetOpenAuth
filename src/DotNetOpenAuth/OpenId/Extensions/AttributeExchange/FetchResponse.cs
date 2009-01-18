//-----------------------------------------------------------------------
// <copyright file="FetchResponse.cs" company="Andrew Arnott">
//     Copyright (c) Andrew Arnott. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace DotNetOpenAuth.OpenId.Extensions.AttributeExchange {
	using System;
	using System.Collections.Generic;
	using System.Diagnostics;
	using System.Globalization;
	using System.Linq;
	using DotNetOpenAuth.Messaging;
	using DotNetOpenAuth.OpenId.Messages;

	/// <summary>
	/// The Attribute Exchange Fetch message, response leg.
	/// </summary>
	public sealed class FetchResponse : ExtensionBase, IMessageWithEvents {
		/// <summary>
		/// The factory method that may be used in deserialization of this message.
		/// </summary>
		internal static readonly OpenIdExtensionFactory.CreateDelegate Factory = (typeUri, data, baseMessage) => {
			if (typeUri == Constants.TypeUri && baseMessage is IndirectSignedResponse) {
				string mode;
				if (data.TryGetValue("mode", out mode) && mode == Mode) {
					return new FetchResponse();
				}
			}

			return null;
		};

		/// <summary>
		/// The value of the 'mode' parameter.
		/// </summary>
		[MessagePart("mode", IsRequired = true)]
		private const string Mode = "fetch_response";

		/// <summary>
		/// The list of provided attributes.  This field will never be null.
		/// </summary>
		private readonly List<AttributeValues> attributesProvided = new List<AttributeValues>();

		/// <summary>
		/// Initializes a new instance of the <see cref="FetchResponse"/> class.
		/// </summary>
		public FetchResponse()
			: base(new Version(1, 0), Constants.TypeUri, null) {
		}

		/// <summary>
		/// Gets a sequence of the attributes whose values are provided by the OpenID Provider.
		/// </summary>
		public IEnumerable<AttributeValues> Attributes {
			get { return this.attributesProvided; }
		}

		/// <summary>
		/// Gets a value indicating whether the OpenID Provider intends to
		/// honor the request for updates.
		/// </summary>
		public bool UpdateUrlSupported {
			get { return this.UpdateUrl != null; }
		}

		/// <summary>
		/// Gets or sets the URL the OpenID Provider will post updates to.  
		/// Must be set if the Provider supports and will use this feature.
		/// </summary>
		[MessagePart("update_url", IsRequired = false)]
		public Uri UpdateUrl { get; set; }

		/// <summary>
		/// Adds attributes to the response for the relying party.
		/// Applicable to Providers.
		/// </summary>
		/// <param name="attribute">The attribute and values to add to the response.</param>
		public void AddAttribute(AttributeValues attribute) {
			ErrorUtilities.VerifyArgumentNotNull(attribute, "attribute");
			ErrorUtilities.VerifyArgumentNamed(!this.ContainsAttribute(attribute.TypeUri), "attribute", OpenIdStrings.AttributeAlreadyAdded, attribute.TypeUri);
			this.attributesProvided.Add(attribute);
		}

		/// <summary>
		/// Gets the value(s) returned by the OpenID Provider
		/// for a given attribute, or null if that attribute was not provided.
		/// Applicable to Relying Parties.
		/// </summary>
		/// <param name="attributeTypeUri">The type URI of the attribute.</param>
		/// <returns>The values given by the Provider for the given attribute; or <c>null</c> if the attribute was not included in the message.</returns>
		public AttributeValues GetAttribute(string attributeTypeUri) {
			return this.attributesProvided.SingleOrDefault(attribute => string.Equals(attribute.TypeUri, attributeTypeUri, StringComparison.Ordinal));
		}

		/// <summary>
		/// Determines whether the specified <see cref="T:System.Object"/> is equal to the current <see cref="T:System.Object"/>.
		/// </summary>
		/// <param name="obj">The <see cref="T:System.Object"/> to compare with the current <see cref="T:System.Object"/>.</param>
		/// <returns>
		/// true if the specified <see cref="T:System.Object"/> is equal to the current <see cref="T:System.Object"/>; otherwise, false.
		/// </returns>
		/// <exception cref="T:System.NullReferenceException">
		/// The <paramref name="obj"/> parameter is null.
		/// </exception>
		public override bool Equals(object obj) {
			FetchResponse other = obj as FetchResponse;
			if (other == null) {
				return false;
			}

			if (this.Version != other.Version) {
				return false;
			}

			if (this.UpdateUrl != other.UpdateUrl) {
				return false;
			}

			if (!MessagingUtilities.AreEquivalentUnordered(this.Attributes.ToList(), other.Attributes.ToList())) {
				return false;
			}

			return true;
		}

		/// <summary>
		/// Serves as a hash function for a particular type.
		/// </summary>
		/// <returns>
		/// A hash code for the current <see cref="T:System.Object"/>.
		/// </returns>
		public override int GetHashCode() {
			unchecked {
				int hashCode = this.Version.GetHashCode();

				if (this.UpdateUrl != null) {
					hashCode += this.UpdateUrl.GetHashCode();
				}

				foreach (AttributeValues value in this.Attributes) {
					hashCode += value.GetHashCode();
				}

				return hashCode;
			}
		}

		#region IMessageWithEvents Members

		/// <summary>
		/// Called when the message is about to be transmitted,
		/// before it passes through the channel binding elements.
		/// </summary>
		void IMessageWithEvents.OnSending() {
			var extraData = ((IMessage)this).ExtraData;
			AXUtilities.SerializeAttributes(extraData, this.attributesProvided);
		}

		/// <summary>
		/// Called when the message has been received,
		/// after it passes through the channel binding elements.
		/// </summary>
		void IMessageWithEvents.OnReceiving() {
			var extraData = ((IMessage)this).ExtraData;
			foreach (var att in AXUtilities.DeserializeAttributes(extraData)) {
				this.AddAttribute(att);
			}
		}

		#endregion

		/// <summary>
		/// Checks the message state for conformity to the protocol specification
		/// and throws an exception if the message is invalid.
		/// </summary>
		/// <remarks>
		/// 	<para>Some messages have required fields, or combinations of fields that must relate to each other
		/// in specialized ways.  After deserializing a message, this method checks the state of the
		/// message to see if it conforms to the protocol.</para>
		/// 	<para>Note that this property should <i>not</i> check signatures or perform any state checks
		/// outside this scope of this particular message.</para>
		/// </remarks>
		/// <exception cref="ProtocolException">Thrown if the message is invalid.</exception>
		protected override void EnsureValidMessage() {
			base.EnsureValidMessage();

			if (this.UpdateUrl != null && !this.UpdateUrl.IsAbsoluteUri) {
				this.UpdateUrl = null;
				Logger.ErrorFormat("The AX fetch response update_url parameter was not absolute ('{0}').  Ignoring value.", this.UpdateUrl);
			}
		}

		/// <summary>
		/// Determines whether some attribute has values in this fetch response.
		/// </summary>
		/// <param name="typeUri">The type URI of the attribute in question.</param>
		/// <returns>
		/// 	<c>true</c> if the specified attribute appears in the fetch response; otherwise, <c>false</c>.
		/// </returns>
		private bool ContainsAttribute(string typeUri) {
			return this.GetAttribute(typeUri) != null;
		}
	}
}
