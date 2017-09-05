using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using DuiAsynSocket;

namespace Service
{
    class Program
    {
        static void Main(string[] args)
        {

            var p = new Program();
            p.Start("", 1568);
            Console.ReadLine();
        }

        SocketListener _serviceListener = null;

        /// <summary>
        /// 开启AsyncSocketService 监听,指定IP
        /// </summary>
        /// <param name="ip"></param>
        /// <param name="port"></param>
        public void Start(string ip, int port)
        {
            try
            {
                IPAddress ipaddress = null;
                if (IPAddress.TryParse(ip, out ipaddress) == false)
                    ipaddress = IPAddress.Any;

                _serviceListener = new SocketListener(1000, 1024 * 4);


                _serviceListener.SocketTimeOutMS =  60 * 1000;
                IPEndPoint listenPoint = new IPEndPoint(ipaddress, port);
                _serviceListener.OnClientConnChange += _serviceListener_OnClientConnChange; ;
                _serviceListener.OnReceived += _serviceListener_OnReceived; ;
                _serviceListener.OnServiceStatusChange += _serviceListener_OnServiceStatusChange;

                _serviceListener.OnDisplayLog = (s) => { Console.WriteLine(s); };
                _serviceListener.OnDisplayExceptionLog = (s) => { Console.WriteLine(s); };

                _serviceListener.Start(listenPoint);

            }
            catch (Exception ex)
            {
            }
        }



        private void _serviceListener_OnServiceStatusChange(ConnStatusChangeArgs obj)
        {

        }

        private void _serviceListener_OnReceived(DataReceivedArgs obj)
        {
        }


        private int _successCount = 0;


        private void _serviceListener_OnClientConnChange(ConnStatusChangeArgs obj)
        {
            if (obj.IsConnected)
                Interlocked.Increment(ref _successCount);
            else
                Interlocked.Decrement(ref _successCount);
            Console.WriteLine("count:" + _successCount);
        }
    }
}
