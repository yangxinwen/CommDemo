using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using DuiAsynSocket;

namespace Service
{
    class Program
    {
        static void Main(string[] args)
        {
            TestClient();

            //for (int i = 0; i < 1000; i++)
            //{
            //    var client = new TcpClient();
            //    client.Connect("192.168.31.70", 2991);
            //    if (client.Connected)
            //        successCount++;
            //    else
            //        errorCount++;
            //}

            //Console.WriteLine($"成功:{successCount} 失败{errorCount}");
            Console.ReadLine();
        }
        private static uint sequenceId = 1;
        private static byte[] GetSendData()
        {
            var model = ExchangeData.CreateBuilder();
            model.IsRequest = true;
            model.MessageType = true;
            model.SequenceId = sequenceId++;
            model.JsonBody = "test";

            var stream = new MemoryStream();
            model.Build().WriteTo(stream);
            return stream.ToArray();
        }

        private static void TestClient()
        {

            var clients = new List<SocketClient>();
            for (int i = 0; i < 1000; i++)
            {
                try
                {
                    var client = new SocketClient();
                    client.HeartBeatsEnable = true;
                    //client.HeartBeatSpan = 30;
                    client.OnConnChangeEvent += Client_OnConnChangeEvent;
                    client.OnReceivedEvent += Client_OnReceivedEvent;
                    client.Connect("127.0.0.1", 2991);
                    //var bytes = UTF8Encoding.Default.GetBytes("test" + i);
                    //client.Send(bytes);
                    clients.Add(client);
                }
                catch (Exception ex)
                {

                }
            }


            Console.WriteLine("----------------");

            //Thread.Sleep(5 * 1000);

            //for (int i = 0; i < 100000; i++)
            //{
            //    var client = clients[i % 5000];
            //    if (client.ConnStatus != ConnectStatus.Connected)
            //        continue;
            //    var bytes = GetSendData();
            //    client.Send(bytes);
            //    //Thread.Sleep(1000*10);
            //}
            Console.ReadLine();
        }

        private static void Client_OnReceivedEvent(DataReceivedArgs obj)
        {
            //Console.WriteLine("接收:"+Encoding.UTF8.GetString(obj.Data));
        }

        private static int successCount = 0;

        private static int errorCount = 0;

        private static void Client_OnConnChangeEvent(ConnStatusChangeArgs obj)
        {
            if (obj.ConnStatus == ConnectStatus.Connected)
                successCount++;
            else
                errorCount++;
            Console.WriteLine($"成功:{successCount}   失败:{errorCount}");
        }
    }
}
