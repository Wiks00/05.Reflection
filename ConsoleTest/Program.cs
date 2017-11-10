using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using IoCSample;
using MyIoC;

namespace ConsoleTest
{
    class Program
    {
        static void Main(string[] args)
        {
            var container = new Container();
            container.RegistrateAssembly(Assembly.GetExecutingAssembly());

            container.RegistrateTypes(typeof(object));
            container.RegistrateTypes(typeof(CustomerDAL));
            container.RegistrateTypes(typeof(A1));
            container.RegistrateTypes(typeof(A2));

            Console.ReadKey();
        }
    }
}
