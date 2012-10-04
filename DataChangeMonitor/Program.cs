using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using System.Data.SqlClient;
using System.Threading;

namespace DataChangeMonitor
{
    class Program
    {
        static void Main(string[] args)
        {
            SqlConnection conn;
            SqlDataReader reader = null;
            UInt64 ts = 0;
            List<DataHistoryRow> readedData = new List<DataHistoryRow>(); 
            
            //Connect to database
            conn = new SqlConnection("Network Library=DBMSSOCN;" +
                                     "Data Source=" + "WIN-83YVOQWW9RH\\SQLEXPRESS" + "," + "1649" + ";" +
                                     "Initial Catalog=" + "USERS" + ";" +
                                     "User Id=" + "rootuser" + ";" +
                                     "Password=" + "qwerty" + ";"
                                     );
            try
            {
                conn.Open();
            }
            catch (Exception e0)
            {
                Console.WriteLine("Cannot connect");
                Console.WriteLine(e0.Message);
            }
            while (true)
            {
                SqlCommand sqlCommand = new SqlCommand("SELECt * FROM UserHistory where \"ThisRowVersion\" > " + ts + " Order by \"ThisRowVersion\" DESC", conn);
            
                try
                {
                    bool first = true;
                    reader = sqlCommand.ExecuteReader();
                    while(reader.Read())
                    {
                        if (first)
                        {
                            ts = BitConverter.ToUInt64(((byte[])reader["ThisRowVersion"]).Reverse().ToArray(), 0);
                            first = false;
                        }
                        readedData.Add(new DataHistoryRow(reader));
                    }
                    if (readedData.Count != 0)
                    {
                        Console.WriteLine("Readed {0} records",readedData.Count);
                        foreach (DataHistoryRow row in readedData)
                        {
                            StateObject s = new StateObject(row);
                            while (!s.IsOperation && s.StateEnabled)
                            {
                                s.Next();
                            }
                            if (s.StateEnabled)
                            {
                                Console.WriteLine(s.GetAnalizedRow().ToString());
                                s.GetOperation().Invoke(new Object());
                            }
                            else
                            {
                                Console.WriteLine("State not enabled");
                            }
                        }
                        readedData.Clear();
                    }
                }
                catch(Exception e)
                {
                    Console.WriteLine("Exception : {0}",e.Message);
                }
                finally
                {
                    if (reader != null && !reader.IsClosed)
                        reader.Close();
                }
                Thread.Sleep(1000);
            }
            
        }
    }
}
