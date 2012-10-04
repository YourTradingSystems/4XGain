using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Database.LocalDatabase;

namespace Database.LocalDatabase
{
    public static class DbFactory
    {
        private static List<LocalDbAdapter> _listOfDbDrivers = new List<LocalDbAdapter>();
        private static Object _DbDriverListLock = new Object();
        private static int _maxDbDriverInList = 50;

        public static int MaxUniqDbDrivers { get { return _maxDbDriverInList; } set { _maxDbDriverInList = value; } }

        public static LocalDbAdapter GetDbDriver(Object caller)
        {
            LocalDbAdapter resultDbDriver;

            lock (_DbDriverListLock)
            {
                if (_listOfDbDrivers.Count == 0)
                {
                    resultDbDriver = new LocalDbAdapter();
                    _listOfDbDrivers.Add(resultDbDriver);
                }
                else
                {
                    if (_listOfDbDrivers.Count < _maxDbDriverInList)
                    {
                        resultDbDriver = new LocalDbAdapter();
                        _listOfDbDrivers.Add(resultDbDriver);
                    }
                    else
                    {
                        resultDbDriver = _listOfDbDrivers[0];
                        for (int i = 1; i < _listOfDbDrivers.Count; ++i)
                        {
                            if (_listOfDbDrivers[i].LinkedObjectCount < resultDbDriver.LinkedObjectCount)
                                resultDbDriver = _listOfDbDrivers[i];
                        }
                    }
                }
                resultDbDriver.link(caller);
                Console.WriteLine("Database list {0}", _listOfDbDrivers.Count);
                return resultDbDriver;
            }
        }
    }
}
