using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Configuration;
using System.Drawing;
using System.Net.Http;

using CloudflareSolverRe;
using HtmlAgilityPack;

namespace HLTV_CLI
{
    class Etc
    {
        //TODO consider removing ansi shit
        public const string UNDERLINE = "\x1B[4m";
        public const string RESET = "\x1B[0m";

        public static Color T = Color.FromArgb(250, 178, 0);
        public static Color T_DEAD = Color.FromArgb(156, 126, 51);
        public static Color CT = Color.FromArgb(0, 145, 212);
        public static Color CT_DEAD = Color.FromArgb(53, 106, 130);
        public static Color NORM = Color.FromArgb(204, 204, 204);
        public static Color BOMB = Color.FromArgb(232, 67, 56);
        public static Color LOST = Color.FromArgb(195, 4, 4);
        public static Color WON = Color.FromArgb(9, 179, 0);

        public static Color DEFAULT_BG = Color.FromArgb(12, 12, 12);
        public static Color DEFAULT_FG = Color.FromArgb(204, 204, 204);

        public const string DEFAULT_URI = "https://hltv.org";

        public static string ConvertToStringTime(int hour, int min)
        {
            string h = hour.ToString(), m = min.ToString();
            if (h.Length == 1)
                h = "0" + h;
            if (m.Length == 1)
                m = "0" + m;
            return h + m;
        }

        //should consider getting rid of this. doesn't work on all terminals
        //public static string MakeUnderline(string s) => $"{UNDERLINE}{s}{RESET}";
        public static string MakeUnderline(string s) => s;

        public static HtmlDocument GetDocFromURL(string url, bool retry = false)
        {
            if (retry) Console.WriteLine("Retrying now...");
            if (url == "")
            {
                return new HtmlDocument();
            }
            Uri target = new Uri(url);
            ClearanceHandler cHandler = new ClearanceHandler
            {
                MaxTries = 3,
                ClearanceDelay = 3000
            };

            HttpClientHandler handler = new HttpClientHandler();
            // handler.SslProtocols = System.Security.Authentication.SslProtocols.Tls13;
            HttpClient httpClient = new HttpClient(cHandler);

            string source = "";
            try
            {
                source = httpClient.GetStringAsync(target).Result;
            }
            catch (AggregateException ex)
            {
                Console.WriteLine("Exception " + ex.Message + " Error with getting " + target + ". Retrying in 2 seconds...");
                System.Threading.Thread.Sleep(2000);
                GetDocFromURL(url, retry: true);
            }

            HtmlDocument doc = new HtmlDocument();
            doc.LoadHtml(source);
            return doc;
        }

        public static void InitMatchFilters()
        {
            NameValueCollection filterConfig = ConfigurationManager.AppSettings;
            //will default to these if they haven't been initialised
            if (filterConfig.Get("stars") == null) filterConfig.Set("stars", "0");
            if (filterConfig.Get("lan") == null) filterConfig.Set("lan", "false");
            if (filterConfig.Get("live") == null) filterConfig.Set("live", "false");
            if (filterConfig.Get("teams") == null) filterConfig.Set("teams", "");
        }
        public static void SetMatchFilters()
        {
            NameValueCollection filterConfig = ConfigurationManager.AppSettings;
            //stars
            Console.Write("\nStars minimum per match (0-5): ");
            string stars = Console.ReadLine().Trim();
            if (int.TryParse(stars, out _))
            {
                int intStars = int.Parse(stars);
                if (0 <= intStars && intStars <= 5)
                {
                    filterConfig.Set("stars", stars);
                }
                else
                    Console.WriteLine("Invalid");
            }
            else
                Console.WriteLine("Invalid");

            //islan
            Console.Write("\nMatch is LAN (Y/n): ");
            string isLAN = Console.ReadLine().Trim().ToLower();
            if (isLAN == "" || isLAN == "y" || isLAN == "yes")
            {
                filterConfig.Set("lan", "true");
            }
            else if (isLAN == "n" || isLAN == "no")
            {
                filterConfig.Set("lan", "false");
            }
            else
                Console.WriteLine("Invalid");

            //islive
            Console.Write("\nMatch is live (Y/n): ");
            string isLive = Console.ReadLine().Trim().ToLower();
            if (isLive == "" || isLive == "y" || isLive == "yes")
            {
                filterConfig.Set("live", "true");
            }
            else if (isLive == "n" || isLive == "no")
            {
                filterConfig.Set("live", "false");
            }
            else
                Console.WriteLine("Invalid");

            Console.Write("\n");
        }

        //returns true if match meets up to filter's standard, false if doesn't
        //only has to meet 1 of the requirements
        public static bool CheckMatchFilter(HtmlNode match)
        {
            NameValueCollection filterConfig = ConfigurationManager.AppSettings;
            //retrieves match filter attributes
            int matchStars = int.Parse(match.GetAttributeValue("stars", "0"));
            string matchIsLAN = match.GetAttributeValue("lan", "false");
            string matchIsLive = match.GetAttributeValue("filteraslive", "false");
            string[] matchTeamIDs = {
                match.GetAttributeValue("team1", "-1"), match.GetAttributeValue("team2", "-1")
            };

            //retrieves filter thresholds from config
            int filterStars = int.Parse(filterConfig.Get("stars"));
            string filterIsLAN = filterConfig.Get("lan"),
                   filterIsLive = filterConfig.Get("live");
            string[] teams = filterConfig.Get("teams").Split("//");

            return (matchStars >= filterStars) ||
                   (matchIsLAN == filterIsLAN) ||
                   (matchIsLive == filterIsLive);
        }

        //what an abomination of a name
        public static List<List<string>> GenerateEmptyListWithHeaders(int count = 6)
        {
            List<List<string>> ret = new List<List<string>>();
            for (int i = 0; i < count; i++)
            {
                ret.Add(new List<string>());
            }
            return ret;
        }
    }
}