// --------------------------------------------------------------------------------------------------------------------
// <copyright file="StateDictionary.cs" company="iPrinciples Ltd">
//   Copyright (c) 2013 iPrinciples Ltd.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------
namespace DotNetOpenAuth.AspNet
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Web;

    /// <summary>
    /// Defines a dictionary for storing state passed on a callback url.
    /// </summary>
    public class StateDictionary : Dictionary<string, string>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="StateDictionary"/> class.
        /// </summary>
        public StateDictionary()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="StateDictionary"/> class.
        /// </summary>
        /// <param name="dictionary">
        /// An existing dictionary containing state.
        /// </param>
        public StateDictionary(IDictionary<string, string> dictionary)
            : base(dictionary)
        {
        }

        /// <summary>
        /// Attempts to parse the specified <paramref name="value"/> into a <see cref="StateDictionary"/>.
        /// </summary>
        /// <param name="value">The string value to parse.</param>
        /// <param name="state">The generated <see cref="StateDictionary"/>.</param>
        /// <returns><c>true</c> if the specified <paramref name="value"/> was parsed, <c>false</c> otherwise.</returns>
        public static bool TryParse(string value, out StateDictionary state)
        {
            state = null;
            if (string.IsNullOrWhiteSpace(value))
            {
                return false;
            }

            try
            {
                state  = new StateDictionary(HttpUtility.UrlDecode(value).Split('&').Select(x => x.Split('=')).ToDictionary(x => x[0], x => x[1]));
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        /// <summary>
        /// Converts the current <see cref="StateDictionary"/> into an url encoded string.
        /// </summary>
        /// <returns>The generated string value.</returns>
        public string ToEncodedString()
        {
            return HttpUtility.UrlEncode(string.Join("&", this.Select(x => string.Format("{0}={1}", x.Key, x.Value))));
        }
    }
}
