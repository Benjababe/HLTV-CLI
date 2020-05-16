using System.Drawing;

using Console = Colorful.Console;
using HtmlAgilityPack;

namespace HLTV_CLI.src {
    public static class Highlights {
        public static void Show(HtmlNode docNode) {
            HtmlNode highlightsContainer = docNode.SelectSingleNode(".//div[@class=\"highlights\"]");
            //no highlights are available for finished match, returns
            if (highlightsContainer == null) {
                Console.WriteLine("There are no highlights available for this match.");
                return;
            }
                                                                //why did hltv give it a space at the end
            HtmlNodeCollection highlights = highlightsContainer.SelectNodes(".//div[@class=\"col \"]");
            for (int i = 0; i < highlights.Count; i++) {
                HtmlNode highlight = highlights[i].SelectSingleNode(".//div[contains(@class, \"highlight\")]");
                string title = highlight.InnerText;
                string url = highlight.GetAttributeValue("data-highlight-embed", "Error with url provided...");
                Console.Write("\n");
                Console.WriteLine((i + 1) + ". " + title + ": ", Color.LightGreen);
                Console.Write(url, Color.Magenta);
            }
        }
    }
}