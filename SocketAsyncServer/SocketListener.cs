using System;
using System.IO;
using System.Net.Sockets;
using System.Threading;
using System.Net;
using System.Text;
using System.Collections.Generic;
using XXJR.Communication;

namespace SocketAsyncServer
{
    /// <summary>
    /// Based on example from http://msdn2.microsoft.com/en-us/library/system.net.sockets.socketasynceventargs.aspx
    /// Implements the connection logic for the socket server.  
    /// After accepting a connection, all data read from the client is sent back. 
    /// The read and echo back to the client pattern is continued until the client disconnects.
    /// </summary>
    public sealed class SocketListener
    {
        /// <summary>
        /// The socket used to listen for incoming connection requests.
        /// </summary>
        private Socket listenSocket;

        /// <summary>
        /// Buffer size to use for each socket I/O operation.
        /// </summary>
        private Int32 bufferSize;

        /// <summary>
        /// The total number of clients connected to the server.
        /// </summary>
        public Int32 ConnectedCount
        {
            get
            {
                return _clientDic.Count;
            }

        }

        /// <summary>
        /// the maximum number of connections the sample is designed to handle simultaneously.
        /// </summary>
        private Int32 _maxConnCount;
        public Int32 MaxConnCount
        {
            get
            {
                return _maxConnCount;
            }
            
        }

        /// <summary>
        /// Pool of reusable SocketAsyncEventArgs objects for write, read and accept socket operations.
        /// </summary>
        private SocketAsyncEventArgsPool readWritePool;

        /// <summary>
        /// Controls the total number of clients connected to the server.
        /// </summary>
        private Semaphore semaphoreAcceptedClients;

        private Dictionary<string, AsyncUserToken> _clientDic = new Dictionary<string, AsyncUserToken>();
        /// <summary>
        /// 客户端连接状态变更事件
        /// </summary>
        public event Action<ConnStatusChangeArgs> OnClientConnChangeEvent;
        /// <summary>
        /// 数据接收事件
        /// </summary>
        public event Action<DataReceivedArgs> OnReceivedEvent;

        /// <summary>
        /// Create an uninitialized server instance.  
        /// To start the server listening for connection requests
        /// call the Init method followed by Start method.
        /// </summary>
        /// <param name="numConnections">Maximum number of connections to be handled simultaneously.</param>
        /// <param name="bufferSize">Buffer size to use for each socket I/O operation.</param>
        public SocketListener(Int32 numConnections, Int32 bufferSize)
        {
            this._maxConnCount = numConnections;
            this.bufferSize = bufferSize;

            this.readWritePool = new SocketAsyncEventArgsPool(numConnections);
            this.semaphoreAcceptedClients = new Semaphore(numConnections, numConnections);

            // Preallocate pool of SocketAsyncEventArgs objects.
            for (Int32 i = 0; i < this._maxConnCount; i++)
            {
                SocketAsyncEventArgs readWriteEventArg = new SocketAsyncEventArgs();
                readWriteEventArg.Completed += new EventHandler<SocketAsyncEventArgs>(OnIOCompleted);
                readWriteEventArg.SetBuffer(new Byte[this.bufferSize], 0, this.bufferSize);

                // Add SocketAsyncEventArg to the pool.
                this.readWritePool.Push(readWriteEventArg);
            }
        }

        /// <summary>
        /// Close the socket associated with the client.
        /// </summary>
        /// <param name="e">SocketAsyncEventArg associated with the completed send/receive operation.</param>
        private void CloseClientSocket(SocketAsyncEventArgs e)
        {
            AsyncUserToken token = e.UserToken as AsyncUserToken;
            RemoveClient(token.SessionId);
            token.Socket.Close();
            // Decrement the counter keeping track of the total number of clients connected to the server.
            this.semaphoreAcceptedClients.Release();

            // Free the SocketAsyncEventArg so they can be reused by another client.
            this.readWritePool.Push(e);
        }

        private void AddClient(string sessionId, AsyncUserToken token)
        {
            _clientDic.Add(sessionId, token);
            RaiseOnClientConnChange(new ConnStatusChangeArgs(sessionId, ConnectStatus.Connected));
        }
        private void RemoveClient(string sessionId)
        {
            _clientDic.Remove(sessionId);
            RaiseOnClientConnChange(new ConnStatusChangeArgs(sessionId, ConnectStatus.Closed));
        }

        private void RaiseOnClientConnChange(ConnStatusChangeArgs args)
        {
            OnClientConnChangeEvent?.Invoke(args);
        }

        private void RaiseOnReceive(DataReceivedArgs args)
        {
            OnReceivedEvent?.Invoke(args);
        }

        /// <summary>
        /// Callback method associated with Socket.AcceptAsync 
        /// operations and is invoked when an accept operation is complete.
        /// </summary>
        /// <param name="sender">Object who raised the event.</param>
        /// <param name="e">SocketAsyncEventArg associated with the completed accept operation.</param>
        private void OnAcceptCompleted(object sender, SocketAsyncEventArgs e)
        {
            this.ProcessAccept(e);
        }

        /// <summary>
        /// Callback called whenever a receive or send operation is completed on a socket.
        /// </summary>
        /// <param name="sender">Object who raised the event.</param>
        /// <param name="e">SocketAsyncEventArg associated with the completed send/receive operation.</param>
        private void OnIOCompleted(object sender, SocketAsyncEventArgs e)
        {
            // Determine which type of operation just completed and call the associated handler.
            switch (e.LastOperation)
            {
                case SocketAsyncOperation.Receive:
                    this.ProcessReceive(e);
                    break;
                case SocketAsyncOperation.Send:
                    this.ProcessSend(e);
                    break;
                default:
                    throw new ArgumentException("The last operation completed on the socket was not a receive or send");
            }
        }

        /// <summary>
        /// Process the accept for the socket listener.
        /// </summary>
        /// <param name="e">SocketAsyncEventArg associated with the completed accept operation.</param>
        private void ProcessAccept(SocketAsyncEventArgs e)
        {
            Socket s = e.AcceptSocket;
            if (s.Connected)
            {
                try
                {
                    SocketAsyncEventArgs readEventArgs = this.readWritePool.Pop();
                    if (readEventArgs != null)
                    {
                        // Get the socket for the accepted client connection and put it into the 
                        // ReadEventArg object user token.
                        var token = new AsyncUserToken(e);
                        readEventArgs.UserToken = token;

                        //添加到客户列表
                        AddClient(token.SessionId, token);

                        if (!s.ReceiveAsync(readEventArgs))
                        {
                            this.ProcessReceive(readEventArgs);
                        }
                    }
                    else
                    {
                        Console.WriteLine("There are no more available sockets to allocate.");
                    }
                }
                catch (SocketException ex)
                {
                    AsyncUserToken token = e.UserToken as AsyncUserToken;
                    Console.WriteLine("Error when processing data received from {0}:\r\n{1}", token.Socket.RemoteEndPoint, ex.ToString());
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.ToString());
                }

                // Accept the next connection request.
                this.StartAccept(e);
            }
        }

        private void ProcessError(SocketAsyncEventArgs e)
        {
            AsyncUserToken token = e.UserToken as AsyncUserToken;
            IPEndPoint localEp = token.Socket.LocalEndPoint as IPEndPoint;

            this.CloseClientSocket(e);

            Console.WriteLine("Socket error {0} on endpoint {1} during {2}.", (Int32)e.SocketError, localEp, e.LastOperation);
        }

        /// <summary>
        /// This method is invoked when an asynchronous receive operation completes. 
        /// If the remote host closed the connection, then the socket is closed.  
        /// If data was received then the data is echoed back to the client.
        /// </summary>
        /// <param name="e">SocketAsyncEventArg associated with the completed receive operation.</param>
        private void ProcessReceive(SocketAsyncEventArgs e)
        {
            // Check if the remote host closed the connection.
            if (e.BytesTransferred > 0)
            {
                if (e.SocketError == SocketError.Success)
                {
                    AsyncUserToken token = e.UserToken as AsyncUserToken;
                    var bytes = new byte[e.BytesTransferred];
                    Array.Copy(e.Buffer, e.Offset, bytes, 0, e.BytesTransferred);
                    RaiseOnReceive(new DataReceivedArgs(token.SessionId, bytes));

                    if (!token.Socket.ReceiveAsync(e))
                    {
                        // Read the next block of data sent by client.
                        this.ProcessReceive(e);
                    }
                }
                else
                {
                    this.ProcessError(e);
                }
            }
            else
            {
                this.CloseClientSocket(e);
            }
        }


        #region 发送数据

        /// <summary>
        /// 异步的发送数据
        /// </summary>
        /// <param name="e"></param>
        /// <param name="data"></param>
        //public void SendAsyn(string sessionId, byte[] data)
        //{
        //    if (_clientDic.ContainsKey(sessionId))
        //    {
        //        var e = _clientDic[sessionId].SocketArgs;
        //        if (e.SocketError == SocketError.Success)
        //        {
        //            Socket s = e.AcceptSocket;//和客户端关联的socket
        //            if (s.Connected)
        //            {
        //                Array.Copy(data, 0, e.Buffer, 0, data.Length);//设置发送数据

        //                e.SetBuffer(data, 0, data.Length); //设置发送数据
        //                if (!s.SendAsync(e))//投递发送请求，这个函数有可能同步发送出去，这时返回false，并且不会引发SocketAsyncEventArgs.Completed事件
        //                {
        //                    // 同步发送时处理发送完成事件
        //                    ProcessSend(e);
        //                }
        //                else
        //                {
        //                    CloseClientSocket(e);
        //                }
        //            }
        //        }
        //    }
        //}

        /// <summary>
        /// 同步发送数据
        /// </summary>
        /// <param name="sessionId"></param>
        /// <param name="data"></param>
        /// <param name="timeout">超时时间(ms)</param>
        /// <returns></returns>
        public int Send(string sessionId, byte[] data, int timeout=10*1000)
        {
            int sent = 0; // how many bytes is already sent
            if (_clientDic.ContainsKey(sessionId))
            {
                var socket = _clientDic[sessionId].Socket;
                socket.SendTimeout = 0;
                int startTickCount = Environment.TickCount;
                //使用do while后期可改造大数据分多次发送
                do
                {
                    if (Environment.TickCount > startTickCount + timeout)
                    {
                        return sent;
                    }
                    try
                    {
                        sent += socket.Send(data, sent, data.Length, SocketFlags.None);
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
                            throw ex; // any serious error occurr
                        }
                    }
                } while (true);
            }
            return sent;
        }


        /// <summary>
        /// This method is invoked when an asynchronous send operation completes.  
        /// The method issues another receive on the socket to read any additional 
        /// data sent from the client.
        /// </summary>
        /// <param name="e">SocketAsyncEventArg associated with the completed send operation.</param>
        private void ProcessSend(SocketAsyncEventArgs e)
        {
            if (e.SocketError == SocketError.Success)
            {
                // Done echoing data back to the client.
                AsyncUserToken token = e.UserToken as AsyncUserToken;

                if (!token.Socket.ReceiveAsync(e))
                {
                    // Read the next block of data send from the client.
                    this.ProcessReceive(e);
                }
            }
            else
            {
                this.ProcessError(e);
            }
        }
        #endregion

        /// <summary>
        /// Starts the server listening for incoming connection requests.
        /// </summary>
        /// <param name="port">Port where the server will listen for connection requests.</param>
        public void Start(Int32 port)
        {
            // Get host related information.
            IPAddress[] addressList = Dns.GetHostEntry(Environment.MachineName).AddressList;

            // Get endpoint for the listener.
            IPEndPoint localEndPoint = new IPEndPoint(addressList[addressList.Length - 1], port);

            // Create the socket which listens for incoming connections.
            this.listenSocket = new Socket(localEndPoint.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
            this.listenSocket.ReceiveBufferSize = this.bufferSize;
            this.listenSocket.SendBufferSize = this.bufferSize;

            if (localEndPoint.AddressFamily == AddressFamily.InterNetworkV6)
            {
                // Set dual-mode (IPv4 & IPv6) for the socket listener.
                // 27 is equivalent to IPV6_V6ONLY socket option in the winsock snippet below,
                // based on http://blogs.msdn.com/wndp/archive/2006/10/24/creating-ip-agnostic-applications-part-2-dual-mode-sockets.aspx
                this.listenSocket.SetSocketOption(SocketOptionLevel.IPv6, (SocketOptionName)27, false);
                this.listenSocket.Bind(new IPEndPoint(IPAddress.IPv6Any, localEndPoint.Port));
            }
            else
            {
                // Associate the socket with the local endpoint.
                this.listenSocket.Bind(localEndPoint);
            }

            // Start the server.
            this.listenSocket.Listen(this._maxConnCount);

            // Post accepts on the listening socket.
            this.StartAccept(null);
        }

        /// <summary>
        /// Begins an operation to accept a connection request from the client.
        /// </summary>
        /// <param name="acceptEventArg">The context object to use when issuing 
        /// the accept operation on the server's listening socket.</param>
        private void StartAccept(SocketAsyncEventArgs acceptEventArg)
        {
            if (acceptEventArg == null)
            {
                acceptEventArg = new SocketAsyncEventArgs();
                acceptEventArg.Completed += new EventHandler<SocketAsyncEventArgs>(OnAcceptCompleted);
            }
            else
            {
                // Socket must be cleared since the context object is being reused.
                acceptEventArg.AcceptSocket = null;
            }

            this.semaphoreAcceptedClients.WaitOne();
            if (!this.listenSocket.AcceptAsync(acceptEventArg))
            {
                this.ProcessAccept(acceptEventArg);
            }
        }

        /// <summary>
        /// Stop the server.
        /// </summary>
        public void Stop()
        {
            foreach (var item in _clientDic)
            {
                CloseClientSocket(item.Value.SocketArgs);
            }

            this.listenSocket.Close();
        }
    }
}
