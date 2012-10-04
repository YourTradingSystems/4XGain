using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net.Sockets;
using System.Threading;

namespace Database.PortListener
{
    class NewUserListener
    {
        private TcpListener listener;

        public NewUserListener()
        {
            listener = new TcpListener(8000);
        }

        public void BeginZListen()
        {
            listener.Start();
            TcpClient client = listener.AcceptTcpClient();
            Thread thread = new Thread(new ParameterizedThreadStart(processClient));
            thread.Start(client);
        }

        /*
        public String read(TcpClient client)
        {
            String readed;
            int bytesRead;
            while (true)
            {
                try
                {
                    NetworkStream networkStream = client.GetStream();
                    byte[] bytesFrom = new byte[10025];
                    bytesRead = networkStream.Read(bytesFrom, 0, (int)client.ReceiveBufferSize);
                    if (bytesRead == 0) { break; }
                    readed = System.Text.Encoding.ASCII.GetString(bytesFrom);
                    Console.WriteLine(">> Data From Client: " + readed);
                }
                catch (Exception ex)
                {
                    Program.WriteError(ex.ToString());
                    client.Close();
                }
            }
            client.Close();
        }
        */
        
        public void processClient(Object client)
        {
            TcpClient AccClient = (TcpClient)client;

        }


    }
}
