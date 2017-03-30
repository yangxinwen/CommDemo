using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using DuiAsynSocket;

namespace Service
{
    class Program
    {
        static void Main(string[] args)
        {
            TestService();

            Console.ReadLine();
        }

        static SocketListener service = null;
        private static void TestService()
        {
            service = new SocketListener(1000, 1024*1024);
            service.Start(2991);
            service.OnReceived += Service_OnReceivedEvent;
            service.OnClientConnChange += Service_OnClientConnChangeEvent;

            var sendData = UTF8Encoding.UTF8.GetBytes("服务端发送");


            //while (true)
            //{
            //    var i = 1;
            //    Thread.Sleep(10 * 1000);

            //    foreach (var item in service.GetClients())
            //    {
            //        service.Send(item, UTF8Encoding.UTF8.GetBytes($"{i++}服务端发送"));
            //    }
            //}

            //service.Stop();
        }

        private static void Service_OnClientConnChangeEvent(ConnStatusChangeArgs obj)
        {
            if (obj.ConnStatus == ConnectStatus.Connected)
                successCount++;
            else if (obj.ConnStatus == ConnectStatus.Closed)
                errorCount++;
            Console.WriteLine($"接入:{successCount}   关闭:{errorCount}");
        }

        private static int successCount = 0;

        private static int errorCount = 0;

        private static void Service_OnReceivedEvent(DataReceivedArgs obj)
        {
            var model = ExchangeData.ParseFrom(obj.Data);
            Console.WriteLine($"data: {model.SequenceId} {model.MessageType} {model.IsRequest} {model.JsonBody}");
            service.Send(obj.SessionId, obj.Data);
        }

        private static void Service_DataReceived(byte[] obj)
        {
            Console.WriteLine(UTF8Encoding.UTF8.GetString(obj));


        }
    }
}
