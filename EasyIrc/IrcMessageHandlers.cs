using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EasyIrc.Events;
using EasyIrc.Exceptions;

namespace EasyIrc
{
    internal static class IrcMessageHandlers
    {
        [MessageHandler("PRIVMSG")]
        [MessageHandler("NOTICE")]
        internal static Task HandleTargetedMessage(IrcClient client, IrcMessage message)
        {
            IrcTargetedMessage targetedMessage = new IrcTargetedMessage(client, message);

            if (targetedMessage.IsChannelMessage)
            {
                var channel = (IrcChannel)targetedMessage.Target;
                var e = new IrcChannelMessageEventArgs(targetedMessage);

                if (targetedMessage.Type == IrcTargetedMessage.MessageType.Privmsg)
                {
                    client.OnChannelMessageReceived(e);
                    channel.OnMessageReceived(e);
                }
                else if (targetedMessage.Type == IrcTargetedMessage.MessageType.Notice)
                {
                    client.OnChannelNoticeReceived(e);
                    channel.OnNoticeReceived(e);
                }
            }
            else
            {
                var e = new IrcUserMessageEventArgs(targetedMessage);

                if (targetedMessage.Type == IrcTargetedMessage.MessageType.Privmsg)
                    client.OnUserMessageReceived(e);
                else if (targetedMessage.Type == IrcTargetedMessage.MessageType.Notice)
                    client.OnUserNoticeReceived(e);
            }

            return Task.CompletedTask;
        }

        [MessageHandler("JOIN")]
        internal static Task HandleJoinMessage(IrcClient client, IrcMessage message)
        {
            if (message.Parameters.Length > 0)
            {
                string channelName = message.Parameters[0].Substring(1);
                var hostMask = IrcHostMask.Parse(message.Prefix);

                if(hostMask != null)
                {
                    IrcChannel channel;
                    var user = new IrcUser(client, hostMask);

                    if (!client.Channels.ContainsKey(channelName))
                    {
                        // The client joined the channel.

                        if (client is TwitchIrcClient)
                        {
                            channel = new TwitchChannel(client, channelName);
                            client.Channels.Add(channelName, channel);

                            // Return to not fire the ChannelJoined event and let the ROOMSTATE message handler handle it.
                            return Task.CompletedTask;
                        }
                            
                        channel = new IrcChannel(client, channelName);
                        client.Channels.Add(channelName, channel);
                        client.User.Update(hostMask);
                    }
                    else
                    {
                        channel = client.Channels[channelName];
                    }

                    var e = new IrcUserEventArgs(channel, user);
                    client.OnChannelJoined(e);
                }
                else
                    throw new IrcProtocolException("Invalid source.");
            }
            else
                throw new IrcProtocolException("1 parameter is required to determine the channel.");

            return Task.CompletedTask;
        }

        [MessageHandler("PART")]
        internal static Task HandlePartMessage(IrcClient client, IrcMessage message)
        {
            if (message.Parameters.Length > 0)
            {
                string channelName = message.Parameters[0].Substring(1);
                
                if (client.Channels.ContainsKey(channelName))
                {
                    var hostMask = IrcHostMask.Parse(message.Prefix);

                    if (hostMask != null)
                    {
                        var user = new IrcUser(client, IrcHostMask.Parse(message.Prefix));
                        var channel = client.Channels[channelName];

                        //The local user left the channel.
                        if (Equals(client.User, user))
                        {
                            client.Channels.Remove(channelName);
                            client.OnChannelLeft(new IrcChannelEventArgs(client, channel));
                        }
                        else
                        {
                            var e = new IrcUserEventArgs(channel, user);
                            channel.OnUserLeft(e);
                        }
                    }
                    else
                        throw new IrcProtocolException("Invalid source.");
                }
                else
                    throw new IrcProtocolException("PART message received from a channel the client is not present in.");
            }
            else
                throw new IrcProtocolException("1 parameter is required to determine the channel.");

            return Task.CompletedTask;
        }

        [MessageHandler("PING")]
        internal static async Task HandlePingMessageAsync(IrcClient client, IrcMessage message)
        {
            if (message.Parameters.Length > 0)
            {
                string pongMessage = message.Parameters[0];
                await client.SendRawMessageAsync(string.Format("PONG :{0}", message.Parameters[0]));
                Debug.WriteLine("Responded to PING message.");
            }
            else
                throw new Exception("1 parameter is required to respond to the PING message.");
        }

        [MessageHandler("433")] // Nickname already in use.
        internal static async Task Handle433Message(IrcClient client, IrcMessage message)
        {
            if(message.Parameters.Length >= 3)
            {
                string nickname = message.Parameters[1];

                if (String.Equals(nickname, client.User.NickName, StringComparison.OrdinalIgnoreCase))
                {
                    client.User.NickName = client.User + "_";
                    await client.SendNickAsync(client.User.NickName);
                    Debug.WriteLine("Changed nick after 433 error");
                }     
            } 
        }

        [MessageHandler("NICK")]
        internal static Task HandleNickMessage(IrcClient client, IrcMessage message)
        {
            if (message.Parameters.Length == 1)
            {
                string newNickname = message.Parameters[0];
                var hostMask = IrcHostMask.Parse(message.Prefix);
                var user = new IrcUser(client, hostMask);

                if (user == client.User)
                    client.User.NickName = newNickname;
            }

            return Task.CompletedTask;
        }

        [MessageHandler("MODE")]
        internal static Task HandleModeMessage(IrcClient client, IrcMessage message)
        {
            if (message.Parameters.Length < 2)
                throw new IrcProtocolException("Received MODE message with an invalid amount of parameters.");

            string target = message.Parameters[0];
            string flags = message.Parameters[1];

            if(target.StartsWith("#"))
            {
                string channelName = target.Substring(1);

                // Handle all mode messages related to channels.
                if(client.Channels.ContainsKey(channelName))
                {
                    IrcChannel channel = client.Channels[channelName];
                    bool grant = flags[0] == '+';
                    char[] modes = flags.Substring(1).ToCharArray();

                    foreach(char mode in modes)
                    {
                        // Operator status
                        if(mode == 'o')
                        {
                            if (message.Parameters.Length == 3)
                            {
                                string nickName = message.Parameters[2];
                                IrcUser user = new IrcUser(client, nickName);

                                // Check if the local user has been granted operator status and update the channel's IsOperator property if necessary.
                                if(user.IsClientUser())
                                    channel.IsOperator = true;

                                client.OnUserOperatorStatusUpdated(new IrcOperatorEventArgs(channel, user, grant));
                            }
                            else
                                throw new IrcProtocolException("Received granting operator MODE message with an invalid amount of parameters.");
                        }
                    }
                }
                else
                    throw new IrcProtocolException(string.Format("Received mode message from a channel the client has not joined. (Channel: {0}", channelName));
            }

            return Task.CompletedTask;
        }
    }
}
