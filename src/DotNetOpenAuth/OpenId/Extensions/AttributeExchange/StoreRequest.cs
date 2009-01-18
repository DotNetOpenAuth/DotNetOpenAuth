//-----------------------------------------------------------------------
// <copyright file="StoreRequest.cs" company="Andrew Arnott">
//     Copyright (c) Andrew Arnott. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace DotNetOpenAuth.OpenId.Extensions.AttributeExchange {
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using DotNetOpenAuth.Messaging;
	using DotNetOpenAuth.OpenId.Messages;

	/// <summary>
	/// The Attribute Exchange Store message, request leg.
	/// </summary>
	public sealed class StoreRequest : ExtensionBase, IMessageWithEvents {
		/// <summary>
		/// The factory method that may be used in deserialization of this message.
		/// </summary>
		internal static readonly OpenIdExtensionFactory.CreateDelegate Factory = (typeUri, data, baseMessage) => {
			if (typeUri == Constants.TypeUri && baseMessage is SignedResponseRequest) {
				string mode;
				if (data.TryGetValue("mode", out mode) && mode == Mode) {
					return new StoreRequest();
				}
			}

			return null;
		};

		/// <summary>
		/// The value of the 'mode' parameter.
		/// </summary>
		[MessagePart("mode", IsRequired = true)]
		private const string Mode = "store_request";

		/// <summary>
		/// The list of provided attribute values.  This field will never be null.
		/// </summary>
		private readonly List<AttributeValues> attributesProvided = new List<AttributeValues>();

		/// <summary>
		/// Initializes a new instance of the <see cref="StoreRequest"/> class.
		/// </summary>
		public StoreRequest()
			: base(new Version(1, 0), Constants.TypeUri, null) {
		}

		/// <summary>
		/// Gets a list of all the attributes that are included in the store request.
		/// </summary>
		public IEnumerable<AttributeValues> Attributes {
			get { return this.attributesProvided; }
		}

		/// <summary>
		/// Adds a given attribute with one or more values to the request for storage.
		/// Applicable to Relying Parties only.
		/// </summary>
		/// <param name="attribute">The attribute values.</param>
		public void AddAttribute(AttributeValues attribute) {
			ErrorUtilities.VerifyArgumentNotNull(attribute, "attribute");
			ErrorUtilities.VerifyArgumentNamed(!this.ContainsAttribute(attribute.TypeUri), "attribute", OpenIdStrings.AttributeAlreadyAdded, attribute.TypeUri);
			this.attributesProvided.Add(attribute);
		}

		/// <summary>
		/// Adds a given attribute with one or more values to the request for storage.
		/// Applicable to Relying Parties only.
		/// </summary>
		/// <param name="typeUri">The type URI of the attribute.</param>
		/// <param name="values">The attribute values.</param>
		public void AddAttribute(string typeUri, params string[] values) {
			this.AddAttribute(new AttributeValues(typeUri, values));
		}

		/// <summary>
		/// Gets the value(s) associated with a given attribute that should be stored.
		/// Applicable to Providers only.
		/// </summary>
		/// <param name="attributeTypeUri">The type URI of the attribute whose values are being sought.</param>
		/// <returns>The attribute values.</returns>
		public AttributeValues GetAttribute(string attributeTypeUri) {
			return this.attributesProvided.SingleOrDefault(attribute => string.Equals(attribute.TypeUri, attributeTypeUri, StringComparison.Ordinal));
		}

		#region IMessageWithEvents Members

		/// <summary>
		/// Called when the message is about to be transmitted,
		/// before it passes through the channel binding elements.
		/// </summary>
		void IMessageWithEvents.OnSending() {
			var fields = ((IMessage)this).ExtraData;
			fields.Clear();

			AXUtilities.SerializeAttributes(fields, this.attributesProvided);
		}

		/// <summary>
		/// Called when the message has been received,
		/// after it passes through the channel binding elements.
		/// </summary>
		void IMessageWithEvents.OnReceiving() {
			var fields = ((IMessage)this).ExtraData;
			foreach (var att in AXUtilities.DeserializeAttributes(fields)) {
				this.AddAttribute(att);
			}
		}

		#endregion

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
			var other = obj as StoreRequest;
			if (other == null) {
				return false;
			}

			if (this.Version != other.Version) {
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
				foreach (AttributeValues att in this.Attributes) {
					hashCode += att.GetHashCode();
				}
				return hashCode;
			}
		}

		/// <summary>
		/// Determines whether some attribute has values in this store request.
		/// </summary>
		/// <param name="typeUri">The type URI of the attribute in question.</param>
		/// <returns>
		/// 	<c>true</c> if the specified attribute appears in the store request; otherwise, <c>false</c>.
		/// </returns>
		private bool ContainsAttribute(string typeUri) {
			return this.GetAttribute(typeUri) != null;
		}
	}
}
