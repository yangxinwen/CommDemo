using System;
using System.IO;
using System.Net.Sockets;
using System.Threading;
using System.Net;
using System.Text;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using System.Diagnostics;
using System.Threading.Tasks;

namespace DuiAsynSocket
{
    /// <summary>
    /// Based on example from http://msdn2.microsoft.com/en-us/library/system.net.sockets.socketasynceventargs.aspx
    /// Implements the connection logic for the socket server.  
    /// After accepting a connection, all data read from the client is sent back. 
    /// The read and echo back to the client pattern is continued until the client disconnects.
    /// </summary>
    public sealed class SocketListener
    {
        #region Fields  
        /// <summary>
        /// The socket used to listen for incoming connection requests.
        /// </summary>
        private Socket listenSocket;

        /// <summary>
        /// Buffer size to use for each socket I/O operation.
        /// </summary>
        private Int32 bufferSize;

        /// <summary>
        /// Pool of reusable SocketAsyncEventArgs objects for write, read and accept socket operations.
        /// </summary>
        private SocketAsyncEventArgsPool readWritePool;
        /// <summary>
        /// 存储sessionid和用户socket数据
        /// </summary>
        private Hashtable _clients = null;
        #endregion

        #region Properties
        /// <summary>
        /// 抛出日志内容事件
        /// </summary>
        public Action<string> OnDisplayLog = null;
        /// <summary>
        /// 抛出异常日志内容事件
        /// </summary>
        public Action<Exception> OnDisplayExceptionLog = null;

        /// <summary>
        /// The total number of clients connected to the server.
        /// </summary>
        public int ConnectedCount
        {
            get
            {
                return _clients.Count;
            }

        }

        /// <summary>
        /// the maximum number of connections the sample is designed to handle simultaneously.
        /// </summary>
        private int _maxConnCount;
        public int MaxConnCount
        {
            get
            {
                return _maxConnCount;
            }

        }

        /// <summary>
        /// 客户端连接状态变更事件
        /// </summary>
        public event Action<ConnStatusChangeArgs> OnClientConnChange;
        /// <summary>
        /// 数据接收事件
        /// </summary>
        public event Action<DataReceivedArgs> OnReceived;

        private bool _isListening;
        /// <summary>
        /// 服务端状态,是否正在监听
        /// </summary>
        public bool IsListening
        {
            get
            {
                return _isListening;
            }
            set
            {
                if (value != _isListening)
                {
                    _isListening = value;
                    OnServiceStatusChange?.Invoke(new ConnStatusChangeArgs(value));
                }
            }
        }
        /// <summary>
        /// 服务端连接状态变更事件
        /// </summary>
        public event Action<ConnStatusChangeArgs> OnServiceStatusChange;

        /// <summary>
        /// 是否把网络字节顺序转为本地字节顺序
        /// </summary>
        public bool NetByteOrder { get; set; }

        /// <summary>
        /// 是否使用心跳验证,启用后要求客户端连接发送的第一个报文为验证报文
        /// </summary>
        public bool IsUseHeartBeatCertificate { get; set; } = true;
        /// <summary>
        /// 超时时间ms,默认5分钟,若一个链路在指定时间内都没有动作则断开该连接，为0则表示不主动断开
        /// </summary>
        public int SocketTimeOutMS { get; set; } = 5 * 60 * 1000;

        #endregion

        #region Constructors

        /// <summary>
        /// Create an uninitialized server instance.  
        /// To start the server listening for connection requests
        /// call the Init method followed by Start method.
        /// </summary>
        /// <param name="numConnections">Maximum number of connections to be handled simultaneously.</param>
        /// <param name="bufferSize">Buffer size to use for each socket I/O operation.</param>
        public SocketListener(int numConnections, int bufferSize)
        {
            _clients = new Hashtable(numConnections);
            this._maxConnCount = numConnections;
            this.bufferSize = bufferSize;

            this.readWritePool = new SocketAsyncEventArgsPool(numConnections);
            // Preallocate pool of SocketAsyncEventArgs objects.
            for (var i = 0; i < this._maxConnCount; i++)
            {
                // Add SocketAsyncEventArg to the pool.
                this.readWritePool.Push(CreateSocketAsync());
            }
        }

        #endregion

        #region Methods

        private void KillOutTimeSocket()
        {
            Task.Factory.StartNew(() =>
            {
                IEnumerable<AsyncUserToken> list;
                while (true)
                {
                    Thread.Sleep(60000);
                    try
                    {
                        //控制自动恢复接收新的连接，防止队列满了后会出现不能接收新连接的问题
                        if (_isAccepting == false)
                        {
                            if (_clients.Count < MaxConnCount)
                            {
                                try
                                {
                                    StartAccept();
                                }
                                catch (Exception ex)
                                {
                                    ShowLog("恢复接收新连接错误:" + ex.Message);
                                }
                            }

                        }


                        //StartAccept(_acceptEventArg);
                        if (SocketTimeOutMS <= 0)
                            continue;

                        lock (_clients.SyncRoot)
                        {
                            list = _clients.Values.Cast<AsyncUserToken>().ToList();
                        }
                        if (list != null)
                        {
                            foreach (var item in list)
                            {
                                if (item != null)
                                {
                                    if (Environment.TickCount - item.LastExchangeTime > SocketTimeOutMS)
                                    {
                                        CloseClient(item);
                                        ShowLog("断开超时链路:" + item.SessionId);
                                    }
                                }
                            }
                        }
                    }
                    catch
                    {

                    }

                }
            });
        }

        /// <summary>
        /// 创建一个新的异步操作
        /// </summary>
        /// <returns></returns>
        private SocketAsyncEventArgs CreateSocketAsync()
        {
            SocketAsyncEventArgs readWriteEventArg = new SocketAsyncEventArgs();
            readWriteEventArg.Completed += new EventHandler<SocketAsyncEventArgs>(OnIOCompleted);
            readWriteEventArg.SetBuffer(new Byte[this.bufferSize], 0, this.bufferSize);
            return readWriteEventArg;
        }

        /// <summary>
        /// Starts the server listening for incoming connection requests.
        /// </summary>
        /// <param name="port">Port where the server will listen for connection requests.</param>
        public void Start(IPEndPoint localEndPoint)
        {
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
                this.listenSocket.Bind(localEndPoint);
            }
            else
            {
                // Associate the socket with the local endpoint.
                this.listenSocket.Bind(localEndPoint);
            }

            // Start the server.
            this.listenSocket.Listen(518);

            IsListening = true;

            // Post accepts on the listening socket.
            if (_acceptEventArg == null)
            {
                _acceptEventArg = new SocketAsyncEventArgs();
                _acceptEventArg.Completed += new EventHandler<SocketAsyncEventArgs>(OnAcceptCompleted);
            }
            this.StartAccept();
            KillOutTimeSocket();

            //设置动态缓存管理的日志显示委托方法
            if (DynamicBufferManager.OnDisplayLog == null && this.OnDisplayLog != null)
                DynamicBufferManager.OnDisplayLog = this.OnDisplayLog;
        }

        /// <summary>
        /// 是否在等待接收新连接，控制自动恢复接收新的连接，防止队列满了后会出现不能接收新连接的问题
        /// </summary>
        private bool _isAccepting = false;

        /// <summary>
        /// Callback method associated with Socket.AcceptAsync 
        /// operations and is invoked when an accept operation is complete.
        /// </summary>
        /// <param name="sender">Object who raised the event.</param>
        /// <param name="e">SocketAsyncEventArg associated with the completed accept operation.</param>
        private void OnAcceptCompleted(object sender, SocketAsyncEventArgs e)
        {
            try
            {
                Trace.WriteLine("accepted");
                _isAccepting = false;
                this.ProcessAccept(e);
            }
            catch (Exception ex)
            {
                ShowLog(ex);
            }
        }



        /// <summary>
        /// Begins an operation to accept a connection request from the client.
        /// </summary>
        /// <param name="acceptEventArg">The context object to use when issuing 
        /// the accept operation on the server's listening socket.</param>
        private void StartAccept()
        {
            _acceptEventArg.AcceptSocket = null;
            Trace.WriteLine("accepting");
            _isAccepting = true;
            if (!this.listenSocket.AcceptAsync(_acceptEventArg))
            {
                this.ProcessAccept(_acceptEventArg);
            }
        }

        /// <summary>
        /// Callback called whenever a receive or send operation is completed on a socket.
        /// </summary>
        /// <param name="sender">Object who raised the event.</param>
        /// <param name="e">SocketAsyncEventArg associated with the completed send/receive operation.</param>
        private void OnIOCompleted(object sender, SocketAsyncEventArgs e)
        {
            try
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
            catch (Exception ex)
            {
                ShowLog(ex);
            }
        }

        private SocketAsyncEventArgs _acceptEventArg = null;

        /// <summary>
        /// Process the accept for the socket listener.
        /// </summary>
        /// <param name="e">SocketAsyncEventArg associated with the completed accept operation.</param>
        private void ProcessAccept(SocketAsyncEventArgs e)
        {
            try
            {
                Socket s = e.AcceptSocket;
                if (s != null && s.Connected)
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
                            if (readEventArgs.Buffer == null)
                                readEventArgs.SetBuffer(new Byte[this.bufferSize], 0, this.bufferSize);
                            //添加到客户列表
                            AddClient(token.SessionId, token);

                            if (!s.ReceiveAsync(readEventArgs))
                            {
                                this.ProcessReceive(readEventArgs);
                            }
                        }
                        else
                        {
                            ShowLog("There are no more available sockets to allocate.");
                        }
                    }
                    catch (SocketException ex)
                    {
                        AsyncUserToken token = e.UserToken as AsyncUserToken;
                        ShowLog(string.Format("Error when processing data received from {0}:\r\n{1}", token.Socket.RemoteEndPoint, ex.ToString()));
                    }
                    catch (Exception ex)
                    {
                        ShowLog(ex);
                    }
                }
                else
                    StartAccept();

            }
            catch (Exception ex)
            {
                ShowLog(ex);
            }
        }

        private void ProcessError(SocketAsyncEventArgs e)
        {
            try
            {
                AsyncUserToken token = e.UserToken as AsyncUserToken;
                this.CloseClient(token);
            }
            catch (Exception ex)
            {
                ShowLog(ex);
            }
        }

        /// <summary>
        /// 显示日志内容
        /// </summary>
        /// <param name="content"></param>
        private void ShowLog(string content)
        {
            try
            {
                OnDisplayLog?.Invoke("SocketListener:" + content);
                Console.WriteLine(content);
            }
            catch (Exception)
            {

            }
        }
        /// <summary>
        /// 显示异常日志内容
        /// </summary>
        /// <param name="ex"></param>
        private void ShowLog(Exception ex)
        {
            try
            {
                OnDisplayExceptionLog?.Invoke(ex);
                Console.WriteLine(ex);
            }
            catch (Exception)
            {

            }
        }

        /// <summary>
        /// 验证心跳数据
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        private bool ValidHeartBeats(byte[] data)
        {
            if (data.Length == 16)
            {
                string tmpStr = Encoding.UTF8.GetString(data);
                DateTime dt = DateTime.Parse("1900-08-01 07:00:00");
                if (DateTime.TryParse(tmpStr, out dt))
                {
                    //时间差不超过5分钟则有效
                    var sec = (dt - DateTime.Now).TotalSeconds;
                    if (Math.Abs(sec) < 300000)
                    {
                        return true;

                    }
                }
            }
            return false;
        }

        /// <summary>
        /// This method is invoked when an asynchronous receive operation completes. 
        /// If the remote host closed the connection, then the socket is closed.  
        /// If data was received then the data is echoed back to the client.
        /// </summary>
        /// <param name="e">SocketAsyncEventArg associated with the completed receive operation.</param>
        private void ProcessReceive(SocketAsyncEventArgs e)
        {
            try
            {
                AsyncUserToken token = e.UserToken as AsyncUserToken;
                token.LastExchangeTime = Environment.TickCount;

                token.AsynSocketArgs = e;
                // Check if the remote host closed the connection.
                if (e.BytesTransferred > 0)
                {
                    if (e.SocketError == SocketError.Success)
                    {
                        if (IsSplitPack)
                        {
                            //if (e.BytesTransferred == 16)
                            //{
                            //    //组装心跳  todo:暂时使用，兼容以前的客户端，以前的通讯组件心跳包没加长度
                            //    var buff = new byte[20];
                            //    Array.Copy(BitConverter.GetBytes(16), 0, buff, 0, 4);
                            //    Array.Copy(e.Buffer, e.Offset, buff, 4, e.BytesTransferred);
                            //    DynamicBufferManager.WriteBuffer(token.SessionId, buff, 0, buff.Length);
                            //}
                            //else
                            {
                                DynamicBufferManager.WriteBuffer(token.SessionId, e.Buffer, e.Offset, e.BytesTransferred);
                            }

                            var list = DynamicBufferManager.PopPackets(token.SessionId);
                            foreach (var item in list)
                            {
                                if (IsUseHeartBeatCertificate && token.IsCertified == false)
                                {
                                    token.IsCertified = ValidHeartBeats(item);
                                    if (token.IsCertified)
                                    {
                                        continue;
                                    }
                                    else
                                    {
                                        ShowLog("客户端未通过心跳验证，关闭该连接");
                                        CloseClient(token);
                                        return;
                                    }
                                }
                                else if (IsUseHeartBeatCertificate && item.Length == 16)
                                {
                                    //若客户端发送的是心跳数据则跳过
                                    if (ValidHeartBeats(item))
                                        continue;
                                }
                                RaiseOnReceive(new DataReceivedArgs(token.SessionId, item));
                            }
                        }
                        else
                        {
                            var data = new byte[e.BytesTransferred];
                            Array.Copy(e.Buffer, e.Offset, data, 0, e.BytesTransferred);
                            RaiseOnReceive(new DataReceivedArgs(token.SessionId, data));
                        }

                        if (token.Socket.Connected)
                        {
                            if (!token.Socket.ReceiveAsync(e))
                            {
                                // Read the next block of data sent by client.
                                this.ProcessReceive(e);
                            }
                        }
                    }
                    else
                    {
                        this.ProcessError(e);
                    }
                }
                else
                {
                    this.CloseClient(token);
                }
            }
            catch (Exception ex)
            {
                this.ProcessError(e);
                ShowLog(ex);
            }
        }

        private void AddClient(string sessionId, AsyncUserToken token)
        {
            lock (_clients.SyncRoot)
            {
                _clients.Add(sessionId, token);
                if (_clients.Count < MaxConnCount)
                    StartAccept();
            }
            ShowLog($"{token.Socket?.RemoteEndPoint?.ToString()} 连接成功");
            RaiseOnClientConnChange(new ConnStatusChangeArgs(sessionId, true));
        }
        private void RemoveClient(string sessionId)
        {
            lock (_clients.SyncRoot)
            {
                if (_clients.ContainsKey(sessionId))
                {
                    _clients.Remove(sessionId);
                    if (_clients.Count == MaxConnCount - 1)
                        StartAccept();
                }
            }
            RaiseOnClientConnChange(new ConnStatusChangeArgs(sessionId, false));
        }

        private void RaiseOnClientConnChange(ConnStatusChangeArgs args)
        {
            try
            {
                OnClientConnChange?.Invoke(args);
            }
            catch (Exception)
            {

            }
        }

        private void RaiseOnReceive(DataReceivedArgs args)
        {
            try
            {
                //var data = args.Data;
                OnReceived?.Invoke(args);
            }
            catch (Exception)
            {

            }
        }

        #region 发送数据   
        /// <summary>
        /// 是否分包标识，每次数据发送和接收的前4个字节代表数据长度
        /// </summary>
        public bool IsSplitPack = true;
        /// <summary>
        /// 同步发送数据
        /// </summary>
        /// <param name="sessionId"></param>
        /// <param name="data"></param>
        /// <param name="timeout">超时时间(ms)</param>
        /// <returns></returns>
        public int Send(string sessionId, byte[] data, int timeout = 0)
        {
            int sent = 0; // how many bytes is already sent
            if (_clients.ContainsKey(sessionId))
            {
                var token = _clients[sessionId] as AsyncUserToken;
                var socket = token.Socket;

                if (socket == null || socket.Connected == false)
                    return 0;

                token.LastExchangeTime = Environment.TickCount;

                socket.SendTimeout = timeout;

                try
                {
                    if (IsSplitPack)
                    {
                        var lenght = data.Length;
                        var list = new List<byte>(BitConverter.GetBytes(lenght));
                        list.AddRange(data);
                        sent += socket.Send(list.ToArray());
                    }
                    else
                    {
                        sent += socket.Send(data);
                    }
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
                        return 0;
                        //throw ex; // any serious error occurr
                    }
                }
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

        public IEnumerable<string> GetClients()
        {
            return _clients.Keys.Cast<string>();
        }

        /// <summary>
        /// 根据sessionId获取指定客户端的socket
        /// </summary>
        /// <param name="sessionId"></param>
        /// <returns></returns>
        public Socket GetClient(string sessionId)
        {
            if (_clients.ContainsKey(sessionId))
                return (_clients[sessionId] as AsyncUserToken).Socket;
            else
                return null;
        }
        /// <summary>
        /// 根据sessionId获取指定客户端的socket
        /// </summary>
        /// <param name="sessionId"></param>
        /// <returns></returns>
        public void CloseClient(string sessionId)
        {
            if (_clients.ContainsKey(sessionId))
                CloseClient(_clients[sessionId] as AsyncUserToken);
        }

        /// <summary>
        /// Close the socket associated with the client.
        /// </summary>
        /// <param name="e">SocketAsyncEventArg associated with the completed send/receive operation.</param>
        private void CloseClient(AsyncUserToken token)
        {
            lock (token)
            {
                if (token == null || _clients.ContainsKey(token.SessionId) == false)
                    return;
                try
                {
                    DynamicBufferManager.Remove(token.SessionId);
                    var args = token.AsynSocketArgs;
                    //释放资源
                    //this.semaphoreAcceptedClients.Release();
                    //回收到pool中
                    this.readWritePool.Push(args);
                    RemoveClient(token.SessionId);
                    ShowLog($"{token.Socket?.RemoteEndPoint?.ToString()} 连接已关闭");
                    token.Dispose();
                }
                catch (Exception)
                {

                }
            }
        }

        /// <summary>
        /// Stop the server.
        /// </summary>
        public void Stop()
        {
            this.listenSocket.Close();
            IsListening = false;
            var values = new AsyncUserToken[_clients.Count];
            _clients.Values.CopyTo(values, 0);
            foreach (var item in values)
            {
                CloseClient(item);
            }
            readWritePool.Clear();
            //主动回收内存，否则内存不会被马上回收
            GC.Collect();
        }

        #endregion
    }
}
