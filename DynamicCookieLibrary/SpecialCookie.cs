using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace DynamicCookieLibrary.DynamicCookie
{
    [Serializable()]
    public class FirefoxCookie
    {
        private String hostNameBacking
        {
            get; set;
        }
        [SQLite("host")]
        public String hostName
        {
            get
            {
                return hostNameBacking;
            }
            set
            {
                hostNameBacking = CookieFactory.fixHostString(value);
            }
        }

        [SQLite("name")]
        public String name { get; set; }

        [SQLite("value")]
        public String value { get; set; }

        [SQLite("path")]
        public String path { get; set; }
    }

    [Serializable()]
    public class ChromeCookie
    {
        private String hostNameBacking
        {
            get; set;
        }
        [SQLite("host_key")]
        public String hostName
        {
            get
            {
                return hostNameBacking;
            }
            set
            {
                hostNameBacking = CookieFactory.fixHostString(value);
            }
        }

        [SQLite("name")]
        public String name { get; set; }

        public String value { get; set; }

        [SQLite("encrypted_value")]
        public byte[] encryptedValue
        {
            set
            {
                this.value = String.Empty;
                byte[] data = ProtectedData.Unprotect(value, null, DataProtectionScope.CurrentUser);
                foreach (byte b in data)
                {
                    this.value += (char)b;
                }
            }
        }

        [SQLite("path")]
        public String path { get; set; }
    }

    [Serializable()]
    [AttributeUsage(AttributeTargets.Property)]
    public class SQLiteAttribute : Attribute
    {
        public String dataName { get; set; }
        public SQLiteAttribute(String dataName)
        {
            this.dataName = dataName;
        }
    }
}