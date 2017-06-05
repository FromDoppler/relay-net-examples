using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ApiClient
{
    class Program
    {
        static void Main()
        {
            MainAsync().GetAwaiter().GetResult(); // to avoid exceptions being wrapped into AggregateException
        }

        static async Task MainAsync()
        {
            Console.WriteLine("Press ENTER to continue . . .");
            Console.ReadLine();
        }
    }
}
