using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EasyIrc.Events.Twitch
{
    /// <summary>
    ///     Provides data for the <see cref="TwitchCheerEventArgs.UserCheered"/> event.
    /// </summary>
    public class TwitchCheerEventArgs : EventArgs
    {
        /// <summary>
        ///     The twitch user who cheered.
        /// </summary>
        public TwitchUser User { get; private set; }

        /// <summary>
        ///     The channel the user cheered to.
        /// </summary>
        public TwitchChannel Channel { get; private set; }

        /// <summary>
        ///     The cheer message.
        /// </summary>
        public string Message { get; private set; }

        /// <summary>
        ///     Amount of bits sent.
        /// </summary>
        public int Bits { get; private set; }

        /// <summary>
        ///     Initializes a new instance of the <see cref="TwitchClearchatEventArgs"/> class.
        /// </summary>
        internal TwitchCheerEventArgs(TwitchUser user, TwitchChannel channel, string message, int bits)
        {
            Channel = channel;
            User = user;
            Message = message;
            Bits = bits;
        }
    }
}
