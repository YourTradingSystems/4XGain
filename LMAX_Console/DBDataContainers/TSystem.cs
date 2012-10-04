using System;
using System.Collections.Generic;
using System.Linq;
using CS_server_addin.DBDataContainers;

namespace CS_server_addin
{
    public class TSystem
    {
        public int SystemID = 0;
        private List<DBResult> positions = new List<DBResult>();
        
        public TSystem(int systemID)
        {
            this.SystemID = systemID;
        }

        public void AddPosition(DBResult position)
        {
            positions.Add(position);
        }

        public List<DBResult> GetPositions()
        {
            return positions;
        }

        public void SimplyToLastPosition()
        {
            List<DBResult> lastPositions = new List<DBResult>();
            DBResult cItem;

            while(positions.Count > 0)
            {
                cItem = positions[0];
                positions.RemoveAt(0);

                for (int i = 0; i < positions.Count; ++i)
                {
                    if (cItem.Symbol.CompareTo(positions[i].Symbol) == 0)
                    {
                        if (cItem.DbTime < positions[i].DbTime)
                        {
                            cItem = positions[i];
                        }
                        positions.RemoveAt(i);
                        --i;
                    }
                }
                lastPositions.Add(cItem);
            }
            positions = lastPositions;
        }
    }
}
