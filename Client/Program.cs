using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using DuiAsynSocket;

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
            for (int i = 0; i < 5; i++)
            {
                try
                {
                    var client = new SocketClient("127.0.0.1", 1991);
                    client.HeartBeatsEnable = true;
                    //client.HeartBeatSpan = 30;
                    client.OnConnChangeEvent += Client_OnConnChangeEvent;
                    client.OnReceivedEvent += Client_OnReceivedEvent;
                    client.Connect();
                    //var bytes = UTF8Encoding.Default.GetBytes("test" + i);
                    //client.Send(bytes);
                    clients.Add(client);
                }
                catch (Exception ex)
                {
                }
            }


            Console.WriteLine("----------------");

            for (int i = 0; i < 10000; i++)
            {
                Console.WriteLine("test" + (i % 5) + "发送");
                clients[i % 5].Send(UTF8Encoding.UTF8.GetBytes("test" + (i % 5) + "发送"));
                Thread.Sleep(1000*10);
            }
            Console.ReadLine();
        }

        private static void Client_OnReceivedEvent(DataReceivedArgs obj)
        {
            Console.WriteLine("接收:"+Encoding.UTF8.GetString(obj.Data));
        }

        private static void Client_OnConnChangeEvent(ConnStatusChangeArgs obj)
        {
            Console.WriteLine(obj.ConnStatus.ToString());
        }   
    }
}
