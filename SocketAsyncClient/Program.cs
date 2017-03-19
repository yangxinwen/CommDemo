using System;

namespace SocketAsyncClient
{
    public static class Program
    {
        public static void Main(string[] args)
        {
            try
            {
                String host = "127.0.0.1"; //args[0];
                Int32 port = 9900;// Convert.ToInt32(args[1]);
                Int16 iterations = 10000;
                if (args.Length == 3)
                {
                    iterations = Convert.ToInt16(args[2]);
                }
                SocketClient sa;

                for (int i = 0; i < 10000; i++)
                {
                    sa = new SocketClient(host, port);

                    sa.Connect();


                    sa.SendReceive("Message #" + i.ToString());
                 
                    //sa.Disconnect();


                }
            }
            catch (IndexOutOfRangeException)
            {
                Console.WriteLine("Usage: SocketAsyncClient <host> <port> [iterations]");
            }
            catch (FormatException)
            {
                Console.WriteLine("Usage: SocketAsyncClient <host> <port> [iterations]." +
                    "\r\n\t<host> Name of the host to connect." +
                    "\r\n\t<port> Numeric value for the host listening TCP port." +
                    "\r\n\t[iterations] Number of iterations to the host.");
            }
            catch (Exception ex)
            {
                Console.WriteLine("ERROR: " + ex.Message);
            }
            Console.WriteLine("Press any key to terminate the client process...");
            Console.Read();
        }
    }
}
