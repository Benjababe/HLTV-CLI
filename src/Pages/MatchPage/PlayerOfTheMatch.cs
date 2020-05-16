using System;
using System.Web;

using Console = Colorful.Console;
using HtmlAgilityPack;
using Newtonsoft.Json.Linq;

namespace HLTV_CLI.src {
    public static class PlayerOfTheMatch {
        public static void Show(HtmlNode docNode) {
            HtmlNode potm = docNode.SelectSingleNode("//div[@class=\"highlighted-player\"]");
            string flag = potm.SelectSingleNode(".//img[@class=\"flag\"]").GetAttributeValue("title", "no-flag");
            string name = potm.SelectSingleNode(".//span[@class=\"gtSmartphone-only\"]").InnerText;
            Console.WriteLine("\nPlayer of the match: \n" + name);
            //                                              mind the space
            string str = potm.SelectSingleNode(".//div[@class=\"graph \"]").GetAttributeValue("data-fusionchart-config", "{}");
            str = HttpUtility.HtmlDecode(str);
            JObject json = JObject.Parse(str);
            JObject dataSource = (JObject) json.GetValue("dataSource");
            JArray data = (JArray) dataSource.GetValue("data");

            foreach (JObject stat in data) {
                string toolText = (string) stat.GetValue("tooltext");
                if (toolText == null)   
                    toolText = stat.GetValue("label") + ": " + stat.GetValue("value");
                float value = (float) stat.GetValue("value");
                string perc = Convert.ToInt32((value - 1) * 100) + "%";
                Console.WriteLine(toolText + " (" + perc + " better than the average)\n");
            }
        }
    }
}