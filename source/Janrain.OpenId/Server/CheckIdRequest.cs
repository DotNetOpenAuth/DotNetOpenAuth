using System;
using System.Collections;
using System.Collections.Specialized;
using System.Text;
using Janrain.OpenId.RegistrationExtension;

namespace Janrain.OpenId.Server
{

    public class CheckIdRequest : AssociatedRequest
    {

        #region Private Members

        private bool _immediate;
        private string _trust_root;
        private Uri _identity;
        private string _mode;
        private Uri _return_to;
        private Uri _policyUrl;

        private ProfileRequest requestNicknameDefault = ProfileRequest.NoRequest;
        private ProfileRequest requestEmailDefault = ProfileRequest.NoRequest;
        private ProfileRequest requestFullNameDefault = ProfileRequest.NoRequest;
        private ProfileRequest requestBirthdateDefault = ProfileRequest.NoRequest;
        private ProfileRequest requestGenderDefault = ProfileRequest.NoRequest;
        private ProfileRequest requestPostalCodeDefault = ProfileRequest.NoRequest;
        private ProfileRequest requestCountryDefault = ProfileRequest.NoRequest;
        private ProfileRequest requestLanguageDefault = ProfileRequest.NoRequest;
        private ProfileRequest requestTimeZoneDefault = ProfileRequest.NoRequest;

        #endregion

        #region Constructor(s)

        public CheckIdRequest(Uri identity, Uri return_to, string trust_root, bool immediate, string assoc_handle)
        {
            this.AssocHandle = assoc_handle;

            _identity = identity;
            _return_to = return_to;

            if (trust_root == null)
                _trust_root = return_to.AbsoluteUri;
            else
                _trust_root = trust_root;

            _immediate = immediate;
            if (_immediate)
                _mode = "checkid_immediate";
            else
                _mode = "checkid_setup";

            try
            {
                TrustRoot trustRoot = new TrustRoot(_return_to.AbsolutePath);
            }
            catch (ArgumentException)
            {
                throw new MalformedReturnUrl(null, _return_to.AbsoluteUri);
            }

            if (!this.TrustRootValid)
                throw new UntrustedReturnUrl(null, _return_to, _trust_root);

        }

        public CheckIdRequest(NameValueCollection query)
        {
            // handle the mandatory protocol fields
            string mode = query[QueryStringArgs.OpenIdMode];

            if (mode == "checkid_immediate")
            {
                _immediate = true;
                _mode = "checkid_immediate";
            }
            else
            {
                _immediate = false;
                _mode = "checkid_setup";
            }

            string identity = GetField(query, QueryStringArgs.OpenIdIdentity);
            try
            {
                _identity = new Uri(identity);
            }
            catch (UriFormatException)
            {
                throw new ProtocolException(query, "openid.identity not a valid url: " + identity);
            }

            string return_to = GetField(query, QueryStringArgs.OpenIdReturnTo);
            try
            {
                _return_to = new Uri(return_to);
            }
            catch (UriFormatException)
            {
                throw new MalformedReturnUrl(query, return_to);
            }

            // TODO This just seems wonky to me
            _trust_root = query.Get("openid.trust_root");
            if (_trust_root == null)
                _trust_root = _return_to.AbsoluteUri;

            this.AssocHandle = query.Get("openid.assoc_handle");

            try
            {
                TrustRoot tr = new TrustRoot(_return_to.AbsoluteUri);
            }
            catch (ArgumentException)
            {
                throw new MalformedReturnUrl(query, _return_to.AbsoluteUri);
            }

            if (!TrustRootValid)
                throw new UntrustedReturnUrl(query, _return_to, _trust_root);


            // Handle the optional Simple Registration extension fields
            string policyUrl = GetSimpleRegistrationExtensionField(query, QueryStringArgs.OpenIdSregPolicyUrl);
            if (!String.IsNullOrEmpty(policyUrl))
            {
                _policyUrl = new Uri(policyUrl);
            }

            string optionalFields = GetSimpleRegistrationExtensionField(query, QueryStringArgs.OpenIdSregOptional);
            if (!String.IsNullOrEmpty(optionalFields))
            {
                string[] splitOptionalFields = optionalFields.Split(',');
                setSimpleRegistrationExtensionFields(splitOptionalFields, ProfileRequest.Request);
            }

            string requiredFields = GetSimpleRegistrationExtensionField(query, QueryStringArgs.OpenIdSregRequired);
            if (!String.IsNullOrEmpty(requiredFields))
            {
                string[] splitRrequiredFields = requiredFields.Split(',');
                setSimpleRegistrationExtensionFields(splitRrequiredFields, ProfileRequest.Require);
            }
        }

        #endregion

        #region Private Methods

        private string GetField(NameValueCollection query, string field)
        {
            string value = query.Get(field);

            if (value == null)
                throw new ProtocolException(query, "Missing required field " + field);

            return value;
        }

        private string GetSimpleRegistrationExtensionField(NameValueCollection query, string field)
        {
            string value = query.Get(field);
            return value;
        }


        private void setSimpleRegistrationExtensionFields(string[] fields, ProfileRequest request)
        {
            foreach (string field in fields)
            {
                switch (field)
                {
                    case "nickname":
                        {
                            this.requestNicknameDefault = request;
                            break;
                        }
                    case "email":
                        {
                            this.requestEmailDefault = request;
                            break;
                        }
                    case "fullname":
                        {
                            this.requestFullNameDefault = request;
                            break;
                        }
                    case "dob":
                        {
                            this.requestBirthdateDefault = request;
                            break;
                        }
                    case "gender":
                        {
                            this.requestGenderDefault = request;
                            break;
                        }
                    case "postcode":
                        {
                            this.requestPostalCodeDefault = request;
                            break;
                        }
                    case "country":
                        {
                            this.requestCountryDefault = request;
                            break;
                        }
                    case "language":
                        {
                            this.requestLanguageDefault = request;
                            break;
                        }
                    case "timezone":
                        {
                            this.requestTimeZoneDefault = request;
                            break;
                        }
                }
            }
        }

        #endregion

        #region Public Methods

        public Response Answer(bool allow, Uri server_url)
        {
            return Answer(allow, server_url, null);
        }

        public Response Answer(bool allow, Uri server_url, OpenIdProfileFields openIdProfileFields)
        {
            string mode;

            if (allow || _immediate)
                mode = "id_res";
            else
                mode = "cancel";

            #region  Trace
            if (TraceUtil.Switch.TraceInfo)
            {
                TraceUtil.ServerTrace("Start processing Response for CheckIdRequest");
                if (TraceUtil.Switch.TraceVerbose)
                {
                    TraceUtil.ServerTrace(String.Format("mode = '{0}',  server_url = '{1}", mode, server_url.ToString()));
                    if (openIdProfileFields != null)
                    {
                        TraceUtil.ServerTrace("Simple registration fields follow: ");
                        TraceUtil.ServerTrace(openIdProfileFields);                        
                    }
                    else
                    {
                        TraceUtil.ServerTrace("No simple registration fields have been supplied.");
                    }

                }                    
            }
        
            #endregion        

            Response response = new Response(this);

            if (allow)
            {
                Hashtable fields = new Hashtable();

                fields.Add("mode", mode);
                fields.Add("identity", _identity.AbsoluteUri);
                fields.Add("return_to", _return_to.AbsoluteUri);

                if (openIdProfileFields != null)
                {
                    if (openIdProfileFields.Birthdate != null)
                    {
                        fields.Add("sreg.dob", openIdProfileFields.Birthdate.ToString());
                    }
                    if (!String.IsNullOrEmpty(openIdProfileFields.Country))
                    {
                        fields.Add("sreg.country", openIdProfileFields.Country);
                    }
                    if (openIdProfileFields.Email != null)
                    {                        
                        fields.Add("sreg.email", openIdProfileFields.Email.ToString());
                    }
                    if ((!String.IsNullOrEmpty(openIdProfileFields.Fullname)))
                    {
                        fields.Add("sreg.fullname", openIdProfileFields.Fullname);
                    }

                    if (openIdProfileFields.Gender != null)
                    {
                        if (openIdProfileFields.Gender == Gender.Female)
                        {
                            fields.Add("sreg.gender", "F");
                        }
                        else
                        {
                            fields.Add("sreg.gender", "M");
                        }

                    }

                    if (!String.IsNullOrEmpty(openIdProfileFields.Language))
                    {
                        fields.Add("sreg.language", openIdProfileFields.Language);
                    }

                    if (!String.IsNullOrEmpty(openIdProfileFields.Nickname))
                    {
                        fields.Add("sreg.nickname", openIdProfileFields.Nickname);
                    }

                    if (!String.IsNullOrEmpty(openIdProfileFields.PostalCode))
                    {
                        fields.Add("sreg.postcode", openIdProfileFields.PostalCode);
                    }

                    if (!String.IsNullOrEmpty(openIdProfileFields.TimeZone))
                    {
                        fields.Add("sreg.timezone", openIdProfileFields.TimeZone);
                    }

                }

                response.AddFields(null, fields, true);

            }
            response.AddField(null, "mode", mode, false);
            if (_immediate)
            {
                if (server_url == null) { throw new ApplicationException("setup_url is required for allow=False in immediate mode."); }

                CheckIdRequest setup_request = new CheckIdRequest(_identity, _return_to, _trust_root, false, this.AssocHandle);

                Uri setup_url = setup_request.EncodeToUrl(server_url);

                response.AddField(null, "user_setup_url", setup_url.AbsoluteUri, false);
            }

            #region  Trace
            if (TraceUtil.Switch.TraceInfo)
            {
                TraceUtil.ServerTrace("CheckIdRequest response successfully created. ");
                if (TraceUtil.Switch.TraceVerbose)
                {
                    TraceUtil.ServerTrace("Response follows. ");
                    TraceUtil.ServerTrace(response.ToString());
                }                
            }

            #endregion                     

            return response;
        }

        public Uri EncodeToUrl(Uri server_url)
        {
            NameValueCollection q = new NameValueCollection();

            q.Add("openid.mode", _mode);
            q.Add("openid.identity", _identity.AbsoluteUri);
            q.Add("openid.return_to", _return_to.AbsoluteUri);

            if (_trust_root != null)
                q.Add("openid.trust_root", _trust_root);

            if (this.AssocHandle != null)
                q.Add("openid.assoc_handle", this.AssocHandle);

            UriBuilder builder = new UriBuilder(server_url);
            UriUtil.AppendQueryArgs(builder, q);

            return new Uri(builder.ToString());
        }

        public Uri GetCancelUrl()
        {
            if (_immediate)
                throw new ApplicationException("Cancel is not an appropriate response to immediate mode requests.");

            UriBuilder builder = new UriBuilder(_return_to);
            NameValueCollection args = new NameValueCollection();

            args.Add("openid.mode", "cancel");
            UriUtil.AppendQueryArgs(builder, args);

            return new Uri(builder.ToString());
        }

        public bool IsAnySimpleRegistrationFieldsRequestedOrRequired
        {
            get
            {
                return (!(this.requestBirthdateDefault == ProfileRequest.NoRequest
                          && this.requestCountryDefault == ProfileRequest.NoRequest
                          && this.requestEmailDefault == ProfileRequest.NoRequest
                          && this.requestFullNameDefault == ProfileRequest.NoRequest
                          && this.requestGenderDefault == ProfileRequest.NoRequest
                          && this.requestLanguageDefault == ProfileRequest.NoRequest
                          && this.requestNicknameDefault == ProfileRequest.NoRequest
                          && this.requestPostalCodeDefault == ProfileRequest.NoRequest
                          && this.requestTimeZoneDefault == ProfileRequest.NoRequest));
            }
        }

        public bool TrustRootValid
        {
            get
            {
                // TODO this doesn't seem right to me
                if (_trust_root == null)
                    return true;

                TrustRoot tr = new TrustRoot(_trust_root);
                if (tr == null)
                    throw new MalformedTrustRoot(null, _trust_root);

                // TODO (willem.muller) - The trust code on 04/04/07 is dodgy. So returing true so all trust roots are valid for now.
                return true;
                
                //return tr.ValidateUrl(_return_to);
            }
        }

        #endregion

        #region Properties

        public bool Immediate
        {
            get { return _immediate; }
        }

        public string TrustRoot
        {
            get { return _trust_root; }
        }

        public Uri IdentityUrl
        {
            get { return _identity; }
        }

        public Uri ReturnTo
        {
            get { return _return_to; }
        }

        public ProfileRequest RequestNicknameDefault
        {
            get { return requestNicknameDefault; }
        }

        public ProfileRequest RequestEmailDefault
        {
            get { return requestEmailDefault; }
        }

        public ProfileRequest RequestFullNameDefault
        {
            get { return requestFullNameDefault; }
        }

        public ProfileRequest RequestBirthdateDefault
        {
            get { return requestBirthdateDefault; }
        }

        public ProfileRequest RequestGenderDefault
        {
            get { return requestGenderDefault; }
        }

        public ProfileRequest RequestPostalCodeDefault
        {
            get { return requestPostalCodeDefault; }
        }

        public ProfileRequest RequestCountryDefault
        {
            get { return requestCountryDefault; }
        }

        public ProfileRequest RequestLanguageDefault
        {
            get { return requestLanguageDefault; }
        }

        public ProfileRequest RequestTimeZoneDefault
        {
            get { return requestTimeZoneDefault; }
        }

        #endregion

        #region Inherited Properties

        public override string Mode
        {
            get { return _mode; }
        }

        public Uri PolicyUrl
        {
            get { return _policyUrl; }
        }

        #endregion

        public override string ToString()
        {
            string returnString = @"CheckIdRequest._immediate = '{0}'
CheckIdRequest._trust_root = '{1}'
CheckIdRequest._identity = '{2}' 
CheckIdRequest._mode = '{3}' 
CheckIdRequest._return_to = '{4}' 
CheckIdRequest._policyUrl = '{5}' 
CheckIdRequest.requestNicknameDefault = '{6}' 
CheckIdRequest.requestEmailDefault = '{7}' 
CheckIdRequest.requestFullNameDefault = '{8}' 
CheckIdRequest.requestBirthdateDefault = '{9}'                         
CheckIdRequest.requestGenderDefault = '{10}'                         
CheckIdRequest.requestPostalCodeDefault = '{11}'                         
CheckIdRequest.requestCountryDefault = '{12}'                         
CheckIdRequest.requestLanguageDefault = '{13}'                         
CheckIdRequest.requestTimeZoneDefault = '{14}'";

            return base.ToString() + Environment.NewLine + String.Format(returnString,
                                                                         _immediate,
                                                                         _trust_root,
                                                                         _identity,
                                                                         _mode,
                                                                         _return_to,
                                                                         _policyUrl,
                                                                         requestNicknameDefault,
                                                                         requestEmailDefault,
                                                                         requestFullNameDefault,
                                                                         requestBirthdateDefault,
                                                                         requestGenderDefault,
                                                                         requestPostalCodeDefault,
                                                                         requestCountryDefault,
                                                                         requestLanguageDefault,
                                                                         requestTimeZoneDefault);


        }

    }
}
