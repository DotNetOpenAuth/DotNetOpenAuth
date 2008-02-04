using System;
using System.Collections;
using System.Collections.Specialized;
using System.Text;
using System.Security.Cryptography;
using Janrain.OpenId.Store;

namespace Janrain.OpenId.Server
{
    public class Signatory
    {
        public static readonly TimeSpan SECRET_LIFETIME = new TimeSpan(0, 0, 14 * 24 * 60 * 60);

        #region Private Members

        private static readonly Uri _normal_key = new Uri("http://localhost/|normal");
        private static readonly Uri _dumb_key = new Uri("http://localhost/|dumb");
        private IAssociationStore _store;

        #endregion

        #region Constructor(s)

        public Signatory(IAssociationStore store)
        {           
            if (store == null)
                throw new ArgumentNullException("store");

            _store = store;
        }

        #endregion

        #region Methods

        public void Sign(Response response)
        {
            #region  Trace
            if (TraceUtil.Switch.TraceInfo)
            {
                TraceUtil.ServerTrace(String.Format("Digitally sign the response."));
            }
            #endregion        	
            
            NameValueCollection nvc = new NameValueCollection();
            Association assoc;
            string assoc_handle = ((AssociatedRequest)response.Request).AssocHandle;

            if (assoc_handle != null && assoc_handle != "")
            {
                assoc = this.GetAssociation(assoc_handle, false);

                if (assoc == null)
                {
                    #region  Trace
                    if (TraceUtil.Switch.TraceInfo)
                    {
                        TraceUtil.ServerTrace(String.Format("No assocaiton found with assoc_handle. Setting invalidate_handle and creating new Association."));
                    }
                    #endregion      
                    
                    response.Fields["invalidate_handle"] = assoc_handle;
                    assoc = this.CreateAssociation(true);
                }
                else
                {
                    #region  Trace
                    if (TraceUtil.Switch.TraceInfo)
                    {
                        TraceUtil.ServerTrace(String.Format("No association found."));
                    }
                    #endregion                          
                }
            }
            else
            {
                assoc = this.CreateAssociation(true);
                TraceUtil.ServerTrace(String.Format("No assoc_handle supplied. Creating new assocation."));
            }

            response.Fields[QueryStringArgs.openidnp.assoc_handle] = assoc.Handle;

            foreach (DictionaryEntry pair in response.Fields)
            {
                nvc.Add(pair.Key.ToString(), pair.Value.ToString());
            }

            string sig = assoc.SignDict(response.Signed, nvc, "");
            string signed = String.Join(",", response.Signed);

            response.Fields[QueryStringArgs.openidnp.sig] = sig;
            response.Fields[QueryStringArgs.openidnp.signed] = signed;

            #region  Trace
            if (TraceUtil.Switch.TraceInfo)
            {
                TraceUtil.ServerTrace(String.Format("Digital signature successfully created"));
            }
            #endregion        	            

        }

        public virtual bool Verify(string assoc_handle, string sig, NameValueCollection signed_pairs)
        {
            #region  Trace
            if (TraceUtil.Switch.TraceInfo)
            {
                TraceUtil.ServerTrace(String.Format("Start signature verification for assoc_handle = '{0}'", assoc_handle));
            }
            #endregion        	    
            
            Association assoc = this.GetAssociation(assoc_handle, true);
            
            string expected_sig;

            if (assoc == null)
            {
                #region  Trace
                if (TraceUtil.Switch.TraceInfo)
                {
                    TraceUtil.ServerTrace("End signature verification. Signature verification failed. No matching association handle found ");
                }
                #endregion                      
                
                return false;
            }
            else
            {                
                #region  Trace
                if (TraceUtil.Switch.TraceInfo)
                {
                    TraceUtil.ServerTrace("Found matching association handle. ");
                }
                if (TraceUtil.Switch.TraceVerbose)
                {
                    TraceUtil.ServerTrace(assoc.ToString());
                }
                
                #endregion                       
            }

            #region  Trace
            if (TraceUtil.Switch.TraceInfo)
            {
                TraceUtil.ServerTrace("Matching association found ");
            }
            #endregion                    

            expected_sig = CryptUtil.ToBase64String(assoc.Sign(signed_pairs));

            #region  Trace
            if (TraceUtil.Switch.TraceInfo)
            {
                TraceUtil.ServerTrace(String.Format("Expected signature is '{0}'. Actual signature is '{1}' ", expected_sig, sig));
                
                if (sig == expected_sig)
                {
                    TraceUtil.ServerTrace("End signature verification. Signature verification passed");    
                }
                else
                {
                    TraceUtil.ServerTrace("End signature verification. Signature verification failed");    
                }
            }
            #endregion                    

            return (sig == expected_sig);
        }

        public virtual Association CreateAssociation(bool dumb)
        {
            #region  Trace
            if (TraceUtil.Switch.TraceInfo)
            {
                TraceUtil.ServerTrace(String.Format("Start Create Association. InDumbMode = {0}", dumb));
            }
            #endregion
            
            RNGCryptoServiceProvider generator = new RNGCryptoServiceProvider();
            Uri key;
            byte[] secret = new byte[20];
            byte[] uniq_bytes = new byte[4];
            string uniq;
            double seconds;
            string handle;
            Association assoc;


            generator.GetBytes(secret);
            generator.GetBytes(uniq_bytes);

            uniq = CryptUtil.ToBase64String(uniq_bytes);

            TimeSpan time = DateTime.UtcNow.Subtract(Association.UNIX_EPOCH);
            seconds = time.TotalSeconds;

            handle = "{{HMAC-SHA1}{" + seconds + "}{" + uniq + "}";

            assoc = new HMACSHA1Association(handle, secret, SECRET_LIFETIME);

            if (dumb)
                key = _dumb_key;
            else
                key = _normal_key;

            _store.StoreAssociation(key, assoc);

            #region  Trace
            if (TraceUtil.Switch.TraceInfo)
            {                
                TraceUtil.ServerTrace(String.Format("End Create Association. Association successfully created. key = '{0}', handle = '{1}' ", key, handle));
            }
            #endregion            

            return assoc;
        }

        public virtual Association GetAssociation(string assoc_handle, bool dumb)
        {
            #region  Trace
            if (TraceUtil.Switch.TraceInfo)
            {
                TraceUtil.ServerTrace(String.Format("Start get association from store '{0}'.", assoc_handle));
            }
            #endregion     
            
            Uri key;

            if (assoc_handle == null)
                throw new ArgumentNullException(QueryStringArgs.openidnp.assoc_handle);

            if (dumb)
                key = _dumb_key;
            else
                key = _normal_key;

            Association assoc = _store.GetAssociation(key, assoc_handle);
            if (assoc != null && assoc.ExpiresIn <= 0)
            {
                TraceUtil.ServerTrace("Association expired or not in store. Trying to remove association if it still exists.");
                _store.RemoveAssociation(key, assoc_handle);
                assoc = null;
            }

            #region  Trace
            if (TraceUtil.Switch.TraceInfo)
            {
                TraceUtil.ServerTrace(String.Format("End get association from store '{0}'. Association found? =  {1}", assoc_handle, (assoc != null).ToString().ToUpper()));
            }
            #endregion                 
            
            return assoc;
        }

        public virtual void Invalidate(string assoc_handle, bool dumb)
        {
            #region  Trace
            if (TraceUtil.Switch.TraceInfo)
            {
                TraceUtil.ServerTrace(String.Format("Start invalidate association '{0}'.", assoc_handle));
            }
            #endregion        	    
            
            Uri key;


            if (dumb)
                key = _dumb_key;
            else
                key = _normal_key;

            _store.RemoveAssociation(key, assoc_handle);

            #region  Trace
            if (TraceUtil.Switch.TraceInfo)
            {                
                TraceUtil.ServerTrace(String.Format("End invalidate association '{0}'.", assoc_handle));
            }
            #endregion        	            
        }

        #endregion

    }
}
