using System;
using System.Linq;
using System.Reflection;
using EasyIrc.Events.Twitch;

namespace EasyIrc
{
    /// <summary>
    ///     This derived class estabilishes a connection to the Twitch IRC server. It handles all custom twitch IRC commands and has additional events.  
    /// </summary>
    public class TwitchIrcClient : IrcClient
    {
        // Limits per 30 seconds.
        public const int RATE_LIMIT_NORMAL = 20;
        public const int RATE_LIMIT_KNOWN = 50;
        public const int RATE_LIMIT_MOD = 100;
        public const int RATE_LIMIT_VERIFIED = 7500;

        private static string[] capabilities = new string[] { ":twitch.tv/membership", ":twitch.tv/tags", ":twitch.tv/commands" };

        public TwitchIrcClient(string username, string oAuthToken, int rateLimit) : base("irc.twitch.tv", 6667, new IrcRegistrationInfo(username, username, oAuthToken), new IrcRateLimiter(rateLimit, 30), capabilities)
        {
            InitTwitchMessageHandlers();
        }

        public TwitchIrcClient(string username, string oAuthToken) : this(username, oAuthToken, RATE_LIMIT_NORMAL) { }

        private void InitTwitchMessageHandlers()
        {
            var methods = typeof(TwitchMessageHandlers).GetTypeInfo().GetMethods(BindingFlags.Static | BindingFlags.NonPublic).Where(m => m.GetCustomAttributes(typeof(MessageHandlerAttribute), true).Length > 0).ToArray();
            foreach (MethodInfo methodInfo in methods)
            {
                var messageHandler = (MessageHandler)methodInfo.CreateDelegate(typeof(MessageHandler));
                var attributes = (MessageHandlerAttribute[])methodInfo.GetCustomAttributes(typeof(MessageHandlerAttribute));

                foreach (var attribute in attributes)
                {
                    if (!_messageHandlers.ContainsKey(attribute.CommandName))
                        _messageHandlers.Add(attribute.CommandName, messageHandler);
                    else
                        _messageHandlers[attribute.CommandName] = messageHandler;
                }
            }
        }

        #region Twitch events
        /// <summary>
        ///     Occurs when an user got timeouted from a channel.
        /// </summary>
        public event EventHandler<TwitchClearchatEventArgs> UserTimeouted;

        /// <summary>
        ///     Occurs when an user got timeouted from a channel.
        /// </summary>
        public event EventHandler<TwitchClearchatEventArgs> UserBanned;

        /// <summary>
        ///     Occurs when an user subscribes to a channel or gifts a subscription.
        /// </summary>
        public event EventHandler<TwitchSubscriptionEventArgs> UserSubscribed;

        /// <summary>
        ///     Occurs when an user subscribes to a channel or gifts a subscription.
        /// </summary>
        public event EventHandler<TwitchCheerEventArgs> UserCheered;

        /// <summary>
        ///     Occurs when a channel hosts another channel.
        /// </summary>
        public event EventHandler<TwitchHostEventArgs> HostModeStarted;

        /// <summary>
        ///     Occurs when a channel stops hosting another channel.
        /// </summary>
        public event EventHandler<TwitchHostEventArgs> HostModeStopped;
        #endregion

        #region Twitch callbacks
        /// <summary>
        ///     Raises the <see cref="UserTimeouted"/> event.
        /// </summary>
        /// <param name="e">The <see cref="EventArgs"/> instance holding the event data.</param>
        internal void OnUserTimeouted(TwitchClearchatEventArgs e)
        {
            UserTimeouted?.Invoke(this, e);
        }

        /// <summary>
        ///     Raises the <see cref="UserBanned"/> event.
        /// </summary>
        /// <param name="e">The <see cref="EventArgs"/> instance holding the event data.</param>
        internal void OnUserBanned(TwitchClearchatEventArgs e)
        {
            UserBanned?.Invoke(this, e);
        }

        /// <summary>
        ///     Raises the <see cref="UserSubscribed"/> event.
        /// </summary>
        /// <param name="e">The <see cref="EventArgs"/> instance holding the event data.</param>
        internal void OnUserSubscribed(TwitchSubscriptionEventArgs e)
        {
            UserSubscribed?.Invoke(this, e);
        }

        /// <summary>
        ///     Raises the <see cref="UserCheered"/> event.
        /// </summary>
        /// <param name="e">The <see cref="EventArgs"/> instance holding the event data.</param>
        internal void OnUserCheered(TwitchCheerEventArgs e)
        {
            UserCheered?.Invoke(this, e);
        }

        /// <summary>
        ///     Raises the <see cref="HostModeStarted"/> event.
        /// </summary>
        /// <param name="e">The <see cref="EventArgs"/> instance holding the event data.</param>
        internal void OnHostModeStarted(TwitchHostEventArgs e)
        {
            HostModeStarted?.Invoke(this, e);
        }

        /// <summary>
        ///     Raises the <see cref="HostModeStopped"/> event.
        /// </summary>
        /// <param name="e">The <see cref="EventArgs"/> instance holding the event data.</param>
        internal void OnHostModeStopped(TwitchHostEventArgs e)
        {
            HostModeStopped?.Invoke(this, e);
        }
        #endregion
    }
}
