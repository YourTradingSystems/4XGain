using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DBproxy
{
    class DbFactory
    {
        private static List<DbDriver> _listOfDbDrivers = new List<DbDriver>();
        private static Object _DbDriverListLock = new Object();
        private static int _maxDbDriverInList = 50;

        public static int MaxUniqDbDrivers { get { return _maxDbDriverInList; } set { _maxDbDriverInList = value; } }

        public DbDriver GetDbDriver(Object caller)
        {
            DbDriver resultDbDriver;

            lock(_DbDriverListLock)
            {
                if (_listOfDbDrivers.Count == 0)
                {
                    resultDbDriver = new DbDriver();
                }
                else
                {
                    if (_listOfDbDrivers.Count < _maxDbDriverInList)
                    {
                        resultDbDriver = new DbDriver();
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
                return resultDbDriver;
            }
        }
    }
}
