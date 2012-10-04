using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Net.Sockets;
using Utilites;
using Sender;
using Follower;

namespace Database.PortListener
{
    class NETlistener
    {
        private TcpListener     _serverSocket;
        private Socket          _client;
        private ASCIIEncoding   _stringCoder;
        private Thread          _thread;
        private bool            isRunning;

        public NETlistener()
        {
            isRunning = true;
            _stringCoder = new System.Text.ASCIIEncoding();

            Thread thread = new Thread(new ThreadStart(this.startListen));
            thread.Start();
        }

        public void startListen()
        {
            Program.log.Info("NETlistener strart to listen socket ");

            _serverSocket = null;
            try
            {
                _serverSocket = new TcpListener(8001);
            }
            catch (ArgumentOutOfRangeException e0)
            {
                Program.WriteError("Argument out of range exception occured when try to start TCP listener socket");
                Program.log.Error("Argument out of range exception occured when try to start TCP listener socket");
                Program.log.Debug(e0.Message);
                Program.log.Debug(e0.StackTrace);
            }
            _serverSocket.Start();

            while (isRunning)
            {
                try
                {
                    _client = _serverSocket.AcceptSocket();
                    _thread = new Thread(new ParameterizedThreadStart(this.processReq));
                    _thread.Start(_client);
                }
                catch (InvalidOperationException e0)
                {
                    Program.log.Error("Invalid operation exception while try to accept socket");
                    Program.log.Debug(e0.Message);
                    Program.log.Debug(e0.StackTrace);
                }
            }
            try
            {
                _serverSocket.Stop();
            }
            catch (SocketException e0)
            {
                Program.log.Error("Socket exception occured when try to stop server socket");
                Program.log.Debug(e0.Message);
                Program.log.Debug(e0.StackTrace);
            }
        }

        public void ShutDown()
        {
            lock(this)
            {
                isRunning = false;
            }
        }

        public void processReq(object osocket)
        {
            Socket socket = (Socket)osocket;
            byte[] buffData = new byte[300];
            int recLen = 0;
            string message = "";
            int endls = 0;

            try
            {
                while (isRunning)
                {
                    recLen = socket.Receive(buffData);
                    message += _stringCoder.GetString(buffData, 0, recLen);

                    endls = 0;
                    for (int i = 0; i < message.Length; ++i)
                    {
                        if (message[i] == '\n') ++endls;
                    }
                    if (endls != 0) break;
                }
            }
            catch(Exception e)
            {
                Program.log.Error("Error occured when try to recieve data from socket");
                Program.log.Debug(e.Message);
                Program.log.Debug(e.StackTrace);
            }

            if (!isRunning)
            {
                try
                {
                    socket.Shutdown(SocketShutdown.Both);
                }
                catch (Exception e)
                {
                    Program.log.Error("Error trying to Shutdown client socket");
                    Program.log.Debug(e.Message);
                    Program.log.Debug(e.StackTrace);
                }
                socket.Close();
                return;
            }

            string[] funcArgs = message.Split(';');
            Int32 strategyID = Convert.ToInt32(funcArgs[2]);
            List<TradingClass> connectObjList;

            System.Console.WriteLine("NET listener get request. try to process it");
            
            switch (funcArgs[0].ToLower().Trim())
            {
                case "stop" :
                    TradingClass tc = Program.GetFollowerById(funcArgs[1]);
                    tc.StopUser();
                    break;
                case "continue" :
                    Program.MarkUserToSync(funcArgs[1]);
                    break;
            }
             
        }
    } // end class
} // end namespace