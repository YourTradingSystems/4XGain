using System;
using System.Linq;

namespace CS_server_addin.DBDataContainers
{
    public class OrdersReq
    {
        public Utilites.Presets.OrderOperations Operation = 0;
        public String Symbol = "";
        public int Direction = -1;
        public double Volume = 0.0;
        public Int32 SystemID;
    }
}
