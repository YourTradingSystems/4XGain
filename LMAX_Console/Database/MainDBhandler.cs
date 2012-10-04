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
    public class ErrorDbHandler
    {
        private const String PLATFORM = "LMAX";
        private SqlConnection conn;
        private Object SQLlock;

        public ErrorDbHandler()
        {
            SQLlock = new Object();
            ConnectToUserHistoy();
        }

        public void ConnectToUserHistoy()
        {
            String sqlStr = "Network Library=DBMSSOCN;" +
                            "Data Source=" + Program.config["error_db_location"] + "," + Program.config["error_db_port"] + ";" +
                            "Initial Catalog=" + Program.config["error_db_name"] + ";" +
                            "User Id=" + Program.config["error_db_login"] + ";" +
                            "Password=" + Program.config["error_db_password"] + ";";

            conn = new SqlConnection(sqlStr);
            conn.Open();
        }

        public void CloseConnection()
        {
            conn.Close();
        }

        private void WriteError(string userId)
        {
            StringBuilder sqlQuery = new StringBuilder();
            sqlQuery.Append("INSERT INTO ERRORMARKS (USERID, MARKEDERROR) VALUE ('" + userId + "', " + "'FALSE')");
            SqlCommand sqlCommand = new SqlCommand(sqlQuery.ToString(), conn);
            try
            {
                sqlCommand.ExecuteNonQuery();
            }
            catch (Exception e)
            {
                Program.log.Error("Exception in MainDBhandler.WriteError : " + e.Message + " SqlQuery : " + sqlQuery.ToString());
                Program.log.Debug(e.StackTrace.ToString());
                Program.WriteError("Exception in ErrorDBhandler.WriteError : " + e.Message);
                Program.WriteError("SQL query : " + sqlQuery);
            }
            finally
            {
                sqlCommand.Dispose();
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
                WriteError(userID);

                StringBuilder sqlQuery = new StringBuilder();
                sqlQuery.Append("INSERT INTO \"USERS\".dbo.\"ERROR_LOGS\" (\"USERID\", \"SYSTEMID\", \"ERRORTEXT\", \"SOLVED\", ")
                    .Append("\"ERRORINITTIME\", \"ERRORSOLVETIME\", \"PREVTIME\", \"ORDERID\", \"DIRECTION\", \"SYMBOL\", \"LOTS\")\nVALUES\n")
                //UserID
                    .Append("('").Append(userID).Append("', ")
                //SYSTEMID
                    .Append(order.FromID).Append(", ")
                //ERRORTEXT
                    .Append("'").Append(order.Comment??"").Append("', ")
                //SOLVED
                    .Append("0, ")
                //ERRORINITTIME
                    .Append("'").Append(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff")).Append("', ")
                //ERRORSOLVETIME
                    .Append("NULL, ")
                //PREVTIME
                    .Append("'").Append(order.PrevScanTime.ToString("yyyy-MM-dd HH:mm:ss.fff")).Append("', ")
                //ORDERID
                    .Append("'").Append(order.lOrderId).Append("', ")
                //DIRECTION
                    .Append(order.Direction).Append(", ")
                //SYMBOL
                    .Append("'").Append(order.Symbol).Append("', ")
                //LOTS
                    .Append(order.Lots.ToString("G", CultureInfo.InvariantCulture))
                    .Append(");");
                SqlCommand sqlCommand = new SqlCommand(sqlQuery.ToString(), conn);
                try
                {
                    sqlCommand.ExecuteNonQuery();

                }
                catch(Exception e)
                {
                    Program.log.Error("Exception in MainDBhandler.WriteLog : " + e.Message+" SqlQuery : "+sqlQuery.ToString());
                    Program.log.Debug(e.StackTrace.ToString());
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
