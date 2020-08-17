using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EasyIrc.Interfaces;

namespace EasyIrc
{
    public class IrcServer : IIrcMessageSource
    {
        public string SourceName
        {
            get
            {
                return HostName;
            }
        }

        public string HostName { get; private set; }

        public IrcServer(string hostName)
        {
            HostName = hostName;
        }
    }
}
