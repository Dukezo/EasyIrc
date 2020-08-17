using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EasyIrc.Events.Twitch
{
    /// <summary>
    ///     Provides data for the <see cref="TwitchIrcClient.UserBanned"/> and <see cref="TwitchIrcClient.UserTimeouted"/> events.
    /// </summary>
    public class TwitchClearchatEventArgs : EventArgs
    {
        /// <summary>
        ///     The duration of the ban/timeout.
        /// </summary>
        public int Duration { get; private set; }

        /// <summary>
        ///     The channel the user got timeouted/banned from.
        /// </summary>
        public IrcChannel Channel { get; private set; }

        /// <summary>
        ///     The user who got timeouted/banned.
        /// </summary>
        public IrcUser User { get; private set; }

        /// <summary>
        ///     Initializes a new instance of the <see cref="IrcRawMessageEventArgs"/> class.
        /// </summary>
        internal TwitchClearchatEventArgs(IrcChannel channel, IrcUser user, int duration)
        {
            Channel = channel;
            User = user;
            Duration = duration;
        }
    }
}
