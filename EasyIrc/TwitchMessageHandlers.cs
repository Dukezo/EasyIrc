using EasyIrc.Exceptions;
using System;
using System.Threading.Tasks;
using EasyIrc.Events.Twitch;
using EasyIrc.Utils;
using System.Collections.Generic;
using EasyIrc.Events;
using EasyIrc.Enums;

namespace EasyIrc
{
    internal static class TwitchMessageHandlers
    {
        [MessageHandler("HOSTTARGET")]
        internal static Task HandleHosttargetMessage(IrcClient client, IrcMessage message)
        {
            string channelName = message.Parameters[0].Substring(1);
            TwitchChannel channel = (TwitchChannel)client.Channels[channelName];
            string[] chunks = message.Parameters[1].Split(' ');

            if (chunks.Length == 2)
            {
                string targetChannelName = chunks[0];
                int viewers = chunks[1] == "-" ? 0 : Convert.ToInt32(chunks[1]);

                if (targetChannelName != "-")
                {
                    // Hosting started.
                    var e = new TwitchHostEventArgs(channel, targetChannelName, viewers);
                    ((TwitchIrcClient)client).OnHostModeStarted(e);
                }
                else
                {
                    // Hosting stopped
                    var e = new TwitchHostEventArgs(channel, null, viewers);
                    ((TwitchIrcClient)client).OnHostModeStopped(e);
                }
            }
            else
                throw new TwitchProtocolException("Invalid HOSTTARGET command syntax");

            return Task.CompletedTask;
        }

            [MessageHandler("PRIVMSG")]
        internal static Task HandlePrivmsgMessage(IrcClient client, IrcMessage message)
        {
            // Check if it's a cheer message and handle event, otherwise pass it to IrcClient.HandleTargetedMessage.
            if (message.Tags.ContainsKey("bits"))
            {
                IrcTargetedMessage targetedMessage = new IrcTargetedMessage(client, message);
                TwitchUser user = (TwitchUser)targetedMessage.Source;
                TwitchChannel channel = (TwitchChannel)targetedMessage.Target;
                int bits = Convert.ToInt32(message.Tags["bits"]);

                var e = new TwitchCheerEventArgs(user, channel, targetedMessage.Message, bits);
                ((TwitchIrcClient)client).OnUserCheered(e);
            }
            else
                IrcMessageHandlers.HandleTargetedMessage(client, message);

            return Task.CompletedTask;
        }

        [MessageHandler("USERNOTICE")]
        internal static Task HandleUsernoticeMessage(IrcClient client, IrcMessage message)
        {
            string channelName = message.Parameters[0].Substring(1);
            TwitchChannel channel = (TwitchChannel)client.Channels[channelName];

            if (message.Tags.ContainsKey("login"))
            {
                TwitchUser user = new TwitchUser(client, message.Tags["login"], message.Tags);

                if (message.Tags.ContainsKey("msg-id"))
                {
                    string noticeType = message.Tags["msg-id"];

                    if(noticeType == "sub" | noticeType == "resub")
                    {
                        int totalMonthsSubscribed = Convert.ToInt32(message.Tags["msg-param-cumulative-months"]);
                        TwitchSubscriptionPlan subscriptionPlan = TwitchUtils.ConvertSubscriptionPlan(message.Tags["msg-param-sub-plan"]);
                        int consecutiveMonthsSubscribed = 0;
                        string subMessage = null;

                        if (message.Parameters.Length == 2)
                            subMessage = message.Parameters[1];

                        if (message.Tags.ContainsKey("msg-param-should-share-streak") && message.Tags["msg-param-should-share-streak"] == "1")
                            consecutiveMonthsSubscribed = Convert.ToInt32(message.Tags["msg-param-streak-months"]);

                        var e = new TwitchSubscriptionEventArgs(user, channel, subscriptionPlan, subMessage, totalMonthsSubscribed, consecutiveMonthsSubscribed);
                        ((TwitchIrcClient)client).OnUserSubscribed(e);
                    }
                    else if(noticeType == "subgift" | noticeType == "anonsubgift")
                    {
                        int totalMonthsSubscribed = Convert.ToInt32(message.Tags["msg-param-months"]);
                        TwitchSubscriptionPlan subscriptionPlan = TwitchUtils.ConvertSubscriptionPlan(message.Tags["msg-param-sub-plan"]);
                        int recipientUserID = Convert.ToInt32(message.Tags["msg-param-recipient-id"]); ;
                        string recipientUserName = message.Tags["msg-param-recipient-user-name"];
                        string recipientDisplayName = message.Tags["msg-param-recipient-display-name"];
                        TwitchUser recipient = new TwitchUser(client, recipientUserName, recipientDisplayName, recipientUserID);
                        string subMessage = null;

                        if (message.Parameters.Length == 2)
                            subMessage = message.Parameters[1];

                        if (noticeType == "anonsubgift")
                        {
                            var e = new TwitchSubscriptionEventArgs(null, channel, subscriptionPlan, subMessage, totalMonthsSubscribed, true, recipient, true);
                            ((TwitchIrcClient)client).OnUserSubscribed(e);
                        }
                        else
                        {
                            var e = new TwitchSubscriptionEventArgs(user, channel, subscriptionPlan, subMessage, totalMonthsSubscribed, true, recipient, false);
                            ((TwitchIrcClient)client).OnUserSubscribed(e);
                        }
                    }
                }
                else
                    throw new TwitchProtocolException("USERNOTICE message does not have a msg-id tag.");
            }
            else
                throw new TwitchProtocolException("USERNOTICE message does not have a login tag.");

            return Task.CompletedTask;
        }

        [MessageHandler("ROOMSTATE")]
        internal static Task HandleRommstateMessage(IrcClient client, IrcMessage message)
        {
            string channelName = message.Parameters[0].Substring(1);
            TwitchChannel channel = (TwitchChannel) client.Channels[channelName];

            if (message.Tags.ContainsKey("emote-only") && int.TryParse(message.Tags["emote-only"], out int emoteOnlyMode))
                channel.EmoteOnlyMode = emoteOnlyMode == 1;

            if (message.Tags.ContainsKey("followers-only") && int.TryParse(message.Tags["followers-only"], out int followersOnlyModeMinutes))
            {
                channel.FollowersOnlyMode = followersOnlyModeMinutes != -1;
                channel.FollowersOnlyModeMinutes = followersOnlyModeMinutes;
            }
            if (message.Tags.ContainsKey("r9k") && int.TryParse(message.Tags["r9k"], out int r9kMode))
                channel.R9KMode = r9kMode == 1;

            if (message.Tags.ContainsKey("slow") && int.TryParse(message.Tags["slow"], out int slowModeSeconds))
                channel.SlowModeSeconds = slowModeSeconds;

            if (message.Tags.ContainsKey("subs-only") && int.TryParse(message.Tags["subs-only"], out int subscribersOnlyMode))
                channel.SubscribersOnlyMode = subscribersOnlyMode == 1;

            if (!channel.ChannelJoinedEventFired)
            {
                // This is the first ROOMSTATE message for this channel received.
                // Fire the ChannelJoined event with a fully initialized TwitchChannel instance.
                var e = new IrcUserEventArgs(channel, client.User);
                client.OnChannelJoined(e);
                channel.ChannelJoinedEventFired = true;
            }

            return Task.CompletedTask;
        }

        [MessageHandler("USERSTATE")]
        internal static Task HandleUserstateMessage(IrcClient client, IrcMessage message)
        {
            string channelName = message.Parameters[0].Substring(1);
            TwitchChannel channel = (TwitchChannel) client.Channels[channelName];

            if(message.Tags.ContainsKey("badges") && message.Tags["badges"].Length > 0)
            {
                Dictionary<string, int> badges = TwitchUtils.ParseBadges(message.Tags["badges"]);

                if (badges.ContainsKey("subscriber"))
                    channel.IsSubscriber = true;
            }

            if(message.Tags.ContainsKey("mod") && int.TryParse(message.Tags["mod"], out int isModerator))
                channel.IsModerator = isModerator == 1;

            return Task.CompletedTask;
        }

        [MessageHandler("CLEARCHAT")]
        internal static Task HandleClearchatMessage(IrcClient client, IrcMessage message)
        {
            int duration = 0;

            if(message.Tags.ContainsKey("ban-duration"))
                duration = Convert.ToInt32(message.Tags["ban-duration"]);

            if(message.Parameters.Length == 2)
            {
                string channelName = message.Parameters[0].Substring(1);
                IrcChannel channel = client.Channels[channelName];
                IrcUser user = new IrcUser(client, message.Parameters[1]);

                var e = new TwitchClearchatEventArgs(channel, user, duration);

                if (duration == 0)
                    ((TwitchIrcClient)client).OnUserBanned(e);
                else
                    ((TwitchIrcClient)client).OnUserTimeouted(e);
            }
            else
                throw new TwitchProtocolException("CLEARCHAT message has an invalid amount of parameters.");

            return Task.CompletedTask;
        }
    }
}
