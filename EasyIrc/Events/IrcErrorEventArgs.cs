using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EasyIrc.Events
{
    /// <summary>
    ///     Provides data for the <see cref="IrcClient.Error"/> event.
    /// </summary>
    public class IrcErrorEventArgs : EventArgs
    {
        /// <summary>
        ///     The thrown exception.
        /// </summary>
        public Exception Error { get; set; }

        /// <summary>
        ///     Initializes a new instance of the <see cref="IrcErrorEventArgs"/> class.
        /// </summary>
        /// <param name="error">The thrown exception.</param>
        internal IrcErrorEventArgs(Exception error)
        {
            Error = error;
        }
    }
}
