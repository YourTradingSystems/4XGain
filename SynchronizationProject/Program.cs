using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ClientHandler2.DataType;

namespace SynchronizationProject
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Start program");
            // Create a list of positions
            List<Position> positionList = new List<Position>();

            positionList.Add(new Position
            {
                direction = 0,
                qty = 2.0,
                symbol = "EURUSD"
            });

            positionList.Add(new Position
            {
                direction = 1,
                qty = 5.0,
                symbol = "USDJPY"
            });

            SyncClass syncClass = new SyncClass(true);
            syncClass.SetPositions(positionList);
            
            Console.WriteLine("Positions in SyncClass : \n{0}",syncClass.PositionsToString());
            
            String readedSymbol;
            int readedDirection;
            double readedVolume;

            while (true)
            {
                try
                {
                    Console.Write("Symbol="); readedSymbol = Console.ReadLine();
                    Console.Write("Volume="); readedVolume = Convert.ToDouble(Console.ReadLine());
                    Console.Write("Direction="); readedDirection = Convert.ToInt32(Console.ReadLine());

                    syncClass.FilterOrder(new Order(readedSymbol,readedVolume, readedDirection));
                    Console.WriteLine("New posiiotns after add order : \n{0}",syncClass.PositionsToString());
                }
                catch (Exception e)
                {
                    Console.WriteLine("Error while reading and creating position. Try again");
                    Console.WriteLine("Detailed info : {0}",e.Message);
                }
             }
        }
    }
}
