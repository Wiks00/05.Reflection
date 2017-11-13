using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyIoC.Exceptions
{
    [Serializable]
    public class ImportConstructorException : Exception
    {
        public ImportConstructorException() : base()
        {
            
        }

        public ImportConstructorException(string message) : base(message)
        {

        }

        public ImportConstructorException(string message, Exception innerException) : base(message, innerException)
        {

        }
    }
}
