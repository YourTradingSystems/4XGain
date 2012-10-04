using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data.SqlClient;
using DataTypess;
using Sender;

namespace Database
{
    class LocalDbHandler
    {
        private object SQLlock;
        private SqlConnection conn;

        public LocalDbHandler()
        {
            SQLlock = new Object();
            ConnectToLocalDb();
        }

        private void ConnectToLocalDb()
        {
            String sqlStr = "Network Library=DBMSSOCN;" +
                            "Data Source=" + Program.config["local_db_datasource"] + "," + Program.config["local_db_port"] + ";" +
                            "Initial Catalog=" + Program.config["local_db_database"] + ";" +
                            "User Id=" + Program.config["local_db_login"] + ";" +
                            "Password=" + Program.config["local_db_password"] + ";";

            conn = new SqlConnection(sqlStr);
            conn.Open();
        }

        /// <summary>
        /// This method try to get the lastest positions from users positions. This method throws Exceptions when error occurs
        /// </summary>
        /// <param name="userID">user ID\n</param>
        /// <param name="systemID">users listened system ID</param>
        /// <returns>a list of Position elements</returns>
        public List<Position> GetUserLastPositions(String userID, Int64 systemID)
        {
            StringBuilder sqlCommandBuilder = new StringBuilder();
            SqlDataReader reader;
            SqlCommand    sqlCommand;
            List<Position> resultList = new List<Position>();

            sqlCommandBuilder.Append("SELECT USERHISTORY.dbo.POSITIONHISTORY.SYMBOL \n");
            sqlCommandBuilder.Append(", USERHISTORY.dbo.POSITIONHISTORY.DIRECTION \n");
            sqlCommandBuilder.Append(", USERHISTORY.dbo.POSITIONHISTORY.VOLUME \n");
            sqlCommandBuilder.Append(", USERHISTORY.dbo.POSITIONHISTORY.DBTIME \n");
            sqlCommandBuilder.Append("FROM USERHISTORY.dbo.POSITIONHISTORY, ");
            sqlCommandBuilder.Append("( SELECT USERHISTORY.dbo.POSITIONHISTORY.SYMBOL AS SYMBOL_, ");
            sqlCommandBuilder.Append("Max(USERHISTORY.dbo.POSITIONHISTORY.DBTIME) AS DBTIME_ ");
            sqlCommandBuilder.Append("FROM USERHISTORY.dbo.POSITIONHISTORY ");
            sqlCommandBuilder.Append("WHERE USERHISTORY.dbo.POSITIONHISTORY.USERID='").Append(userID).Append("' ");
            sqlCommandBuilder.Append("GROUP BY USERHISTORY.dbo.POSITIONHISTORY.SYMBOL)\n");
            sqlCommandBuilder.Append("AS TEMP WHERE USERHISTORY.dbo.POSITIONHISTORY.SYMBOL=TEMP.SYMBOL_ ");
            sqlCommandBuilder.Append("AND USERHISTORY.dbo.POSITIONHISTORY.DBTIME=TEMP.DBTIME_ ");
            //sqlCommandBuilder.Append("AND USERHISTORY.dbo.POSITIONHISTORY.SYSTEMID=").Append(systemID).Append(" ");
            sqlCommandBuilder.Append("AND USERHISTORY.dbo.POSITIONHISTORY.USERID='").Append(userID).Append("';");

            sqlCommand = new SqlCommand(sqlCommandBuilder.ToString(), conn);

            lock (SQLlock)
            {
               reader = sqlCommand.ExecuteReader();

                while (reader.Read())
                {
                    resultList.Add(new Position()
                    {
                        direction = Convert.ToInt32(reader["DIRECTION"].ToString()),
                        qty       = Convert.ToDouble(reader["VOLUME"].ToString()),
                        symbol    = reader["SYMBOL"].ToString().Trim()
                    }
                    );
                }
                reader.Close();
                return resultList;
            }
        }
    }
}
