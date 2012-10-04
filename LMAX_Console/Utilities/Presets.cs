using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DataTypess;

namespace Utilites
{
    public class Presets
    {
        public enum OrderOperations { Open, Clsoe, Delete };
        public enum SynchronizationMode { NotSynchronize, SynchronizePositions, WaitForDirectionChange };

        public const int ORDER_FILLED     = 0;
        public const int ORDER_NOT_FILLED = 1;
        public const int ORDER_REJECTED   = 2;
        public const int DISCONNECTED     = 3;
        public const int LOGGINED         = 4;

        public delegate void ErrorEvent(Order order); 
        /// <summary>
        /// This delegate called when an event occurs from API class.
        /// </summary>
        /// <param name="eventCode">The code, that shows the event type</param>
        /// <param name="eventArg">Mostly OrderType</param>
        public delegate void APIEvent(int eventCode, Object eventArg);
        public delegate void OrderEvent(Order order);

        
        public delegate void EmailEvent(String messReciver, String orderInfo, int messType);

        public static String OperationsToStr(OrderOperations operation)
        {
            switch (operation)
            {
                case OrderOperations.Open: return "Open";
                case OrderOperations.Clsoe: return "Close";
                case OrderOperations.Delete: return "Delete";
                default: return "Error converting type to str";
            }
        }

        public static SynchronizationMode GetSyncMode(int iSyncMode)
        {
            switch (iSyncMode)
            {
                case 1: return SynchronizationMode.NotSynchronize; 
                case 2: return SynchronizationMode.WaitForDirectionChange; 
                case 3: return SynchronizationMode.SynchronizePositions;
            }
            return SynchronizationMode.NotSynchronize;
        }

    }
}
