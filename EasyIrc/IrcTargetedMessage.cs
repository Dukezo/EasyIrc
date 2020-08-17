using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EasyIrc.Interfaces;

namespace EasyIrc
{
    /// <summary>
    ///     Represents a targeted irc message - either PRIVMSG or NOTICE.
    /// </summary>
    public class IrcTargetedMessage
    {
        /// <summary>
        ///     Represents the type of the irc message based on the command. PRIVMSG and NOTICE are supported.
        /// </summary>
        public enum MessageType
        {
            Unknown,
            Privmsg,
            Notice
        }

        /// <summary>
        ///     The type of the message.
        /// </summary>
        public MessageType Type { get; private set; }

        /// <summary>
        ///     The source of this message. Can either be an user or server.
        /// </summary>
        public IIrcMessageSource Source { get; private set; }

        /// <summary>
        ///     The user or channel specified as the message's target.
        /// </summary>
        public IIrcMessageTarget Target { get; private set; }

        /// <summary>
        ///     True if the target is a channel.
        /// </summary>
        public bool IsChannelMessage { get; private set; }

        /// <summary>
        ///     The message.
        /// </summary>
        public string Message { get; private set; }

        /// <summary>
        ///     The tags of the message.
        /// </summary>
        public Dictionary<string, string> Tags { get; private set; }

        /// <summary>
        ///     The client responsible for this message.
        /// </summary>
        public IrcClient Client { get; private set; }

        /// <summary>
        ///     Initializes a new instance of <see cref="IrcTargetedMessage"/>.
        /// </summary>
        /// <param name="message"></param>
        public IrcTargetedMessage(IrcClient client, IrcMessage message)
        {
            if(message.Prefix == null)
                throw new ArgumentException("Message contains no prefix.");

            if(message.Parameters.Length < 2)
                throw new ArgumentException("The message needs to have 2 parameters to specify the target and the message.");

            if (message.Command == "PRIVMSG")
                Type = MessageType.Privmsg;
            else if (message.Command == "NOTICE")
                Type = MessageType.Notice;
            else
                Type = MessageType.Unknown;

            IrcHostMask hostMask = IrcHostMask.Parse(message.Prefix);

            if (hostMask != null)
            {
                if(client is TwitchIrcClient)
                    Source = new TwitchUser(client, hostMask, message.Tags);
                else
                    Source = new IrcUser(client, hostMask);
            }
            else
                Source = new IrcServer(message.Prefix);

            if (message.Parameters[0].StartsWith("#"))
            {
                string channelName = message.Parameters[0].Substring(1);

                if (client.Channels.ContainsKey(channelName))
                    Target = client.Channels[channelName];
                else
                    Debug.WriteLine("[IrcTargetedMessage] Could not find the targeted channel in the client's channels list.");

                IsChannelMessage = true;
            }
            else
            {
                Target = client.User;
            }

            Tags = message.Tags;
            Message = message.Parameters[1];
            Client = client;
        }
    }
}
