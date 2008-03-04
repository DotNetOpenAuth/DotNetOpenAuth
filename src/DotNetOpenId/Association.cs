using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Security.Cryptography;
using System.IO;
using System.Text;
using System.Globalization;

namespace DotNetOpenId {
	public abstract class Association {
		protected Association(string handle, byte[] secretKey, TimeSpan totalLifeLength, DateTime issued) {
			Handle = handle;
			SecretKey = secretKey;
			TotalLifeLength = totalLifeLength;
			Issued = cutToSecond(issued);
		}

		/// <summary>
		/// Represents January 1, 1970 12 AM.
		/// </summary>
		protected internal readonly static DateTime UnixEpoch = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
		/// <summary>
		/// A unique handle by which this <see cref="Association"/> may be stored or retrieved.
		/// </summary>
		internal string Handle { get; set; }
		/// <summary>
		/// Gets the time that this <see cref="Association"/> was first created
		/// and the <see cref="SecretKey"/> issued.
		/// </summary>
		internal DateTime Issued { get; set; }
		/// <summary>
		/// The lifetime the OpenID provider permits this <see cref="Association"/>.
		/// </summary>
		protected TimeSpan TotalLifeLength { get; private set; }

		/// <summary>
		/// The shared secret key between the consumer and provider.
		/// </summary>
		protected internal byte[] SecretKey { get; private set; }

		/// <summary>
		/// Gets the time when this <see cref="Association"/> will expire.
		/// </summary>
		public DateTime Expires {
			get { return Issued + TotalLifeLength; }
		}

		/// <summary>
		/// Gets whether this <see cref="Association"/> has already expired.
		/// </summary>
		public bool IsExpired {
			get { return Expires < DateTime.UtcNow; }
		}

		/// <summary>
		/// The number of seconds until this <see cref="Association"/> expires.
		/// Never negative (counter runs to zero).
		/// </summary>
		protected internal long SecondsTillExpiration {
			get { return Math.Max(0, (long)(Expires - DateTime.UtcNow).TotalSeconds); }
		}

		/// <summary>
		/// The string to pass as the assoc_type value in the OpenID protocol.
		/// </summary>
		protected internal abstract string AssociationType { get; }

		/// <summary>
		/// Signs certain given key/value pairs in a supplied dictionary.
		/// </summary>
		/// <param name="data">
		/// A dictionary with key/value pairs, at least some of which you want to include in the signature.
		/// </param>
		/// <param name="keysToSign">
		/// A list of the keys in the supplied dictionary you wish to sign.
		/// </param>
		/// <param name="keyLookupPrefix">
		/// An optional prefix to use in front of a given name in <paramref name="fields"/>
		/// when looking up the value from <paramref name="data"/>.
		/// </param>
		/// <returns>The signature of the key-value pairs.</returns>
		internal byte[] Sign(IDictionary<string, string> data, IList<string> keysToSign, string keyLookupPrefix) {
			var nvc = new Dictionary<string, string>();

			foreach (string field in keysToSign) {
				nvc.Add(field, data[keyLookupPrefix + field]);
			}

			return Sign(nvc, keysToSign);
		}
		/// <summary>
		/// Generates a signature from a given dictionary.
		/// </summary>
		/// <param name="data">The dictionary.  This dictionary will not be changed.</param>
		/// <returns>The calculated signature of the data in the dictionary.</returns>
		protected internal abstract byte[] Sign(IDictionary<string, string> data, IList<string> keyOrder);
		/// <summary>
		/// Rounds the given <see cref="DateTime"/> downward to the whole second.
		/// </summary>
		static DateTime cutToSecond(DateTime dateTime) {
			return new DateTime(dateTime.Ticks - (dateTime.Ticks % TimeSpan.TicksPerSecond));
		}

		public override bool Equals(object obj) {
			Association a = obj as Association;
			if (a == null) return false;
			if (a.GetType() != GetType()) return false;

			if (a.Handle != this.Handle ||
				a.Issued != this.Issued ||
				a.TotalLifeLength != this.TotalLifeLength)
				return false;

			if (!Util.ArrayEquals(a.SecretKey, this.SecretKey))
				return false;

			return true;
		}
		public override int GetHashCode() {
			HMACSHA1 hmac = new HMACSHA1(SecretKey);
			CryptoStream cs = new CryptoStream(Stream.Null, hmac, CryptoStreamMode.Write);

			byte[] hbytes = ASCIIEncoding.ASCII.GetBytes(this.Handle);

			cs.Write(hbytes, 0, hbytes.Length);
			cs.Close();

			byte[] hash = hmac.Hash;
			hmac.Clear();

			long val = 0;
			for (int i = 0; i < hash.Length; i++) {
				val = val ^ (long)hash[i];
			}

			val = val ^ this.Expires.ToFileTimeUtc();

			return (int)val;
		}
		public override string ToString() {
			string returnString = @"Association.Handle= '{0}'
Association.Issued = '{1}'
Association.Secret = '{2}' 
Association.Expires = '{3}' 
Association.IsExpired = '{4}' 
Association.ExpiresIn = '{5}' ";
			return String.Format(CultureInfo.CurrentUICulture, 
				returnString, Handle, Issued, SecretKey,
				Expires, IsExpired, SecondsTillExpiration);
		}
	}
}
