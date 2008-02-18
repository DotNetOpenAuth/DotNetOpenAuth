using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Web;

namespace DotNetOpenId.Session
{
    public class SimpleSessionState : ISessionState
    {
        private static SimpleSessionState local_instance = new SimpleSessionState();

        private Hashtable hashtable = new Hashtable();

        public SimpleSessionState()
        {
        }

        public static SimpleSessionState GetInstance()
        {
            return local_instance;
        }

        public object this[string index] { get { return hashtable[index]; } set { hashtable[index] = value; } }
        public void Add(string name, object value) { hashtable.Add(name, value); }
        public void Remove(string name) { hashtable.Remove(name); }
    }
}
