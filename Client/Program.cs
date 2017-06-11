using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using DuiAsynSocket;
using DuiCommun;

namespace Service
{
    class Program
    {
        static void Main(string[] args)
        {
            //TestClient();

            var list = new List<SocketClient>();

            for (int i = 0; i < 700; i++)
            {
                var client = new SocketClient();
                client.IsSplitPack = true;
                client.IsUseHeartBeatCertificate = false;
                client.HeartBeatsEnable = false;
                client.BufferSize = 1024 * 4;
                //client.OnReceivedEvent += Client_OnReceivedEvent1;
                client.Connect("127.0.0.1", 2991);

                list.Add(client);
            }
            {
                var random = new Random();
                while (true)
                {
                    var index = random.Next(list.Count - 1);
                    var value = BitConverter.GetBytes(index);
                    list[index].Send(value);
                }




                ////while (true)-
                //{
                //    {
                //        var data = new byte[1024 * 1024];
                //        var value = BitConverter.GetBytes(10000);
                //        Array.Copy(value, data, 4);
                //        data[data.Length - 1] = 2;
                //        client.Send(data);
                //    }

                //    for (int i = 0; i < 10000; i++)
                //    {
                //        //Thread.Sleep(100);
                //        var data = new byte[1024 * 1024];
                //        var value = BitConverter.GetBytes(i);
                //        Array.Copy(value, data, 4);
                //        data[data.Length - 1] = 2;
                //        client.Send(data);
                //        //client.Send(BitConverter.GetBytes(i));
                //    }
                //}
            }


            Console.WriteLine($"成功:{successCount} 失败{errorCount}");
            Console.ReadLine();
        }

        private static void Client_OnReceivedEvent1(DataReceivedArgs obj)
        {
            Console.WriteLine(BitConverter.ToInt32(obj.Data, 0) + "  " + obj.Data[obj.Data.Length - 1]);
        }

        private static uint sequenceId = 1;
        private static byte[] GetSendData()
        {
            var model = Request.CreateBuilder();
            model.SessionId = 123456;
            model.SequenceId = sequenceId++;
            model.JsonBody = "test";

            var stream = new MemoryStream();
            model.Build().WriteTo(stream);
            return stream.ToArray();
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
