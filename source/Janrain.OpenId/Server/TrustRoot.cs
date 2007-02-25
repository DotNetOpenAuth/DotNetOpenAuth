using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace Janrain.OpenId.Server
{
    public class TrustRoot
    {

        #region Private Members

        private static Regex _tr_regex = new Regex("^(?<scheme>https?)://((?<wildcard>\\*)|(?<wildcard>\\*\\.)?(?<host>[a-zA-Z0-9-]+(\\.[a-zA-Z0-9-]+)*)\\.?)(:(?<port>[0-9]+))?(?<path>(/.*|$))");
        private static string[] _top_level_domains =    {"com", "edu", "gov", "int", "mil", "net", "org", "biz", "info", "name", "museum", "coop", "aero", "ac", "ad", "ae", "" +
                                                        "af", "ag", "ai", "al", "am", "an", "ao", "aq", "ar", "as", "at", "au", "aw", "az", "ba", "bb", "bd", "be", "bf", "bg", "bh", "bi", "bj", "" +
                                                        "bm", "bn", "bo", "br", "bs", "bt", "bv", "bw", "by", "bz", "ca", "cc", "cd", "cf", "cg", "ch", "ci", "ck", "cl", "cm", "cn", "co", "cr", "" +
                                                        "cu", "cv", "cx", "cy", "cz", "de", "dj", "dk", "dm", "do", "dz", "ec", "ee", "eg", "eh", "er", "es", "et", "fi", "fj", "fk", "fm", "fo", "" +
                                                        "fr", "ga", "gd", "ge", "gf", "gg", "gh", "gi", "gl", "gm", "gn", "gp", "gq", "gr", "gs", "gt", "gu", "gw", "gy", "hk", "hm", "hn", "hr", "" +
                                                        "ht", "hu", "id", "ie", "il", "im", "in", "io", "iq", "ir", "is", "it", "je", "jm", "jo", "jp", "ke", "kg", "kh", "ki", "km", "kn", "kp", "" +
                                                        "kr", "kw", "ky", "kz", "la", "lb", "lc", "li", "lk", "lr", "ls", "lt", "lu", "lv", "ly", "ma", "mc", "md", "mg", "mh", "mk", "ml", "mm", "" +
                                                        "mn", "mo", "mp", "mq", "mr", "ms", "mt", "mu", "mv", "mw", "mx", "my", "mz", "na", "nc", "ne", "nf", "ng", "ni", "nl", "no", "np", "nr", "" +
                                                        "nu", "nz", "om", "pa", "pe", "pf", "pg", "ph", "pk", "pl", "pm", "pn", "pr", "ps", "pt", "pw", "py", "qa", "re", "ro", "ru", "rw", "sa", "" +
                                                        "sb", "sc", "sd", "se", "sg", "sh", "si", "sj", "sk", "sl", "sm", "sn", "so", "sr", "st", "sv", "sy", "sz", "tc", "td", "tf", "tg", "th", "" +
                                                        "tj", "tk", "tm", "tn", "to", "tp", "tr", "tt", "tv", "tw", "tz", "ua", "ug", "uk", "um", "us", "uy", "uz", "va", "vc", "ve", "vg", "vi", "" +
                                                        "vn", "vu", "wf", "ws", "ye", "yt", "yu", "za", "zm", "zw"};
        private string _unparsed;
        private string _scheme;
        private bool _wildcard;
        private string _host;
        private int _port;
        private string _path;

        #endregion

        #region Constructor(s)

        public TrustRoot(string unparsed)
        {
            Match mo = _tr_regex.Match(unparsed);

            if (mo.Success)
            {
                _unparsed = unparsed;
                _scheme = mo.Groups["scheme"].Value;
                _wildcard = mo.Groups["wildcard"].Value != String.Empty;
                _host = mo.Groups["host"].Value.ToLower();

                Group port_group = mo.Groups["port"];
                if (port_group.Success)
                    _port = Convert.ToInt32(port_group.Value);
                else if (_scheme == "https")
                    _port = 443;
                else
                    _port = 80;

                _path = mo.Groups["path"].Value;
                if (_path == String.Empty)
                    _path = "/";
            }
            else
            {
                throw new ArgumentException(unparsed + " does not appear to be a valid TrustRoot");
            }
        }

        #endregion

        #region Properties

        public bool IsSane
        {
            get
            {
                if (_host == "localhost")
                    return true;

                string[] host_parts = _host.Split('.');

                string tld = host_parts[host_parts.Length - 1];

                if (!Util.InArray(_top_level_domains, tld))
                    return false;

                if (tld.Length == 2)
                {
                    if (host_parts.Length == 1)
                        return false;

                    if (host_parts[host_parts.Length - 2].Length <= 3)
                        return host_parts.Length > 2;

                }
                else
                {
                    return host_parts.Length > 1;
                }

                return false;
            }
        }

        #endregion

        #region Methods

        public bool ValidateUrl(Uri url)
        {
            if (url.Scheme != _scheme)
                return false;

            if (url.Port != _port)
                return false;

            if (!_wildcard)
            {
                if (url.Host != _host)
                {
                    return false;
                }
            }
            else if (_host != String.Empty)
            {
                string[] host_parts = _host.Split('.');
                string[] url_parts = url.Host.Split('.');
                string end_parts = url_parts[url_parts.Length - host_parts.Length];

                for (int i = 0; i < end_parts.Length; i++)
                {
                    if (end_parts != host_parts[i])
                        return false;
                }
            }

            if (url.PathAndQuery == _path)
                return true;

            int path_len = _path.Length;
            string url_prefix = url.PathAndQuery.Substring(0, path_len);

            if (_path != url_prefix)
                return false;

            string allowed = "";
            if (_path.Contains("?"))
                allowed = "&";
            else
                allowed = "?";

            
            return (allowed.IndexOf(_path[_path.Length - 1]) >= 0  || allowed.IndexOf(url.PathAndQuery[path_len]) >= 0);
        }

        #endregion

    }
}
