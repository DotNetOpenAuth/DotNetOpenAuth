using System;
using System.Collections.Generic;
using System.Text;

namespace DotNetOpenId.Server
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
}
