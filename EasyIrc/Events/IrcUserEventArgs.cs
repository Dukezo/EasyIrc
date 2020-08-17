using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EasyIrc.Events
{
    /// <summary>
    ///     Provides data for the <see cref="IrcChannel.UserJoinedEvent"/>.
    /// </summary>
    public class IrcUserEventArgs : EventArgs
    {
        /// <summary>
        ///     A reference to the client instance.
        /// </summary>
        public IrcClient Client { get; private set; }

        /// <summary>
        ///     The channel the event got raised in.
        /// </summary>
        public IrcChannel Channel { get; private set; }

        /// <summary>
        ///     The user who joined the channel.
        /// </summary>
        public IrcUser User { get; private set; }

        /// <summary>
        ///     Initializes a new instance of the <see cref="IrcUserEventArgs"/> class.
        /// </summary>
        /// <param name="channel">The channel the event got raised in.</param>
        /// <param name="user">The user who joined the channel.</param>
        public IrcUserEventArgs(IrcChannel channel, IrcUser user)
        {
            Client = channel.Client;
            Channel = channel;
            User = user;
        }  
    }
}
