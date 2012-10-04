using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Utilites;
using DataTypess;
using Database.LocalDatabase;

namespace Follower
{
    public class OrderTransaction
    {
        private Presets.ErrorEvent errorListsner;
        private Presets.OrderEvent eventListener;
        private LocalDbAdapter _localDb = null;
        private String userID;
        private Object addOrderLock;

        public OrderTransaction(Presets.OrderEvent _Listener, Presets.ErrorEvent _Error, String _userID)
        {
            addOrderLock = new Object();
            userID = _userID;
            _localDb = DbFactory.GetDbDriver(this);
            eventListener = _Listener;
            errorListsner = _Error;
        }

        /// <summary>
        /// Add a close order to the database.\n Important! The direction of order will chnaged 
        /// </summary>
        /// <param name="order">an Order object</param>
        public void AddClose(Order order)
        {
            Order order2 = order.Clone();
            order2.Direction = (order.Direction == 0 ? 1 : 0);
            Add(order2);
        }

        /// <summary>
        /// Add an open Order object to the database
        /// </summary>
        /// <param name="order"></param>
        public void AddOpen(Order order)
        {
            Add(order);
        }

        /// <summary>
        /// Add an order to the database
        /// </summary>
        /// <param name="order">Order object to add to database</param>
        private void Add(Order order)
        {
            _localDb.InsertToQueue(order, userID);
        }

        public void ApiEvent(int code, Object order)
        {
            Order fullSpecifiedOrder;
            Order convertedOrder;
            switch (code)
            {
                case Presets.ORDER_FILLED:
                        convertedOrder = order as Order;
                        fullSpecifiedOrder = _localDb.ReturnFromQueue(convertedOrder.lOrderId);
                        if (fullSpecifiedOrder != null)
                        {
                            fullSpecifiedOrder.Price = (convertedOrder).Price;
                            eventListener.Invoke(fullSpecifiedOrder);
                        }
                        break;
                case Presets.ORDER_NOT_FILLED:
                        convertedOrder = order as Order;    
                        fullSpecifiedOrder = _localDb.ReturnFromQueue(convertedOrder.lOrderId);
                        if (fullSpecifiedOrder != null)
                        {
                            fullSpecifiedOrder.Comment = convertedOrder.Comment;
                            Console.WriteLine("Order not filled " + fullSpecifiedOrder.strOrderId);
                            errorListsner.Invoke(fullSpecifiedOrder);
                        }
                        break;
                case Presets.ORDER_REJECTED:
                        convertedOrder = order as Order;
                        fullSpecifiedOrder = _localDb.ReturnFromQueue(convertedOrder.lOrderId);
                        if (fullSpecifiedOrder != null)
                        {
                            fullSpecifiedOrder.Comment = convertedOrder.Comment;
                            Console.WriteLine("Order rejected " + fullSpecifiedOrder.strOrderId);
                            errorListsner.Invoke(fullSpecifiedOrder);
                        }
                        break;
                case Presets.LOGGINED:
                    {
                        Console.WriteLine("Logined");
                    }
                    break;
                default:
                    {
                        Order order1 = new Order();
                        order1.Comment = "Unknow error";
                        errorListsner.Invoke(order1);
                        break;
                    }
            }
        }

        /// <summary>
        /// Get next order from queue but not delete him
        /// </summary>
        /// <returns>an Order object</returns>
        public Order GetNextOrder()
        {
            return _localDb.GetNextUserOrder(userID);   
        }

        /// <summary>
        /// Remove all user orders from queue
        /// </summary>
        public void RemoveOrders()
        {
            _localDb.RemoveUserOrders(userID);
        }

        public void AddOrderToHistory(String userId, Order order)
        {
            _localDb.OrderToHistory(userId, order);
        }
    }
}
