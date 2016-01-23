using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace DynamicCookieLibrary.DynamicCookie
{
    public static class CookieFactory
    {
        public static byte[] getDataSafe(String path)
        {
            String output = Path.GetTempFileName();
            File.Delete(output);
            File.Copy(path, output);
            byte[] data = File.ReadAllBytes(output);
            File.Delete(output);
            return data;
        }
        public static List<T> processSQLData<T>(String table, byte[] fileData)
        {
            String directory = "Databases";
            Directory.CreateDirectory(directory);
            String file = Path.Combine(directory, randomString(32));
            File.WriteAllBytes(file, fileData);
            List<T> returnData = new List<T>();
            using (SQLiteConnection cnn = new SQLiteConnection("data source=\"" + file + "\""))
            {
                cnn.Open();
                using (SQLiteCommand command = new SQLiteCommand("SELECT * FROM " + table + ";", cnn))
                {
                    using (SQLiteDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            T obj = (T)Activator.CreateInstance(typeof(T));
                            foreach (PropertyInfo info in obj.GetType().GetProperties())
                            {
                                Object[] attributes = info.GetCustomAttributes(false);
                                if (attributes.Length != 0)
                                {
                                    if (attributes[0] is SQLiteAttribute)
                                    {
                                        SQLiteAttribute attribute = attributes[0] as SQLiteAttribute;
                                        info.SetValue(obj, reader[attribute.dataName], null);
                                    }
                                }
                            }
                            returnData.Add(obj);
                        }
                    }
                }
                cnn.Close();
            }
            File.Delete(file);
            return returnData;
        }
        public static string randomString(int length)
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            var random = new Random();
            return new string(Enumerable.Repeat(chars, length)
              .Select(s => s[random.Next(s.Length)]).ToArray());
        }
        public static String fixHostString(String host)
        {
            return (host[0].Equals('.')) ? "http://" + host.Substring(1) : "http://" + host;
        }
    }
}
