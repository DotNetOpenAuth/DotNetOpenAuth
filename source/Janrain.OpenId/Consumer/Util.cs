using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Janrain.OpenId.Consumer
{
    public class Util
    {
        public static byte[] ReadAndClose(Stream stream)
        {
            StreamReader sr = new StreamReader(stream, Encoding.UTF8);
            List<byte> bytes = new List<byte>();
            int byteValue = 0;
            bool keepReading = true;
            while (keepReading)
            {
                try
                {
                    byteValue = sr.Read();
                    if (byteValue == -1)
                    {
                        keepReading = false;
                    }
                    else
                    {
                        bytes.Add(Convert.ToByte(byteValue));
                    }
                   
                }
                catch (Exception)
                {
                    keepReading = false;
                }
            }
            stream.Close();
            byte[] allbytes = bytes.ToArray();
            return allbytes;
        }            
    }
}
