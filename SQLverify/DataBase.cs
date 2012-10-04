using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data.SqlClient;

namespace SQLverify
{
    

    class DataBase
    {
        private SqlConnection conn;
        private SqlDependency sqlDependency;

        public DataBase()
        {
            ConnectToDB();
            sqlDependency = new SqlDependency();
            //sqlDependency.AddCommandDependency(new SqlCommand("INSERT", conn));
            sqlDependency.OnChange += new OnChangeEventHandler(onChange);
        }

        public void ConnectToDB()
        {
            conn = new SqlConnection("Network Library=DBMSSOCN;" +
                                     "Data Source=" + "portativ\\MSQL" + "," + "4055" + ";" +
                                     "Initial Catalog=" + "USERHISTORY" + ";" +
                                     "User Id=" + "Sasha" + ";" +
                                     "Password=" + "qwerty_111" + ";"
                                     );
            //conn.Open();
            SqlDependency.Start(conn.ConnectionString);
        }



        public void onChange(object sender, SqlNotificationEventArgs e)
        {
            Console.WriteLine("Sender : {0}", sender.ToString());
            Console.WriteLine("Event arguments : {1}",e.ToString());
        }
    }
}
