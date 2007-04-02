using System;
using System.Collections.Generic;
using System.Text;

namespace Janrain.OpenId.Server
{

    public abstract class Request
    {
        
        public abstract string Mode { get; }

        public override string ToString()
        {
            string returnString = @"Request.Mode = {0}";
            return String.Format(returnString, Mode);
        }
    }

    // TODO Move this ABC out to it's own file
    public abstract class AssociatedRequest : Request
    {

        private string _assoc_handle;

        public string AssocHandle
        {
            get { return _assoc_handle; }
            set { _assoc_handle = value; }
        }

        public override string ToString()
        {
            string returnString ="AssociatedRequest.AssocHandle = {0}";
            return  base.ToString() + Environment.NewLine +  String.Format(returnString, AssocHandle);
        }        

    }
}
