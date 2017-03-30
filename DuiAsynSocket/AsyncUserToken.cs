using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;

namespace DuiAsynSocket
{
    /// <summary>
    /// 异步操作的用户token信息
    /// </summary>
    public class AsyncUserToken : IDisposable
    {
        public SocketAsyncEventArgs AsynSocketArgs { get; set; }
        public Socket Socket { get; private set; }

        public string SessionId
        {
            get; set;
            //get
            //{
            //    if (Socket != null)
            //        return Socket.Handle.ToString();
            //    else
            //        return string.Empty;
            //}
        }

        public AsyncUserToken(SocketAsyncEventArgs args)
        {
            AsynSocketArgs = args;
            Socket = args.AcceptSocket;
            SessionId = Guid.NewGuid().ToString();
        }
        public void Dispose()
        {
            if (Socket != null)
            {
                //Socket.Shutdown(SocketShutdown.Send);
                Socket.Close();
            }
            AsynSocketArgs = null;
        }
    }
}
