using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CS_server_addin.DBDataContainers
{
    public class Operations
    {
        public List<OrdersReq> Requests = new List<OrdersReq>();
        public int SystemID = 0;
        public Operations(int systemID)
        {
            SystemID = systemID;
        }
    }
}
