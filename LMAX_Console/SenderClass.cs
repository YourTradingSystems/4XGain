using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using CS_server_addin.DBDataContainers;
using System.Collections.Concurrent;
using Utilites;
using DataTypess;
using System.Threading.Tasks;
using Sender;
using Follower;

namespace Database
{
    /// <summary>
    /// This class response for sending datas via traders classes to user accounts (place orders)
    /// This class is running in a thread, and take datas to send by a blocking queue
    /// </summary>
    class SenderClass
    {
        private BlockingCollection<Operations> _dataQueve;
        private Thread _senderThread;
        private Boolean _isRunning;
        private const int WAIT_TIME = 1000;

        /// <summary>
        /// This method shuting down the thread in the class
        /// </summary>
        public void ShutDown()
        {
            lock (this)
            {
                _isRunning = false;
            }
        }

        /// <summary>
        /// Creates a SenderClass object
        /// </summary>
        /// <param name="dataQueve">a blocing queue object, wich takes orders and parameters to send him to accounts</param>
        public SenderClass(BlockingCollection<Operations> dataQueve)
        {
            _dataQueve = dataQueve;
            _isRunning = true;           
            // Starting the sender thread
            _senderThread = new Thread(new ThreadStart(sendList));
            _senderThread.Name = "Sender_thread";
            _senderThread.Start();
        }

        /// <summary>
        /// This method runn in a new Thread and response for
        /// sending orders from queue immediatly when datas appears 
        /// in the queue
        /// </summary>
        private void sendList()
        {
            Program.log.Info("Sender object started");
            Operations item;
            while (_isRunning)
            {
                // Try totake data with defined wait time, to provide abbility to exit 
                // from loop whan tha _isRunning variable will be "FALSE"
                if (_dataQueve.TryTake(out item, WAIT_TIME))
                {
                    //ThreadPool.QueueUserWorkItem(new WaitCallback(this.sendData),item);
                    sendData(item);
                }
            }
            Program.log.Info("Sender object shuting down");
        }

        /// <summary>
        /// Sends orders to users
        /// </summary>
        /// <param name="parameter"> An Operations object wich cast to Object. This object contains list of users wich to send datas
        /// and the datas</param>
        public void sendData(Object parameter)
        {
            Order order = null;
            Operations operation = (Operations)parameter;
            List<OrdersReq> req = operation.Requests;
            //Set system id
            foreach (OrdersReq or in req)
            {
                or.SystemID = operation.SystemID;
            }

            if (operation == null) return;
            if (req == null) return;

            Parallel.ForEach<TradingClass>(operation.traders, (tc, state, i) =>
                {
                    foreach (OrdersReq r in req)
                    {
                        switch (r.Operation)
                        {
                            case Presets.OrderOperations.Open:
                                Console.WriteLine("-----Order open command for symbol {0} to class {1}", r.Symbol, tc.FollowerID);
                                order = new Order(r.SystemID, r.Symbol, r.Volume, r.Direction);
                                order.PrevScanTime = operation.reqPrevScanTime;
                                tc.SendOrder(order);
                                break;
                            case Presets.OrderOperations.Clsoe:
                                Console.WriteLine("-----Order close command for symbol {0} to class {1}", r.Symbol, tc.FollowerID);
                                order = new Order(r.SystemID, r.Symbol, r.Volume, r.Direction);
                                order.PrevScanTime = operation.reqPrevScanTime;
                                tc.CloseOrder(order);
                                break;
                        }
                    }
                });
        }
    }
}
