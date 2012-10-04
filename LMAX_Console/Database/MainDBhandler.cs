using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data.SqlClient;
using DataTypess;
using System.Globalization;
using Sender;

namespace Database
{
    public class MainDBhandler
    {
        private const String PLATFORM = "LMAX";
        private SqlConnection conn;
        private SqlConnection errorConn;
        private Object SQLlock;

        public MainDBhandler()
        {
            //Create lock objects
            SQLlock = new Object();
            ConnectToUserHistoy();
        }

        public void ConnectToUserHistoy(String dataSource = "WIN-83YVOQWW9RH\\SQLEXPRESS",
                             String dbPort = "1642",
                             String initialCatalog = "USERHISTORY",
                             String user = "rootuser",
                             String password = "qwerty"
            )
        {
            String sqlStr = "Network Library=DBMSSOCN;" +
                            "Data Source=" + dataSource + "," + dbPort + ";" +
                            "Initial Catalog=" + initialCatalog + ";" +
                            "User Id=" + user + ";" +
                            "Password=" + password + ";";

            conn = new SqlConnection(sqlStr);
            conn.Open();
        }

        public void CloseConnection()
        {
            conn.Close();
        }

        public void SendOrder(String userID, Order order)
        {
            lock(SQLlock)
            {
            
                    String sqlString =
                        "INSERT INTO HISTORY (\"USERID\", \"SYSTEMID\", \"DIRECTION\", \"SYMBOL\", \"VOLUME\", \"PRICE\")\n VALUES (";
                    sqlString += " '" + userID + "', " + Convert.ToString(order.FromID) + ", " + order.Direction + ", '" +
                                 order.Symbol + "', " + order.Lots.ToString("G", CultureInfo.InvariantCulture) + ", " +
                                 Convert.ToString(order.Price, CultureInfo.InvariantCulture) + ");";
                    SqlCommand sqlCommads = new SqlCommand(sqlString, conn);
                    Console.WriteLine(sqlCommads.CommandText);
                    sqlCommads.ExecuteNonQuery();
                
            }
        }

        /// <summary>
        /// This method write error logs to database
        /// </summary>
        /// <param name="userID">owner user id of this log\n</param>
        /// <param name="order">an order object that contains all neseserry data describe the log</param>
        public void WriteLog(String userID, Order order)
        {
            lock(SQLlock)
            {
                StringBuilder sqlQuery = new StringBuilder();
                sqlQuery.Append("INSERT INTO \"USERS\".dbo.\"ERROR_LOGS\" (\"USERID\", \"SYSTEMID\", \"ERRORTEXT\", \"SOLVED\", ");
                sqlQuery.Append("\"ERRORINITTIME\", \"ERRORSOLVETIME\", \"PREVTIME\", \"ORDERID\", \"DIRECTION\", \"SYMBOL\", \"LOTS\")\nVALUES\n");
                //UserID
                sqlQuery.Append("('").Append(userID).Append("', ");
                //SYSTEMID
                sqlQuery.Append(order.FromID).Append(", ");
                //ERRORTEXT
                sqlQuery.Append("'").Append(order.Comment??"").Append("', ");
                //SOLVED
                sqlQuery.Append("0, ");
                //ERRORINITTIME
                sqlQuery.Append("'").Append(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff")).Append("', ");
                //ERRORSOLVETIME
                sqlQuery.Append("NULL, ");
                //PREVTIME
                sqlQuery.Append("'").Append(order.PrevScanTime.ToString("yyyy-MM-dd HH:mm:ss.fff")).Append("', ");
                //ORDERID
                sqlQuery.Append("'").Append(order.lOrderId).Append("', ");
                //DIRECTION
                sqlQuery.Append(order.Direction).Append(", ");
                //SYMBOL
                sqlQuery.Append("'").Append(order.Symbol).Append("', ");
                //LOTS
                sqlQuery.Append(order.Lots.ToString("G", CultureInfo.InvariantCulture));
                sqlQuery.Append(");");
                SqlCommand sqlCommand = new SqlCommand(sqlQuery.ToString(), conn);
                try
                {
                    sqlCommand.ExecuteNonQuery();
                }
                catch(Exception e)
                {
                    Program.WriteError("Exception in MainDBhandler.WriteLog : "+e.Message);
                    Program.WriteError("SQL query : "+sqlQuery.ToString());
                }
                finally
                {
                    sqlCommand.Dispose();
                }
            }
        }
    }
}
