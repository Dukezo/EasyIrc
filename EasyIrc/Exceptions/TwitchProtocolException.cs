using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EasyIrc.Exceptions
{
    class TwitchProtocolException : Exception
    {
        public TwitchProtocolException(string message) : base(message) { }
    }
}
