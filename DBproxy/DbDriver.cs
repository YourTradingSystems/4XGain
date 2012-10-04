using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data.SqlClient;
using System.Globalization;
using ClientHandler2.DataType;

namespace DBproxy
{
    class DbDriver
    {
        private SqlConnection _connection;
        private List<Object>  _linkedObjects;
        //------- sync objects -------
        private Object _syncLinkedObjects;
        private Object _sqlLock;

        public int LinkedObjectCount { get { lock (_linkedObjects) { return _linkedObjects.Count; } } set { } }

        public DbDriver()
        {
            _syncLinkedObjects = new Object();
            _sqlLock = new Object();
            _linkedObjects = new List<Object>();
            ConnectToDatabase();
        }

        public void link(Object linkedObject)
        {
            lock (_linkedObjects)
            {
                _linkedObjects.Add(linkedObject);
            }
        }

        public void ConnectToDatabase()
        {
            String sqlStr = "Network Library=DBMSSOCN;" +
                            "Data Source=portativ\\MSQL,4055;" +
                            "Initial Catalog=USERHISTORY;" +
                            "User Id=Sasha;" +
                            "Password=qwerty_111;";

            _connection = new SqlConnection(sqlStr);
            _connection.Open();
        }

        /// <summary>
        /// This method write an order to History table
        /// </summary>
        /// <param name="userId">user ID</param>
        /// <param name="order">anOrder object to write to database</param>
        public void OrderToHistory(String userId, Order order)
        {
            lock (_sqlLock)
            {
                StringBuilder sqlString = new StringBuilder();
                sqlString.Append("INSERT INTO HISTORY (\"USERID\", \"SYSTEMID\", \"DIRECTION\", \"SYMBOL\", \"VOLUME\", \"PRICE\")\n VALUES (");
                sqlString.Append(" '").Append(userId).Append("', ").Append(order.FromID).Append(", ").Append(
                    order.Direction).Append(", '").Append(order.Symbol).Append("', ");
                sqlString.Append(Convert.ToString(order.Lots, CultureInfo.InvariantCulture)).Append(", ");
                sqlString.Append(Convert.ToString(order.Price, CultureInfo.InvariantCulture)).Append(");");
                SqlCommand sqlCommads = new SqlCommand(sqlString.ToString(), _connection);
                Console.WriteLine(sqlCommads.CommandText);
                sqlCommads.ExecuteNonQuery();
                sqlCommads.Dispose();
            }
        }

        /// <summary>
        /// Insert an Order to a queue, that phisicaly stored in a database table
        /// </summary>
        /// <param name="order">an Order object to store</param>
        /// <param name="userId">a user ID wich order will be stores</param>
        public void InsertToQueue(Order order, String userId)
        {
            lock (_sqlLock)
            {
                order.Time = DateTime.Now;
                StringBuilder sqlString = new StringBuilder();
                sqlString.Append(
                    "Insert Into Orders(\"OrderId\",\"Direction\",\"Symbol\",\"Lots\",\"OrderTime\", \"Price\", \"FromID\", \"UserID\", \"PrevScanTime\")");
                sqlString.Append("VALUES ('").Append(order.strOrderId).Append("', ").Append(order.Direction).Append(
                    ", '");
                sqlString.Append(order.Symbol).Append("',").Append(order.Lots.ToString("G", CultureInfo.InvariantCulture));
                sqlString.Append(", '").Append(order.Time.ToString("yyyy-MM-dd HH:mm:ss")).Append("', ");
                sqlString.Append(order.Price.ToString("G", CultureInfo.InvariantCulture)).Append(", ");
                sqlString.Append(order.FromID).Append(", '").Append(userId).Append("', '").Append(
                    order.PrevScanTime.ToString("yyyy-MM-dd HH:mm:ss"));
                sqlString.Append("')");
                SqlCommand command = new SqlCommand(sqlString.ToString(), _connection);
                command.ExecuteNonQuery();
                command.Dispose();
            }
        }

        /// <summary>
        /// Select and delete an order from a queue
        /// </summary>
        /// <param name="orderId">an order ID</param>
        /// <returns>the stored order object</returns>
        public Order ReturnFromQueue(long orderId)
        {
            lock (_sqlLock)
            {
                int rowCount = -1;
                Order order = null;
                SqlCommand command = new SqlCommand("SELECT * FROM ORDERS WHERE ORDERID = " + orderId + ";", _connection);
                // Warning

                SqlDataReader reader = command.ExecuteReader();
                if (reader.Read())
                {
                    order = ReaderToOrder(reader);
                    reader.Close();
                    command.Dispose();
                    command = new SqlCommand("DELETE FROM ORDERS WHERE ORDERID=" + orderId + ";", _connection);
                    command.ExecuteNonQuery();
                    command.Dispose();
                }
                reader.Close();
                return order;
            }
        }

        /// <summary>
        /// Gets a next order from user orders table, witch contains orders, that not sended yet.
        /// </summary>
        /// <param name="userId">ID of the user</param>
        /// <returns>a selected order object or null if no orders</returns>
        public Order GetNextUserOrder(String userId)
        {
            lock (_sqlLock)
            {
                SqlCommand sqlCommand =
                    new SqlCommand(
                        "SELECT TOP 1 * FROM Orders WHERE \"USERID\"='" + userId + "' ORDER BY \"OrderTime\" ",
                        _connection);
                Order order = null;

                SqlDataReader reader = sqlCommand.ExecuteReader();
                if (reader.Read())
                {
                    order = ReaderToOrder(reader);
                }
                sqlCommand.Dispose();
                reader.Close();

                return order;
            }
        }

        /// <summary>
        /// Convert datas from Reader to an Order object
        /// </summary>
        /// <param name="reader">a non empty reader</param>
        /// <returns>na Order object instance</returns>
        private Order ReaderToOrder(SqlDataReader reader)
        {
            Order order = new Order();
            order.OrderID = Convert.ToInt64(reader["ORDERID"]);
            order.Direction = Convert.ToInt32(reader["DIRECTION"]);
            order.Symbol = reader["SYMBOL"].ToString();
            order.Lots = Convert.ToDouble(reader["LOTS"]);
            order.Time = Convert.ToDateTime(reader["OrderTime"]);
            order.Price = Convert.ToDouble(reader["PRICE"]);
            order.FromID = Convert.ToInt64(reader["FROMID"]);
            order.PrevScanTime = Convert.ToDateTime(reader["PrevScanTime"]);
            return order;
        }

        public void RemoveUserOrders(String userId)
        {
            lock (_sqlLock)
            {
                SqlCommand command = new SqlCommand("DELETE FROM Orders WHERE \"USERID\"='" + userId + "'", _connection);
                command.ExecuteNonQuery();
                command.Clone();
            }
        }

    }
}
