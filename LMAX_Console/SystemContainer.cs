using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Database;
using System.Collections.Concurrent;
using Database.LocalDatabase;
using Utilites;
using Sender;
using Follower;

namespace CS_server_addin.DBDataContainers
{
    public class SystemContainer
    {
        // Dictionary <Int32 - SystemID, TSystem - provider systems>
        private Dictionary<Int32, TSystem> NewSystems = new Dictionary<Int32, TSystem>();
        private Dictionary<Int32, TSystem> OldSystems = new Dictionary<Int32, TSystem>();
        private BlockingCollection<Operations> _BlockingQueue;
        private Object lockSystems = new Object();

        public SystemContainer(BlockingCollection<Operations> BlockingQueue, DateTime limitTime)
        {
            _BlockingQueue = BlockingQueue;
            IEnumerable<Int32> keys;
            TSystem system;

            lock (Program.usersLock)
            {
                keys = from key in Program.idUsers.Keys select key;
            }
            List<DBResult> loadedResult = Program.remoteDbHandler.GetSystemsLastPositions(keys.ToList<Int32>(), limitTime);
            lock (lockSystems)
            {
                foreach (DBResult loadedResultItem in loadedResult)
                {
                    try
                    {
                        system = OldSystems[loadedResultItem.SystemId];
                    }
                    catch (KeyNotFoundException)
                    {
                        system = new TSystem(loadedResultItem.SystemId);
                    }
                    system.AddPosition(loadedResultItem);
                    OldSystems[loadedResultItem.SystemId] = system;
                }
            }
        }

        /// <summary>
        /// Add selected providers systems
        /// </summary>
        /// <param name="datasToProcess"></param>
        public void InitializeSystems(List<DBResult> datasToProcess)
        {
            TSystem system;
            lock (lockSystems)
            {
                foreach (DBResult dataItem in datasToProcess)
                {
                    try
                    {
                        system = OldSystems[dataItem.SystemId];
                    }
                    catch (KeyNotFoundException)
                    {
                        system = new TSystem(dataItem.SystemId);
                    }
                    system.AddPosition(dataItem);
                    OldSystems[dataItem.SystemId] = system;
                }
            }
        }

        public Dictionary<Int32, TSystem> GetOldSystems()
        {
            lock (lockSystems)
            {
                return OldSystems;
            }
        }

        /// <summary>
        /// Verify if some data exists in new systems
        /// </summary>
        /// <returns>TRUE if exists, FALSE otherwise</returns>
        public bool IsEmpty()
        {
            lock (lockSystems)
            {
                return NewSystems.Count == 0;
            }
        }

        /// <summary>
        /// Return new systems
        /// </summary>
        /// <returns>new systems in dictionary</returns>
        public Dictionary<Int32, TSystem> getData()
        {
            lock (lockSystems)
            {
                return NewSystems;
            }
        }

        public IEnumerable<Int32> GetListenedSystemIds()
        {
            lock (lockSystems)
            {
                return OldSystems.Keys;
            }
        }

        public void AddElement(DBResult element)
        {
            TSystem selectedSystem;
            lock (lockSystems)
            {
                try
                {
                    selectedSystem = NewSystems[element.SystemId];
                }
                catch (KeyNotFoundException)
                {
                    selectedSystem = new TSystem(element.SystemId);
                }
                selectedSystem.AddPosition(element);
                NewSystems[element.SystemId] = selectedSystem;
            }
        }

        public void processSystems(Dictionary<Int32, List<TradingClass>> customers, DateTime lastScanTime)
        {
            lock (lockSystems)
            {
                try
                {
                    List<Operations> operations = GetDifferent(OldSystems, NewSystems);
                    NewSystems = new Dictionary<Int32, TSystem>();

                    for (int i = 0; i < operations.Count; ++i)
                    {
                        try
                        {
                            operations.ElementAt(i).traders = customers[operations.ElementAt(i).SystemID];
                        }
                        catch (KeyNotFoundException)
                        {
                            operations.RemoveAt(i);
                            --i;
                        }
                    }

                    foreach (Operations op in operations)
                    {
                        op.reqPrevScanTime = lastScanTime;
                        _BlockingQueue.Add(op);
                    }

                    // Debug only : write to console all operations
                    foreach (Operations item in operations)
                    {
                        System.Console.WriteLine("System id:{0}; Requests count:{1}; ", item.SystemID, item.Requests.Count);
                        System.Console.WriteLine("Write all requests : ");
                        for (int i = 0; i < item.Requests.Count; ++i)
                        {
                            System.Console.WriteLine("{0}:Operation:{1}; Symbol:{2}; Direction:{3}; Volume:{4}", i, Presets.OperationsToStr(item.Requests.ElementAt(i).Operation),
                                item.Requests.ElementAt(i).Symbol, item.Requests.ElementAt(i).Direction, item.Requests.ElementAt(i).Volume);
                        }
                        System.Console.WriteLine("-------------------------------------------------------------------------------");
                    }
                }
                catch (Exception e)
                {
                    Program.log.Error("Exception processing systems");
                    Program.log.Debug(e.Message);
                    Program.log.Debug(e.StackTrace);
                }
            }

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
                                                              where item.Symbol.CompareTo(newSystemItems[j].Symbol) == 0 //i <---!!!!
                                                              select item;
                        /* якщо щось знайдено тоді */
                        if (selectedItems.Count() != 0)
                        {
                            System.Console.WriteLine("Found somthing in old list");
                            if (selectedItems.Count() == 0) System.Console.WriteLine("Error is SystemContainers???");
                            List<OrdersReq> res = CreateOrder(selectedItems.ElementAt(0), newSystemItems.ElementAt(j));
                            operations.Requests.AddRange(res); // <--- Add to global list
                            oldSystemItems.Remove(selectedItems.ElementAt(0));
                            oldSystemItems.Add(newSystemItems.ElementAt(j));
                        }
                        /* якщо нічого не знайдено тоді ми маємо новий */
                        else
                        {
                            System.Console.WriteLine("Nothing found in new list");
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
                    Console.WriteLine("Close some lots");
                    OrdersReq req = new OrdersReq();
                    req.Operation = Utilites.Presets.OrderOperations.Clsoe;
                    req.Direction = oldItem.Direction;
                    req.Volume = oldItem.Lots - newItem.Lots;
                    Console.WriteLine("Lots size {0}", Convert.ToString(req.Volume));
                    req.Symbol = oldItem.Symbol;
                    reqList.Add(req);
                }
                else
                {
                    Console.WriteLine("Open some new lots");
                    OrdersReq req = new OrdersReq();
                    req.Operation = Utilites.Presets.OrderOperations.Open;
                    req.Direction = oldItem.Direction;
                    req.Volume = newItem.Lots - oldItem.Lots;
                    Console.WriteLine("Lots size {0}", Convert.ToString(req.Volume));
                    req.Symbol = oldItem.Symbol;
                    reqList.Add(req);
                }
            }
            else
            {
                Console.WriteLine("New direction orders");
                OrdersReq req = new OrdersReq();
                req.Operation = Utilites.Presets.OrderOperations.Clsoe;
                req.Direction = oldItem.Direction;
                req.Volume = oldItem.Lots;
                req.Symbol = oldItem.Symbol;
                reqList.Add(req);

                req = new OrdersReq();
                req.Operation = Utilites.Presets.OrderOperations.Open;
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
            req.Operation =Utilites.Presets.OrderOperations.Open;
            req.Symbol = item.Symbol;
            req.Volume = item.Lots;
            reqList.Add(req);
            return reqList;
        }
    }
}
