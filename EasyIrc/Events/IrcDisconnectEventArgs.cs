using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EasyIrc.Events
{

    /// <summary>
    ///     Prvides data for the <see cref="IrcClient.Disconnected"/> event.
    /// </summary>
    public class IrcDisconnectEventArgs : EventArgs
    {
        /// <summary>
        ///     True if the connection was closed by the server - otherwise false.
        /// </summary>
        public bool ConnectionClosedByServer { get; private set; }

        /// <summary>
        ///     Initializes a new instance of the <see cref="IrcDisconnectEventArgs"/> class.
        /// </summary>
        /// <param name="connectionClosedByServer"></param>
        public IrcDisconnectEventArgs(bool connectionClosedByServer)
        {
            ConnectionClosedByServer = connectionClosedByServer;
        }
    }
}
