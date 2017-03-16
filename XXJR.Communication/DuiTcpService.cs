using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace XXJR.Communication
{
    public class DuiTcpService
    {
        TcpListener _tcpListener = null;

        public IPEndPoint EndPoint { get; set; }

        public static Dictionary<string, DuiTcpClient> ClientList { get; } = new Dictionary<string, DuiTcpClient>();

        public DuiTcpService()
        {
        }

        public void Start()
        {
            if (EndPoint == null)
                throw new Exception("Need Set EndPoint");
            _tcpListener = new TcpListener(EndPoint);
            _tcpListener.Start();
            Task.Factory.StartNew(() => Listen());
        }


        public void Listen()
        {
            while (true)
            {
                _tcpListener.BeginAcceptTcpClient(new AsyncCallback(AcceptTcpClient), null);
                Thread.Sleep(100);
            }
        }

        public void AcceptTcpClient(IAsyncResult ar)
        {
            var remoteClient = new DuiTcpClient(_tcpListener.EndAcceptTcpClient(ar));
            ClientList.Add(remoteClient.SeesionId, remoteClient);
            remoteClient.DataReceived += RemoteClient_DataReceived;

            remoteClient.StatusChange += (e) =>
            {
                if (e.Status == ConnectStatus.Fault)
                    ClientList.Remove(e.Sender.SeesionId);
            };
        }


        private void RemoteClient_DataReceived(byte[] obj)
        {
            Trace.WriteLine(obj.Length);
        }
    }
}
