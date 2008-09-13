//-----------------------------------------------------------------------
// <copyright file="DataContractMemberComparer.cs" company="Andrew Arnott">
//     Copyright (c) Andrew Arnott. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace DotNetOAuth.Messaging {
	using System;
	using System.Collections.Generic;
	using System.Diagnostics;
	using System.IO;
	using System.Linq;
	using System.Reflection;
	using System.Runtime.Serialization;
	using System.Xml;
	using System.Xml.Linq;

	/// <summary>
	/// A sorting tool to arrange fields in an order expected by the <see cref="DataContractSerializer"/>.
	/// </summary>
	internal class DataContractMemberComparer : IComparer<string> {
		/// <summary>
		/// The cached calculated inheritance ranking of every [DataMember] member of a type.
		/// </summary>
		private Dictionary<string, int> ranking;

		/// <summary>
		/// Initializes a new instance of the <see cref="DataContractMemberComparer"/> class.
		/// </summary>
		/// <param name="dataContractType">The data contract type that will be deserialized to.</param>
		internal DataContractMemberComparer(Type dataContractType) {
			// The elements must be serialized in inheritance rank and alphabetical order 
			// so the DataContractSerializer will see them.
			this.ranking = GetDataMemberInheritanceRanking(dataContractType);
		}

		#region IComparer<string> Members

		/// <summary>
		/// Compares to fields and decides what order they should appear in.
		/// </summary>
		/// <param name="field1">The first field.</param>
		/// <param name="field2">The second field.</param>
		/// <returns>-1 if the first field should appear first, 0 if it doesn't matter, 1 if it should appear last.</returns>
		public int Compare(string field1, string field2) {
			int rank1, rank2;
			bool field1Valid = this.ranking.TryGetValue(field1, out rank1);
			bool field2Valid = this.ranking.TryGetValue(field2, out rank2);

			// If both fields are invalid, we don't care about the order.
			if (!field1Valid && !field2Valid) {
				return 0;
			}

			// If exactly one is valid, put that one first.
			if (field1Valid ^ field2Valid) {
				return field1Valid ? -1 : 1;
			}

			// First compare their inheritance ranking.
			if (rank1 != rank2) {
				// We want DESCENDING rank order, putting the members defined in the most
				// base class first.
				return -rank1.CompareTo(rank2);
			}

			// Finally sort alphabetically with case sensitivity.
			return string.CompareOrdinal(field1, field2);
		}

		#endregion

		/// <summary>
		/// Generates a dictionary of field name and inheritance rankings for a given DataContract type.
		/// </summary>
		/// <param name="type">The type to generate member rankings for.</param>
		/// <returns>The generated dictionary.</returns>
		private static Dictionary<string, int> GetDataMemberInheritanceRanking(Type type) {
			Debug.Assert(type != null, "type == null");
			var ranking = new Dictionary<string, int>();

			// TODO: review partial trust scenarios and this NonPublic flag.
			Type currentType = type;
			int rank = 0;
			do {
				foreach (MemberInfo member in currentType.GetMembers(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.DeclaredOnly)) {
					if (member is PropertyInfo || member is FieldInfo) {
						DataMemberAttribute dataMemberAttribute = member.GetCustomAttributes(typeof(DataMemberAttribute), true).OfType<DataMemberAttribute>().FirstOrDefault();
						if (dataMemberAttribute != null) {
							string name = dataMemberAttribute.Name ?? member.Name;
							ranking.Add(name, rank);
						}
					}
				}

				rank++;
				currentType = currentType.BaseType;
			} while (currentType != null);

			return ranking;
		}
	}
}
