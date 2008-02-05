using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Xml.Serialization;

namespace DotNetOpenId
{
    public class TraceUtil
    {
        
        private static TraceSwitch openIDTraceSwitch;
        private static TraceSwitch openIDPageOutputTraceSwitch;



        private static void Trace(string message, string category)
        {
            string messagePrefix = String.Format("[{0}] : ", DateTime.Now.ToString());
            char[] newLine = new char[2];
            newLine[0] = '\r';
            newLine[1] = '\n';
            string[] splitMessage = message.Split(newLine, StringSplitOptions.RemoveEmptyEntries);
            bool isMultiLineMessage = (splitMessage.Length > 1);
            
            if (isMultiLineMessage)
            {
                foreach (string line in splitMessage)
                {
                    System.Diagnostics.Trace.WriteLine(messagePrefix + line, category);
                }
            }
            else
            {
                System.Diagnostics.Trace.WriteLine(messagePrefix + message, category);
            }            
        }
        

        public static void ServerTrace(string message)
        {
            Trace(message, "OpenID Server");            
        }

        public static void ServerTrace(NameValueCollection collection)
        {            
            foreach (string key in collection.Keys)
            {
                ServerTrace(String.Format("{0} = '{1}'", key, collection[key]));                
            }            
        }

        public static void ServerTrace(object serializableObject)
        {
            if (serializableObject == null)
            {
                ServerTrace("<NULL>");
            }
            else
            {
                ServerTrace(SerializeToString(serializableObject));
            }
            
        }





        public static void ConsumerTrace(string message)
        {
            Trace(message, "OpenID Consumer");
        }

        public static void ConsumerTrace(NameValueCollection collection)
        {
            foreach (string key in collection.Keys)
            {
                ConsumerTrace(String.Format("{0} = '{1}'", key, collection[key]));
            }
        }

        public static void ConsumerTrace(object serializableObject)
        {
            if (serializableObject == null)
            {
                ConsumerTrace("<NULL>");
            }
            else
            {
                ConsumerTrace(SerializeToString(serializableObject));
            }

        }        
        
        
        public static TraceSwitch Switch
        {
            get
            {
                if (openIDTraceSwitch == null) { openIDTraceSwitch = new TraceSwitch("OpenID", "OpenID Trace Switch"); }                
                return openIDTraceSwitch;
            }
        }
        
        public static bool TracePageOutput
        {
            get
            {
                if (openIDPageOutputTraceSwitch == null) { openIDPageOutputTraceSwitch = new TraceSwitch("OpenIDEntireResponse", "OpenID Trace Switch"); }
                return (((int)openIDPageOutputTraceSwitch.Level) >= 2);
            }
        }
        
       

        /// <summary>
        /// Serialize obj to an xml string.
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public static string SerializeToString(object obj)
        {
            XmlSerializer serializer = new XmlSerializer(obj.GetType());
            StringWriter writer = new StringWriter();

            serializer.Serialize(writer, obj);
            string result = writer.ToString();
            writer.Close();
            return result;
        }

        public static string RecursivelyExtractExceptionMessage(Exception ex)
        {
            string message = "Exceptions with inner exceptions extracted:";
            message += Environment.NewLine + "Exception: " + ex.GetType().ToString();
            message += Environment.NewLine + "Exception.Message: " + ex.Message;
            message += Environment.NewLine + "Exception.StackTrace:" + ex.StackTrace;
            message += Environment.NewLine;

            if ((ex.InnerException) != null) { message += RecursivelyExtractExceptionMessage(ex.InnerException); }
            return message;
        }
    }
}
