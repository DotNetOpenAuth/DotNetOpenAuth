//-----------------------------------------------------------------------
// <copyright file="TextBoxTextWriter.cs" company="Outercurve Foundation">
//     Copyright (c) Outercurve Foundation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace DotNetOpenAuth.OpenIdOfflineProvider {
	using System;
	using System.IO;
	using System.Text;
	using System.Windows.Controls;
	using Validation;

	/// <summary>
	/// A text writer that appends all write calls to a text box.
	/// </summary>
	internal class TextBoxTextWriter : TextWriter {
		/// <summary>
		/// Initializes a new instance of the <see cref="TextBoxTextWriter"/> class.
		/// </summary>
		/// <param name="box">The text box to append log messages to.</param>
		internal TextBoxTextWriter(TextBox box) {
			Requires.NotNull(box, "box");
			this.Box = box;
		}

		/// <summary>
		/// Gets the <see cref="T:System.Text.Encoding"/> in which the output is written.
		/// </summary>
		/// <returns>
		/// The Encoding in which the output is written.
		/// </returns>
		public override Encoding Encoding {
			get { return Encoding.Unicode; }
		}

		/// <summary>
		/// Gets the box to append to.
		/// </summary>
		internal TextBox Box { get; private set; }

		/// <summary>
		/// Writes a character to the text stream.
		/// </summary>
		/// <param name="value">The character to write to the text stream.</param>
		/// <exception cref="T:System.ObjectDisposedException">
		/// The <see cref="T:System.IO.TextWriter"/> is closed.
		/// </exception>
		/// <exception cref="T:System.IO.IOException">
		/// An I/O error occurs.
		/// </exception>
		public override void Write(char value) {
			this.Box.Dispatcher.BeginInvoke((Action<string>)this.AppendText, value.ToString());
		}

		/// <summary>
		/// Writes a string to the text stream.
		/// </summary>
		/// <param name="value">The string to write.</param>
		/// <exception cref="T:System.ObjectDisposedException">
		/// The <see cref="T:System.IO.TextWriter"/> is closed.
		/// </exception>
		/// <exception cref="T:System.IO.IOException">
		/// An I/O error occurs.
		/// </exception>
		public override void Write(string value) {
			this.Box.Dispatcher.BeginInvoke((Action<string>)this.AppendText, value);
		}

		/// <summary>
		/// Appends text to the text box.
		/// </summary>
		/// <param name="value">The string to append.</param>
		private void AppendText(string value) {
			this.Box.AppendText(value);
			this.Box.ScrollToEnd();
		}
	}
}
