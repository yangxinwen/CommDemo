using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using Com.Xxjr.Core.Model.Contract;
using DuiContract;
using DuiHelpers;
using Google.ProtocolBuffers;

namespace Service
{
    class Program
    {
        static void Main(string[] args)
        {
            var p = new Program();
            p.Connect("192.168.31.70", 1568);
            Console.ReadLine();
        }

        private void Connect(string ip, int port)
        {
            for (int i = 0; i < 1000; i++)
            {
                try
                {
                    //Thread.Sleep(1000);
                    var client = new DuiAsynSocket.SocketClient();

                    client.IsSplitPack = true;
                    client.HeartBeatsEnable = true;
                    client.IsUseHeartBeatCertificate = true;

                    client.OnConnChangeEvent += _client_OnConnChangeEvent;
                    client.OnReceivedEvent += _client_OnReceivedEvent;
                    client.Connect(ip, port);


                    string[] loginInfos = new string[]
                      {
                    "yangxinwen",
                    Md5Hash.GetMd5Hash("12345678"),
                    "933f63a173540f09b587fb7f95625bbb",
                    "1.1.1"
                        };
                    SubmitMessage submitMsg = new SubmitMessage()
                    {
                        ParamString = loginInfos,
                        TranName = "UserLogin",
                        IsUseZip = false,
                        Marker = "Local"
                    };


                    client.Send(GetData(submitMsg));

                    var submit = new SubmitMessage() { TranName = "GetHQCache", Marker = "Local" };
                    client.Send(GetData(submit));

                    submit = new SubmitMessage() { TranName = "GetConfigCache", Marker = "Local" };
                    submit.ParamString = new string[] { "sdfsdf" };
                    client.Send(GetData(submit));
                }
                catch (Exception)
                {

                }
            }
        }

        private byte[] GetData(SubmitMessage submit)
        {
            var request = new RequestBase.Builder();
            request.FunCode = 1;
            if ("HQServer".Equals(submit.Marker))
                request.IsHQ = true;
            request.Body = ConvertSubmitMessageToReqOld(submit).ToByteString();
            return request.Build().ToByteArray();
        }


        private void _client_OnReceivedEvent(DuiAsynSocket.DataReceivedArgs obj)
        {

        }

        private void _client_OnConnChangeEvent(DuiAsynSocket.ConnStatusChangeArgs obj)
        {
            Console.WriteLine(obj.IsConnected);
        }

        private ReqOld ConvertSubmitMessageToReqOld(SubmitMessage submit)
        {
            var req = new ReqOld.Builder();
            if (submit.ParamJSON != null)
                req.ParamJSON = ByteString.CopyFrom(DuiHelperConvert.ObjectToByteArray(submit.ParamJSON));
            if (submit.ParamStringList != null)
                req.ParamStringList = ByteString.CopyFrom(DuiHelperConvert.ObjectToByteArray(submit.ParamStringList));
            if (submit.ParamString != null)
                req.ParamString = ByteString.CopyFrom(DuiHelperConvert.ObjectToByteArray(submit.ParamString));
            if (submit.ParamBytes != null)
                req.ParamBytes = ByteString.CopyFrom(DuiHelperConvert.ObjectToByteArray(submit.ParamBytes));
            if (submit.ParamByteList != null)
                req.ParamByteList = ByteString.CopyFrom(DuiHelperConvert.ObjectToByteArray(submit.ParamByteList));
            req.Marker = submit.Marker ?? "";
            req.IsUseZip = submit.IsUseZip;
            req.TranName = submit.TranName ?? "";
            req.Function = submit.Function ?? "";
            req.Action = submit.Action ?? "";
            req.DBMarker = submit.DBMarker ?? "";
            req.Json = submit.Json ?? "";
            return req.Build();
        }


    }
}
