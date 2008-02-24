using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Web;

namespace DotNetOpenId.Session {
	public class SimpleSessionState : ISessionState {
		static SimpleSessionState local_instance = new SimpleSessionState();

		Dictionary<string, object> stateBag = new Dictionary<string, object>();

		private SimpleSessionState() { }

		public static SimpleSessionState Instance {
			get { return local_instance; }
		}

		public object this[string key] {
			get { return stateBag.ContainsKey(key) ? stateBag[key] : null; }
			set { stateBag[key] = value; }
		}
		public void Add(string key, object value) {
			stateBag.Add(key, value);
		}
		public void Remove(string key) {
			stateBag.Remove(key);
		}
	}
}
