using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Com.Lmax.Api;
using Com.Lmax.Api.Order;
using Com.Lmax.Api.Reject;
using System.Threading;
using Utilites;
using System.Net.Sockets;
using System.IO;
using System.Globalization;

namespace Follower
{
    class API
    {
        private string username, userpassword;
        private Dictionary<long, string> _newLimitOrder = new Dictionary<long, string>();
        private Presets.APIEvent apiEventListener;

        //propereties
        public String UserName     { get { return username;     } set { }  }
        public String UserPassword { get { return userpassword; } set { }  }

        // -----------------------------------------------------------------
        private Thread _workThread;
        private TcpClient client;
        private StreamReader reader;
        private StreamWriter writer;
        private Queue<DataTypess.Order> queue;
        private Object queueLock;
        private bool isLive;

        public String UserBrocker { get { return "LMAX"; } }

        public API(string url, string uName, string uPasswd, Presets.APIEvent listener)
        {
            apiEventListener = listener;
            username     = uName;
            userpassword = uPasswd;
            queue = new Queue<DataTypess.Order>();
            queueLock = new Object();
        }

        public void Login()
        {
            isLive = true;
            AccountLogin(username, userpassword, ProductType.CFD_DEMO);
        }

        private void AccountLogin(string uName, string uPasswd, ProductType pt)
        {
            _workThread = new Thread(new ThreadStart(ConnectionManager));
            _workThread.Start();
            _workThread.Name = UserName;
        }

        public void Logout()
        {
            isLive = false;
            apiEventListener.Invoke(Presets.DISCONNECTED, "");
        }

        private void ConnectionManager()
        {
            client = new TcpClient("192.168.2.80", 9001); //83
            String line;

            try
            {
                reader = new StreamReader(client.GetStream(), new UTF8Encoding());
                writer = new StreamWriter(client.GetStream(), new UTF8Encoding());
                writer.AutoFlush = true;
            }
            catch (Exception e)
            {
                Console.WriteLine("{0} : Error connection to stream", UserName);
                Console.WriteLine("Additional info : " + e.Message);
                return;
            }

            try
            {
                writer.WriteLine("Connect;" + UserName);

                line = "";
                line = reader.ReadLine();
                if (line.Equals("OK"))
                {
                    apiEventListener.Invoke(Presets.LOGGINED, new DataTypess.Order());
                }
                else
                {
                    apiEventListener.Invoke(Presets.DISCONNECTED, new DataTypess.Order());
                }
                //loop
                DataTypess.Order order = null;

                while (isLive)
                {
                    order = null;
                    lock (queueLock)
                    {
                        if (queue.Count != 0)
                        {
                            order = queue.Dequeue();
                            //Console.WriteLine("Dequeue an order in {0}", UserName);
                        }
                    }
                    if (order != null)
                    {
                        PlaceMarketOrder(order);
                    }
                    Thread.Sleep(500);
                }
                Console.WriteLine(getIdent() + " __ Exit from account");
            }
            catch (Exception e)
            {
                Console.WriteLine("{0} : Exception occured while try to send datat to server");
                Console.WriteLine("{0} : Exception detailis : " + e.Message);
                try
                {
                    client.Close();
                }
                catch (Exception e2)
                {
                    Console.WriteLine("{0} : Cannot close socket, so exit thread without closing socket");
                }
                return;
            }
        }

        public void PlaceMarketOrder(DataTypess.Order order)
        {
            //Console.WriteLine("__(0)try send {0}", UserName);
            String datatToWrite="";
            try
            {
                datatToWrite = "SendOrder;" + order.Symbol + ";" + Convert.ToString(order.Direction) + ";" + Convert.ToString(order.Lots, CultureInfo.InvariantCulture) + ";" + order.OrderID + ";" + username;
                writer.WriteLine(datatToWrite);
                //Console.WriteLine("__(1)sended {0}", UserName);
                String line = reader.ReadLine();
                //Console.WriteLine("**_(2)Readed {0} in {1}",line, username);
                if (line.Length != 0)
                {
                    if (line.Equals("OK"))
                        apiEventListener.Invoke(Presets.ORDER_FILLED, order);
                    else
                    {
                        order.Comment = line;
                        apiEventListener.Invoke(Presets.ORDER_NOT_FILLED, order);
                    }
                }
                else apiEventListener.Invoke(Presets.ORDER_REJECTED, order);
            }
            catch (Exception e)
            {
                Console.WriteLine("Exception in PlaceMarketOrder. Additional info : "+e.Message);
            }
        }

        public void SendOrder(DataTypess.Order order)
        {
            lock (queueLock)
            {
                //Console.WriteLine("Add to queue in {0}",UserName);
                queue.Enqueue(order);

            }
        }

        public void CloseOrder(DataTypess.Order order)
        {
            order.Direction = order.Direction == 0 ? 1 : 0;
            lock (queueLock)
            {
                //Console.WriteLine("Add to queue in {0}", UserName);
                queue.Enqueue(order);
            }
        }

        public String getIdent()
        {
            return "L:"+username+"; P:"+userpassword;
        }

    }
}