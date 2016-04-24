using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using System.Net;

namespace AccountManager.Models
{
    public static class SteamAPiCalls
    {
        private const string APIKey = "BABAF79A4A54E2C23DD7A0A849AD7E25";

        internal static List<PlayerSummaryData> LoadPlayerSummaries(IList<Account> accounts)
        {
            StringBuilder ids = new StringBuilder();
            foreach (Account a in accounts)
            {
                ids.Append(a.ProfileID).Append(",");
            }

            string result = null;
            using (WebClient client = new WebClient())
            {
                client.Encoding = Encoding.UTF8;
                try
                {
                    result = client.DownloadString("http://api.steampowered.com/ISteamUser/GetPlayerSummaries/v0002/?key=" + APIKey + "&steamids=" + ids.ToString());
                }
                catch
                {
                    return null;
                }
            }

            try
            {
                var players = (JArray)((JObject.Parse(result)["response"])["players"]);

                var output = new List<PlayerSummaryData>(accounts.Count);

                foreach (JObject player in players)
                {
                    var summaryData = new PlayerSummaryData();
                    summaryData.ProfileURL = (string)player["profileurl"];
                    summaryData.AvatarURL = (string)player["avatar"];
                    summaryData.Nickname = (string)player["personaname"];
                    summaryData.ProfileID = (string)player["steamid"];
                    summaryData.Online = (int)player["personastate"] != 0;
                    summaryData.IsIngame = false;

                    try
                    {
                        summaryData.IsIngame = (int)player["gameid"] != 0;
                    }
                    catch
                    {

                    }

                    output.Add(summaryData);
                }

                return output;
            }
            catch
            {
                return null;
            }
        }
        internal static List<PlayerBanData> LoadBans(IList<Account> accounts)
        {

            StringBuilder ids = new StringBuilder();
            foreach(Account a in accounts)
            {
                ids.Append(a.ProfileID).Append(",");               
            }

            string result = null;
            using(WebClient client = new WebClient())
            {
                try
                {
                    result = client.DownloadString("http://api.steampowered.com/ISteamUser/GetPlayerBans/v1/?key=" + APIKey + "&steamids=" + ids.ToString());
                }
                catch
                {
                    return null;
                }
            }
                     
            try
            {
                var players = (JArray)JObject.Parse(result)["players"];

                var output = new List<PlayerBanData>(accounts.Count);

                foreach (JObject player in players)
                {
                    var banData = new PlayerBanData();
                    banData.ProfileID = (string)player["SteamId"];
                    banData.EconomyBan = (string)player["EconomyBan"];
                    banData.VACBan = (bool)player["VACBanned"];
                    banData.CommunityBan = (bool)player["CommunityBanned"];

                    output.Add(banData);
                }

                return output;
            }
            catch
            {
                return null;
            }
        }
    }
}
