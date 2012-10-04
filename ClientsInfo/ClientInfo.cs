using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ClientsInfo
{
    public class ClientInfo
    {
        public String ClientId;
        public Int32  ClientsListenedSystemId;
        public String ClientBrockerLogin;
        public String ClientEmail;
        public String ClientBrocker;

        public ClientInfo()
        {
            ClientId = String.Empty;
            ClientBrockerLogin = String.Empty;
            ClientBrocker = String.Empty;
            ClientEmail = String.Empty;
            ClientsListenedSystemId = 0;
        }
    }
}
