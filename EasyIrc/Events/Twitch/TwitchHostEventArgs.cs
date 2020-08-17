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
    public class TwitchHostEventArgs : EventArgs
    {
        /// <summary>
        ///     The channel who is hosting.
        /// </summary>
        public TwitchChannel Source { get; private set; }

        /// <summary>
        ///     The channel name of the hosted channel.
        ///     Only set during host mode. Null when channel stops host mode.
        /// </summary>
        public string TargetName { get; private set; }

        /// <summary>
        ///     Number of viewers watching the host. This is optional and not always set.
        /// </summary>
        public int Viewers { get; private set; }

        /// <summary>
        ///     Initializes a new instance of the <see cref="TwitchHostEventArgs"/> class.
        /// </summary>
        internal TwitchHostEventArgs(TwitchChannel source, string target, int viewers)
        {
            Source = source;
            TargetName = target;
            Viewers = viewers;
        }
    }
}
