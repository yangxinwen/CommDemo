using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DuiAsynSocket
{
    /// <summary>
    /// 连接状态
    /// </summary>
   public enum ConnectStatus
    {    
       Created,
       Connected,
       Listening,       
       Fault,
       Closed,
    }
    /// <summary>
    /// 连接状态变更事件参数
    /// </summary>
    public class ConnStatusChangeArgs
    {
        public string SessionId { get; set; }
        public ConnectStatus ConnStatus { get; set; }

        public ConnStatusChangeArgs(ConnectStatus status)
        {
            this.SessionId = string.Empty;
            this.ConnStatus = status;
        }
        public ConnStatusChangeArgs(string sessionId,ConnectStatus status)
        {
            this.SessionId = sessionId;
            this.ConnStatus = status;
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
        public DataReceivedArgs(string sessionId,byte[] data)
        {
            this.SessionId = sessionId;
            this.Data = data;
        }
    }
}
