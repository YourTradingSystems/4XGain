using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DataTypess;
using CS_server_addin;
using System.Data.SqlClient;
using Sender;

namespace Database.LocalDatabase
{
    class SynchronizationSolver
    {
        public List<Order> solve(String userID, Int64 systemID, DateTime endTime)
        {
            // Пролема полягатиме в тому, що коли при відновленні позицій обєкт клієнта виставлятиме позиції,
            // то після їх підтвредження він буде шукати наступні оредри в черзі. А за час додавання можливо добавиться в чергу якийсь ордер. Треба це 
            // якось обробити в плоть до того що ще в базі можуть бути  ордери які залишились незафіленими з давана

            //Другий момент полягає в тому що коли будеться робитись щось з словником потрібно доступ до нього синхронізувати
            //Обєкт синхронізації віляєтьтся в Program.cs
            
            //Позиції провайдера мають запистуватись не з бази а з програмою збереженого списку позицій.
            double userMoney = 0.0;
            List<Position> userPositions;
            List<Position> providerPositions;
            List<Order>    syncOrders = new List<Order>();
            //1.Get user money
            //2.Get client last poss and verify it
            userPositions = Program.localDbHandler.GetUserLastPositions(userID, systemID);

            //3.Get provider last poss and verify it
            //providerPositions = Program.remoteDbHandler.GetProviderLastPosition(systemID, endTime); 
            Dictionary<Int32, TSystem> systems = Program.systemContainer.GetOldSystems();
            TSystem system = systems[(Int32)systemID];
            providerPositions = system.GetPositionsList();
            
            if (providerPositions == null || providerPositions.Count == 0)
            {
                Program.WriteError("Provider positions in SynchronizationSolver.solve() is null or empty");
                return syncOrders;
            }
            //4.Compare and get diff positions into an order
            Boolean finded = false;

            for (int i = 0; i < providerPositions.Count; ++i)
            {
                finded = false;
                for (int j = 0; j < userPositions.Count; ++j)
                {
                   if (providerPositions[i].symbol.Trim() == userPositions[j].symbol.Trim())
                   {
                       finded = true;
                       if (providerPositions[i].qty != userPositions[j].qty
                           || providerPositions[i].direction != userPositions[j].direction)
                       {
                           Position pos = providerPositions[i] - userPositions[j];
                           Order order = pos.ToOrder();
                           order.FromID = systemID;
                           syncOrders.Add(order);
                           userPositions.RemoveAt(j);
                           break;
                       }
                       else
                       {
                           userPositions.RemoveAt(j);
                       }
                    }
                }
                if (!finded)
                {
                    if (providerPositions[i].qty != 0.0)
                    {
                        Order order = providerPositions[i].ToOrder();
                        order.FromID = systemID;
                        syncOrders.Add(order);
                    }
                }
            }
            if (userPositions.Count != 0)
            {
                Program.WriteError("May be som error in SynchronizationSolver.solve() becouse after all iterations of loop user pos list not empty");
            }
            //5.Calc required money
            return syncOrders;
        }
    }
}
