using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ClientsInfo;
using Utilites;
using Sender;
using Follower;

namespace ClientsManagerUtilities
{
    public class ExternalClientManager : IExternalCleitnManager
    {
        public ExternalClientManager()
        {
            
        }

        public int GetProblematicListenersCount()
        {
            int problematiClistenersCount = 0;
            Dictionary<Int32, List<TradingClass>>.ValueCollection tradingClassList = Program.idUsers.Values;

            lock (Program.usersLock)
            {
                foreach (List<TradingClass> list in tradingClassList)
                {
                    foreach (TradingClass listItem in list)
                    {
                        problematiClistenersCount += listItem.Enabled ? 0 : 1;
                    }
                }
            }
            return problematiClistenersCount;
        }

        public List<ClientInfo> GetAllProblematicClients()
        //public String GetAllProblematicClients()
        {
            List<ClientInfo>   problematicClients = new List<ClientInfo>();
            List<TradingClass> usersList;
            Dictionary<Int32, List<TradingClass>>.KeyCollection keysList = Program.idUsers.Keys;

            lock (Program.usersLock)
            {
                foreach (Int32 key in keysList)
                {
                    usersList = Program.idUsers[key];

                    foreach (TradingClass listItem in usersList)
                    {
                        if (!listItem.Enabled)
                        {
                            problematicClients.Add(new ClientInfo
                            {
                                ClientBrocker      = listItem.FollowerBrocker,
                                ClientBrockerLogin = listItem.FollowerBrockerLogin,
                                ClientEmail        = listItem.FollowerEmail,
                                ClientId           = listItem.FollowerID,
                                ClientsListenedSystemId = key
                            });
                        }
                    }
                }
                return problematicClients;
            }
        }

        public void ContinueClientWork(String clientId, Int32 listenedSystemId, ClientContinueMode clientContinueMode)
        {
            //Поки що відбувається просте продовження роботи клієнта
            /*
            TradingClass findedUser = null;

            lock(Program.usersLock)
            {
                try
                {
                    foreach (TradingClass user in Program.idUsers[listenedSystemId])
                    {
                        if (user.UserID.Equals(clientId))
                        {
                            findedUser = user;
                            break;
                        }
                    }
                }
                catch (Exception e)
                {
                    Program.WriteError("Error in ExternalClientManager.ContinueClientWork(). Additional info : "+e.Message);
                }
            }
            if (findedUser != null)
            {
                Program.addSyncUser(findedUser.UserID);
                //findedUser.UnlockUser();
            }
             * */
            Program.MarkUserToSync(clientId);
        }

        public String GetVersion() { return "V 1.0"; }

        public void StopFollower(String followerId)
        {
            TradingClass follower = Program.GetFollowerById(followerId);
            try
            {
                follower.StopUser();
            }
            catch (Exception e)
            {
                Program.log.Error(e.Message);
                Program.log.Debug(e.StackTrace.ToString());
                Program.WriteError("Error in ExternalClientManager.StopUser. Additional info : " + e.Message);
            }
        }
    }
}
