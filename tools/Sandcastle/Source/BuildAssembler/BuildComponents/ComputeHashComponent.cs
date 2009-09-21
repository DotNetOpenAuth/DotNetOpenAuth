using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;
using System.Xml;
using System.Xml.XPath;


namespace Microsoft.Ddue.Tools {

	internal class HashComputation {

		public HashComputation (string input, string output) {
			Input = XPathExpression.Compile(input);
			Output = XPathExpression.Compile(output);
		}

		public XPathExpression Input;
		public XPathExpression Output;

	}

	public class ComputeHashComponent : BuildComponent {

		public ComputeHashComponent (XPathNavigator configuration) : base(configuration) {

			if (configuration == null) throw new ArgumentNullException("configuraton");

			XPathNodeIterator hash_nodes = configuration.Select("hash");
			foreach (XPathNavigator hash_node in hash_nodes) {
				string input_xpath = hash_node.GetAttribute("input", String.Empty);
				string output_xpath = hash_node.GetAttribute("output", String.Empty);
				computations.Add( new HashComputation(input_xpath, output_xpath) );
			}

		}

		// A list of the hash computations to do

		private List<HashComputation> computations = new List<HashComputation>();

		// Logic to compute a unique hash of a comment id string

		private static Guid ComputeHash (string key) {
			byte[] input = Encoding.UTF8.GetBytes(key);
			byte[] output = md5.ComputeHash(input);
			return( new Guid(output) );
		}

		private static HashAlgorithm md5 = new MD5CryptoServiceProvider();

		// The actual action of the component

		public override void Apply (XmlDocument document, string key) {

			Guid id = ComputeHash(key);

			foreach (HashComputation computation in computations) {
				XPathNavigator output = document.CreateNavigator().SelectSingleNode(computation.Output);
				if (output == null) continue;
				output.SetValue(id.ToString());
			}

		}

	}

}
