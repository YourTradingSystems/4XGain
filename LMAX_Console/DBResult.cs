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
            UserId = Convert.ToString(reader["USERID"]).Trim();
            SystemId = Convert.ToInt32(reader["SYSTEMID"]);
            Platform = Convert.ToString(reader["PLATFORM"]).Trim();
            Symbol = Convert.ToString(reader["SYMBOL"]).Trim();
            Lots = Convert.ToDouble(reader["LOTS"]);
            Direction = Convert.ToInt32(reader["DIRECTION"]);
            OpenTime = new DateTime();
            OpenPrice = 0.0;
            DbTime = (DateTime)reader["DBTIME"]; 
        }

        public override String ToString()
        {
            String resultStr = "UserID=" + Convert.ToString(UserId) + "; SystemID=" + Convert.ToString(SystemId) + "; Platform=" + Platform;
            resultStr += "; Symbol=" + Symbol + "; Lots=" + Convert.ToString(Lots) + "; Direction=" + Convert.ToString(Direction) + "; OpenTime=" + OpenTime.ToString("yyyy-MM-dd HH:mm:ss.fff");
            resultStr += "; OpenPrice=" + Convert.ToString(OpenPrice) + "; DBTime=" + DbTime.ToString("yyyy-MM-dd HH:mm:ss.fff");
            return resultStr;
        }
    }
}
