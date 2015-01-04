//-----------------------------------------------------------------------
// <copyright file="FetchRequest.cs" company="Outercurve Foundation">
//     Copyright (c) Outercurve Foundation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace DotNetOpenAuth.OpenId.Extensions.AttributeExchange {
	using System;
	using System.Collections.Generic;
	using System.Collections.ObjectModel;
	using System.Globalization;
	using System.Linq;

	using DotNetOpenAuth.Logging;
	using DotNetOpenAuth.Messaging;
	using DotNetOpenAuth.OpenId.Messages;

	/// <summary>
	/// The Attribute Exchange Fetch message, request leg.
	/// </summary>
	[Serializable]
	public sealed class FetchRequest : ExtensionBase, IMessageWithEvents {
		/// <summary>
		/// The factory method that may be used in deserialization of this message.
		/// </summary>
		internal static readonly StandardOpenIdExtensionFactory.CreateDelegate Factory = (typeUri, data, baseMessage, isProviderRole) => {
			if (typeUri == Constants.TypeUri && isProviderRole) {
				string mode;
				if (data.TryGetValue("mode", out mode) && mode == Mode) {
					return new FetchRequest();
				}
			}

			return null;
		};

		/// <summary>
		/// Characters that may not appear in an attribute alias list.
		/// </summary>
		internal static readonly char[] IllegalAliasListCharacters = new[] { '.', '\n' };

		/// <summary>
		/// Characters that may not appear in an attribute Type URI alias.
		/// </summary>
		internal static readonly char[] IllegalAliasCharacters = new[] { '.', ',', ':' };

		/// <summary>
		/// The value for the 'mode' parameter.
		/// </summary>
		[MessagePart("mode", IsRequired = true)]
		private const string Mode = "fetch_request";

		/// <summary>
		/// The collection of requested attributes.
		/// </summary>
		private readonly KeyedCollection<string, AttributeRequest> attributes = new KeyedCollectionDelegate<string, AttributeRequest>(ar => ar.TypeUri);

		/// <summary>
		/// Initializes a new instance of the <see cref="FetchRequest"/> class.
		/// </summary>
		public FetchRequest()
			: base(new Version(1, 0), Constants.TypeUri, null) {
		}

		/// <summary>
		/// Gets a collection of the attributes whose values are 
		/// requested by the Relying Party.
		/// </summary>
		/// <value>A collection where the keys are the attribute type URIs, and the value
		/// is all the attribute request details.</value>
		public KeyedCollection<string, AttributeRequest> Attributes {
			get {
				return this.attributes;
			}
		}

		/// <summary>
		/// Gets or sets the URL that the OpenID Provider may re-post the fetch response 
		/// message to at some time after the initial response has been sent, using an
		/// OpenID Authentication Positive Assertion to inform the relying party of updates
		/// to the requested fields.
		/// </summary>
		[MessagePart("update_url", IsRequired = false)]
		public Uri UpdateUrl { get; set; }

		/// <summary>
		/// Gets or sets a list of aliases for optional attributes.
		/// </summary>
		/// <value>A comma-delimited list of aliases.</value>
		[MessagePart("if_available", IsRequired = false)]
		private string OptionalAliases { get; set; }

		/// <summary>
		/// Gets or sets a list of aliases for required attributes.
		/// </summary>
		/// <value>A comma-delimited list of aliases.</value>
		[MessagePart("required", IsRequired = false)]
		private string RequiredAliases { get; set; }

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
			FetchRequest other = obj as FetchRequest;
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

				foreach (AttributeRequest att in this.Attributes) {
					hashCode += att.GetHashCode();
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
			var fields = ((IMessage)this).ExtraData;
			fields.Clear();

			List<string> requiredAliases = new List<string>(), optionalAliases = new List<string>();
			AliasManager aliasManager = new AliasManager();
			foreach (var att in this.attributes) {
				string alias = aliasManager.GetAlias(att.TypeUri);

				// define the alias<->typeUri mapping
				fields.Add("type." + alias, att.TypeUri);

				// set how many values the relying party wants max
				fields.Add("count." + alias, att.Count.ToString(CultureInfo.InvariantCulture));

				if (att.IsRequired) {
					requiredAliases.Add(alias);
				} else {
					optionalAliases.Add(alias);
				}
			}

			// Set optional/required lists
			this.OptionalAliases = optionalAliases.Count > 0 ? string.Join(",", optionalAliases.ToArray()) : null;
			this.RequiredAliases = requiredAliases.Count > 0 ? string.Join(",", requiredAliases.ToArray()) : null;
		}

		/// <summary>
		/// Called when the message has been received,
		/// after it passes through the channel binding elements.
		/// </summary>
		void IMessageWithEvents.OnReceiving() {
			var extraData = ((IMessage)this).ExtraData;
			var requiredAliases = ParseAliasList(this.RequiredAliases);
			var optionalAliases = ParseAliasList(this.OptionalAliases);

			// if an alias shows up in both lists, an exception will result implicitly.
			var allAliases = new List<string>(requiredAliases.Count + optionalAliases.Count);
			allAliases.AddRange(requiredAliases);
			allAliases.AddRange(optionalAliases);
			if (allAliases.Count == 0) {
				Logger.OpenId.Error("Attribute Exchange extension did not provide any aliases in the if_available or required lists.");
				return;
			}

			AliasManager aliasManager = new AliasManager();
			foreach (var alias in allAliases) {
				string attributeTypeUri;
				if (extraData.TryGetValue("type." + alias, out attributeTypeUri)) {
					aliasManager.SetAlias(alias, attributeTypeUri);
					AttributeRequest att = new AttributeRequest {
						TypeUri = attributeTypeUri,
						IsRequired = requiredAliases.Contains(alias),
					};
					string countString;
					if (extraData.TryGetValue("count." + alias, out countString)) {
						if (countString == "unlimited") {
							att.Count = int.MaxValue;
						} else {
							int count;
							if (int.TryParse(countString, out count) && count > 0) {
								att.Count = count;
							} else {
								Logger.OpenId.Error("count." + alias + " could not be parsed into a positive integer.");
							}
						}
					} else {
						att.Count = 1;
					}
					this.Attributes.Add(att);
				} else {
					Logger.OpenId.Error("Type URI definition of alias " + alias + " is missing.");
				}
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
				Logger.OpenId.ErrorFormat("The AX fetch request update_url parameter was not absolute ('{0}').  Ignoring value.", this.UpdateUrl);
			}

			if (this.OptionalAliases != null) {
				if (this.OptionalAliases.IndexOfAny(IllegalAliasListCharacters) >= 0) {
					Logger.OpenId.Error("Illegal characters found in Attribute Exchange if_available alias list.  Ignoring value.");
					this.OptionalAliases = null;
				}
			}

			if (this.RequiredAliases != null) {
				if (this.RequiredAliases.IndexOfAny(IllegalAliasListCharacters) >= 0) {
					Logger.OpenId.Error("Illegal characters found in Attribute Exchange required alias list.  Ignoring value.");
					this.RequiredAliases = null;
				}
			}
		}

		/// <summary>
		/// Splits a list of aliases by their commas.
		/// </summary>
		/// <param name="aliasList">The comma-delimited list of aliases.  May be null or empty.</param>
		/// <returns>The list of aliases.  Never null, but may be empty.</returns>
		private static IList<string> ParseAliasList(string aliasList) {
			if (string.IsNullOrEmpty(aliasList)) {
				return EmptyList<string>.Instance;
			}

			return aliasList.Split(',');
		}
	}
}
