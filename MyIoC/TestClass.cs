using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyIoC
{
    public class A
    {
        [ImportConstructor]
        public A(A1 a1, A2 a2)
        {
        }
    }

    [Export]
    public class A1
    {
        [Import]
        public A6 a6 { get; set; }
    }

    [Export]
    public class A2
    {
        [Import]
        public A3 a3;

        [Import]
        public A4 a4 { get; set; }
    }

    [Export]
    public class A3
    {
        [ImportConstructor]
        public A3(A5 a5)
        {
        }
    }

    [Export]
    public class A4
    {
        [Import]
        public A6 a6;
    }

    [Export]
    public class A5
    {
        [ImportConstructor]
        public A5(A7 a7, A6 a6, A8 a8)
        {
        }
    }

    [Export]
    public class A6
    {
    }

    [Export]
    public class A7
    {
        [ImportConstructor]
        public A7(A6 a6)
        {
        }
    }

    [Export]
    public class A8
    {
        [ImportConstructor]
        public A8(ICustomerDAL cd)
        {
        }
    }

    public interface ICustomerDAL
    {
    }


    [Export(typeof(ICustomerDAL))]
    public class CustomerDAL : ICustomerDAL
    {
    }

}
