using System;
using System.Collections;
using System.Collections.Specialized;
using System.Text;
using System.Security.Cryptography;
using DotNetOpenId.Store;
using System.Collections.Generic;

namespace DotNetOpenId.Provider {
	/// <summary>
	/// Signs things.
	/// </summary>
	internal class Signatory {
		static readonly TimeSpan associationLifetime = TimeSpan.FromDays(14);

		static readonly Uri _normal_key = new Uri("http://localhost/|normal");
		static readonly Uri _dumb_key = new Uri("http://localhost/|dumb");
		IAssociationStore store;

		public Signatory(IAssociationStore store) {
			if (store == null)
				throw new ArgumentNullException("store");

			this.store = store;
		}

		public void Sign(Response response) {
			#region  Trace
			if (TraceUtil.Switch.TraceInfo) {
				TraceUtil.ServerTrace(String.Format("Digitally sign the response."));
			}
			#endregion

			Association assoc;
			string assoc_handle = ((AssociatedRequest)response.Request).AssociationHandle;

			if (!string.IsNullOrEmpty(assoc_handle)) {
				assoc = this.GetAssociation(assoc_handle, false);

				if (assoc == null) {
					#region  Trace
					if (TraceUtil.Switch.TraceInfo) {
						TraceUtil.ServerTrace(String.Format("No associaton found with assoc_handle. Setting invalidate_handle and creating new Association."));
					}
					#endregion

					response.Fields[QueryStringArgs.openidnp.invalidate_handle] = assoc_handle;
					assoc = this.CreateAssociation(true);
				} else {
					#region  Trace
					if (TraceUtil.Switch.TraceInfo) {
						TraceUtil.ServerTrace(String.Format("No association found."));
					}
					#endregion
				}
			} else {
				assoc = this.CreateAssociation(true);
				TraceUtil.ServerTrace(String.Format("No assoc_handle supplied. Creating new association."));
			}

			response.Fields[QueryStringArgs.openidnp.assoc_handle] = assoc.Handle;

			response.Fields[QueryStringArgs.openidnp.signed] = String.Join(",", response.Signed.ToArray());
			response.Fields[QueryStringArgs.openidnp.sig] =
				CryptUtil.ToBase64String(assoc.Sign(response.Fields, response.Signed, string.Empty));

			#region  Trace
			if (TraceUtil.Switch.TraceInfo) {
				TraceUtil.ServerTrace(String.Format("Digital signature successfully created"));
			}
			#endregion

		}

		public virtual bool Verify(string assoc_handle, string signature, IDictionary<string, string> signed_pairs) {
			#region  Trace
			if (TraceUtil.Switch.TraceInfo) {
				TraceUtil.ServerTrace(String.Format("Start signature verification for assoc_handle = '{0}'", assoc_handle));
			}
			#endregion

			Association assoc = this.GetAssociation(assoc_handle, true);

			string expected_sig;

			if (assoc == null) {
				#region  Trace
				if (TraceUtil.Switch.TraceInfo) {
					TraceUtil.ServerTrace("End signature verification. Signature verification failed. No matching association handle found ");
				}
				#endregion

				return false;
			} else {
				#region  Trace
				if (TraceUtil.Switch.TraceInfo) {
					TraceUtil.ServerTrace("Found matching association handle. ");
				}
				if (TraceUtil.Switch.TraceVerbose) {
					TraceUtil.ServerTrace(assoc.ToString());
				}

				#endregion
			}

			#region  Trace
			if (TraceUtil.Switch.TraceInfo) {
				TraceUtil.ServerTrace("Matching association found ");
			}
			#endregion

			expected_sig = CryptUtil.ToBase64String(assoc.Sign(signed_pairs));

			#region  Trace
			if (TraceUtil.Switch.TraceInfo) {
				TraceUtil.ServerTrace(String.Format("Expected signature is '{0}'. Actual signature is '{1}' ", expected_sig, signature));

				if (signature == expected_sig) {
					TraceUtil.ServerTrace("End signature verification. Signature verification passed");
				} else {
					TraceUtil.ServerTrace("End signature verification. Signature verification failed");
				}
			}
			#endregion

			return expected_sig.Equals(signature, StringComparison.OrdinalIgnoreCase);
		}

		public virtual Association CreateAssociation(bool dumb) {
			if (TraceUtil.Switch.TraceInfo) {
				TraceUtil.ServerTrace(String.Format("Start Create Association. InDumbMode = {0}", dumb));
			}

			RNGCryptoServiceProvider generator = new RNGCryptoServiceProvider();
			byte[] secret = new byte[20];
			byte[] uniq_bytes = new byte[4];
			string uniq;
			string handle;
			Association assoc;

			generator.GetBytes(secret);
			generator.GetBytes(uniq_bytes);

			uniq = CryptUtil.ToBase64String(uniq_bytes);

			double seconds = DateTime.UtcNow.Subtract(Association.UNIX_EPOCH).TotalSeconds;

			handle = "{{HMAC-SHA1}{" + seconds + "}{" + uniq + "}";

			assoc = new HmacSha1Association(handle, secret, associationLifetime);

			Uri key = dumb ? _dumb_key : _normal_key;
			store.StoreAssociation(key, assoc);

			if (TraceUtil.Switch.TraceInfo) {
				TraceUtil.ServerTrace(String.Format("End Create Association. Association successfully created. key = '{0}', handle = '{1}' ", key, handle));
			}

			return assoc;
		}

		public virtual Association GetAssociation(string assoc_handle, bool dumb) {
			if (TraceUtil.Switch.TraceInfo) {
				TraceUtil.ServerTrace(String.Format("Start get association from store '{0}'.", assoc_handle));
			}


			if (assoc_handle == null)
				throw new ArgumentNullException(QueryStringArgs.openidnp.assoc_handle);

			Uri key = dumb ? _dumb_key : _normal_key;
			Association assoc = store.GetAssociation(key, assoc_handle);
			if (assoc == null || assoc.IsExpired) {
				TraceUtil.ServerTrace("Association expired or not in store. Trying to remove association if it still exists.");
				store.RemoveAssociation(key, assoc_handle);
				assoc = null;
			}

			if (TraceUtil.Switch.TraceInfo) {
				TraceUtil.ServerTrace(String.Format("End get association from store '{0}'. Association found? =  {1}", assoc_handle, (assoc != null).ToString().ToUpper()));
			}

			return assoc;
		}

		public virtual void Invalidate(string assoc_handle, bool dumb) {
			if (TraceUtil.Switch.TraceInfo) {
				TraceUtil.ServerTrace(String.Format("Start invalidate association '{0}'.", assoc_handle));
			}

			Uri key = dumb ? _dumb_key : _normal_key;
			store.RemoveAssociation(key, assoc_handle);

			if (TraceUtil.Switch.TraceInfo) {
				TraceUtil.ServerTrace(String.Format("End invalidate association '{0}'.", assoc_handle));
			}
		}
	}
}
