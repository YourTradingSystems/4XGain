using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ConsoleTestProject
{
    class Program
    {
        static void Main(string[] args)
        {
            ExternalCleitnManagerClient cl = new ExternalCleitnManagerClient();
            Console.WriteLine(cl.GetVersion());
        }
    }
}
