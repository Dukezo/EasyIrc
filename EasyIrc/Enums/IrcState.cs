using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EasyIrc.Enums
{
    /// <summary>
    ///     The state of the <see cref="IrcClient"/> class.
    /// </summary>
    public enum IrcState
    {
        Offline,
        Disconnecting,
        Connecting,
        Connected,
    }
}
