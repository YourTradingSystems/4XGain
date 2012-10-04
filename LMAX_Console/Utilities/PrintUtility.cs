using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CS_server_addin;
using CS_server_addin.DBDataContainers;


namespace Database.LocalDatabase
{
    public class PrintUtility
    {
        public static void PrintDictionary(Dictionary<Int32, TSystem> d)
        {
            int keyCount = d.Keys.Count;

            System.Console.WriteLine("**** Begin ***********************************");
            for (int i = 0; i < keyCount; ++i)
            {
                System.Console.WriteLine("Key : " + d.Keys.ElementAt(i).ToString() + "\nValue : \n" + d[d.Keys.ElementAt(i)].ToString());
                System.Console.WriteLine("----------------------------------------------");
            }
            System.Console.WriteLine("**** End *************************************");
        }
    }
}
