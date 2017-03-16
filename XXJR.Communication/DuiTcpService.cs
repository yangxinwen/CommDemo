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
        public event Action<byte[]> DataReceived;
        TcpListener _tcpListener = null;

        public IPEndPoint EndPoint { get; set; }

        public Dictionary<string, DuiTcpClient> ClientList { get; } = new Dictionary<string, DuiTcpClient>();

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
            Console.WriteLine(ClientList.Count);
            remoteClient.DataReceived += RemoteClient_DataReceived;
            remoteClient.StatusChange += (e) =>
            {
                if (e == ConnectStatus.Fault)
                {
                    ClientList.Remove(remoteClient.SeesionId);
                }
            };
        }

        private bool Send(string sessionId, byte[] data)
        {
            if (ClientList.ContainsKey(sessionId))
                return ClientList[sessionId].Send(data);
            else
                return false;
        }

        private  void RemoteClient_DataReceived(byte[] obj)
        {
            DataReceived?.Invoke(obj);
        }
    }
}
