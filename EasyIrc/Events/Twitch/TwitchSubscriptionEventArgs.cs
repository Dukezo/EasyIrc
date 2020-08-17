using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EasyIrc.Enums;

namespace EasyIrc.Events.Twitch
{
    public class TwitchSubscriptionEventArgs : EventArgs
    {
        /// <summary>
        ///     The user who subscribed or gifted a subscription.
        /// </summary>
        public TwitchUser User { get; private set; }

        /// <summary>
        ///     The channel the user subscribed to.
        /// </summary>
        public TwitchChannel Channel { get; private set; }

        /// <summary>
        ///     The subscription plan used.
        /// </summary>
        public TwitchSubscriptionPlan SubscriptionPlan { get; private set; }

        /// <summary>
        ///     The subscription message. Null if the user did not specify one.
        /// </summary>
        public string Message { get; private set; }

        /// <summary>
        ///     The total amount of months the user is subscribed to the channel. 
        /// </summary>
        public int TotalMonthsSubscribed { get; private set; }

        /// <summary>
        ///     True if the subscription is a gift.
        /// </summary>
        public bool IsGift { get; private set; }

        /// <summary>
        ///     True if the subscription is an anonymous gift.
        /// </summary>
        public bool IsAnonymousGift { get; private set; }

        /// <summary>
        ///     The recipient of the subscription.
        /// </summary>
        public TwitchUser GiftRecipent { get; private set; }

        /// <summary>
        ///     Amount of consecutive months the user has subscribed to the channel. Only set if the user wants to share the streak.
        /// </summary>
        public int ConsecutiveMonthsSubscribed { get; private set; }

        internal TwitchSubscriptionEventArgs(TwitchUser user, TwitchChannel channel, TwitchSubscriptionPlan subscriptionPlan, string message, int totalMonthsSubscribed)
        {
            User = user;
            Channel = channel;
            SubscriptionPlan = subscriptionPlan;
            Message = message;
            TotalMonthsSubscribed = totalMonthsSubscribed;
        }

        internal TwitchSubscriptionEventArgs(TwitchUser user, TwitchChannel channel, TwitchSubscriptionPlan subscriptionPlan, string message, int totalMonthsSubscribed, int consecutiveMonthsSubscribed) : this(user, channel, subscriptionPlan, message, totalMonthsSubscribed)
        {
            ConsecutiveMonthsSubscribed = consecutiveMonthsSubscribed;
        }

        internal TwitchSubscriptionEventArgs(TwitchUser user, TwitchChannel channel, TwitchSubscriptionPlan subscriptionPlan, string message, int totalMonthsSubscribed, bool isGift, TwitchUser recipient) : this(user, channel, subscriptionPlan, message, totalMonthsSubscribed)
        {
            IsGift = isGift;
            GiftRecipent = recipient;
        }

        internal TwitchSubscriptionEventArgs(TwitchUser user, TwitchChannel channel, TwitchSubscriptionPlan subscriptionPlan, string message, int totalMonthsSubscribed, bool isGift, TwitchUser recipient, bool isAnonymousGift) : this(user, channel, subscriptionPlan, message, totalMonthsSubscribed, isGift, recipient)
        {
            IsAnonymousGift = isAnonymousGift;
        }
    }
}
