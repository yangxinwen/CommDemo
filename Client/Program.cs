using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace Service
{
    class Program
    {
        static void Main(string[] args)
        {
            var p = new Program();
            p.Connect("127.0.0.1", 1568);
            Console.ReadLine();
        }

        private void Connect(string ip, int port)
        {
            var client = new DuiAsynSocket.SocketClient();

            client.IsSplitPack = true;
            client.HeartBeatsEnable = true;
            client.IsUseHeartBeatCertificate = true;

            client.OnConnChangeEvent += _client_OnConnChangeEvent;
            client.OnReceivedEvent += _client_OnReceivedEvent;
            client.Connect(ip, port);
        }

        private void _client_OnReceivedEvent(DuiAsynSocket.DataReceivedArgs obj)
        {

        }

        private void _client_OnConnChangeEvent(DuiAsynSocket.ConnStatusChangeArgs obj)
        {
            Console.WriteLine(obj.IsConnected);
        }
    }
}
