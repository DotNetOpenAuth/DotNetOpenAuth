using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Web;

namespace DotNetOpenId
{
    public class CustomTraceStream : MemoryStream
    {
        // Private members
        private Stream m_outputStream;
        
        private string savedPageOutput;

        // Ctor
        public CustomTraceStream(Stream outputStream)
        {
            m_outputStream = outputStream;
        }

        public string SavedPageOutput
        {
            get { return savedPageOutput; }
        }

        public void ClearSavedPageOutput()
        {
            savedPageOutput = "";
        }


        // Write method does it
        public override void Write(byte[] buffer, int offset, int count)
        {
            // Gets a string out of bytes
            Encoding enc = HttpContext.Current.Response.ContentEncoding;
            string theText = enc.GetString(buffer, offset, count);
            
            // save to our local string
            savedPageOutput = SavedPageOutput + theText;;

            // Write to the original stream
            byte[] data = enc.GetBytes(theText);
            m_outputStream.Write(data, 0, theText.Length);
        }
    }
}
