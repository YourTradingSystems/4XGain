using System;
using System.Collections.Generic;
using System.Linq;
using System.Data.SqlClient;
using CS_server_addin.DBDataContainers;
using System.Data.SqlTypes;
using CS_server_addin;
using System.Data;
using Utilites;
using Database;
using DataTypess;
using System.Text;
using Database.Types;
using Sender;
using Follower;
using DataChangeMonitor;

namespace Database
{
    public class DataBase
    {
        public String PLATFORM { get { return "FXCM"; } }
        private SqlConnection conn;
        private Object sqlQueryLock;
        private  Dictionary<String, String> fieldsMatching = new Dictionary<string, string>(){
            {"ACCOUNTID","AccountId_"},
            {"ACCOUNTTYPE","AccountType_"},
            {"BROKER","BrockerName_"},
            {"BROKERLOGIN","BrockerLogin_"},
            {"BROKERPASSWORD","BrockerPassswd_"},
            {"SYNCHRONIZATIONTYPE","SyncMethod_"},
            {"EMAIL","Email"}
        };
           
        /// <summary>
        /// Constructor of the DataBase object. WHen constructor called 
        /// this object try to connect to the database
        /// </summary>
        public DataBase()
        {
            sqlQueryLock = new Object();
            SetConnection();
        }

        /// <summary>
        /// Connect to database
        /// </summary>
        private void SetConnection()
        {
            conn = new SqlConnection("Network Library=DBMSSOCN;" +
                                     "Data Source=" + Program.config["DB_location"] + "," + Program.config["DB_port"] + ";" +
                                     "Initial Catalog=" + Program.config["DB_name"] + ";" +
                                     "User Id=" + Program.config["DB_user"] + ";" +
                                     "Password=" + Program.config["DB_passwd"] + ";"
                                     );
            try
            {
                conn.Open();
            }
            catch (InvalidOperationException e0)
            {
                Program.log.Error("Invalid operation exception\n");
                Program.log.Debug(e0.Message);
                Program.log.Debug(e0.StackTrace);
            }
            catch (SqlException e)
            {
                Program.log.Error("Cannot create database connection");
                Program.log.Debug(e.Message);
                Program.log.Debug(e.StackTrace);
            }
        }

        public void CloseConnection()
        {
            lock (sqlQueryLock)
            {
                try
                {
                    conn.Close();
                }
                catch (SqlException e0)
                {
                    Program.log.Error("SQL exception occured whe trying to close SQL database connection");
                    Program.log.Debug(e0.Message);
                    Program.log.Debug(e0.StackTrace);
                }
            }
        }

        public object GetDBResult(DateTime time)
        {
            List<DBResult> ResultList = new List<DBResult>();
            String conditions = getSelectSQLquery(Program.idUsers);
            if (conditions.Length == 0) return null;
            SqlCommand cmd = new SqlCommand("SELECT * FROM POSITIONS WHERE DBTIME > '" + time.ToString("yyyy-MM-dd HH:mm:ss.fff") + "' " + conditions + " ORDER BY DBTIME DESC;", conn);
            SqlDataReader reader = null;
            lock (sqlQueryLock)
            {
                try
                {
                    if (conn.State == ConnectionState.Closed)
                        conn.Open();
                    reader = cmd.ExecuteReader();
                    while (reader.Read())
                    {
                        ResultList.Add(new DBResult(reader));
                    }
                    cmd.Dispose();
                    reader.Close();
                }
                catch (InvalidOperationException e0)
                {
                    Program.log.Error("Invalid operation exception occured when try to execute SQL command");
                    Program.log.Debug("SQL command is " + cmd.CommandText);
                    Program.log.Debug(e0.Message);
                    Program.log.Debug(e0.StackTrace);
                }
                catch (SqlException e1)
                {
                    Program.log.Error("SQL exception occured when try to execute SQL command");
                    Program.log.Debug("SQL command is " + cmd.CommandText);
                    Program.log.Debug(e1.Message);
                    Program.log.Debug(e1.StackTrace);
                }

                finally
                {
                    if (reader != null)
                        reader.Close();
                    cmd.Dispose();
                }
            }
            return ResultList;
        }

        /// <summary>
        /// This method create a dictionary. The kay in the dictionary is the user listened systemID, the values of the Dictionary 
        /// is a list of users TradingClass that represents the current user account, that listen the current key SystemID
        /// </summary>
        /// <returns>a dictionary : key - user listened SystemID, value - a list of users thet listening current SystemID(key)</returns>
        public Dictionary<Int32, List<TradingClass>> getConnectedUsers(ErrorDbHandler mainDBhandler, ErrorHandler errorHandler)
        {
            Dictionary<Int32, List<TradingClass>> initializedFollowers = new Dictionary<Int32, List<TradingClass>>();
            List<CompleteFollower> followers = new List<CompleteFollower>();
            SqlDataReader reader = null;
            SqlCommand sqlCmd = null;
            StringBuilder sqlStrBuilder = new StringBuilder();

            //Отримати користувачів
            sqlStrBuilder.Append("SELECT TEMP.AccountId_,    TEMP.AccountType_,    TEMP.SubscriptionType_,\n")
                .Append("TEMP.BrockerName_,  TEMP.BrockerLogin_ ,  TEMP.BrockerPassswd_,\n")
                .Append("TEMP.SyncMethod_,   [USERS].[dbo].[aspnet_Membership].[Email]\n")
                .Append("FROM  [USERS].[dbo].[aspnet_Membership],\n")
                .Append("( SELECT [USERS].[dbo].[USERBROKERS].[AccountId] AS AccountId_,\n")
                .Append("[USERS].[dbo].[USERBROKERS].[UserId]			AS UserId_,\n")
                .Append("[USERS].[dbo].[USERBROKERS].[BrokerLogin]      AS BrockerLogin_,\n")
                .Append("[USERS].[dbo].[USERBROKERS].[BrokerName]		AS BrockerName_,\n")
                .Append("[USERS].[dbo].[USERBROKERS].[BrokerPassword]   AS BrockerPassswd_,\n")
                .Append("[USERS].[dbo].[USERBROKERS].[SubscriptionType] AS SubscriptionType_,\n")
                .Append("[USERS].[dbo].[USERBROKERS].[AccountType]	    AS AccountType_,\n")
                .Append("[USERS].[dbo].[USERBROKERS].[SyncMethod]       AS SyncMethod_\n")
                .Append(" FROM [USERS].[dbo].[USERBROKERS]\n")
                .Append("WHERE [USERS].[dbo].[USERBROKERS].[BrokerName] = '").Append(PLATFORM).Append("'\n")
                .Append("AND [USERS].[dbo].[USERBROKERS].[SubscriptionType] = 'Signal receiver'\n")
                .Append(") AS TEMP\n")
                .Append("WHERE [USERS].[dbo].[aspnet_Membership].[UserId] = TEMP.UserId_");

            sqlCmd = new SqlCommand(sqlStrBuilder.ToString(), conn);

            lock (sqlQueryLock)
            {
                try
                {
                    reader = sqlCmd.ExecuteReader();
                    while (reader.Read())
                    {
                        followers.Add(new CompleteFollower(reader, fieldsMatching));
                    }
                    sqlCmd.Dispose();
                    reader.Close();
                }
                catch (Exception e0)
                {
                    String errorStr = "Exception in DatabaseClass while try to get registered users/account from db";
                    Program.log.Error(errorStr);
                    Program.log.Debug("SQL command is " + sqlCmd.CommandText);
                    Program.log.Debug(e0.Message);
                    Program.log.Debug(e0.StackTrace);
                    Program.WriteError(errorStr + "\n" + e0.Message);
                }
                if (followers.Count == 0) return initializedFollowers;

                //Отримати список провайдерів на які підписані отримані користувачі
                sqlStrBuilder.Clear();
                sqlStrBuilder.Append("SELECT [USERS].[DBO].[AccountIdToSystemId].[AccountId],\n")
                    .Append("[USERS].[DBO].[AccountIdToSystemId].[SystemId]\n")
                    .Append("FROM [USERS].[dbo].[AccountIdToSystemId]\n")
                    .Append("WHERE [USERS].[DBO].[AccountIdToSystemId].[AccountId] = '").Append(followers[0].AccountId).Append("'\n");

                for (int i = 1; i < followers.Count; ++i)
                {
                    sqlStrBuilder.Append("OR [USERS].[DBO].[AccountIdToSystemId].[AccountId] = '").Append(followers[i].AccountId).Append("'\n");
                }
                sqlCmd = new SqlCommand(sqlStrBuilder.ToString(), conn);
                reader = sqlCmd.ExecuteReader();

                Dictionary<String, List<Int32>> readedMathing = new Dictionary<string, List<int>>();
                List<Int32> systems = new List<int>();
                String readedAccountId;

                while (reader.Read())
                {
                    readedAccountId = Convert.ToString(reader["AccountId"]);
                    try
                    {
                        systems = readedMathing[readedAccountId];
                    }
                    catch (KeyNotFoundException)
                    {
                        systems = new List<int>();
                    }
                    systems.Add(Convert.ToInt32(reader["SystemId"].ToString().GetHashCode()));
                    readedMathing[readedAccountId] = systems;
                }
                sqlCmd.Dispose();
                reader.Close();

                for (int i = 0; i < followers.Count; ++i)
                {
                    //Можлива ситуація що фоловер ще не підписаний ні наякі системи
                    try { followers[i].AddListenedSystems(readedMathing[followers[i].AccountId]); }
                    catch (KeyNotFoundException) { followers.RemoveAt(i--); }
                }
                //Ініціалізувати кожного фоловера (створити для них обєкти їх АРІ)
                TradingClass initializedFollower;
                List<TradingClass> initializedFollowersList;

                foreach (CompleteFollower follower in followers)
                {
                    initializedFollower = new TradingClass(
                         follower.AccountId, follower.BrokerLogin, follower.BrokerPassword,
                         "https://testapi.lmaxtrader.com/", follower.Email, mainDBhandler,
                         Presets.GetSyncMode(follower.SynchronizationType)
                         );
                    initializedFollower.Login();

                    foreach (Int32 system in follower.ListenedSystems)
                    {
                        try
                        {
                            initializedFollowersList = initializedFollowers[system];
                        }
                        catch (KeyNotFoundException)
                        {
                            initializedFollowersList = new List<TradingClass>();
                        }
                        initializedFollowersList.Add(initializedFollower);
                        initializedFollowers[system] = initializedFollowersList;
                    }
                }
                return initializedFollowers;
            }
        }

        public String getSelectSQLquery(Dictionary<Int32, List<TradingClass>> datas)
        {
            Dictionary<Int32, List<TradingClass>>.KeyCollection keys = datas.Keys;
            
            if (keys.Count == 0) return "";

            String sqlStr = "AND (\"SYSTEMID\"=";
            Boolean notFirst = false;

            lock (Program.usersLock)
            {
                foreach (Int32 key in keys)
                {
                    if (notFirst)
                        sqlStr += " OR \"SYSTEMID\"=";
                    else
                        notFirst = true;

                    sqlStr += Convert.ToString(key);
                }
                sqlStr += " ) ";
            }
            return sqlStr;
        }

        /// <summary>
        /// This method gets Provider lastest positions. This method throws Exception when error occurs
        /// </summary>
        /// <param name="systemID">the required system ID</param>
        /// <returns>a list of Position objects</returns>
        public List<Position> GetProviderLastPosition(Int64 systemID, DateTime endTime)
        {
            StringBuilder sqlCommandBuilder = new StringBuilder();
            SqlDataReader reader;
            SqlCommand    sqlCommand;
            Position      position;
            List<Position> resultList = new List<Position>();

            sqlCommandBuilder.Append("SELECT ORDERHISTORY.dbo.POSITIONS.SYMBOL \n")
                .Append(", ORDERHISTORY.dbo.POSITIONS.DIRECTION \n")
                .Append(", ORDERHISTORY.dbo.POSITIONS.LOTS \n")
                .Append(", ORDERHISTORY.dbo.POSITIONS.DBTIME \n")
                .Append("FROM ORDERHISTORY.dbo.POSITIONS, ")
                .Append("( SELECT ORDERHISTORY.dbo.POSITIONS.SYMBOL AS SYMBOL_, ")
                .Append("Max(ORDERHISTORY.dbo.POSITIONS.DBTIME) AS DBTIME_ ")
                .Append("FROM ORDERHISTORY.dbo.POSITIONS WHERE ORDERHISTORY.dbo.POSITIONS.SYSTEMID=")
                .Append(systemID).Append("AND  ORDERHISTORY.dbo.POSITIONS.DBTIME <='")
                .Append(endTime.ToString("yyyy-MM-dd HH:mm:ss.fff")).Append("' ").Append("GROUP BY ORDERHISTORY.dbo.POSITIONS.SYMBOL)\n")
                .Append("AS TEMP WHERE ORDERHISTORY.dbo.POSITIONS.SYMBOL=TEMP.SYMBOL_ ")
                .Append("AND ORDERHISTORY.dbo.POSITIONS.DBTIME=TEMP.DBTIME_ ")
                .Append("AND ORDERHISTORY.dbo.POSITIONS.SYSTEMID=").Append(systemID).Append(" ");

            lock (sqlQueryLock)
            {            
                sqlCommand = new SqlCommand(sqlCommandBuilder.ToString(), conn);
                reader = sqlCommand.ExecuteReader();
                    
                while(reader.Read())
                {
                    position = new Position();
                    position.direction = Convert.ToInt32(reader["DIRECTION"].ToString());
                    position.qty = Convert.ToDouble(reader["LOTS"].ToString());
                    position.symbol = reader["SYMBOL"].ToString().Trim();
                    resultList.Add(position);
                }
                reader.Close();
                return resultList;
            }
        }

        public List<DBResult> GetSystemsLastPositions(List<Int32> listenedSystemsList, DateTime limitTime)
        {
            List<DBResult> systemsPosList = new List<DBResult>();
            SqlCommand sqlCommand;
            SqlDataReader reader;

            if (listenedSystemsList.Count == 0) return systemsPosList;

            StringBuilder strBuilder = new StringBuilder();
            strBuilder.Append("SELECT POSITIONS.SYMBOL, POSITIONS.DIRECTION, POSITIONS.LOTS, POSITIONS.DBTIME, POSITIONS.SYSTEMID, ")
                .Append("POSITIONS.USERID, POSITIONS.PLATFORM ")
                .Append(" FROM POSITIONS,	( SELECT POSITIONS.SYMBOL AS SYMBOL_, Max(POSITIONS.DBTIME) AS DBTIME_, POSITIONS.SYSTEMID AS SYSTEMID_")
                .Append("	FROM POSITIONS WHERE ")
			    .Append("POSITIONS.DBTIME <= '").Append(limitTime.ToString("yyyy-MM-dd HH:mm:ss.fff")).Append("' ")
                .Append(" AND ( POSITIONS.SYSTEMID=").Append(listenedSystemsList[0]).Append(" ");
            for (int i = 1; i < listenedSystemsList.Count; ++i)
            {
                strBuilder.Append("OR POSITIONS.SYSTEMID = ").Append(listenedSystemsList[i]).Append(" ");
            }
		    strBuilder.Append(") GROUP BY POSITIONS.SYMBOL, POSITIONS.SYSTEMID) AS TEMP ")
	            .Append("WHERE POSITIONS.SYMBOL=TEMP.SYMBOL_ ")
		        .Append("AND POSITIONS.DBTIME=TEMP.DBTIME_ ")
		        .Append("AND POSITIONS.SYSTEMID = TEMP.SYSTEMID_ ")
                .Append("ORDER BY POSITIONS.SYSTEMID");

            lock (sqlQueryLock)
            {
                sqlCommand = new SqlCommand(strBuilder.ToString(), conn);
                reader = sqlCommand.ExecuteReader();

                while (reader.Read())
                {
                    systemsPosList.Add(new DBResult(reader));
                }
                sqlCommand.Dispose();
                reader.Close();
            }

            return systemsPosList;
        }

        public List<CompleteFollower> GetCompleteFollowers(List<string> userIds)
        {
            List<CompleteFollower> resultList = new List<CompleteFollower>();
            SqlDataReader sqlReader;
            StringBuilder sqlStr = new StringBuilder();

            if (userIds.Count == 0) return resultList;
           
            sqlStr.Append("select [USERS].[dbo].[ACCOUNTS].[EMAIL], ")
                .Append("[users].[dbo].[ACCOUNTDETAILS].[BROCKER], ")
                .Append("[users].[dbo].[ACCOUNTDETAILS].[BROCKERLOGIN], ")
                .Append("[users].[dbo].[ACCOUNTDETAILS].[BROCKERPASSWD], ")
                .Append("[users].[dbo].[ACCOUNTDETAILS].[LISTENEDSYSTEM] ")
                .Append("FROM [USERS].[DBO].[ACCOUNTS], [USERS].[DBO].[ACCOUNTDETAILS] ");

            sqlStr.Append("WHERE ( [USERS].[dbo].[ACCOUNTDETAILS].[USERID] = '").Append(userIds[0]).Append("' AND ")
                .Append("[USERS].[dbo].[ACCOUNTS].[USERID] = '").Append(userIds[0]).Append("')");

            for (int i = 1; i < userIds.Count; ++i)
            {
                sqlStr.Append("OR ([USERS].[dbo].[ACCOUNTDETAILS].[USERID] = '").Append(userIds[i]).Append("' AND ")
                    .Append("[USERS].[dbo].[ACCOUNTS].[USERID] = '").Append(userIds[0]).Append("')");
            }

            lock (sqlQueryLock)
            {
                sqlReader = (new SqlCommand(sqlStr.ToString(), conn)).ExecuteReader();
                while (sqlReader.Read())
                {
                    //resultList.Add(new CompleteFollower(sqlReader));
                }
                sqlReader.Close();
            }
            return resultList;
        }

        /// <summary>
        /// Get a list of changes in history, and return a timestamp value into functon argument
        /// </summary>
        /// <param name="timeValue">a ref parameter, the timestamp last readed</param>
        /// <returns>a list of DataHistoryRow objects</returns>
        List<DataHistoryRow> GetHistory(ref UInt64 timeValue)
        {
            List<DataHistoryRow> readedData = new List<DataHistoryRow>();
            SqlCommand sqlCommand = new SqlCommand("SELECT * FROM \"USERS\".DBO.\"UserHistory\" WHERE \"USERS\".DBO.\"UserHistory\".\"ThisRowVersion\" > " + timeValue + " Order by \"USERS\".DBO.\"UserHistory\".\"ThisRowVersion\" DESC", conn);
            SqlDataReader reader = null;
            Boolean first = true;

            lock (sqlQueryLock)
            {
                reader = sqlCommand.ExecuteReader();
                while (reader.Read())
                {
                    if (first)
                    {
                        timeValue = BitConverter.ToUInt64(((byte[])reader["ThisRowVersion"]).Reverse().ToArray(), 0);
                        first = false;
                    }
                    readedData.Add(new DataHistoryRow(reader));
                }
                sqlCommand.Dispose();
                reader.Close();
            }
            return readedData;
        }

    }
}