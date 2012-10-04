using System;
using System.Collections.Generic;
using System.Linq;
using System.Data.SqlClient;

using System.IO;
using CS_server_addin.DBDataContainers;
using Database.PortListener;
using System.Collections.Concurrent;
using System.Threading;
using log4net;
using log4net.Config;
using log4net.Repository.Hierarchy;
using Database.LocalDatabase;
using Utilites;
using Database;
using DataTypess;
using ClientsManagerUtilities;
using Database.Types;
using Follower;

/**
 * TODO : 1. Add user ID to all trading class
 * */

namespace Sender
{
    partial class Program
    {
        //Тут зберігаються акаунти фоловерів
        public static Dictionary<Int32, List<TradingClass>> idUsers; 
        //Клас віддаленої бази даних
        public static DataBase remoteDbHandler;
        //Список тих фоловерів, яких потрібно синхронізувати
        public static List<String> syncUsers;
        //Lock objects for this variables
        public  static Object mustSyncLock;
        public  static Object usersLock;
        public  static Object errorWriteLock;
        private static Object randomLock;
        private static Object newUserLock;
        //Shared objects that do not nead lock's
        public static System.Text.Encoding ascii = System.Text.Encoding.ASCII;
        public static ILog log = LogManager.GetLogger("LMAX_data_sender");
        public static Dictionary<String, String> config = null;
        public static SynchronizationSolver syncSolver = new SynchronizationSolver();
        //Conastants
        private const long MUST_WAIT = 3000;
        private static Random random;
        //Private object members
        private static Boolean isQuit, isRefrash;
        private static ExternalClientManager externalClientManager;
        //???
        public static ErrorDbHandler  dbHandler;
        public  static LocalDbHandler localDbHandler;
        public static SystemContainer systemContainer;
        public static List<String>    NewUsers;
        

        /// <summary>
        /// Write error messages to console
        /// </summary>
        /// <param name="errorMess">A string object that describe an error</param>
        public static void WriteError(String errorMess)
        {
            lock (errorWriteLock)
            {
                ConsoleColor prevColor = Console.ForegroundColor;
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine(errorMess);
                Console.ForegroundColor = prevColor;
            }
        }

        public static Int32 GetRandom()
        {
            lock (randomLock)
            {
                return random.Next();
            }
        }

        public static void KeyReadHandler()
        {
            while (true)
            {
                String readedLine = Console.ReadLine();
                if (readedLine.Equals("quit"))
                {
                    isQuit = true;
                }
                if (readedLine.Equals("refresh"))
                {
                    isRefrash = true;
                }
            }
        }

        static void Main(string[] args)
        {
            XmlConfigurator.Configure();
            log.Info("Starting program");

            //Loading configuration from file
            try
            {
                config = new ConfLoader().loadConfFromFile("LMAX_sender_config.txt");
            }
            catch (Exception e)
            {
                log.Error(e.Message);
                log.Debug(e.StackTrace.ToString());
                log.Error("Exit program");
                return;
            }
            random = new Random((int)DateTime.Now.Ticks / 10000);
            isQuit = isRefrash = false;
            Thread t = new Thread(new ThreadStart(KeyReadHandler));
            t.Name = "Key handler thread";
            t.Start();

            syncUsers = new List<string>();
            NewUsers  = new List<string>();

            //Create lock objects
            usersLock      = new Object();
            errorWriteLock = new Object();
            mustSyncLock   = new Object();
            randomLock     = new Object();
            newUserLock    = new Object();

            //Create necessary datas
            BlockingCollection<Operations>  blokingQueue = new BlockingCollection<Operations>();

            //create sender db handler
            dbHandler = new ErrorDbHandler();

            //create object for manipulat with local DB
            localDbHandler = new LocalDbHandler();

            //Create error handler class for error solutions
            ErrorHandler errorHandler = new ErrorHandler("ytsnotify", "YTS_admin password");

            // Get datas from DB about users
            remoteDbHandler = new DataBase();
            
            try
            {
                lock (usersLock)
                {
                    idUsers = remoteDbHandler.getConnectedUsers(dbHandler, errorHandler);
                    /*
                    idUsers = new Dictionary<int, List<TradingClass>>();
                    List<TradingClass> list1 = new List<TradingClass>();
                    List<TradingClass> list2 = new List<TradingClass>();
                   
                    for (int i = 0; i < 2; ++i)
                    {
                        list.Add(new TradingClass("user_"+i,"userName_"+i,"Passwd_0"+i,"URL","userEmail",dbHandler));
                        list[i].Login();
                        Console.WriteLine("\tConnected users {0}", i);
                    }
                     
                    list1.Add(new TradingClass("user_1", "userName_1", "Passwd_1", "URL", "userEmail", dbHandler));
                    list2.Add(new TradingClass("user_2", "userName_2", "Passwd_2", "URL", "userEmail", dbHandler));
                    list1[0].Login();
                    list2[0].Login();
                    idUsers.Add(10001, list1);
                    idUsers.Add(10002, list2);*/
                }
            }
            catch (Exception e)
            {
                log.Error(e.Message);
                log.Debug(e.StackTrace.ToString());
                log.Error("Exit program");
                log.Info("Exit program becouse of error");
                return;
            }
            System.Console.WriteLine("Get {0} users from DB", idUsers.Values.Count);

            //Create class for user control directly after create users
            externalClientManager = new ExternalClientManager();

            //Create listen port
            Type serviceType = typeof(ExternalClientManager);
            Uri  serviceUri  = new Uri("http://localhost:9081/");
            System.ServiceModel.ServiceHost host = new System.ServiceModel.ServiceHost(serviceType, serviceUri);
            host.Open();

            // Start NETlistener to get new users from DB
            //NETlistener netListener = new NETlistener();

            DateTime time0 = new DateTime();
            DateTime oTime = new DateTime();

            //Create time reader from file
            DateFileWritter dateFile = new DateFileWritter();

            time0 = dateFile.ReadTime();

            //Create systemContainer to cach DB lastest data
            systemContainer = new SystemContainer(blokingQueue,time0);

            //Create sender class
            SenderClass sender = new SenderClass(blokingQueue);

            List<DBResult> res = new List<DBResult>();

            Console.WriteLine("Loaded time value : {0}", time0.ToString("yyyy-MM-dd HH:mm:ss.fff"));

            long start;
            long finish;
            long waitTime;

            while (true)
            {
                if (isQuit)
                {
                    Console.WriteLine("Buy....");
                    return;
                }
                //Створити нових фоловерів якщо вони є в списку
                //CreateFromMarketFollowers(time0);
                //Вирішити проблеми синхронізації
                ProcessUsersSolveProblem(time0);

                start = DateTime.Now.Ticks / 10000;
                
                res = (List<DBResult>)remoteDbHandler.GetDBResult(time0);

                if(res != null && res.Count > 0)
                {
                    System.Console.WriteLine("Get results : {0}", res.Count);
                    oTime = time0;
                    time0 = res[0].DbTime;
                    dateFile.WriteTime(time0);

                    foreach (DBResult item in res)
                    {
                        systemContainer.AddElement(item);
                    }
                    systemContainer.processSystems(idUsers, oTime);
                }

                finish = DateTime.Now.Ticks / 10000;
                waitTime = MUST_WAIT - (finish - start);

                if (waitTime > 0)
                {
                    Thread.Sleep((int)waitTime);
                }
                Console.WriteLine("wait in main loop");
            }
            //netListener.ShutDown();
            sender.ShutDown();
        }
  
    }
}
