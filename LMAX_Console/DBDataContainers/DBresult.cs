using System;
using System.Linq;
using System.Data.SqlClient;

namespace CS_server_addin.DBDataContainers
{
    public struct DBResult
    {
        public string UserId;
        public int    SystemId;
        public string Platform;
        public string Symbol;
        public double Lots;
        public int    Direction;
        public DateTime OpenTime;
        public double OpenPrice;
        public DateTime DbTime;

        public DBResult(SqlDataReader reader)
        {
            UserId = Convert.ToString(reader["USERID"]);
            SystemId = Convert.ToInt32(reader["SYSTEMID"]);
            Platform = Convert.ToString(reader["PLATFORM"]);
            Symbol = Convert.ToString(reader["SYMBOL"]);
            Lots = Convert.ToDouble(reader["LOTS"]);
            Direction = Convert.ToInt32(reader["DIRECTION"]);
            OpenTime = (DateTime)reader["OPENTIME"];
            OpenPrice = Convert.ToDouble(reader["OPENPRICE"]);
            DbTime = (DateTime)reader["DBTIME"]; 
        }
    }
}
