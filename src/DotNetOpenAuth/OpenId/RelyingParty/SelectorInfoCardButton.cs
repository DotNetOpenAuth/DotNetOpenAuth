//-----------------------------------------------------------------------
// <copyright file="SelectorInfoCardButton.cs" company="Andrew Arnott">
//     Copyright (c) Andrew Arnott. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace DotNetOpenAuth.OpenId.RelyingParty {
	using System;
	using System.Collections.ObjectModel;
	using System.ComponentModel;
	using System.Diagnostics.Contracts;
	using System.Web.UI;
	using DotNetOpenAuth.InfoCard;

	/// <summary>
	/// A button that appears in the <see cref="OpenIdSelector"/> control that
	/// activates the Information Card selector on the browser, if one is available.
	/// </summary>
	[Serializable]
	public class SelectorInfoCardButton : SelectorButton {
		/// <summary>
		/// The backing field for the <see cref="InfoCardSelector"/> property.
		/// </summary>
		private InfoCardSelector infoCardSelector;

		/// <summary>
		/// Initializes a new instance of the <see cref="SelectorInfoCardButton"/> class.
		/// </summary>
		public SelectorInfoCardButton() {
		}

		/// <summary>
		/// Gets or sets the InfoCard selector which may be displayed alongside the OP buttons.
		/// </summary>
		[PersistenceMode(PersistenceMode.InnerProperty)]
		public InfoCardSelector InfoCardSelector {
			get {
				if (this.infoCardSelector == null) {
					this.infoCardSelector = new InfoCardSelector();
				}

				return this.infoCardSelector;
			}

			set {
				Contract.Requires<ArgumentNullException>(value != null);
				if (this.infoCardSelector != null) {
					Logger.Library.WarnFormat("{0}.InfoCardSelector property is being set multiple times.", GetType().Name);
				}

				this.infoCardSelector = value;
			}
		}

		/// <summary>
		/// Ensures that this button has been initialized to a valid state.
		/// </summary>
		internal override void EnsureValid() {
		}

		/// <summary>
		/// Renders the leading attributes for the LI tag.
		/// </summary>
		/// <param name="writer">The writer.</param>
		protected internal override void RenderLeadingAttributes(HtmlTextWriter writer) {
			writer.AddAttribute(HtmlTextWriterAttribute.Class, "infocard");
		}

		/// <summary>
		/// Renders the content of the button.
		/// </summary>
		/// <param name="writer">The writer.</param>
		/// <param name="selector">The containing selector control.</param>
		protected internal override void RenderButtonContent(HtmlTextWriter writer, OpenIdSelector selector) {
			this.InfoCardSelector.RenderControl(writer);
		}
	}
}
