using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections;

namespace VerifyDictionery
{

    class Program
    {
        private static Dictionary<Int32, String> dictionary;

        static void Main(string[] args)
        {   
            Dictionary<Int32, List<String>> dict = new Dictionary<int, List<string>>();
            List<String> value;
            // fill dictionary
            for (int i = 0; i < 10; ++i)
            {
                value = new List<string>();

                for (int j = 0; j < 5; ++j)
                {
                    value.Add(Convert.ToString(j)+Convert.ToString(i));
                }

                dict[i] = value;
            }

            IEnumerable<Int32> r = from item in dict
                                   from subItem in item.Value
                                   where subItem == "20"
                                   select item.Key;
                                  
            
                                       // Generate linq query
            
            Console.WriteLine("Len = {0}",r.Count());
            foreach (Int32 s in r)
                Console.WriteLine(s);

            Console.ReadKey();


        }
    }
}
