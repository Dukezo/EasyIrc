using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EasyIrc
{
    public class TwitchChannel: IrcChannel
    {
        /// <summary>
        ///     True if the channel is in emote only mode.
        /// </summary>
        public bool EmoteOnlyMode { get; internal set; }

        /// <summary>
        ///     Boolean indicating whether the channel is in followers only mode.
        /// </summary>
        public bool FollowersOnlyMode { get; internal set; }

        /// <summary>
        ///     Minutes an user needs to be following the channel in order to use the chat. -1 if followers only mode is disabled.
        /// </summary>
        public int FollowersOnlyModeMinutes { get; internal set; }

        /// <summary>
        ///     True if the channel is in R9K mode.
        /// </summary>
        public bool R9KMode { get; internal set; }

        /// <summary>
        ///     True if the channel is in subscribers only mode.
        /// </summary>
        public bool SubscribersOnlyMode { get; internal set; }

        /// <summary>
        ///     Seconds users have to wait untill they can send a message again. 0 if slow mode is disabled
        /// </summary>
        public int SlowModeSeconds { get; internal set; }

        /// <summary>
        ///     The moderator status of the client user in this channel.
        /// </summary>
        public bool IsModerator { get; internal set; }

        /// <summary>
        ///     The subscriber status of the client user in this channel.
        /// </summary>
        public bool IsSubscriber { get; internal set; }

        /// <summary>
        ///     Shows if the ChannelJoined event for this channel has been triggered.
        ///     This is necessary so the event can be triggered after the first USERSTATE and ROOMSTATE messages
        ///     have been processed to have a fully initialized <see cref="TwitchChannel"/> instance.
        /// </summary>
        internal bool ChannelJoinedEventFired { get; set; }

        internal TwitchChannel(IrcClient client, string name): base(client, name)
        {

        }
    }
}
