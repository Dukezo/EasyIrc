using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EasyIrc.Events
{
    public class IrcChannelEventArgs : EventArgs
    {
        public IrcClient Client { get; private set; }
        public IrcChannel Channel { get; private set; }

        public IrcChannelEventArgs(IrcClient client, IrcChannel channel)
        {
            Client = client;
            Channel = channel;
        }
    }
}
