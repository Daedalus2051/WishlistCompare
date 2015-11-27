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
        public List<string> GetWishlistGameData(string url)
        {
            // Declaration
            List<string> rawData = new List<string>();
            WebClient client = new WebClient();
            string htmlCode = client.DownloadString(url); // get the raw HTML page returned from the URL call
            string gameName, gameRank, originalPrice = "$0.00", salePrice = "$0.00", salePercent = "-0", lowestRegPrice, lowestSalePrice;

            HtmlDocument hdoc = new HtmlDocument();
            hdoc.LoadHtml(htmlCode);

            // Grab the wishlistRowItem tags, this will indicate each game that is in the list
            foreach (HtmlNode wishlistRowItem in hdoc.DocumentNode.SelectNodes("//div[@class='wishlistRowItem']"))
            {
                // Note: the web request (thru WebClient) seems to be slightly different than if you d/l through a browser...not sure why
                HtmlDocument innerRow = new HtmlDocument();
                HtmlDocument priceData = new HtmlDocument();
                bool onSale = false;
                innerRow.LoadHtml(wishlistRowItem.InnerHtml);

                // Fill data
                // <h4 class="ellipsis">Big Pharma</h4>
                gameName = innerRow.DocumentNode.SelectSingleNode("//h4[@class]").InnerHtml.ToString();
                if (String.IsNullOrEmpty(gameName))
                    gameName = "[Error] No data for game name";
                // "//div[@class='wishlist_rank_ro']"
                gameRank = innerRow.DocumentNode.SelectSingleNode("//div[@class='wishlist_rank_ro']").InnerHtml.ToString();
                // Game price data
                priceData.LoadHtml( innerRow.DocumentNode.SelectSingleNode("//div[@class='gameListPriceData']").InnerHtml.ToString() );
                try
                {
                    originalPrice = priceData.DocumentNode.SelectSingleNode("//div[@class='price']").InnerHtml.ToString().Trim();
                    if (String.IsNullOrEmpty(originalPrice))
                        originalPrice = "$0.00";
                }
                catch (NullReferenceException nex) { onSale = true; }
                try
                {
                    salePrice = priceData.DocumentNode.SelectSingleNode("//div[@class='discount_final_price']").InnerHtml.ToString().Trim();
                    salePercent = priceData.DocumentNode.SelectSingleNode("//div[@class='discount_pct']").InnerHtml.ToString().Trim();

                    // Scrub the percent... because it gives me problems
                    decimal pct = Convert.ToDecimal(salePercent.Replace('%', ' ').Trim());
                    pct = pct / 100;
                    salePercent = pct.ToString();

                    if (onSale)
                        originalPrice = priceData.DocumentNode.SelectSingleNode("//div[@class='discount_original_price']").InnerHtml.ToString().Trim();
                }
                catch (NullReferenceException nex) 
                {
                    salePrice = "$0.00";
                    salePercent = "-0";
                }

                // Get lowest price data
                lowestRegPrice = "$0.00"; // dummy data for now
                lowestSalePrice = "$0.00";

                // Put the data into the collection
                rawData.Add( String.Format("{0}|{1}|{2}|{3}|{4}|{5}|{6}", gameName, gameRank, originalPrice, salePrice, salePercent, lowestRegPrice, lowestSalePrice) );
            }

            return rawData;
        }
    }
}
