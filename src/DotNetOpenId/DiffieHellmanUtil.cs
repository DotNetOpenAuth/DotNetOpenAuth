using System;
using System.Collections.Generic;
using System.Text;
using System.Security.Cryptography;

namespace DotNetOpenId {
	class DiffieHellmanUtil {
		class DHSha {
			public DHSha(HashAlgorithm algorithm, Util.Func<Protocol, string> getName) {
				if (algorithm == null) throw new ArgumentNullException("algorithm");
				if (getName == null) throw new ArgumentNullException("getName");

				GetName = getName;
				Algorithm = algorithm;
			}
			internal Util.Func<Protocol, string> GetName;
			internal readonly HashAlgorithm Algorithm;
		}

		static DHSha[] DiffieHellmanSessionTypes = {
			new DHSha(new SHA1Managed(), protocol => protocol.Args.SessionType.DH_SHA1),
			new DHSha(new SHA256Managed(), protocol => protocol.Args.SessionType.DH_SHA256),
			new DHSha(new SHA384Managed(), protocol => protocol.Args.SessionType.DH_SHA384),
			new DHSha(new SHA512Managed(), protocol => protocol.Args.SessionType.DH_SHA512),
		};

		public static HashAlgorithm Lookup(Protocol protocol, string name) {
			foreach (DHSha dhsha in DiffieHellmanSessionTypes) {
				if (String.Equals(dhsha.GetName(protocol), name, StringComparison.Ordinal)) {
					return dhsha.Algorithm;
				}
			}
			throw new ArgumentOutOfRangeException("name");
		}
	}
}
