using System;
using System.Threading.Tasks;
using EasyIrc;
using EasyIrc.Enums;

namespace BasicIrcBotSample
{
    class BasicIrcBotSample
    {
        public static async Task Main()
        {
            var ircClient = new IrcClient("irc.freenode.net", 6667, new IrcRegistrationInfo("imnotabot"), new IrcBasicAntiFlood(500));

            ircClient.Connected += async (sender, e) =>
            {
                Console.WriteLine(string.Format("Connected to the IRC server. ({0})", ircClient.Host));
                await ircClient.JoinChannelAsync("channelname");
            };

            ircClient.ChannelJoined += async (sender, e) =>
            {
                // The client joined a channel.
                if (e.User.IsClientUser())
                    Console.WriteLine(string.Format("Joined channel: #{0}", e.Channel.Name));
                else
                    Console.WriteLine(string.Format("{0} joined #{1}", e.User.NickName, e.Channel.Name));
            };

            ircClient.ChannelMessageReceived += async (sender, e) =>
            {
                if (e.SourceType == IrcSourceType.User)
                {
                    IrcUser user = ((IrcUser)e.Source);

                    Console.WriteLine(string.Format("[{0}] {1}: {2}", e.Channel.Name, user.NickName, e.Message));

                    if (e.Message.ToLower() == "!whisperme")
                        await user.SendMessageAsync(string.Format("Hi {0}!", user.NickName));
                    else
                        await e.Channel.SendMessageAsync(string.Format("Hi {0}!", user.NickName));
                }
                else if (e.SourceType == IrcSourceType.Server)
                    Console.WriteLine(string.Format("[{0}] SERVER ({1}): {2}", e.Channel.Name, ((IrcServer)e.Source).HostName, e.Message));
            };

            ircClient.Disconnected += (sender, e) =>
            {
                Console.WriteLine("Disconnected from IRC.");
            };

            ircClient.Error += (sender, e) =>
            {
                Console.WriteLine(e.Error);
            };

            await ircClient.ConnectAsync();
            Console.ReadKey();
            await ircClient.DisconnectAsync();
        }
    }
}
