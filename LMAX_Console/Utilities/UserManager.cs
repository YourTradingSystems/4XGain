using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Sender;
using Follower;
using System.Collections;
using Utilites;
using Database.DataTypes;

namespace Database.Utilities
{
    class UserManager
    {
        /*
         * Ідея цього класу проста : організувати управління користувачами оптимально з точки зору часових затрат та ресурсів памяті
         * --------------------------
         * Додавання нового користувача.
         *      Оскільки при додаванні користувача ще не відомо на які системи він підписаний, то обєкт API користувача не створюється
         *      Замість цього дані, необхідні для створення API обєкта зберігаються в окремий вектор, тим самим можна вважати що він
         *      очікує коли його буде створено
         * 
         * Підписування фолловера до системи
         *      При цьому спочатку перевіряється чи такий користувач вже не підписаний на цю систему. Якщо так - генерується виключна ситуація
         *      Якщо все ОК, перевіряється чи такий обєкт користувача вже не існує. Якщо не існує, тоді ймовірно дані для створення
         *      API користувача вже містяться у спеціальному векторі. В такому випадку їх потрібно звідти вийняти, виджалити, і на 
         *      основі цих даних створити обєки API користувача. Якщо обєкт користувача і там не знайдено - значить ймовірно сталась якась помилка в логіці програми - 
         *      буде згенерована виключна ситуація
         *      
         * Відписування фоловера
         *      В цьому випадку просто видаляється запис про відповідність ІД системи і класу фоловера. Якщо при цьому виявиться що волловер більше ні наяку систему не 
         *      підписаний, значить його обєкт потрібно знищити, а його метадані зберегти в спец. таблицю, щоб при підписці його можна
         *      було ще раз легко створити
         *      
         * Видалення фолловера
         *      При видаленні фолловера спочатку необхідно перевірити чи він ще не підписаний на якісь системи. Якщо так - кинути відповідне повідомлення
         *      Відписати його а потім видалити сам єбєкт
         */
        private List<FollowerPropereties> _notInitializedFollowers;
        private Object _syncFollowerList;

        public const String url = "Put URL to this filed";
        
        public UserManager() {
            _notInitializedFollowers = new List<FollowerPropereties>();
            _syncFollowerList = new Object();
        }

        /// <summary>
        /// Add a follower. The follower object not created immediatly, but datas neaded to created follower stored in a special list
        /// When a "SubscribeFollower" method called a follower will be created and subscribe
        /// </summary>
        /// <param name="followerID">a follower id</param>
        /// <param name="userName"> follower broker login</param>
        /// <param name="userPassword">follower broker password</param>
        /// <param name="email">follower email</param>
        /// <param name="syncMode">follower sync mode</param>
        public void AddNewFollower(String followerID, String userName, String userPassword, String email,
                    Presets.SynchronizationMode syncMode = Presets.SynchronizationMode.SynchronizePositions)
        {
            //Перевірити чи фоловер ще не додано у список очікування
            IEnumerable<FollowerPropereties> selectedFollower = from sf in _notInitializedFollowers
                                                                where sf.FollowerID == followerID
                                                                select sf;
            if (selectedFollower.Count() != 0) throw new Exception("Follower allredy exist in a wait list");
            //Перевірити чи фоловер ще не існує
            if (Program.GetFollowerById(followerID) != null) throw new Exception("Follower allredy exist");

            FollowerPropereties followerProps = new FollowerPropereties
            {
                FollowerID = followerID,
                FollowerBrokerLogin = userName,
                FollowerBrokerPasswd = userPassword,
                FollowerEmail = email,
                FollowerSynchMode = syncMode
            };
            lock (_syncFollowerList)
            {
                _notInitializedFollowers.Add(followerProps);
            }
        }
    
        /// <summary>
        /// Subscrube a follower to a specified system
        /// </summary>
        /// <param name="followerID">followers ID</param>
        /// <param name="systemID">systems ID</param>
        public void Subscribe(String followerID, Int32 systemID)
        {
            FollowerPropereties followerProps;
            //Перевірити чи існує такий користувач
            TradingClass follower = Program.GetFollowerById(followerID);
            if (follower == null)
            {
                lock (_syncFollowerList)
                {
                    IEnumerable<FollowerPropereties> followerToStart = from f in _notInitializedFollowers
                                                                       where f.FollowerID == followerID
                                                                       select f;
                    if (followerToStart.Count() == 0)
                        throw new Exception("Cannot find follower by id '" + followerID + "' at list of followers");
                    if (followerToStart.Count() != 0)
                        throw new Exception("There are more than 1 follower (" + followerToStart.Count());
                    //Створити фоловера
                    followerProps = followerToStart.ElementAt(0);
                    _notInitializedFollowers.Remove(followerProps);
                }
                follower = new TradingClass(followerProps.FollowerID, followerProps.FollowerBrokerLogin, followerProps.FollowerBrokerLogin,
                    url, followerProps.FollowerEmail, Program.dbHandler, followerProps.FollowerSynchMode);
            } 
            //Перевірити чи користувач вже не підписаний на цю систему
            List<TradingClass> listOfFollowers;
            lock (Program.usersLock)
            {
                try
                {
                    listOfFollowers = Program.idUsers[systemID];
                }
                catch (KeyNotFoundException)
                {
                    listOfFollowers = new List<TradingClass>();
                }
                IEnumerable<TradingClass> finded = from searchedFollower in listOfFollowers
                                     where searchedFollower.FollowerID == followerID
                                     select searchedFollower;

                if (finded.Count() != 0) throw new Exception("follower '"+followerID+"' allready subscribe to system "+systemID);
                // Підписати фоловера
                listOfFollowers.Add(follower);
            }
        }

        /// <summary>
        /// Try to unsybscribe follower from systemID
        /// </summary>
        /// <param name="followerId">follower's ID</param>
        /// <param name="systemId">system's ID</param>
        public void UnsybscribeFollower(String followerId, Int32 systemId)
        {
            List<TradingClass> listOfUsers = null;

            lock (Program.usersLock)
            {
                try 
                { 
                    listOfUsers = Program.idUsers[systemId];
                    IEnumerable<TradingClass> searchedFollower = from f in listOfUsers
                                                                 where f.FollowerID == followerId
                                                                 select f;
                    if (searchedFollower.Count() == 0) 
                        throw new Exception("follower " + followerId + " not sybscribe to " + systemId);
                   
                    listOfUsers.Remove(searchedFollower.ElementAt(0));
                    
                    if (listOfUsers.Count == 0)
                    {
                        Program.idUsers.Remove(systemId);
                    }
                    //Якщо фоловер вже не залишився у списку тоді треба його перевести в список очікуючих фоловеріві
                    if (Program.GetFollowerById(followerId) == null)
                    {
                        lock(_syncFollowerList)
                        {
                            _notInitializedFollowers.Add(
                                searchedFollower.ElementAt(0).ToFollowerPropereties()
                                );
                        }
                    }
                    
                }
                catch (KeyNotFoundException) { throw new Exception("System ID "+systemId+" not found"); }
            }
        }

        public void RemoveFollower(String followerId)
        {
            bool finded = false;
            lock(Program.usersLock)
            {
                IEnumerable<Int32> keys = Program.idUsers.Keys;
                Int32 key = 0;
                List<TradingClass> tradersList = null;

                for (int i=0; i<keys.Count(); ++i)
                {
                    key = keys.ElementAt(i);
                    tradersList = Program.idUsers[key];

                    IEnumerable<TradingClass> searcherFollower = from   foll in tradersList
                                                                 where  foll.FollowerID == followerId
                                                                 select foll;
                    if (searcherFollower.Count() != 0)
                    {
                        tradersList.Remove(searcherFollower.ElementAt(0));
                        finded = true;
                        if (tradersList.Count == 0)
                        {
                            Program.idUsers.Remove(key);
                            --i;
                        }
                    }
                }
            }

            if (!finded)
            {
                lock(_syncFollowerList)
                {
                    IEnumerable<FollowerPropereties> searchedFollower = from   f in _notInitializedFollowers
                                                                        where  f.FollowerID == followerId
                                                                        select f;
                    if (searchedFollower.Count() == 0)
                        throw new Exception("Cannot delete follower '"+followerId+"' decouse follower not found");
                    _notInitializedFollowers.Remove(searchedFollower.ElementAt(0));
                }
            }
            
        }

        //this is the user manager
    }
}
