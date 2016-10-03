using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Dmr;

namespace DmrConsoleApplication
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Data om et køretøjer fra motorregisteret o_O");
            Console.WriteLine("Casper Korsgaard, 2015");
            Console.WriteLine();
            Console.WriteLine("Indtast et registreringsnummer(nummerplade) og tast enter:");

            var licencePlate = Console.ReadLine();
            var model = Scraper.LookupVehicle(licencePlate);
            var token = model.Token;
            var json = model.ToJson();

            Console.WriteLine(model.ToJson());

            Console.ReadLine();
        }
    }
}
