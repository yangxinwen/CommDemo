using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DuiAsynSocket
{
    /// <summary>
    /// 连接状态变更事件参数
    /// </summary>
    public class ConnStatusChangeArgs
    {
        public string SessionId { get; set; }
        public bool IsConnected { get; set; }

        public ConnStatusChangeArgs(bool isConnected)
        {
            this.SessionId = string.Empty;
            this.IsConnected = isConnected;
        }
        public ConnStatusChangeArgs(string sessionId, bool isConnected)
        {
            this.SessionId = sessionId;
            this.IsConnected = isConnected;
        }
    }
    /// <summary>
    /// 数据接收事件参数
    /// </summary>
    public class DataReceivedArgs
    {
        public string SessionId { get; set; }
        public byte[] Data { get; set; }
        public DataReceivedArgs(byte[] data)
        {
            this.SessionId = string.Empty;
            this.Data = data;
        }
        public DataReceivedArgs(string sessionId, byte[] data)
        {
            this.SessionId = sessionId;
            this.Data = data;
        }
    }
}
