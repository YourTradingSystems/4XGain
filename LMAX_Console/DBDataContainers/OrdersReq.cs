using System;
using System.Linq;
using CS_server_addin.GeneralConstants;

namespace CS_server_addin.DBDataContainers
{
    public class OrdersReq
    {
        public OrderOperations Operation = 0;
        public String Symbol = "";
        public int Direction = -1;
        public double Volume = 0.0;
    }
}
