//-----------------------------------------------------------------------
// <copyright file="MessageDictionaryTests.cs" company="Outercurve Foundation">
//     Copyright (c) Outercurve Foundation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace DotNetOpenAuth.Test.Messaging.Reflection {
	using System;
	using System.Collections;
	using System.Collections.Generic;
	using System.Collections.ObjectModel;
	using System.Xml;
	using DotNetOpenAuth.Messaging;
	using DotNetOpenAuth.Messaging.Reflection;
	using NUnit.Framework;

	[TestFixture]
	public class MessageDictionaryTests : MessagingTestBase {
		private Mocks.TestMessage message;

		[SetUp]
		public override void SetUp() {
			base.SetUp();

			this.message = new Mocks.TestDirectedMessage();
		}

		[Test, ExpectedException(typeof(ArgumentNullException))]
		public void CtorNull() {
			this.MessageDescriptions.GetAccessor(null);
		}

		/// <summary>
		/// A test for System.Collections.Generic.IDictionary&lt;System.String,System.String>.Values
		/// </summary>
		[Test]
		public void Values() {
			IDictionary<string, string> target = this.MessageDescriptions.GetAccessor(this.message);
			Collection<string> expected = new Collection<string> {
				this.message.Age.ToString(),
				XmlConvert.ToString(DateTime.SpecifyKind(this.message.Timestamp, DateTimeKind.Utc), XmlDateTimeSerializationMode.Utc),
			};
			CollectionAssert<string>.AreEquivalent(expected, target.Values);

			this.message.Age = 15;
			this.message.Location = new Uri("http://localtest");
			this.message.Name = "Andrew";
			target["extra"] = "a";
			expected = new Collection<string> {
				this.message.Age.ToString(),
				this.message.Location.AbsoluteUri,
				this.message.Name,
				XmlConvert.ToString(DateTime.SpecifyKind(this.message.Timestamp, DateTimeKind.Utc), XmlDateTimeSerializationMode.Utc),
				"a",
			};
			CollectionAssert<string>.AreEquivalent(expected, target.Values);
		}

		/// <summary>
		/// A test for System.Collections.Generic.IDictionary&lt;System.String,System.String>.Keys
		/// </summary>
		[Test]
		public void Keys() {
			// We expect that non-nullable value type fields will automatically have keys
			// in the dictionary for them.
			IDictionary<string, string> target = this.MessageDescriptions.GetAccessor(this.message);
			Collection<string> expected = new Collection<string> {
				"age",
				"Timestamp",
			};
			CollectionAssert<string>.AreEquivalent(expected, target.Keys);

			this.message.Name = "Andrew";
			expected.Add("Name");
			target["extraField"] = string.Empty;
			expected.Add("extraField");
			CollectionAssert<string>.AreEquivalent(expected, target.Keys);
		}

		/// <summary>
		/// A test for System.Collections.Generic.IDictionary&lt;System.String,System.String>.Item
		/// </summary>
		[Test]
		public void Item() {
			IDictionary<string, string> target = this.MessageDescriptions.GetAccessor(this.message);

			// Test setting of declared message properties.
			this.message.Age = 15;
			Assert.AreEqual("15", target["age"]);
			target["age"] = "13";
			Assert.AreEqual(13, this.message.Age);

			// Test setting extra fields
			target["extra"] = "fun";
			Assert.AreEqual("fun", target["extra"]);
			Assert.AreEqual("fun", ((IProtocolMessage)this.message).ExtraData["extra"]);

			// Test clearing extra fields
			target["extra"] = null;
			Assert.IsFalse(target.ContainsKey("extra"));
		}

		/// <summary>
		/// A test for System.Collections.Generic.ICollection&lt;System.Collections.Generic.KeyValuePair&lt;System.String,System.String&lt;&lt;.IsReadOnly
		/// </summary>
		[Test]
		public void IsReadOnly() {
			ICollection<KeyValuePair<string, string>> target = this.MessageDescriptions.GetAccessor(this.message);
			Assert.IsFalse(target.IsReadOnly);
		}

		/// <summary>
		/// A test for System.Collections.Generic.ICollection&lt;System.Collections.Generic.KeyValuePair&lt;System.String,System.String&lt;&lt;.Count
		/// </summary>
		[Test]
		public void Count() {
			ICollection<KeyValuePair<string, string>> target = this.MessageDescriptions.GetAccessor(this.message);
			IDictionary<string, string> targetDictionary = (IDictionary<string, string>)target;
			Assert.AreEqual(targetDictionary.Keys.Count, target.Count);
			targetDictionary["extraField"] = "hi";
			Assert.AreEqual(targetDictionary.Keys.Count, target.Count);
		}

		/// <summary>
		/// A test for System.Collections.Generic.IEnumerable&lt;System.Collections.Generic.KeyValuePair&lt;System.String,System.String&lt;&lt;.GetEnumerator
		/// </summary>
		[Test]
		public void GetEnumerator() {
			IEnumerable<KeyValuePair<string, string>> target = this.MessageDescriptions.GetAccessor(this.message);
			IDictionary<string, string> targetDictionary = (IDictionary<string, string>)target;
			var keys = targetDictionary.Keys.GetEnumerator();
			var values = targetDictionary.Values.GetEnumerator();
			IEnumerator<KeyValuePair<string, string>> actual = target.GetEnumerator();

			bool keysLast = true, valuesLast = true, actualLast = true;
			while (true) {
				keysLast = keys.MoveNext();
				valuesLast = values.MoveNext();
				actualLast = actual.MoveNext();
				if (!keysLast || !valuesLast || !actualLast) {
					break;
				}

				Assert.AreEqual(keys.Current, actual.Current.Key);
				Assert.AreEqual(values.Current, actual.Current.Value);
			}
			Assert.IsTrue(keysLast == valuesLast && keysLast == actualLast);
		}

		[Test]
		public void GetEnumeratorUntyped() {
			IEnumerable target = this.MessageDescriptions.GetAccessor(this.message);
			IDictionary<string, string> targetDictionary = (IDictionary<string, string>)target;
			var keys = targetDictionary.Keys.GetEnumerator();
			var values = targetDictionary.Values.GetEnumerator();
			IEnumerator actual = target.GetEnumerator();

			bool keysLast = true, valuesLast = true, actualLast = true;
			while (true) {
				keysLast = keys.MoveNext();
				valuesLast = values.MoveNext();
				actualLast = actual.MoveNext();
				if (!keysLast || !valuesLast || !actualLast) {
					break;
				}

				KeyValuePair<string, string> current = (KeyValuePair<string, string>)actual.Current;
				Assert.AreEqual(keys.Current, current.Key);
				Assert.AreEqual(values.Current, current.Value);
			}
			Assert.IsTrue(keysLast == valuesLast && keysLast == actualLast);
		}

		/// <summary>
		/// A test for System.Collections.Generic.IDictionary&lt;System.String,System.String>.TryGetValue
		/// </summary>
		[Test]
		public void TryGetValue() {
			IDictionary<string, string> target = this.MessageDescriptions.GetAccessor(this.message);
			this.message.Name = "andrew";
			string name;
			Assert.IsTrue(target.TryGetValue("Name", out name));
			Assert.AreEqual(this.message.Name, name);

			Assert.IsFalse(target.TryGetValue("name", out name));
			Assert.IsNull(name);

			target["extra"] = "value";
			string extra;
			Assert.IsTrue(target.TryGetValue("extra", out extra));
			Assert.AreEqual("value", extra);
		}

		/// <summary>
		/// A test for System.Collections.Generic.IDictionary&lt;System.String,System.String>.Remove
		/// </summary>
		[Test]
		public void RemoveTest1() {
			IDictionary<string, string> target = this.MessageDescriptions.GetAccessor(this.message);
			this.message.Name = "andrew";
			Assert.IsTrue(target.Remove("Name"));
			Assert.IsNull(this.message.Name);
			Assert.IsFalse(target.Remove("Name"));

			Assert.IsFalse(target.Remove("extra"));
			target["extra"] = "value";
			Assert.IsTrue(target.Remove("extra"));
			Assert.IsFalse(target.ContainsKey("extra"));
		}

		/// <summary>
		/// A test for System.Collections.Generic.IDictionary&lt;System.String,System.String>.ContainsKey
		/// </summary>
		[Test]
		public void ContainsKey() {
			IDictionary<string, string> target = this.MessageDescriptions.GetAccessor(this.message);
			Assert.IsTrue(target.ContainsKey("age"), "Value type declared element should have a key.");
			Assert.IsFalse(target.ContainsKey("Name"), "Null declared element should NOT have a key.");

			Assert.IsFalse(target.ContainsKey("extra"));
			target["extra"] = "value";
			Assert.IsTrue(target.ContainsKey("extra"));
		}

		/// <summary>
		/// A test for System.Collections.Generic.IDictionary&lt;System.String,System.String&gt;.Add
		/// </summary>
		[Test]
		public void AddByKeyAndValue() {
			IDictionary<string, string> target = this.MessageDescriptions.GetAccessor(this.message);
			target.Add("extra", "value");
			Assert.IsTrue(target.Contains(new KeyValuePair<string, string>("extra", "value")));
			target.Add("Name", "Andrew");
			Assert.AreEqual("Andrew", this.message.Name);
		}

		[Test, ExpectedException(typeof(ArgumentNullException))]
		public void AddNullValue() {
			IDictionary<string, string> target = this.MessageDescriptions.GetAccessor(this.message);
			target.Add("extra", null);
		}

		/// <summary>
		/// A test for System.Collections.Generic.ICollection&lt;System.Collections.Generic.KeyValuePair&lt;System.String,System.String&lt;&lt;.Add
		/// </summary>
		[Test]
		public void AddByKeyValuePair() {
			IDictionary<string, string> target = this.MessageDescriptions.GetAccessor(this.message);
			target.Add(new KeyValuePair<string, string>("extra", "value"));
			Assert.IsTrue(target.Contains(new KeyValuePair<string, string>("extra", "value")));
		}

		[Test, ExpectedException(typeof(ArgumentException))]
		public void AddExtraFieldThatAlreadyExists() {
			IDictionary<string, string> target = this.MessageDescriptions.GetAccessor(this.message);
			target.Add("extra", "value");
			target.Add("extra", "value");
		}

		[Test, ExpectedException(typeof(ArgumentException))]
		public void AddDeclaredValueThatAlreadyExists() {
			IDictionary<string, string> target = this.MessageDescriptions.GetAccessor(this.message);
			target.Add("Name", "andrew");
			target.Add("Name", "andrew");
		}

		[Test]
		public void DefaultReferenceTypeDeclaredPropertyHasNoKey() {
			IDictionary<string, string> target = this.MessageDescriptions.GetAccessor(this.message);
			Assert.IsFalse(target.ContainsKey("Name"), "A null value should result in no key.");
			Assert.IsFalse(target.Keys.Contains("Name"), "A null value should result in no key.");
		}

		[Test]
		public void RemoveStructDeclaredProperty() {
			IDictionary<string, string> target = this.MessageDescriptions.GetAccessor(this.message);
			this.message.Age = 5;
			Assert.IsTrue(target.ContainsKey("age"));
			target.Remove("age");
			Assert.IsTrue(target.ContainsKey("age"));
			Assert.AreEqual(0, this.message.Age);
		}

		/// <summary>
		/// A test for System.Collections.Generic.ICollection&lt;System.Collections.Generic.KeyValuePair&lt;System.String,System.String&lt;&lt;.Remove
		/// </summary>
		[Test]
		public void RemoveByKeyValuePair() {
			ICollection<KeyValuePair<string, string>> target = this.MessageDescriptions.GetAccessor(this.message);
			this.message.Name = "Andrew";
			Assert.IsFalse(target.Remove(new KeyValuePair<string, string>("Name", "andrew")));
			Assert.AreEqual("Andrew", this.message.Name);
			Assert.IsTrue(target.Remove(new KeyValuePair<string, string>("Name", "Andrew")));
			Assert.IsNull(this.message.Name);
		}

		/// <summary>
		/// A test for System.Collections.Generic.ICollection&lt;System.Collections.Generic.KeyValuePair&lt;System.String,System.String&lt;&lt;.CopyTo
		/// </summary>
		[Test]
		public void CopyTo() {
			ICollection<KeyValuePair<string, string>> target = this.MessageDescriptions.GetAccessor(this.message);
			IDictionary<string, string> targetAsDictionary = (IDictionary<string, string>)target;
			KeyValuePair<string, string>[] array = new KeyValuePair<string, string>[target.Count + 1];
			int arrayIndex = 1;
			target.CopyTo(array, arrayIndex);
			Assert.AreEqual(new KeyValuePair<string, string>(), array[0]);
			for (int i = 1; i < array.Length; i++) {
				Assert.IsNotNull(array[i].Key);
				Assert.AreEqual(targetAsDictionary[array[i].Key], array[i].Value);
			}
		}

		/// <summary>
		/// A test for System.Collections.Generic.ICollection&lt;System.Collections.Generic.KeyValuePair&lt;System.String,System.String&lt;&lt;.Contains
		/// </summary>
		[Test]
		public void ContainsKeyValuePair() {
			ICollection<KeyValuePair<string, string>> target = this.MessageDescriptions.GetAccessor(this.message);
			IDictionary<string, string> targetAsDictionary = (IDictionary<string, string>)target;
			Assert.IsFalse(target.Contains(new KeyValuePair<string, string>("age", "1")));
			Assert.IsTrue(target.Contains(new KeyValuePair<string, string>("age", "0")));

			targetAsDictionary["extra"] = "value";
			Assert.IsFalse(target.Contains(new KeyValuePair<string, string>("extra", "Value")));
			Assert.IsTrue(target.Contains(new KeyValuePair<string, string>("extra", "value")));
			Assert.IsFalse(target.Contains(new KeyValuePair<string, string>("wayoff", "value")));
		}

		/// <summary>
		/// A test for System.Collections.Generic.ICollection&lt;System.Collections.Generic.KeyValuePair&lt;System.String,System.String&lt;&lt;.Clear
		/// </summary>
		[Test]
		public void ClearValues() {
			MessageDictionary target = this.MessageDescriptions.GetAccessor(this.message);
			IDictionary<string, string> targetAsDictionary = (IDictionary<string, string>)target;
			this.message.Name = "Andrew";
			this.message.Age = 15;
			targetAsDictionary["extra"] = "value";
			target.ClearValues();
			Assert.AreEqual(2, target.Count, "Clearing should remove all keys except for declared non-nullable structs.");
			Assert.IsFalse(targetAsDictionary.ContainsKey("extra"));
			Assert.IsNull(this.message.Name);
			Assert.AreEqual(0, this.message.Age);
		}

		/// <summary>
		/// Verifies that the Clear method throws the expected exception.
		/// </summary>
		[Test, ExpectedException(typeof(NotSupportedException))]
		public void Clear() {
			MessageDictionary target = this.MessageDescriptions.GetAccessor(this.message);
			target.Clear();
		}
	}
}
