using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace AccountManager.Models
{
    public static class SteamTwoFactor
    {
        private static bool _aligned = false;
        private static int _timeDifference = 0;

        public static string GenerateSteamGuardCodeForTime(long time, string secret)
        {

            if (string.IsNullOrEmpty(secret))
            {
                return "";
            }

            byte[] sharedSecretArray = Convert.FromBase64String(secret);
            byte[] timeArray = new byte[8];

            time /= 30L;
            for (int i = 8; i > 0; i--)
            {
                timeArray[i - 1] = (byte)time;
                time >>= 8;
            }

            HMACSHA1 hmacGenerator = new HMACSHA1();
            hmacGenerator.Key = sharedSecretArray;
            byte[] hashedData = hmacGenerator.ComputeHash(timeArray);
            byte[] codeArray = new byte[5];
            try
            {
                byte b = (byte)(hashedData[19] & 0xF);
                int codePoint = (hashedData[b] & 0x7F) << 24 | (hashedData[b + 1] & 0xFF) << 16 | (hashedData[b + 2] & 0xFF) << 8 | (hashedData[b + 3] & 0xFF);

                for (int i = 0; i < 5; ++i)
                {
                    codeArray[i] = steamGuardCodeTranslations[codePoint % steamGuardCodeTranslations.Length];
                    codePoint /= steamGuardCodeTranslations.Length;
                }
            }
            catch (Exception e)
            {
                return null; //Change later, catch-alls are bad!
            }
            return Encoding.UTF8.GetString(codeArray);
        }

        private static byte[] steamGuardCodeTranslations = new byte[] { 50, 51, 52, 53, 54, 55, 56, 57, 66, 67, 68, 70, 71, 72, 74, 75, 77, 78, 80, 81, 82, 84, 86, 87, 88, 89 };

        public static long GetSteamTime()
        {
            if (!_aligned)
            {
                AlignTime();
            }
            return GetSystemUnixTime() + _timeDifference;
        }

        public static void AlignTime()
        {
            long currentTime = GetSystemUnixTime();
            using (WebClient client = new WebClient())
            {
                try
                {
                    string response = client.UploadString(APIEndpoints.TWO_FACTOR_TIME_QUERY, "steamid=0");
                    TimeQuery query = JsonConvert.DeserializeObject<TimeQuery>(response);
                    _timeDifference = (int)(query.Response.ServerTime - currentTime);
                    _aligned = true;
                }
                catch (WebException)
                {
                    return;
                }
            }
        }

        public static long GetSystemUnixTime()
        {
            return (long)(DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1))).TotalSeconds;
        }

    }

    public static class APIEndpoints
    {
        public const string STEAMAPI_BASE = "https://api.steampowered.com";
        public const string COMMUNITY_BASE = "https://steamcommunity.com";
        public const string MOBILEAUTH_BASE = STEAMAPI_BASE + "/IMobileAuthService/%s/v0001";
        public static string MOBILEAUTH_GETWGTOKEN = MOBILEAUTH_BASE.Replace("%s", "GetWGToken");
        public const string TWO_FACTOR_BASE = STEAMAPI_BASE + "/ITwoFactorService/%s/v0001";
        public static string TWO_FACTOR_TIME_QUERY = TWO_FACTOR_BASE.Replace("%s", "QueryTime");
    }

    internal class TimeQuery
    {
        [JsonProperty("response")]
        internal TimeQueryResponse Response { get; set; }

        internal class TimeQueryResponse
        {
            [JsonProperty("server_time")]
            public long ServerTime { get; set; }
        }
    }

}
