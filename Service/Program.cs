using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using IOCPService;
using SocketAsyncServer;
using XXJR.Communication;

namespace Service
{
    class Program
    {
        static void Main(string[] args)
        {
            //var service =new DuiTcpService();
            //service.EndPoint = new System.Net.IPEndPoint(IPAddress.Any, 1991);
            //service.DataReceived += Service_DataReceived;
            //service.Start();


            //Task.Factory.StartNew(()=>
            //{
            //    Thread.Sleep(12 * 1000);
            //    foreach (var item in service.ClientList)
            //    {
            //        Console.WriteLine("回应-----------");
            //        item.Value.Send(UTF8Encoding.Default.GetBytes("你们好啊  haha "));
            //        //Thread.Sleep(100);
            //    }
            //});



            //var server = new Server(100000, 1024);
            //server.Init();
            //server.Start(new IPEndPoint(IPAddress.Any,1991));
            //Console.WriteLine("服务器已启动....");



            TestService();

            Console.ReadLine();
        }

       static SocketListener service = null;
        private static void TestService()
        {
            service = new SocketListener(10000,1024);
            service.Start(1991);
            service.OnReceivedEvent += Service_OnReceivedEvent;
            service.OnClientConnChangeEvent += Service_OnClientConnChangeEvent;

        }

        private static void Service_OnClientConnChangeEvent(ConnStatusChangeArgs obj)
        {
            Console.WriteLine("sessionId:"+obj.SessionId+ "  status:" + obj.ConnStatus.ToString());
        }

        private static void Service_OnReceivedEvent(DataReceivedArgs obj)
        {
            Console.WriteLine("data:"+UTF8Encoding.Default.GetString(obj.Data));
            service.Send(obj.SessionId, obj.Data);
        }

        private static void Service_DataReceived(byte[] obj)
        {
            Console.WriteLine(UTF8Encoding.Default.GetString(obj));


        }
    }
}
