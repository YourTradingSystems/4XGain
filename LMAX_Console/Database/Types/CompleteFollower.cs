using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data.SqlClient;

namespace Database.Types
{
    public class CompleteFollower
    {
        public string AccountId { get; set; }
        public int AccountType { get; set; }
        public string Email { get; set; }
        public string Broker { get; set; }
        public string BrokerLogin { get; set; }
        public string BrokerPassword { get; set; }
        public int SynchronizationType { get; set; }
        public List<Int32> ListenedSystems { get { return listenedSystems.ToList<Int32>(); }  }
        private HashSet<Int32> listenedSystems = new HashSet<int>();

        /// <summary>
        /// An object constructor wich takes two parameters : an SqlDataReader to get datas from this reader
        /// and a Dictionary which contains field name matching 
        /// </summary>
        /// <param name="dataReader">a data container</param>
        /// <param name="fields">a dictionary with name matching</param>
        public CompleteFollower(SqlDataReader dataReader, Dictionary<String, String> fields)
        {
            InitializeFromMainDB(dataReader, fields);
        }

        public CompleteFollower()
        {
            AccountId = String.Empty;
            AccountType = 0;
            Email = String.Empty;
            Broker = String.Empty;
            BrokerLogin = String.Empty;
            BrokerPassword = String.Empty;
            SynchronizationType = 0;
        }

        public void InitializeFromMainDB(SqlDataReader reader, Dictionary<String, String> fields)
        {
            AccountId   = Convert.ToString(reader[fields["ACCOUNTID"]]);
            AccountType = 0;//Convert.ToInt32(reader[fields["ACCOUNTTYPE"]]);
            Email       = Convert.ToString(reader[fields["EMAIL"]]);
            Broker      = Convert.ToString(reader[fields["BROKER"]]);
            BrokerLogin = Convert.ToString(reader[fields["BROKERLOGIN"]]);
            BrokerPassword = Convert.ToString(reader[fields["BROKERPASSWORD"]]);
            SynchronizationType = Convert.ToInt32(reader[fields["SYNCHRONIZATIONTYPE"]]);
        }

        public void AddListenedSystems(List<Int32> lSystems)
        {
            foreach (Int32 systemId in lSystems)
            {
                listenedSystems.Add(systemId);
            }
        }
    }
}
