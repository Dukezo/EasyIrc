using System;
using System.Threading.Tasks;
using EasyIrc;
using EasyIrc.Enums;

namespace TwitchBotSample
{
    class TwitchBotSample
    {
        public static async Task Main()
        {
            var twitchIrcClient = new TwitchIrcClient("your_username", "your_oauth_token", TwitchIrcClient.RATE_LIMIT_NORMAL);

            twitchIrcClient.Connected += async (sender, e) =>
            {
                Console.WriteLine("Connected to the Twitch IRC server");
                await twitchIrcClient.JoinChannelAsync("dekuzio");
            };

            twitchIrcClient.ChannelJoined += async (sender, e) =>
            {
                var twitchChannel = (TwitchChannel)e.Channel;

                if (e.User.IsClientUser())
                {
                    if (twitchChannel.EmoteOnlyMode)
                        Console.WriteLine(string.Format("Joined channel {0} which is currently in emote-only mode.", twitchChannel.Name));
                    else if (twitchChannel.SubscribersOnlyMode)
                        Console.WriteLine(string.Format("Joined channel {0} which is currently in sub-only mode.", twitchChannel.Name));
                    else
                        Console.WriteLine(string.Format("Joined channel {0}.", twitchChannel.Name));
                }
                else
                    Console.WriteLine(string.Format("{0} joined {1} channel.", e.User.NickName, twitchChannel.Name));
            };

            twitchIrcClient.ChannelMessageReceived += async (sender, e) =>
            {
                if (e.SourceType == IrcSourceType.User)
                {
                    var twitchUser = ((TwitchUser)e.Source);

                    if (e.Message.ToLower() == "!sub")
                    {
                        if (twitchUser.IsSubscriber)
                            await e.Channel.SendMessageAsync(string.Format("Hi {0}, you are a subscriber!", twitchUser.DisplayName));
                        else
                            await e.Channel.SendMessageAsync(string.Format("Hi {0}, you are not a subscriber!", twitchUser.DisplayName));
                    }
                }
            };

            twitchIrcClient.UserSubscribed += (sender, e) =>
            {
                if (e.IsGift)
                {
                    if (e.IsAnonymousGift)
                        Console.WriteLine(string.Format("{0} received a gift from an anonymous user!", e.GiftRecipent.DisplayName));
                    else
                        Console.WriteLine(string.Format("{0} gifted a sub to {1}!", e.User.DisplayName, e.GiftRecipent.DisplayName));
                }
                else
                {
                    if (e.SubscriptionPlan == TwitchSubscriptionPlan.Prime)
                        Console.WriteLine(string.Format("{0} subscribed to {1} using Twitch Prime!", e.User.DisplayName, e.Channel.Name));
                }
            };

            twitchIrcClient.UserCheered += (sender, e) =>
            {
                Console.WriteLine($"[{e.Channel.Name}] {e.User.DisplayName} cheered {e.Bits} bits to the channel. Message: {e.Message}");
            };

            twitchIrcClient.Disconnected += (sender, e) =>
            {
                Console.WriteLine("Disconnected from Twitch IRC.");
            };

            twitchIrcClient.Error += (sender, e) =>
            {
                Console.WriteLine(e.Error);
            };

            await twitchIrcClient.ConnectAsync();
            Console.ReadKey();
            await twitchIrcClient.DisconnectAsync();
        }
    }
}
