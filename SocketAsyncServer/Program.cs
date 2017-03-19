using System;
using System.Net;
using System.Text;

namespace SocketAsyncServer
{
    public static class Program
    {
        private const Int32 DEFAULT_PORT = 9900, DEFAULT_NUM_CONNECTIONS = 10000, DEFAULT_BUFFER_SIZE = Int16.MaxValue;

        public static void Main(String[] args)
        {
            TestClient();


            Console.ReadLine();


            try
            {
                Int32 port = GetValue(args, 0, DEFAULT_PORT);
                Int32 numConnections = GetValue(args, 1, DEFAULT_NUM_CONNECTIONS);
                Int32 bufferSize = GetValue(args, 2, DEFAULT_BUFFER_SIZE);

                SocketListener sl = new SocketListener(numConnections, bufferSize);
                sl.Start(port);

                Console.WriteLine("Server listening on port {0}. Press any key to terminate the server process...", port);
               
                Console.Read();

                sl.Stop();

            }
            catch (IndexOutOfRangeException)
            {
                PrintUsage();
            }
            catch (FormatException)
            {
                PrintUsage();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
        }


        private static void TestClient()
        {
            var client = new SocketClient("127.0.0.1", 1991);
            client.OnConnChangeEvent += Client_OnConnChangeEvent;
            client.OnReceivedEvent += Client_OnReceivedEvent;
            client.Connect();

            var data=UTF8Encoding.Default.GetBytes("123456");
            client.Send(data);
            //client.SendReceive("dfdfdf");
        }

        private static void Client_OnReceivedEvent(XXJR.Communication.DataReceivedArgs obj)
        {
            Console.WriteLine(UTF8Encoding.Default.GetString(obj.Data));
        }

        private static void Client_OnConnChangeEvent(XXJR.Communication.ConnStatusChangeArgs obj)
        {
            Console.WriteLine(obj.ConnStatus.ToString());
        }

        private static Int32 GetValue(String[] args, Int32 index, Int32 defaultValue)
        {
            Int32 value = 0;

            if (args.Length <= index || !Int32.TryParse(args[index], out value))
            {
                return defaultValue;
            }

            return value;
        }

        private static void PrintUsage()
        {
            Console.WriteLine("Usage: SocketAsyncServer <port> [numConnections] [bufferSize].");
            Console.WriteLine("\t<port> Numeric value for the listening TCP port.");
            Console.WriteLine("\t[numConnections] Numeric value for the maximum number of incoming connections.");
            Console.WriteLine("\t[bufferSize] Numeric value for the buffer size of incoming connections.");
        }
    }
}
