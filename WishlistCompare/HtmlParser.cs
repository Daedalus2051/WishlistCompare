using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
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
        #region TestMethods
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
        public string GetGamesAndRanksFromURL(string url)
        {
            WebClient client = new WebClient();
            string htmlCode = client.DownloadString(url);
            string output = ""; 
            string gameName = ""; string gameRank = "";
            string originalPrice = ""; string currentPrice="";
            GamePriceData priceData;

            HtmlDocument hdoc = new HtmlDocument();
            hdoc.LoadHtml(htmlCode);

            // Grab the wishlistRowItem tags, this will indicate each game that was added to the wishlist
            foreach (HtmlNode wishlistRowItem in hdoc.DocumentNode.SelectNodes("//div[@class='wishlistRowItem']"))
            {
                // Note that the web request version of the HTML seems to be different than if you d/l through chrome
                HtmlDocument innerRow = new HtmlDocument();
                innerRow.LoadHtml(wishlistRowItem.InnerHtml);
                
                gameName = innerRow.DocumentNode.SelectSingleNode("//h4[@class]").InnerHtml.ToString();
                if (String.IsNullOrEmpty(gameName))
                    gameName = "[Error] Game name not found";
                gameRank = innerRow.DocumentNode.SelectSingleNode("//div[@class='wishlist_rank_ro']").InnerHtml.ToString();

                priceData = FindPrice(innerRow.DocumentNode.SelectSingleNode("//div[@class='gameListPriceData']").InnerHtml.ToString());
                originalPrice = priceData.OriginalPrice;
                currentPrice = priceData.DiscountPrice;

                output = output + gameName + " - Rank: " + gameRank + " Original price: " + originalPrice + " Sale Price: " + currentPrice + Environment.NewLine;
            }

            return output;
        }
        public GamePriceData FindPrice(string htmlPrice)
        {
            bool onSale = false;
            HtmlDocument priceData = new HtmlDocument();
            priceData.LoadHtml(htmlPrice);
            GamePriceData prices = new GamePriceData { DiscountPrice="", OriginalPrice="", DiscountPercent="" };
            /* 
	            <div class=\"discount_pct\">-80%</div>
	            <div class=\"discount_prices\">
		        <div class=\"discount_original_price\">$14.99</div>
		        <div class=\"discount_final_price\">$2.99</div>
             */

            try
            {// try pulling out the standard price
                prices.OriginalPrice = priceData.DocumentNode.SelectSingleNode("//div[@class='price']").InnerHtml.ToString().Trim();
            }
            catch (NullReferenceException nex)  { onSale=true; }
            try
            {
                prices.DiscountPercent = priceData.DocumentNode.SelectSingleNode("//div[@class='discount_pct']").InnerHtml.ToString().Trim();
                prices.DiscountPrice = priceData.DocumentNode.SelectSingleNode("//div[@class='discount_final_price']").InnerHtml.ToString().Trim();
                if (onSale)
                    prices.OriginalPrice = priceData.DocumentNode.SelectSingleNode("//div[@class='discount_original_price']").InnerHtml.ToString().Trim();
            }
            catch (NullReferenceException nex) { }

            return prices;
        }
        public struct GamePriceData
        {
            public string OriginalPrice;
            public string DiscountPrice;
            public string DiscountPercent;
        }
        #endregion
    }
}
