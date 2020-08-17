using System.Collections.Generic;
using System.Diagnostics;
using EasyIrc.Enums;

namespace EasyIrc.Utils
{
    public static class TwitchUtils
    {
        public static Dictionary<string, int> ParseBadges(string badges)
        {
            Dictionary<string, int> parsedBadges = new Dictionary<string, int>();
            string[] splitted = badges.Split(',');

            for (int i = 0; i < splitted.Length; i++)
            {
                string[] badgeSegments = splitted[i].Split('/');
                if (badgeSegments.Length == 2)
                {
                    if (int.TryParse(badgeSegments[1], out int version))
                        parsedBadges.Add(badgeSegments[0], version);
                    else
                        Debug.WriteLine("badge version is not an integer.");
                }
                else
                    Debug.WriteLine("badge has an invalid format.");
            }

            return parsedBadges;
        }

        public static TwitchSubscriptionPlan ConvertSubscriptionPlan(string subscriptionPlan)
        {
            switch (subscriptionPlan)
            {
                case "Prime":
                    return TwitchSubscriptionPlan.Prime;
                case "1000":
                    return TwitchSubscriptionPlan.Tier1;
                case "2000":
                    return TwitchSubscriptionPlan.Tier2;
                case "3000":
                    return TwitchSubscriptionPlan.Tier3;
                default:
                    return TwitchSubscriptionPlan.Unknown;
            }
        }
    }
}
