using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Security.Cryptography;
using System.IO;
using System.Text;
using System.Globalization;
using System.Diagnostics.CodeAnalysis;
using System.Diagnostics;

namespace DotNetOpenId {
	/// <summary>
	/// Stores a secret used in signing and verifying messages.
	/// </summary>
	/// <remarks>
	/// OpenID associations may be shared between Provider and Relying Party (smart
	/// associations), or be a way for a Provider to recall its own secret for later
	/// (dumb associations).
	/// </remarks>
	[DebuggerDisplay("Handle = {Handle}, Expires = {Expires}")]
	public abstract class Association {
		/// <summary>
		/// Instantiates an <see cref="Association"/> object.
		/// </summary>
		protected Association(string handle, byte[] secret, TimeSpan totalLifeLength, DateTime issued) {
			if (string.IsNullOrEmpty(handle)) throw new ArgumentNullException("handle");
			if (secret == null) throw new ArgumentNullException("secret");
			Handle = handle;
			SecretKey = secret;
			TotalLifeLength = totalLifeLength;
			Issued = cutToSecond(issued);
		}
		/// <summary>
		/// Re-instantiates an <see cref="Association"/> previously persisted in a database or some
		/// other shared store.
		/// </summary>
		/// <param name="handle">
		/// The <see cref="Handle"/> property of the previous <see cref="Association"/> instance.
		/// </param>
		/// <param name="expires">
		/// The value of the <see cref="Expires"/> property of the previous <see cref="Association"/> instance.
		/// </param>
		/// <param name="privateData">
		/// The byte array returned by a call to <see cref="SerializePrivateData"/> on the previous
		/// <see cref="Association"/> instance.
		/// </param>
		/// <returns>
		/// The newly dehydrated <see cref="Association"/>, which can be returned
		/// from a custom association store's 
		/// <see cref="IAssociationStore&lt;TKey&gt;.GetAssociation(TKey)"/> method.
		/// </returns>
		public static Association Deserialize(string handle, DateTime expires, byte[] privateData) {
			if (string.IsNullOrEmpty(handle)) throw new ArgumentNullException("handle");
			if (privateData == null) throw new ArgumentNullException("privateData");
			expires = expires.ToUniversalTime();
			TimeSpan remainingLifeLength = expires - DateTime.UtcNow;
			byte[] secret = privateData; // the whole of privateData is the secret key for now.
			// We figure out what derived type to instantiate based on the length of the secret.
			if(secret.Length == CryptUtil.Sha1.HashSize / 8)
				return new HmacSha1Association(handle, secret, remainingLifeLength);
			if (secret.Length == CryptUtil.Sha256.HashSize / 8)
				return new HmacSha256Association(handle, secret, remainingLifeLength);
			throw new ArgumentException(Strings.BadAssociationPrivateData, "privateData");
		}

		static TimeSpan minimumUsefulAssociationLifetime {
			get { return Protocol.MaximumUserAgentAuthenticationTime; }
		}
		internal bool HasUsefulLifeRemaining {
			get { return timeTillExpiration >= minimumUsefulAssociationLifetime; }
		}

		/// <summary>
		/// Represents January 1, 1970 12 AM.
		/// </summary>
		protected internal readonly static DateTime UnixEpoch = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
		/// <summary>
		/// A unique handle by which this <see cref="Association"/> may be stored or retrieved.
		/// </summary>
		public string Handle { get; private set; }
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
		[SuppressMessage("Microsoft.Performance", "CA1819:PropertiesShouldNotReturnArrays")]
		protected internal byte[] SecretKey { get; private set; }

		/// <summary>
		/// Returns private data required to persist this <see cref="Association"/> in
		/// permanent storage (a shared database for example) for deserialization later.
		/// </summary>
		/// <returns>
		/// An opaque byte array that must be stored and returned exactly as it is provided here.
		/// The byte array may vary in length depending on the specific type of <see cref="Association"/>,
		/// but in current versions are no larger than 256 bytes.
		/// </returns>
		/// <remarks>
		/// Values of public properties on the base class <see cref="Association"/> are not included
		/// in this byte array, as they are useful for fast database lookup and are persisted separately.
		/// </remarks>
		public byte[] SerializePrivateData() {
			// We may want to encrypt this secret using the machine.config private key,
			// and add data regarding which Association derivative will need to be
			// re-instantiated on deserialization.
			// For now, we just send out the secret key.  We can derive the type from the length later.
			byte[] secretKeyCopy = new byte[SecretKey.Length];
			SecretKey.CopyTo(secretKeyCopy, 0);
			return secretKeyCopy;
		}

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
		/// Gets the TimeSpan till this association expires.
		/// </summary>
		TimeSpan timeTillExpiration {
			get { return Expires - DateTime.UtcNow; }
		}

		/// <summary>
		/// The number of seconds until this <see cref="Association"/> expires.
		/// Never negative (counter runs to zero).
		/// </summary>
		protected internal long SecondsTillExpiration {
			get { return Math.Max(0, (long)timeTillExpiration.TotalSeconds); }
		}

		/// <summary>
		/// The string to pass as the assoc_type value in the OpenID protocol.
		/// </summary>
		internal abstract string GetAssociationType(Protocol protocol);

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
		/// <param name="keyOrder">The order that the data in the dictionary must be encoded in for the signature to be valid.</param>
		/// <returns>The calculated signature of the data in the dictionary.</returns>
		protected internal byte[] Sign(IDictionary<string, string> data, IList<string> keyOrder) {
			using (HashAlgorithm hasher = CreateHasher()) {
				return hasher.ComputeHash(ProtocolMessages.KeyValueForm.GetBytes(data, keyOrder));
			}
		}
		/// <summary>
		/// Returns the specific hash algorithm used for message signing.
		/// </summary>
		protected abstract HashAlgorithm CreateHasher();

		/// <summary>
		/// Rounds the given <see cref="DateTime"/> downward to the whole second.
		/// </summary>
		static DateTime cutToSecond(DateTime dateTime) {
			return new DateTime(dateTime.Ticks - (dateTime.Ticks % TimeSpan.TicksPerSecond));
		}

		/// <summary>
		/// Tests equality of two <see cref="Association"/> objects.
		/// </summary>
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
		/// <summary>
		/// Returns the hash code.
		/// </summary>
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
	}
}
