using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using DuiAsynSocket;

namespace XXJR.Communication
{
    public class StatusChangeArgs
    {
        public DuiTcpClient Sender { get; set; }
        public ConnectStatus Status { get; set; }
    }


    public class DuiTcpClient
    {
        private ConcurrentQueue<byte[]> _receivedDataQueue = new ConcurrentQueue<byte[]>();
        public IPEndPoint EndPoint { get; set; }
        public string IP
        {
            get
            {
                if (EndPoint != null)
                    return EndPoint.Address.ToString();
                else
                    return string.Empty;
            }
        }
        public int Port
        {
            get
            {
                if (EndPoint != null)
                    return EndPoint.Port;
                else
                    return 0;
            }
        }
        public TcpClient Client
        {
            get
            {
                return _client;
            }
        }
        private TcpClient _client = null;
        public DuiTcpClient()
        {

        }
        public DuiTcpClient(TcpClient client)
        {
            if (client == null)
                throw new Exception("client is null");
            _client = client;
            ReceiveData();
            StartDistributeReceiveEvent();
        }

        public int MaxReciveBuffer { get; set; } = 1024;
        private byte[] _receiveBuffer;

        public event Action<byte[]> DataReceived;
        public event Action<ConnectStatus> StatusChange;

        public ConnectStatus ConnectStatus { get; private set; } = ConnectStatus.Created;
        public string SeesionId { get; } = Guid.NewGuid().ToString();

        private void UpdateStatus(ConnectStatus status)
        {
            ConnectStatus = status;
            var list = StatusChange.GetInvocationList();
            foreach (var item in list)
            {
                try
                {
                    ((Action<ConnectStatus>)item).Invoke(status);
                }
                catch (Exception)
                {
                }
            }
        }

        public void Connect()
        {
            if (EndPoint == null)
                throw new Exception("EndPoint is null");
            if (_client == null)
            {
                _client = new TcpClient();
                _client.Connect(IP, Port);
                UpdateStatus(ConnectStatus.Connected);
                if (_client.Connected == false)
                {
                    _client = null;
                    return;
                }

                ReceiveData();
                StartDistributeReceiveEvent();
            }
            else
                _client.Connect(IP, Port);
        }
        private bool _isStartDistributing = false;
        private void StartDistributeReceiveEvent()
        {
            if (_isStartDistributing)
                return;
            _isStartDistributing = true;

            byte[] model = null;
            Task.Factory.StartNew(() =>
            {
                while (true)
                {
                    if (_receivedDataQueue.IsEmpty)
                    {
                        Thread.Sleep(10);
                        continue;
                    }
                    try
                    {
                        if (_receivedDataQueue.TryDequeue(out model))
                        {
                            DataReceived?.Invoke(model);
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(ex.Message);
                    }
                }
            });
        }

        private bool _isReceiving = false;
        private void ReceiveData()
        {
            if (_isReceiving)
                return;
            _isReceiving = true;

            Task.Factory.StartNew(() =>
            {
                _receiveBuffer = new byte[MaxReciveBuffer];
                int receiveCount = 0;
                NetworkStream stream = null;
                while (true)
                {
                    try
                    {
                        if (_client.Connected == false)
                            break;

                        stream = _client.GetStream();
                        receiveCount = stream.Read(_receiveBuffer, 0, _receiveBuffer.Length);
                        //stream = null;

                        if (receiveCount <= 0)
                        {
                            Thread.Sleep(100);
                            continue;
                        }
                        var bytes = new byte[receiveCount];
                        Array.Copy(_receiveBuffer, 0, bytes, 0, receiveCount);
                        _receivedDataQueue.Enqueue(bytes);                        
                    }
                    catch (Exception ex)
                    {
                        UpdateStatus(ConnectStatus.Fault);
                        _isReceiving = false;
                        break;
                    }
                }
            });
        }

        public bool Send(byte[] data)
        {
            var count = 0;
            if (_client != null && _client.Connected)
                count = _client.Client.Send(data);
            if (count > 0)
                return true;
            else
                return false;
        }

    }
}
