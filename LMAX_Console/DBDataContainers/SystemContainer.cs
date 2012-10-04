using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CS_server_addin.DBDataContainers
{
    class SystemContainer
    {
        private Dictionary<Int32, TSystem> NewSystems = new Dictionary<Int32,TSystem>();
        private Dictionary<Int32, TSystem> OldSystems = new Dictionary<Int32, TSystem>();

        public bool IsEmpty()
        {
            return NewSystems.Count == 0;
        }

        public Dictionary<Int32, TSystem> getData()
        {
            return NewSystems;
        }

        public void AddElement(DBResult element)
        {
            TSystem selectedSystem;
            try
            {
                selectedSystem = NewSystems[element.SystemId];
                selectedSystem.AddPosition(element);
            }
            catch (KeyNotFoundException)
            {
                selectedSystem = new TSystem(element.SystemId);
                selectedSystem.AddPosition(element);
                NewSystems.Add(element.SystemId, selectedSystem);
            }
        }

        public void processSystems()
        {
            List<Operations> operations = GetDifferent(OldSystems, NewSystems);
            NewSystems = new Dictionary<Int32, TSystem>();

        }

        private List<Operations> GetDifferent(Dictionary<Int32, TSystem> oldSystems, Dictionary<Int32, TSystem> newSystems)
        {
            List<Operations> finded = new List<Operations>();

            int keyCount = newSystems.Keys.Count;
            
            for (int i=0; i<keyCount; ++i)
            {
                int key = newSystems.Keys.ElementAt(i);
                TSystem newSystem = newSystems[key];
                
                newSystem.SimplyToLastPosition();

                Operations operations = new Operations(key);

                try
                {
                    TSystem oldSystem = oldSystems[key];

                    List<DBResult> oldSystemItems = oldSystem.GetPositions();
                    List<DBResult> newSystemItems = newSystem.GetPositions();
                    // Пошук одинакових записів по символу в межах СИСТЕМИ
                    for (int j = 0; j < newSystemItems.Count; ++j)
                    {
                        IEnumerable<DBResult> selectedItems = from item in oldSystemItems
                                                              where item.Symbol.CompareTo(newSystemItems[i].Symbol) == 0
                                                              select item;
                        /* якщо щось знайдено тоді */
                        if (selectedItems.Count() != 0)
                        {
                            if (selectedItems.Count() == 0) System.Console.WriteLine("Error is SystemContainers???");
                            List<OrdersReq> res = CreateOrder(selectedItems.ElementAt(0), newSystemItems.ElementAt(j));
                            operations.Requests.AddRange(res); // <--- Add to global list
                            oldSystemItems.Remove(selectedItems.ElementAt(0));
                            oldSystemItems.Add(newSystemItems.ElementAt(j));
                        }
                        /* якщо нічого не знайдено тоді ми маємо новий */
                        else
                        {
                            List<OrdersReq> res = CreateOrder(selectedItems.ElementAt(0));
                            operations.Requests.AddRange(res); // <--- Add to global list
                            oldSystemItems.Add(newSystemItems.ElementAt(j));
                        }
                    }
                }
                catch (KeyNotFoundException)
                {
                    // Create operations
                    List<DBResult> newSystemItems = newSystem.GetPositions();
                    foreach (DBResult item in newSystemItems)
                    {
                        operations.Requests.AddRange(CreateOrder(item));
                    }
                    // Add new System to dictionery
                    oldSystems[key] = newSystem;
                }
                finded.Add(operations);   
            }
            return finded;
        }

        private List<OrdersReq> CreateOrder(DBResult oldItem, DBResult newItem)
        {
            List<OrdersReq> reqList = new List<OrdersReq>();

            if (oldItem.Direction == newItem.Direction)
            {
                if (oldItem.Lots > newItem.Lots)
                {
                    OrdersReq req = new OrdersReq();
                    req.Operation = GeneralConstants.OrderOperations.Clsoe;
                    req.Direction = oldItem.Direction;
                    req.Volume = oldItem.Lots - newItem.Lots;
                    req.Symbol = oldItem.Symbol;
                    reqList.Add(req);
                }
                else
                {
                    OrdersReq req = new OrdersReq();
                    req.Operation = GeneralConstants.OrderOperations.Open;
                    req.Direction = oldItem.Direction;
                    req.Volume = newItem.Lots - oldItem.Lots;
                    req.Symbol = oldItem.Symbol;
                    reqList.Add(req);
                }
            }
            else
            {
                OrdersReq req = new OrdersReq();
                req.Operation = GeneralConstants.OrderOperations.Clsoe;
                req.Direction = oldItem.Direction;
                req.Volume = oldItem.Lots;
                req.Symbol = oldItem.Symbol;
                reqList.Add(req);

                req = new OrdersReq();
                req.Operation = GeneralConstants.OrderOperations.Open;
                req.Direction = newItem.Direction;
                req.Symbol = newItem.Symbol;
                req.Volume = newItem.Lots;
                reqList.Add(req);
            }
            return reqList;
        }

        private List<OrdersReq> CreateOrder(DBResult item)
        {
            List<OrdersReq> reqList = new List<OrdersReq>();
            OrdersReq req = new OrdersReq();
            req.Direction = item.Direction;
            req.Operation = GeneralConstants.OrderOperations.Open;
            req.Symbol = item.Symbol;
            req.Volume = item.Lots;
            reqList.Add(req);
            return reqList;
        }
    }
}
