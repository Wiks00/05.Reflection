using System;
using System.Collections.Generic;
using System.Diagnostics;
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

            Stopwatch watch = new Stopwatch();

            watch.Start();

            container.RegistrateTypes(typeof(CustomerDAL));

            watch.Stop();

            Console.WriteLine("Init DAL: " + watch.ElapsedTicks);

            watch.Reset();

            watch.Start();

            container.RegistrateTypes(typeof(A));

            watch.Stop();

            Console.WriteLine("Init a: " + watch.ElapsedTicks);

            watch.Reset();

            watch.Start();

            var a = container.CreateInstance<A>();

            watch.Stop();

            Console.WriteLine("Create a: " + watch.ElapsedTicks);

            watch.Reset();

            watch.Start();

            var b = container.CreateInstance<A2>();

            watch.Stop();

            Console.WriteLine("Create a2: " + watch.ElapsedTicks);

            Console.ReadKey();
        }
    }
}
