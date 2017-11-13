using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using MyIoC;

namespace ConsoleTest
{
    class Program
    {
        static void Main(string[] args)
        {
            var container = new Container();
            container.RegistrateAssembly(Assembly.GetAssembly(typeof(A)));

            Stopwatch watch = new Stopwatch();

            foreach (Type type in new[] { typeof(CustomerDAL), typeof(A) })
            {
                watch.Start();

                container.RegistrateTypes(type);

                watch.Stop();

                Console.WriteLine($"Init {type}: " + watch.ElapsedTicks);

                watch.Reset();
            }

            //foreach (Type type in new[] { typeof(ContractBLL), typeof(A), typeof(A2), typeof(A5), typeof(A) })
            //{
            //    watch.Start();

            //    var a = container.CreateInstance<A>();

            //    watch.Stop();

            //    Console.WriteLine($"Create {type}: " + watch.ElapsedTicks);

            //    watch.Reset();
            //}

            List<long> ticks = new List<long>();

            var first = container.CreateInstance<A>();

            foreach (Type type in Enumerable.Repeat(typeof(A), 100))
            {
                watch.Start();

                var a = container.CreateInstance<A>();

                watch.Stop();

                //Console.WriteLine($"Create {type} by IoC: " + watch.ElapsedTicks);

                ticks.Add(watch.ElapsedTicks);

                watch.Reset();
            }

            Console.WriteLine("Average by IoC: " + ticks.Average());

            ticks.Clear();

            foreach (Type type in Enumerable.Repeat(typeof(A), 100))
            {
                watch.Start();

                var a = new A(new A1 { a6 = new A6() }, new A2 { a3 = new A3(new A5(new A7(new A6()), new A6(), new A8(new CustomerDAL()))), a4 = new A4 { a6 = new A6() } });

                watch.Stop();

                //Console.WriteLine($"Create {type} by hand: " + watch.ElapsedTicks);

                ticks.Add(watch.ElapsedTicks);

                watch.Reset();
            }

            Console.WriteLine("Average by hand: " + ticks.Average());

            Console.ReadKey();
        }
    }
}
