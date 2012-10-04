using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Database.Types;
using Utilites;
using CS_server_addin.DBDataContainers;
using DataTypess;
using Follower;

namespace Sender
{

    partial class Program
    {

        /// <summary>
        /// Add a user ID to a list of users, that must be synchronized
        /// </summary>
        /// <param name="userID">a user Id</param>
        public static void MarkUserToSync(String userId)
        {
            lock (mustSyncLock)
            {
                syncUsers.Add(userId);
            }
        }

        /// <summary>
        /// Remove a userId from list of users, that must be synchronized
        /// </summary>
        /// <param name="userId">a user ID</param>
        public static void UnmarkUserToSync(String userId)
        {
            lock (mustSyncLock)
            {
                if (!syncUsers.Remove(userId))
                {
                    Console.WriteLine("Some problems in UnmarkUserToSync");
                }
            }
        }

        /// <summary>
        /// Return a list of users that must be synchronized
        /// </summary>
        /// <returns>A loist of user ID's that identificate users that must be synchronized</returns>
        public static List<String> GetUsersMarkedToSync()
        {
            List<String> resList = new List<String>();
            lock (mustSyncLock)
            {
                resList.AddRange(syncUsers);
            }
            return resList;
        }

        /// <summary>
        /// Search for TradingClass by UserID and return it if finded, else return null
        /// </summary>
        /// <param name="userID">the ID of user, search for</param>
        /// <returns>A TradingClass object if finded, or null if not</returns>
        public static TradingClass GetFollowerById(String followerId)
        {
            lock (usersLock)
            {
                IEnumerable<TradingClass> selected = from item    in idUsers.Values
                                                     from subItem in item
                                                     where subItem.FollowerID == followerId
                                                     select subItem;

                return selected.Count() == 0 ? null : selected.ElementAt(0);
            }
        }

        /// <summary>
        /// This method return users listened systemID by userID
        /// </summary>
        /// <param name="UserID">user ID</param>
        /// <returns>listed system ID or -1 if not found</returns>
        public static Int32 getListenedID(String followerId)
        {
            lock (usersLock)
            {
                IEnumerable<Int32> selectedId = from KeyValues in idUsers
                                         from value in KeyValues.Value
                                                where value.FollowerID == followerId
                                         select KeyValues.Key;

                return (selectedId.Count() == 0 ? -1 : selectedId.ElementAt(0));
            }
        }
        
        /// <summary>
        /// Add a follower ID to a list, indicating that at the next iteration user must to be processed
        /// </summary>
        /// <param name="followerId">a follower ID</param>
        public static void MarkNewFollower(String followerId)
        {
            lock (newUserLock)
            {
                NewUsers.Add(followerId);
            }
        }

        /// <summary>
        /// Return and remove from a list market followers
        /// </summary>
        /// <returns>a list of marked folloers</returns>
        public static List<String> ReturnMarkedFollowers()
        {
            List<String> retUsers = new List<string>();
            lock (newUserLock)
            {
                retUsers.AddRange(NewUsers);
                NewUsers.Clear();
            }
            return retUsers;
        }

        //TODO : MAke an URL variable
        //

       
        private static void CreateFromMarketFollowers(DateTime initialTime)
        {
            //Отримати список фоловерів яких потрібно зробити
            List<String> reqNewUsers = ReturnMarkedFollowers();
            //Якщо спсок фоловерів нульовий значить нікого не потрібщно створювати
            if (reqNewUsers.Count == 0) return;

            //Отримати з бази дані про фоловерів
            List<CompleteFollower> completeFollowers = remoteDbHandler.GetCompleteFollowers(reqNewUsers);
            //Якщо нічого не отримали або довжина спиоку ID фоловерів не співпадає з списком отриманих з бази фоловерів - значить помилка
            if (completeFollowers.Count == 0 || (completeFollowers.Count != reqNewUsers.Count))
            {
                String errorStr = "Error in Program.CreateUsers : the count of returned followers from database are not equal " +
                    "to count of required users";
                log.Error(errorStr);
                log.Debug("completeFollowers.Count=" + completeFollowers.Count + "; newUsersList.Count=" + reqNewUsers.Count);
                Program.WriteError(errorStr);
                throw new Exception(errorStr);
            }
            lock (usersLock)
            {
                List<TradingClass> users = null;
                HashSet<Int32> requiredSystems = new HashSet<Int32>();

                //1. Створення користувачів
                foreach (CompleteFollower follower in completeFollowers)
                {
                    try
                    {
                        //users = idUsers[follower.ListenedSystemId];
                    }
                    catch (KeyNotFoundException)
                    {
                        users = new List<TradingClass>();
                        //requiredSystems.Add(follower.ListenedSystemId);
                    }
                    
                //    TradingClass newFollower = new TradingClass(follower.UserId, follower.BrockerLogin, follower.BrockerPassword,
                //            "URL", follower.Email, dbHandler,/*Change*/Presets.SynchronizationMode.SynchronizePositions);
                //    newFollower.Login();
                //    users.Add(newFollower);
                   
                //    idUsers[follower.ListenedSystemId] = users;
                //    Console.WriteLine("Prepared and added user : {0}", follower.UserId);
                }

                //2. Ініціалізація даних провайдерів іна які підписані дані користувачі
                //2.1 Відфільтрувати ІД провайдерів які вже моніторяться
                requiredSystems.ExceptWith(systemContainer.GetListenedSystemIds());
                //2.2 Завантажити початкові дані провайдерів
                List<DBResult> resData = remoteDbHandler.GetSystemsLastPositions(requiredSystems.ToList<Int32>(), initialTime);
                //2.2 Внести початкові дані провайдерів у відповідний клас
                systemContainer.InitializeSystems(resData);
            }
        }

        private static void ProcessUsersSolveProblem(DateTime initialTime)
        {
            //Отримати список користувачів для синхронізації
            List<String> usersToSync = GetUsersMarkedToSync();

            foreach (String aUserID in usersToSync)
            {
                TradingClass someUser = GetFollowerById(aUserID);
                
                if (someUser == null)
                {
                    WriteError("Cannot find user by ID : " + aUserID);
                }
                else
                {
                    //Видалити користувача зі списку 
                    UnmarkUserToSync(aUserID);
                    try
                    {
                        //Отримати ордери - вирішення проблеми
                        List<Order> syncOrders = syncSolver.solve(aUserID, getListenedID(aUserID), initialTime);
                        //Видалити ордери з списку непідтверджених
                        someUser.ClearOrders();
                        //Розблокувати користувача (він ймовірно заблокований)
                        someUser.UnlockUser();
                        foreach (Order order in syncOrders)
                        {
                            order.PrevScanTime = initialTime;
                            someUser.SendOrder(order);
                        }
                    }
                    catch (Exception e)
                    {
                        Program.WriteError("Exception in  Program while resolving sync problem. Detail info : " + e.Message);
                        Program.log.Error("" + e.GetType().Name + " exception");
                        Program.log.Debug("Exception in  Program while resolving sync problem. Error message : " + e.Message + "\n" + e.StackTrace.ToString());
                    }
                }
            }
        }
    }
}
