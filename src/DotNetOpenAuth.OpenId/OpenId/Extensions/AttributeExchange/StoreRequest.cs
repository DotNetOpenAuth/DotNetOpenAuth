//-----------------------------------------------------------------------
// <copyright file="StoreRequest.cs" company="Outercurve Foundation">
//     Copyright (c) Outercurve Foundation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace DotNetOpenAuth.OpenId.Extensions.AttributeExchange {
	using System;
	using System.Collections.ObjectModel;
	using System.Linq;
	using DotNetOpenAuth.Messaging;
	using DotNetOpenAuth.OpenId.Messages;

	/// <summary>
	/// The Attribute Exchange Store message, request leg.
	/// </summary>
	[Serializable]
	public sealed class StoreRequest : ExtensionBase, IMessageWithEvents {
		/// <summary>
		/// The factory method that may be used in deserialization of this message.
		/// </summary>
		internal static readonly StandardOpenIdExtensionFactory.CreateDelegate Factory = (typeUri, data, baseMessage, isProviderRole) => {
			if (typeUri == Constants.TypeUri && isProviderRole) {
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
		/// The collection of provided attribute values.  This field will never be null.
		/// </summary>
		private readonly KeyedCollection<string, AttributeValues> attributesProvided = new KeyedCollectionDelegate<string, AttributeValues>(av => av.TypeUri);

		/// <summary>
		/// Initializes a new instance of the <see cref="StoreRequest"/> class.
		/// </summary>
		public StoreRequest()
			: base(new Version(1, 0), Constants.TypeUri, null) {
		}

		/// <summary>
		/// Gets the collection of all the attributes that are included in the store request.
		/// </summary>
		public KeyedCollection<string, AttributeValues> Attributes {
			get { return this.attributesProvided; }
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
				this.Attributes.Add(att);
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
	}
}
