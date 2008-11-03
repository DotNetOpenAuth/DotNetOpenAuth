using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DotNetOAuth.Messaging;

namespace DotNetOAuth.ApplicationBlock {
	internal class Util {
		/// <summary>
		/// Enumerates through the individual set bits in a flag enum.
		/// </summary>
		/// <param name="flags">The flags enum value.</param>
		/// <returns>An enumeration of just the <i>set</i> bits in the flags enum.</returns>
		internal static IEnumerable<long> GetIndividualFlags(Enum flags) {
			long flagsLong = Convert.ToInt64(flags);
			for (int i = 0; i < sizeof(long) * 8; i++) { // long is the type behind the largest enum
				// Select an individual application from the scopes.
				long individualFlagPosition = (long)Math.Pow(2, i);
				long individualFlag = flagsLong & individualFlagPosition;
				if (individualFlag == individualFlagPosition) {
					yield return individualFlag;
				}
			}
		}

		internal static Uri GetCallbackUrlFromContext() {
			Uri callback = MessagingUtilities.GetRequestUrlFromContext().StripQueryArgumentsWithPrefix("oauth_");
			return callback;
		}
	}
}
