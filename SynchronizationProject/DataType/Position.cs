using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ClientHandler2.DataType
{
    class Position
    {
        public String symbol = "";
        public double qty = 0.0;
        public int direction = 0;

        /// <summary>
        /// This constructor creates a position by order
        /// </summary>
        /// <param name="order"> order which open the position</param>
        public Position(Order order)
        {
            this.symbol = order.Symbol;
            this.qty = order.Lots;
            this.direction = order.Direction;
        }

        
        ///<summary> default constructor </summary>
        public Position()
        {

        }
        ///<summary>
        ///Compares two Position objects. 
        ///</summary>
        ///<param name="otherPosition"> an other position to compare</param>
        ///<returns> true if this Positions is equal
        ///otherwise false</returns>
        public bool compare(Position otherPosition)
        {
            return (otherPosition.symbol.Equals(this.symbol) &&
                    otherPosition.qty == this.qty &&
                    otherPosition.direction == this.direction);
        }

        ///<summary>
        ///Add an order to the current position. Throw an Exception if the position and order not compatibles
        ///</summary>
        ///<param name="order"> an Order object </param>
        ///<returns>Order object if after adding, the position 
        ///changes his direction. In this case the qty of position will be 0 (position closed),
        ///and the Order object represent the carry of position. Otherwise return null.</returns>
        public Order addOrderUntilClose(Order order)
        {
            Order resultOrder = null;
            if (!this.symbol.Equals(order.Symbol)) throw new Exception("The orders symbol and positions symbol not compatible");
            if (this.direction == order.Direction)
            {
                this.qty += order.Lots;
            }
            else
            {
                if (this.qty > order.Lots)
                {
                    this.qty -= order.Lots;
                }
                else
                {
                    resultOrder = new Order();
                    resultOrder.Symbol = this.symbol;
                    resultOrder.Direction = order.Direction;
                    resultOrder.Lots = order.Lots - this.qty;
                    this.qty = 0.0;
                }
            }
            return resultOrder;
        }

        ///<summary>
        ///Add an order to current position
        ///throws Exception when the Order object symbol not compatible with current positions symbol
        ///</summary>
        ///<param name = "order"> order - an Order object</param>
        public void addOrder(Order order)
        {
            if (!this.symbol.Equals(order.Symbol)) throw new Exception("The orders symbol and positions symbol not compatible");
            if (this.direction == order.Direction)
            {
                this.qty += order.Lots;
            }
            else
            {
                this.qty -= order.Lots;
                if (this.qty < 0.0)
                {
                    this.direction = order.Direction;
                    this.qty *= -1.0;
                }
            }
        }

        ///<summary>Represent the position by string</summary>
        ///<returns> a string representation of the Position object</returns>
        public String ToString()
        {
            String res = "Position : qty=" + Convert.ToString(qty) + "; symbol=" + symbol + "; direction=" + (direction == 0 ? "BUY" : "SELL") + ";";
            return res;
        }

        /// <summary>
        /// Convert current posiiton to order
        /// </summary>
        /// <returns>an order that represent tjis position</returns>
        public Order ToOrder()
        {
            Order o = new Order();
            o.Symbol = symbol;
            o.Direction = this.direction;
            o.Lots = this.qty;
            return o;
        }

        /// <summary>
        /// Summ operator for position object
        /// </summary>
        /// <param name="p1"></param>
        /// <param name="p2"></param>
        /// <returns>A new Position object - a summ of two Position objects</returns>
        public static Position operator + (Position p1, Position p2)
        {
            Position res = new Position();
            if (p1.symbol.Length!=0 && p2.symbol.Length!=0 && !p1.symbol.Equals(p2.symbol)) 
                throw new Exception("Position symbols not the same");
            res.symbol = p1.symbol.Length == 0 ? p2.symbol : p1.symbol;
            double pd1 = p1.qty * (p1.direction == 0 ? 1.0 : -1.0);
            double pd2 = p2.qty * (p2.direction == 0 ? 1.0 : -1.0);
            res.qty = pd1 + pd2;
            res.direction = (res.qty < 0.0 ? 1 : 0);
            res.qty = Math.Abs(res.qty);
            return res;
        }

        public static Position operator - (Position p1, Position p2)
        {
            Position res = new Position();
            if (p1.symbol.Length != 0 && p2.symbol.Length != 0 && !p1.symbol.Equals(p2.symbol))
                throw new Exception("Position symbols not the same");
            res.symbol = p1.symbol.Length==0 ? p2.symbol : p1.symbol;
            double pd1 = p1.qty * (p1.direction == 0 ? 1.0 : -1.0);
            double pd2 = p2.qty * (p2.direction == 0 ? 1.0 : -1.0);
            res.qty = pd1 - pd2;
            res.direction = (res.qty < 0.0 ? 1 : 0);
            res.qty = Math.Abs(res.qty);
            return res;
        }

        public static Boolean operator == (Position p1, Position p2)
        {
            return (p1.symbol.Equals(p2.symbol) && p1.qty == p2.qty && p1.direction == p2.direction);
        }

        public static Boolean operator != (Position p1, Position p2)
        {
            return (!p1.symbol.Equals(p2.symbol) || p1.qty != p2.qty || p1.direction != p2.direction);
        }
    }
}
