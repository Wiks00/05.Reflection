using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyIoC.Exceptions
{
    [Serializable]
    class TypeRegistrationException : Exception
    {
        public TypeRegistrationException() : base()
        {

        }

        public TypeRegistrationException(string message) : base(message)
        {

        }

        public TypeRegistrationException(string message, Exception innerException) : base(message, innerException)
        {

        }
    }
}
