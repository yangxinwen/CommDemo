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
            var clients = new List<DuiTcpClient>();
            for (int i = 0; i < 10; i++)
            {
                try
                {
                    var client = new DuiTcpClient();
                    client.EndPoint = new System.Net.IPEndPoint(IPAddress.Parse("127.0.0.1"), 1991);
                    client.StatusChange += Client_StatusChange;
                    client.DataReceived += Client_DataReceived;
                    client.Connect();
                    //var bytes = UTF8Encoding.Default.GetBytes("test" + i);
                    //client.Send(bytes);
                    clients.Add(client);
                }
                catch (Exception)
                {
                }
            }

            Console.ReadLine();
            //clients[0].Send(UTF8Encoding.Default.GetBytes("test" +"发送"));

            for (int i = 0; i < 1000; i++)
            {
                clients[i%10].Send(UTF8Encoding.Default.GetBytes("test" + i + "发送"));
                Thread.Sleep(1000);
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
