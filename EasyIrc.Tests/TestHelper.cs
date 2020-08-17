using EasyIrc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EasyIrc.Tests
{
    public static class TestHelper
    {
        private const string TEST_HOST = "irc.freenode.net";
        private const int TEST_PORT = 6667;
        private const string TEST_NICKNAME = "imnotabot";
        private const string TEST_USERNAME = "imnotabot";

        public static IrcClient GetTestIrcClient()
        {
            return new IrcClient(TEST_HOST, TEST_PORT, new IrcRegistrationInfo(TEST_NICKNAME, TEST_USERNAME));
        }

        public static IrcClient GetTwitchTestIrcClient()
        {
            var twitchUsername = Environment.GetEnvironmentVariable("test_twitch_username");
            var twitchToken = Environment.GetEnvironmentVariable("test_twitch_token");

            return new TwitchIrcClient(twitchUsername, twitchToken);
        }
    }
}
