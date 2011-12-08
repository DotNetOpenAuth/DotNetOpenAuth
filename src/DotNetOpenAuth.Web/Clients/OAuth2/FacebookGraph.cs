using System;
using System.Runtime.Serialization;

namespace DotNetOpenAuth.Web.Clients
{
    /// <summary>
    /// Contains data of a Facebook user.
    /// </summary>
    /// <remarks>
    /// Technically, this class doesn't need to be public, but because we want to make it serializable
    /// in medium trust, it has to be public.
    /// </remarks>
    [DataContract]
    public class FacebookGraph
    {
        [DataMember(Name = "id")]
        public string Id { get; set; }

        [DataMember(Name = "email")]
        public string Email { get; set; }

        [DataMember(Name = "name")]
        public string Name { get; set; }

        [DataMember(Name = "link")]
        public Uri Link { get; set; }

        [DataMember(Name = "gender")]
        public string Gender { get; set; }

        [DataMember(Name = "birthday")]
        public string Birthday { get; set; }
    }
}
