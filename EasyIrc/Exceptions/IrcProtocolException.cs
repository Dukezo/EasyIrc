using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EasyIrc.Exceptions
{
    class IrcProtocolException : Exception
    {
        public IrcProtocolException(string message) : base(message) { }
    }
}
