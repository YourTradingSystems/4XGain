using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data.SqlClient;

namespace DataChangeMonitor
{
    class DataHistoryRow
    {
        //public String UserID { get; set; }
        public String TableName { get; set; }
        public Int32  CRID { get; set; }
        public String RowOldVersion { get; set; }
        public UInt64 ThisRowVersion { get; set; }
        public Char   OperationType { get; set; }

        public DataHistoryRow()
        {
            //UserID = String.Empty;
            TableName = RowOldVersion = String.Empty;
            CRID = 0;
            ThisRowVersion = 0;
            OperationType = ' ';
        }

        public DataHistoryRow(SqlDataReader reader)
        {
            //UserID          = Convert.ToString(reader["UserId"]);
            TableName       = Convert.ToString(reader["TableName"]);
            CRID = Convert.ToInt32(reader["CRID"]);
            RowOldVersion   = Convert.ToString(reader["RowOldVersion"]);
            ThisRowVersion  = BitConverter.ToUInt64(((byte[])reader["ThisRowVersion"]).Reverse().ToArray(),0);
            OperationType   = Convert.ToChar(reader["OperationType"]);
        }

        public override string ToString()
        {
            StringBuilder strBuilder = new StringBuilder();
            strBuilder.Append("UserId=").Append("___").Append("; ").Append("TableName=").Append(TableName)
                .Append("; ").Append("CRID=").Append(CRID).Append("; ").Append("RowOldVersion=[").Append(RowOldVersion)
                .Append("]; ").Append("ThisRowVersion=").Append(ThisRowVersion).Append("; ").Append("OperationType=")
                .Append(OperationType).Append(";");
            return strBuilder.ToString();
        }
    }
}
