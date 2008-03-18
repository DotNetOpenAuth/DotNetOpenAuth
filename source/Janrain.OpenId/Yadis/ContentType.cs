using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Text;
using System.Text.RegularExpressions;

namespace Janrain.Yadis
{
    [Serializable]
    public class ContentType
    {
        protected NameValueCollection parameters;
        protected string subType;
        protected string type;

        public ContentType(string contentType)
        {
            string message = String.Format("\"{0}\" does not appear to be a valid content type", contentType);
            this.parameters = new NameValueCollection();
            const char SEMI = ';';
            const char SLASH = '/';
            const char EQUALS = '=';
            string[] parts = contentType.Split(new char[] { SEMI });
            try
            {
                string[] slashedArray = parts[0].Split(new char[] { SLASH });
                this.type = slashedArray[0];
                this.subType = slashedArray[1];
            }
            catch (IndexOutOfRangeException)
            {
                throw new ArgumentException(message);
            }
            this.type = this.type.Trim();
            this.subType = this.subType.Trim();

            for(int i=1; i<parts.Length; i++)
            {
				string param = parts[i];

                string k;
                string v;
                try
                {
                    string[] equalsArray = param.Split(new char[] { EQUALS });
                    k = equalsArray[0];
                    v = equalsArray[1];
                }
                catch (IndexOutOfRangeException)
                {
                    throw new ArgumentException(message);
                }

                this.parameters[k.Trim()] = v.Trim();
            }
        }

        public string MediaType
        {
            get
            {
                return String.Format("{0}/{1}",this.type,this.subType);
            }
        }

        public NameValueCollection Parameters
        {
            get
            {
                return this.parameters;
            }
        }

        public string SubType
        {
            get
            {
                return this.subType;
            }
            set
            {
                this.subType = value;
            }
        }

        public string Type
        {
            get
            {
                return this.type;
            }
            set
            {
                this.type = value;
            }
        }
    }
}
