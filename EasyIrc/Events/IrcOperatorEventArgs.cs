using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using EasyIrc.Events;

namespace EasyIrc
{
    public class IrcOperatorEventArgs : IrcUserEventArgs
    {
        public bool StatusGranted { get; private set; }

        public IrcOperatorEventArgs(IrcChannel channel, IrcUser user, bool statusGranted) : base(channel, user)
        {
            StatusGranted = statusGranted;
        }
    }
}
