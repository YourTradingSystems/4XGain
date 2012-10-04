using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ClientHandler2.DataType;

namespace SynchronizationProject
{
    class SyncClass
    {
        private List<Position> cumulatedPositons;
        private bool isActiveFilter;

        public SyncClass(bool activateFilter)
        {
            isActiveFilter    = activateFilter;
            cumulatedPositons = new List<Position>();
        }

        //Set a list of positions
        public void SetPositions(List<Position> positionList)
        {
            cumulatedPositons = positionList;
        }

        // Return null if order filtered and cannot return
        public Order FilterOrder(Order order)
        {
            for (int i = 0; i < cumulatedPositons.Count; ++i)
            {
                if (cumulatedPositons[i].symbol == order.Symbol)
                {
                    Order resultOrder = cumulatedPositons[i].addOrderUntilClose(order);
                    if (resultOrder != null)
                    {
                        cumulatedPositons.RemoveAt(i);
                    }
                    return resultOrder;
                }
            }
            return order;
        }

        public String PositionsToString()
        {
            String cumulateString = "";
            foreach (Position position in cumulatedPositons)
            {
                cumulateString += position.ToString() + "\n";
            }
            return cumulateString;
        }
    }
}
