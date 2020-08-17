using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EasyIrc.Events
{
    /// <summary>
    ///     Provides data for the <see cref="IrcClient.RawMessageReceived"/> event.
    /// </summary>
    public class IrcRawMessageEventArgs : EventArgs
    {
        /// <summary>
        ///     The raw message string.
        /// </summary>
        public string RawMessage { get; set; }

        /// <summary>
        ///     Initializes a new instance of the <see cref="IrcRawMessageEventArgs"/> class.
        /// </summary>
        /// <param name="rawMessage">The raw message string.</param>
        internal IrcRawMessageEventArgs(string rawMessage)
        {
            RawMessage = rawMessage;
        }
    }
}
