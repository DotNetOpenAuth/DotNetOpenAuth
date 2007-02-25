using System;
using System.Collections.Generic;
using System.Text;

namespace Janrain.OpenId.Server
{

    public abstract class Request
    {
        
        public abstract string Mode { get; }

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

    }
}
