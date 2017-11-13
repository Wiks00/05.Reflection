using System;
using System.Collections.Generic;

namespace MyIoC
{
    public class BindType<T1> where T1 : class
    {
        private readonly Container container;

        protected BindType()
        {
        }

        internal BindType(Container container)
        {
            this.container = container;
        }

        public void ToType(Type type)
        {
            container.RegistrateTypes(type);
        }

        public void ToType<T>() where T : T1
        {
            container.RegistrateTypes(typeof(T));
        }
    }

    public class BindType<T1, T2> where T1 : class
                                  where T2 : class
    {
        private readonly Container container;

        protected BindType()
        {
        }

        internal BindType(Container container)
        {
            this.container = container;
        }

        public void ToType(Type type)
        {
            container.RegistrateTypes(type);         
        }

        public void ToType<T>() where T : T1, T2
        {
            container.RegistrateTypes(typeof(T));
        }
    }

    public class BindType<T1, T2, T3> where T1 : class
                                      where T2 : class
                                      where T3 : class
    {
        private readonly Container container;

        protected BindType()
        {
        }

        internal BindType(Container container)
        {
            this.container = container;
        }

        public void ToType(Type type)
        {
            container.RegistrateTypes(type);
        }

        public void ToType<T>() where T : T1, T2, T3
        {
            container.RegistrateTypes(typeof(T));
        }
    }

    public class BindType<T1, T2, T3, T4> where T1 : class
                                          where T2 : class
                                          where T3 : class
                                          where T4 : class
    {
        private readonly Container container;

        protected BindType()
        {
        }

        internal BindType(Container container)
        {
            this.container = container;
        }

        public void ToType(Type type)
        {
            container.RegistrateTypes(type);
        }

        public void ToType<T>() where T : T1, T2, T3, T4
        {
            container.RegistrateTypes(typeof(T));
        }
    }
}