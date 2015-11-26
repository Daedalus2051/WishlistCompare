using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HtmlAgilityPack;
/*
 * http://htmlagilitypack.codeplex.com/
 * For example, here is how you would fix all hrefs in an HTML file: 
 HtmlDocument doc = new HtmlDocument();
 doc.Load("file.htm");
 foreach(HtmlNode link in doc.DocumentElement.SelectNodes("//a[@href"])
 {
    HtmlAttribute att = link["href"];
    att.Value = FixLink(att);
 }
 doc.Save("file.htm");
 * 
 */
namespace WishlistCompare
{
    class HtmlParser
    {
        public string GetWishlistGameNames()
        {
            string output = "";
            HtmlDocument hdoc = new HtmlDocument();
            hdoc.Load(@"E:\Test\Steam.html");
            foreach (HtmlNode link in hdoc.DocumentNode.SelectNodes("//h4[@class]"))
            {
                // If we're returned actual data... then add it to the output string
                if( !(String.IsNullOrEmpty(link.InnerHtml.ToString())) )
                    output = output + link.InnerHtml.ToString() + Environment.NewLine;
            }
            return output;
        }
        public string GetWishlistGameRank()
        {
            string output = "";
            HtmlDocument hdoc = new HtmlDocument();
            hdoc.Load(@"E:\Test\Steam.html");
            foreach (HtmlNode link in hdoc.DocumentNode.SelectNodes("//input[@class='wishlist_rank']"))
            {
                output = output + link.GetAttributeValue("value", "default").ToString() + Environment.NewLine;
            }
            return output;
        }
        public string CombineGameAndRank()
        {
            string output = ""; string gameName=""; string gameRank="";
            HtmlDocument hdoc = new HtmlDocument();
            hdoc.Load(@"E:\Test\Steam.html");
            foreach (HtmlNode wishlistRowItem in hdoc.DocumentNode.SelectNodes("//div[@class='wishlistRowItem']"))
            {
                HtmlDocument innerDoc = new HtmlDocument();
                innerDoc.LoadHtml(wishlistRowItem.InnerHtml);
                gameName = innerDoc.DocumentNode.SelectSingleNode("//h4[@class]").InnerHtml.ToString();
                if (String.IsNullOrEmpty(gameName))
                    gameName = "[Error] Game name not found";
                gameRank = innerDoc.DocumentNode.SelectSingleNode("//input[@class='wishlist_rank']").GetAttributeValue("value", "def_null").ToString();
                output = output + gameName + " - Rank: " + gameRank + Environment.NewLine;
            }
            return output;
        }
    }
}
