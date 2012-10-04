using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data.SqlClient;

namespace DataChangeMonitor
{
    class EventRowDetailis
    {
        public String TableName { get; set; }
        public int CRID { get; set; }
        public string RowOldVersion { get; set; }
        public char OperationType { get; set; }

        public EventRowDetailis()
        {
            TableName = "";
            CRID = 0;
            RowOldVersion = "";
            OperationType = ' ';
        }

        public EventRowDetailis(SqlDataReader reader)
        {
            TableName = Convert.ToString(reader["TableName"]);
            CRID = Convert.ToInt32(reader["CRID"]);
            RowOldVersion = Convert.ToString(reader["RowOldVersion"]);
            OperationType = Convert.ToChar(reader["OperationType"]);
        }
    }
}
