using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using XXJR.Communication;

namespace SocketAsyncServer
{
    /// <summary>
    /// Implements the connection logic for the socket client.
    /// </summary>
    public sealed class SocketClient : IDisposable
    {
        /// <summary>
        /// Constants for socket operations.
        /// </summary>
        private const Int32 ReceiveOperation = 1, SendOperation = 0;

        /// <summary>
        /// The socket used to send/receive messages.
        /// </summary>
        private Socket _socket;

        private ConnectStatus _connStatus;

        public ConnectStatus ConnStatus
        {
            private set
            {
                if (_connStatus != value)
                {
                    _connStatus = value;
                    RaiseOnConnChange(new ConnStatusChangeArgs(value));
                }
            }
            get
            {
                return _connStatus;
            }
        }

        /// <summary>
        /// Listener endpoint.
        /// </summary>
        private IPEndPoint hostEndPoint;

        /// <summary>
        /// 客户端连接状态变更事件
        /// </summary>
        public event Action<ConnStatusChangeArgs> OnConnChangeEvent;
        /// <summary>
        /// 数据接收事件
        /// </summary>
        public event Action<DataReceivedArgs> OnReceivedEvent;

        /// <summary>
        /// Create an uninitialized client instance.  
        /// To start the send/receive processing
        /// call the Connect method followed by SendReceive method.
        /// </summary>
        /// <param name="hostName">Name of the host where the listener is running.</param>
        /// <param name="port">Number of the TCP port from the listener.</param>
        public SocketClient(String hostName, Int32 port)
        {
            // Get host related information.
            IPHostEntry host = Dns.GetHostEntry(hostName);

            // Addres of the host.
            IPAddress[] addressList = host.AddressList;

            // Instantiates the endpoint and socket.
            this.hostEndPoint = new IPEndPoint(addressList[addressList.Length - 2], port);
            this._socket = new Socket(this.hostEndPoint.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
            RaiseOnConnChange(new ConnStatusChangeArgs(string.Empty, ConnectStatus.Created));
        }
        SocketAsyncEventArgs connectArgs = null;
        /// <summary>
        /// Connect to the host.
        /// </summary>
        /// <returns>True if connection has succeded, else false.</returns>
        public void Connect()
        {
            connectArgs = new SocketAsyncEventArgs();
            connectArgs.UserToken = this._socket;
            connectArgs.RemoteEndPoint = this.hostEndPoint;
            //connectArgs.SetBuffer(new byte[1024], 0, 1024);
            connectArgs.Completed += new EventHandler<SocketAsyncEventArgs>(OnConnect);
            _socket.ConnectAsync(connectArgs);
            SocketError errorCode = connectArgs.SocketError;
            if (errorCode != SocketError.Success)
            {
                RaiseOnConnChange(new ConnStatusChangeArgs(string.Empty, ConnectStatus.Connected));
            }
        }

        /// <summary>
        /// Disconnect from the host.
        /// </summary>
        public void Disconnect()
        {
            _socket.Disconnect(false);
        }

        private void RaiseOnConnChange(ConnStatusChangeArgs args)
        {
            OnConnChangeEvent?.Invoke(args);
        }

        private void RaiseOnReceive(DataReceivedArgs args)
        {
            OnReceivedEvent?.Invoke(args);
        }

        private void OnConnect(object sender, SocketAsyncEventArgs e)
        {
            switch (e.LastOperation)
            {
                case SocketAsyncOperation.Connect:
                    ProcessConnect(e);
                    break;
                case SocketAsyncOperation.Receive:
                    ProcessReceive(e);
                    break;
                case SocketAsyncOperation.Send:
                    ProcessSend(e);
                    break;
                default:
                    break;
            }
        }
        private void ProcessSend(SocketAsyncEventArgs e)
        {
            if (e.SocketError == SocketError.Success)
            {
                AsyncUserToken token = e.UserToken as AsyncUserToken;

                if (!token.Socket.ReceiveAsync(e))
                {
                    this.ProcessReceive(e);
                }
            }
            else
            {
                this.ProcessError(e);
            }
        }
        private void ProcessReceive(SocketAsyncEventArgs e)
        {
            if (e.SocketError == SocketError.Success)
            {
                var data = new byte[e.BytesTransferred];
                Array.Copy(e.Buffer, e.Offset, data, 0, e.BytesTransferred);
                RaiseOnReceive(new DataReceivedArgs(data));

                if (_socket.ReceiveAsync(e) == false)
                {
                    ProcessReceive(e);
                }
            }
            else
            {
                this.ProcessError(e);
            }
        }

        SocketAsyncEventArgs so = null;
        private void ProcessConnect(SocketAsyncEventArgs e)
        {

            // Set the flag for socket connected.
            if (e.SocketError == SocketError.Success)
            {
                so = new SocketAsyncEventArgs();
                so.Completed += new EventHandler<SocketAsyncEventArgs>(OnConnect);
                so.UserToken = new AsyncUserToken(e);
                so.SetBuffer(new Byte[1024], 0, 1024);
                //so.RemoteEndPoint = hostEndPoint;
                if (_socket.ReceiveAsync(so) == false)
                    ProcessReceive(so);

                ConnStatus = ConnectStatus.Connected;        
            }
            else
                ConnStatus = ConnectStatus.Fault;
        }

        /// <summary>
        /// Close socket in case of failure and throws a SockeException according to the SocketError.
        /// </summary>
        /// <param name="e">SocketAsyncEventArg associated with the failed operation.</param>
        private void ProcessError(SocketAsyncEventArgs e)
        {
            Socket s = e.UserToken as Socket;
            if (s.Connected)
            {
                // close the socket associated with the client
                try
                {
                    s.Shutdown(SocketShutdown.Both);
                }
                catch (Exception)
                {
                    // throws if client process has already closed
                }
                finally
                {
                    if (s.Connected)
                    {
                        s.Close();
                    }
                }
            }
            ConnStatus = ConnectStatus.Fault;
        }


        #region 发送数据

        /// <summary>
        /// <summary>
        /// 异步的发送数据
        /// </summary>
        /// <param name="e"></param>
        /// <param name="data"></param>
        public void SendAsyn(byte[] data)
        {
            if (so != null)
            {
                var e = so;
                if (e.SocketError == SocketError.Success)
                {
                    Socket s = _socket;//和客户端关联的socket
                    if (s.Connected)
                    {
                        Array.Copy(data, 0, e.Buffer, 0, data.Length);//设置发送数据

                        e.SetBuffer(data, 0, data.Length); //设置发送数据
                        if (!s.SendAsync(e))//投递发送请求，这个函数有可能同步发送出去，这时返回false，并且不会引发SocketAsyncEventArgs.Completed事件
                        {
                            // 同步发送时处理发送完成事件
                            //ProcessSend(e);
                        }
                        else
                        {
                            //CloseClientSocket(e);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// 同步发送数据
        /// </summary>
        /// <param name="sessionId"></param>
        /// <param name="data"></param>
        /// <param name="timeout">超时时间(ms)</param>
        /// <returns></returns>
        public int Send(byte[] data, int timeout = 0)
        {
            int sent = 0; // how many bytes is already sent
            if (connectArgs != null)
            {
                var socket = _socket;
                socket.SendTimeout = 0;
                int startTickCount = Environment.TickCount;
                //使用do while后期可改造大数据分多次发送
                do
                {
                    if (timeout > 0 && (Environment.TickCount > startTickCount + timeout))
                    {
                        return sent;
                    }
                    try
                    {
                        //sent += socket.Send(data, sent, data.Length, SocketFlags.None);
                        sent += socket.Send(data);
                        break;
                    }
                    catch (SocketException ex)
                    {
                        if (ex.SocketErrorCode == SocketError.WouldBlock ||
                        ex.SocketErrorCode == SocketError.IOPending ||
                        ex.SocketErrorCode == SocketError.NoBufferSpaceAvailable)
                        {
                            // socket buffer is probably full, wait and try again
                            Thread.Sleep(30);
                        }
                        else
                        {
                            break; // any serious error occurr
                        }
                    }
                } while (true);
            }
            return sent;
        }

        #endregion



        #region IDisposable Members

        /// <summary>
        /// Disposes the instance of SocketClient.
        /// </summary>
        public void Dispose()
        {
            if (this._socket.Connected)
            {
                this._socket.Close();
            }
            this.ConnStatus = ConnectStatus.Closed;
        }

        #endregion
    }
}
