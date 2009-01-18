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
		[MessagePart("mode", IsRequired = true)]
		private const string Mode = "store_request";

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
		/// Used by the Relying Party to add a given attribute with one or more values 
		/// to the request for storage.
		/// </summary>
		public void AddAttribute(AttributeValues attribute) {
			ErrorUtilities.VerifyArgumentNotNull(attribute, "attribute");
			ErrorUtilities.VerifyArgumentNamed(!this.ContainsAttribute(attribute.TypeUri), "attribute", OpenIdStrings.AttributeAlreadyAdded, attribute.TypeUri);
			this.attributesProvided.Add(attribute);
		}

		/// <summary>
		/// Used by the Relying Party to add a given attribute with one or more values 
		/// to the request for storage.
		/// </summary>
		public void AddAttribute(string typeUri, params string[] values) {
			this.AddAttribute(new AttributeValues(typeUri, values));
		}

		/// <summary>
		/// Used by the Provider to gets the value(s) associated with a given attribute
		/// that should be stored.
		/// </summary>
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

			FetchResponse.SerializeAttributes(fields, this.attributesProvided);
		}

		/// <summary>
		/// Called when the message has been received,
		/// after it passes through the channel binding elements.
		/// </summary>
		void IMessageWithEvents.OnReceiving() {
			var fields = ((IMessage)this).ExtraData;
			foreach (var att in FetchResponse.DeserializeAttributes(fields)) {
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

			if (!MessagingUtilities.AreEquivalentUnordered(this.Attributes.ToList(), other.Attributes.ToList())) {
				return false;
			}

			return true;
		}

		private bool ContainsAttribute(string typeUri) {
			return this.GetAttribute(typeUri) != null;
		}
	}
}
