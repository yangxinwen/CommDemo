using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using XXJR.Communication;

namespace Client
{
    class Program
    {
        static void Main(string[] args)
        {
            var bytes = UTF8Encoding.Default.GetBytes("test");
            for (int i = 0; i < 100; i++)
            {
                var client = new DuiTcpClient();
                client.EndPoint = new System.Net.IPEndPoint(IPAddress.Parse("127.0.0.1"), 1991);
                client.StatusChange += Client_StatusChange;
                client.DataReceived += Client_DataReceived;
                client.Connect();
                
                client.Send(bytes);
                //Thread.Sleep(100);
            }
            Console.ReadLine();
        }

        private static void Client_DataReceived(byte[] obj)
        {
            Console.WriteLine(UTF8Encoding.Default.GetString(obj));
        }

        private static void Client_StatusChange(ConnectStatus obj)
        {
            Console.WriteLine(obj.ToString());
        }
    }
}
