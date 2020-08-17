using EasyIrc.Enums;
using EasyIrc.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EasyIrc.Events
{
    public class IrcChannelMessageEventArgs : EventArgs
    {
        /// <summary>
        ///     A reference to the client instance
        /// </summary>
        public IrcClient Client { get; private set; }

        /// <summary>
        ///     The source of the message. Can either be an instance of <see cref="IrcUser"/> or <see cref="IrcServer"/>.
        /// </summary>
        public IIrcMessageSource Source { get; private set; }

        /// <summary>
        ///     The channel the message was sent to.
        /// </summary>
        public IrcChannel Channel { get; private set; }

        /// <summary>
        ///     The tags of the message.
        /// </summary>
        public Dictionary<string, string> Tags { get; private set; }

        /// <summary>
        ///     The message text.
        /// </summary>
        public string Message { get; private set; }

        /// <summary>
        ///     The type of the source. Used to determine if the message was sent from the server or an user. 
        ///     <see cref="IrcChannelMessageEventArgs.Source"/> can be safely cast to either <see cref="IrcUser"/> or <see cref="IrcServer"/> based on this value.
        /// </summary>
        public IrcSourceType SourceType { get; private set; }

        /// <summary>
        ///     Boolean indicating whether the message was received in the Twitch IRC using <see cref="TwitchIrcClient"/>.
        ///     If this is the case, the Channel object can safely be cast to <see cref="TwitchChannel"/>.
        /// </summary>
        public bool IsTwitchMessage { get; private set; }

        /// <summary>
        ///     The ID of the message. This is either the unique identifier for PRIVMSG messages used to delete the specific message.
        ///     Or an unique identifier for NOTICE messages received from the server determining the notice type.
        ///     Check <see href="https://dev.twitch.tv/docs/irc/msg-id">Twitch Docs</see> for more informations.
        ///     Only set if <see cref="IrcChannelMessageEventArgs.IsTwitchMessage"/> is true.
        /// </summary>
        public string TwitchMessageID { get; private set; }

        public IrcChannelMessageEventArgs(IrcTargetedMessage targetedMessage)
        {
            Source = targetedMessage.Source;
            Channel = (IrcChannel) targetedMessage.Target;
            Tags = targetedMessage.Tags;
            Message = targetedMessage.Message;
            Client = targetedMessage.Client;

            if (Source is IrcServer)
                SourceType = IrcSourceType.Server;
            else
                SourceType = IrcSourceType.User;

            IsTwitchMessage = targetedMessage.Client is TwitchIrcClient;

            if (IsTwitchMessage)
            {
                if (targetedMessage.Type == IrcTargetedMessage.MessageType.Privmsg)
                    TwitchMessageID = Tags["id"];
                else if(targetedMessage.Type == IrcTargetedMessage.MessageType.Notice)
                    TwitchMessageID = Tags["msg-id"];
            }
                
        }
    }
}
