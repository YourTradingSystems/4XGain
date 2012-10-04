using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Utilites;
using DataTypess;
using Database;
using Sender;
using Follower;
using Database.DataTypes;

namespace Follower
{
    public class TradingClass
    {
        private Boolean _enabled, inProgress;
        private Object  _enabledLock, _inProgressLock;
        private OrderTransaction orderTransaction;
        private Random  randomGenerator;
        private API     _api;
        private String  site = "http://hotwo.com";
        private ErrorHandler _emailNotifier;
        private ErrorDbHandler dbHandler;
        private String _followerID;
        private Presets.SynchronizationMode _syncMode;

        public String FollowerID           { get { return _followerID; } set { }   }
        public String FollowerEmail        { get; set; }
        public String FollowerBrockerLogin { get { return _api.UserName; }    }
        public String FollowerBrocker      { get { return _api.UserBrocker; } }
        public Presets.SynchronizationMode FollowerSyncMode{ get{return _syncMode;}}
        public ErrorHandler errorHandler
        {
            get { return _emailNotifier; }
            set { _emailNotifier = value; }
        }
        public Boolean Enabled 
        { 
            get { lock (_enabledLock) { return _enabled;  } } 
            set { lock (_enabledLock) { _enabled = value; } } 
        }

        public FollowerPropereties ToFollowerPropereties()
        {
            return new FollowerPropereties
            {
                FollowerBrokerLogin  = this._api.UserName,
                FollowerBrokerPasswd = this._api.UserPassword,
                FollowerEmail        = this.FollowerEmail,
                FollowerID           = this._followerID,
                FollowerSynchMode    = this._syncMode
            };
        }

        public TradingClass(String _userID, String userName, String userPassword, String url, String email, ErrorDbHandler _dbHandler, Presets.SynchronizationMode syncMode = Presets.SynchronizationMode.SynchronizePositions)
        {
            FollowerEmail =  email;
            dbHandler     = _dbHandler;
            _followerID   = _userID;
            _syncMode     = syncMode;
            randomGenerator  = new Random((int)(DateTime.Now.Ticks / 10000));
            _enabledLock     = new Object();
            _inProgressLock  = new Object();
            orderTransaction = new OrderTransaction(new Presets.OrderEvent(onOrder), new Presets.ErrorEvent(onError), _userID);
            _api             = new API(url, userName, userPassword, new Presets.APIEvent(OnApievent));
            _enabled         = true; //<--- Place to the end???
            _emailNotifier   = null;
            // Configure objects
            switch (syncMode)
            {
                case Presets.SynchronizationMode.SynchronizePositions: 
                    Program.MarkUserToSync(_userID);
                    _enabled = false;
                    break;
            }
            //Create inlternal sync class
        }

        public void Login()
        {
            _api.Login();
        }

        /// <summary>
        /// This procedure calls whe OnError event occured. After call this
        /// method the PlaceOrderoperations will blocks, until ResetBlock called
        /// </summary>
        /// <param name="descr">the error description</param>
        private void onError(Order order)
        {
            Program.WriteError("OnError method called with description : "+((order!=null && order.Comment.Length != 0)?order.Comment:"<descr error>"));
            Enabled = false;

            dbHandler.WriteLog(_followerID, order);

            if (_emailNotifier != null)
            {
                _emailNotifier.sendMessage(FollowerEmail, order, Presets.ORDER_NOT_FILLED);
            }
        }

        /// <summary>
        /// This method called when an APIEvent occurs on API class
        /// This method call the orderTransaction method to process event
        /// </summary>
        /// <param name="eventCode">event code, avalible in Utilites.Presets\n</param>
        /// <param name="param">the order id</param>
        private void OnApievent(int eventCode, Object param)
        {
            orderTransaction.ApiEvent(eventCode, param);

            if (eventCode == Presets.ORDER_FILLED)
            {
                Order order = orderTransaction.GetNextOrder();
                if (order == null)
                {
                    lock (_inProgressLock)
                    {
                        inProgress = false;
                        return;
                    }
                }
                // If not enabled order send - exit from this program code
                if (!Enabled) return;
                // Generate order ID and send it to Transactions and API
                _api.SendOrder(order);
            }
            else
            {
                lock (_inProgressLock)
                {
                    inProgress = false;
                }
            }
        }
        
        /// <summary>
        /// This method try to send order via client API
        /// </summary>
        /// <param name="order">an order to send</param>
        public void SendOrder(Order order)
        {
            if (order == null)
            {
                Program.WriteError("Order is null in TradingClass.sendOrder");
                return;
            }
            if (order.Lots == 0.0)
            {
                Program.WriteError("Order.Lots == 0 in TradingClass.CloseOrder()");
                return;
            }
            // Console.WriteLine("Try to send order to {0}", userID);
            // If not enabled order send - exit from this program code
            if (!Enabled) { Console.WriteLine("Not enabled in {0}", _followerID); return; }
            // Generate order ID and send it to Transactions and API
            order.lOrderId = randomGenerator.Next(Int32.MaxValue);
            lock (_inProgressLock)
            {
                orderTransaction.AddOpen(order);
                if (!inProgress)
                {
                    inProgress = true;
                    _api.SendOrder(order);
                }
                else
                    Console.WriteLine("Bizy in {0}", _followerID);
            }
        }

        public void CloseOrder(Order order)
        {
            if (order == null)
            {
                Program.WriteError("Order == null in TradingClass.CloseOrder()");
                return;
            }
            if (order.Lots == 0.0)
            {
                Program.WriteError("Order.Lots == 0 in TradingClass.CloseOrder()");
                return;
            }
            // If not enabled order send - exit from this program code
            if (!Enabled) return;
            // Generate order ID and send it to Transactions and API
            order.lOrderId = randomGenerator.Next(Int32.MaxValue);
            lock (_inProgressLock)
            {
                orderTransaction.AddClose(order);
                if (!inProgress)
                {
                    inProgress = true;
                    _api.CloseOrder(order);
                }
            }            
        }

// Not alloved
        public void onOrder(Order order)
        {
            //dbHandler.SendOrder(userID, order);
            orderTransaction.AddOrderToHistory(_followerID, order);
        }

        /// <summary>
        /// This method clear all orders, stored in the queue
        /// </summary>
        public void ClearOrders()
        {
            orderTransaction.RemoveOrders();
        }

        public void StopUser()
        {
            lock (_enabledLock)
            {
                _enabled = false;
            }
        }

        public void UnlockUser()
        {
            lock(_enabledLock)
            {
                lock (_inProgressLock)
                {
                    _enabled = true;
                    inProgress = false;
                }
            }
        }
    }
}
