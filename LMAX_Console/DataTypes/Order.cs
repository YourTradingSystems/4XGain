using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DataTypess
{
    public class Order
    {
        private Int64 orderId;
        private string symbol;
        private double lots;
        private int direction;
        private DateTime time;
        private double price;
        private String comment;
        private Int64 fromID;
        private DateTime prevScanTime;

        public Order()
        {
            orderId = 0;
            symbol = string.Empty;
            lots = 0.0;
            direction = -1;
            time = new DateTime();
            comment = String.Empty;
            fromID = 0;
        }

        public Order(String _symbol, Double _lots, Int32 _direction)
        {
            orderId = 0;
            symbol = _symbol;
            lots = _lots;
            direction = _direction;
            time = new DateTime();
            comment = String.Empty;
            fromID = 0;
        }

        public Order(Int32 _systemID, String _symbol, Double _lots, Int32 _direction)
        {
            orderId = 0;
            symbol = _symbol;
            lots = _lots;
            direction = _direction;
            time = new DateTime();
            comment = String.Empty;
            fromID = _systemID;
        }

        public Order(string _orderId, string _symbol, double _lots, int _direction, DateTime _time)
        {
            strOrderId = _orderId;
            symbol = _symbol;
            lots = _lots;
            direction = _direction;
            time = _time;
        }

        /// <summary>
        /// Compare two orders
        /// </summary>
        /// <param name="otherOrder">an other order to compare</param>
        /// <returns>return "true" if this orders equal "else" otherwise</returns>
        public bool compare(Order otherOrder)
        {
            return (otherOrder.symbol.Equals(this.symbol) &&
                    otherOrder.lots == this.Lots &&
                    otherOrder.direction == this.direction);
        }

        /// <summary>
        /// Return a text representation of the current Order object
        /// </summary>
        /// <returns>text representation of this Order object</returns>
        public override String ToString()
        {
            String res = "Order : ID="+Convert.ToString(orderId)+"; qty="+Convert.ToString(Lots)+"; symbol="+symbol+"; direction="+(direction==0?"BUY":"SELL")+";";
            res += " FromID="+Convert.ToString(fromID)+"; time="+time.ToString()+"; Comment="+comment;
            return res;
        }

        /// <summary>
        /// Create an independent copy of this Order object
        /// </summary>
        /// <returns>An independent Order object</returns>
        public Order Clone()
        {
            Order clonedOrder = new Order();
            clonedOrder.Comment   = (String)this.Comment.Clone();
            clonedOrder.Direction = this.Direction;
            clonedOrder.FromID    = this.FromID;
            clonedOrder.lOrderId  = this.lOrderId;
            clonedOrder.Lots      = this.Lots;
            clonedOrder.OrderID   = this.OrderID;
            clonedOrder.PrevScanTime = this.PrevScanTime;
            clonedOrder.Price     = this.Price;
            clonedOrder.Symbol    = (String)this.Symbol.Clone();
            clonedOrder.Time      = this.Time;
            return clonedOrder;
        }

        public string strOrderId
        {
            set { try { orderId = Convert.ToInt64(value); } catch (Exception) { orderId = 0; } }
            get { return Convert.ToString(orderId); }
        }

        public Int64 lOrderId
        {
            set { orderId = value; }
            get { return orderId; }
        }

        public string Symbol
        {
            set { symbol = value; }
            get { return symbol; }
        }
        public double Lots
        {
            set { lots = value; }
            get { return lots; }
        }
        public int Direction
        {
            set { direction = value; }
            get { return direction; }
        }
        public DateTime Time
        {
            set { time = value; }
            get { return time; }
        }

        public double Price
        {
            get { return price; }
            set { price = value; }
        }

        public String Comment
        {
            get { return comment; }
            set { comment = value; }
        }

        public Int64 FromID
        {
            get{ return fromID; }
            set { fromID = value; }
        }

        public Int64 OrderID
        {
            get { return orderId; }
            set { orderId = value; }
        }

        public DateTime PrevScanTime
        {
            get { return prevScanTime; }
            set { prevScanTime = value; }
        }
    }
}
