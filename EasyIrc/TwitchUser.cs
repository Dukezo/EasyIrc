using EasyIrc.Utils;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EasyIrc
{
    public class TwitchUser: IrcUser
    {
        /// <summary>
        ///     Amount of months the user is subscribed to the channel
        /// </summary>
        public int MonthsSubscribed { get; private set; }

        /// <summary>
        ///     Boolean for admin status.
        /// </summary>
        public bool IsAdmin { get; private set; }

        /// <summary>
        ///     Boolean showing if the user has donated bits to the channel.
        /// </summary>
        public bool IsBitsDonator { get; private set; }

        /// <summary>
        ///     Boolean showing if user is the broadcaster of the stream.
        /// </summary>
        public bool IsBroadcaster { get; private set; }

        /// <summary>
        ///     Boolean for global mod status
        /// </summary>
        public bool IsGlobalMod { get; private set; }

        /// <summary>
        ///     Boolean for mod status
        /// </summary>
        public bool IsMod { get; private set; }

        /// <summary>
        ///     Boolean for subscriber status
        /// </summary>
        public bool IsSubscriber { get; private set; }

        /// <summary>
        ///     Boolean for staff status
        /// </summary>
        public bool IsStaff { get; private set; }

        /// <summary>
        ///     Boolean for turbo status
        /// </summary>
        public bool IsTurbo { get; private set; }

        /// <summary>
        ///     The display name of the twitch user.
        /// </summary>
        public string DisplayName { get; private set; }

        /// <summary>
        ///     The user's twitch ID.
        /// </summary>
        public int ID { get; private set; }

        public TwitchUser(IrcClient client, string userName, string displayName, int userID): base(client, userName)
        {
            DisplayName = displayName;
            ID = userID;
        }

        public TwitchUser (IrcClient client, string userName, Dictionary<string, string> tags): base(client, userName)
        {
            Init(tags);
        }

        public TwitchUser(IrcClient client, IrcHostMask hostMask, Dictionary<string, string> tags): base(client, hostMask)
        {
            Init(tags);
        }

        private void Init(Dictionary<string, string> tags)
        {
            if (tags.ContainsKey("badges"))
            {
                if (tags["badges"].Length > 0)
                {
                    Dictionary<string, int> badges = TwitchUtils.ParseBadges(tags["badges"]);

                    if (badges.ContainsKey("admin"))
                        IsAdmin = true;

                    if (badges.ContainsKey("bits"))
                        IsBitsDonator = true;

                    if (badges.ContainsKey("broadcaster"))
                        IsBroadcaster = true;

                    if (badges.ContainsKey("global_mod"))
                        IsGlobalMod = true;

                    if (badges.ContainsKey("moderator"))
                        IsMod = true;

                    if (badges.ContainsKey("subscriber"))
                        IsSubscriber = true;

                    if (badges.ContainsKey("staff"))
                        IsStaff = true;

                    if (badges.ContainsKey("turbo"))
                        IsTurbo = true;
                }
            }
            else
                Debug.WriteLine("Missing badges tag in twitch irc message");

            if (tags.ContainsKey("badge-info"))
            {
                if (tags["badge-info"].Length > 0)
                {
                    var badgeInfo = TwitchUtils.ParseBadges(tags["badge-info"]);

                    if (badgeInfo.ContainsKey("subscriber"))
                        MonthsSubscribed = Convert.ToInt32(badgeInfo["subscriber"]);
                }
            }
            else
                Debug.WriteLine("Missing badge-info tag in twitch irc message");

            if (tags.ContainsKey("display-name"))
                DisplayName = tags["display-name"];
            else
                Debug.WriteLine("Missing display-name tag in twitch irc message");

            if (tags.ContainsKey("user-id"))
            {
                if (int.TryParse(tags["user-id"], out int userID))
                    ID = userID;
                else
                    Debug.WriteLine("user-id is not an integer.");
            }
            else
                Debug.WriteLine("Missing user-id tag in twitch irc message");
        }

    }
}
