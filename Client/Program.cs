using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using SocketAsyncServer;
using XXJR.Communication;

namespace Client
{
    class Program
    {
        static void Main(string[] args)
        {


            TestClient();
            Console.ReadLine();
        }



        private static void TestClient()
        {
            var clients = new List<SocketClient>();
            for (int i = 0; i < 10; i++)
            {
                try
                {
                    var client = new SocketClient("127.0.0.1", 1991);
                    client.HeartBeatsEnable = true;
                    client.HeartBeatSpan = 10;
                    client.OnConnChangeEvent += Client_OnConnChangeEvent;
                    client.OnReceivedEvent += Client_OnReceivedEvent;
                    client.Connect();
                    //var bytes = UTF8Encoding.Default.GetBytes("test" + i);
                    //client.Send(bytes);
                    clients.Add(client);
                }
                catch (Exception)
                {
                }
            }


            Console.WriteLine("----------------");

            for (int i = 0; i < 100; i++)
            {
                //Console.WriteLine("test" + (i % 10) + "发送");
                clients[i % 10].Send(UTF8Encoding.Default.GetBytes("test" + (i % 10) + "发送"));
                Thread.Sleep(1000*7);
            }
            Console.ReadLine();
        }

        private static void Client_OnReceivedEvent(XXJR.Communication.DataReceivedArgs obj)
        {
            Console.WriteLine(Encoding.UTF8.GetString(obj.Data));
        }

        private static void Client_OnConnChangeEvent(XXJR.Communication.ConnStatusChangeArgs obj)
        {
            Console.WriteLine(obj.ConnStatus.ToString());
        }


        private static void TestOldClient()
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
                clients[i % 10].Send(UTF8Encoding.Default.GetBytes("test" + i + "发送"));
                Thread.Sleep(1000);
            }
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
