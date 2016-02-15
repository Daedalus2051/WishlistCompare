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
        /// <summary>
        /// Returns raw data delimited by pipes for all games that appear on the user's wishlist
        /// </summary>
        /// <param name="url">Steam Wishlist URL</param>
        /// <returns> {0}gameName|{1}gameRank|{2}originalPrice|{3}salePrice|{4}salePercent|{5}lowestRegPrice|{6}lowestSalePrice|{7}gameID </returns>
        public List<string> GetWishlistGameData(string url, bool getPriceData=false)
        {
            // Declaration
            List<string> rawData = new List<string>();
            WebClient client = new WebClient();
            string htmlCode = client.DownloadString(url); // get the raw HTML page returned from the URL call
            string gameName, gameRank, originalPrice = "$0.00", salePrice = "$0.00", salePercent = "-0", lowestRegPrice = "$0.00", lowestSalePrice = "$0.00";
            string gameID;

            HtmlDocument hdoc = new HtmlDocument();
            hdoc.LoadHtml(htmlCode);

            // Grab the wishlistRowItem tags, this will indicate each game that is in the list 
            // //*[@id]
            //var x = hdoc.DocumentNode.SelectNodes("//*[@class='wishlistRow ']");

            foreach (HtmlNode wishlistRow in hdoc.DocumentNode.SelectNodes("//div[@class='wishlistRow ']"))
            {
                gameID = wishlistRow.Id.Split('_')[1]; //<div class="wishlistRow " id="game_252250">

                // Note: the web request (thru WebClient) seems to be slightly different than if you d/l through a browser...not sure why
                HtmlDocument innerRow = new HtmlDocument();
                HtmlDocument priceData = new HtmlDocument();
                bool onSale = false;
                innerRow.LoadHtml(wishlistRow.InnerHtml);

                // Fill data
                // <h4 class="ellipsis">Big Pharma</h4>
                gameName = innerRow.DocumentNode.SelectSingleNode("//h4[@class]").InnerHtml.ToString();
                if (String.IsNullOrEmpty(gameName))
                    gameName = "[Error] No data for game name - GameID: " + gameID;
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
                    Console.WriteLine("[DEBUG]::(GetWishlistGameData) Decimal conversion yielded: {0}", pct.ToString());
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

                // If specified we should get the price data... if not it can be collected later.
                if (getPriceData)
                {
                    // Get lowest price data
                    string[] prices = GetLowestPrices(gameID).Split(':');
                    lowestRegPrice = prices[0];
                    lowestSalePrice = prices[1];
                }

                // Put the data into the collection
                rawData.Add( String.Format("{0}|{1}|{2}|{3}|{4}|{5}|{6}|{7}", gameName, gameRank, originalPrice, salePrice, salePercent, lowestRegPrice, lowestSalePrice, gameID) );
            }

            return rawData;
        }
        /// <summary>
        /// This method gets the lowest price data [if available] from www.steamprices.com
        /// </summary>
        /// <param name="steamGameId">Steam Game ID</param>
        /// <returns>Returns a string colon delimited containing the lowest regular price and lowest sale price (i.e. $14.99:$4.99)</returns>
        public string GetLowestPrices(string steamGameId)
        {
            Console.WriteLine("[DEBUG]::(GetLowestPrices) Attempting to get price data for gameID: {0}", steamGameId);
            WebClient client = new WebClient();
            string steampricesUrl = @"https://www.steamprices.com/us/app/";
            string pricesHtml = ""; string lowRegPrice = "", lowSalePrice = "", priceHistory="";
            steampricesUrl = steampricesUrl + steamGameId;

            // Pull down the raw HTML as a string
            try
            {
                Console.WriteLine("[DEBUG]::(GetLowestPrices[{0}]->DownloadString[{1}])_Execute", steamGameId, steampricesUrl);
                pricesHtml = client.DownloadString(steampricesUrl);
            }
            catch (WebException wex)
            {
                // If there's a 404 error, the issue might be DLC (which is a different link)
                if (wex.Message.Contains("404"))
                {
                    steampricesUrl = @"https://www.steamprices.com/us/dlc/" + steamGameId;
                    Console.WriteLine("[DEBUG]::(GetLowestPrices:Exception) Hit 404 - Game not found. Trying DLC alt: {0}", steampricesUrl);
                    try
                    {
                        pricesHtml = client.DownloadString(steampricesUrl);
                    }
                    catch (WebException wex2)
                    {
                        Console.WriteLine("Exception: {0}", wex2.Message); //Replace with Logger
                        // return with a zero value
                        return "0:0";
                    }
                }
            }
            
            HtmlDocument lowestPriceData = new HtmlDocument();
            lowestPriceData.LoadHtml(pricesHtml);
            Console.WriteLine("[DEBUG]::(GetLowestPrices) Loaded HTML Data - [{0} - {1}]", steamGameId, steampricesUrl);

            // Timeout delay because if you hit the site too hard it cries
            if (pricesHtml.Contains("Error 429 - Too Many Requests"))
            {
                Console.WriteLine("[DEBUG]::(GetLowestPrices) Hit 429 - too many requests: retry after 10 seconds.");
                System.Threading.Thread.Sleep((10 * 1000));
                // Pull down the raw HTML as a string
                try
                {
                    pricesHtml = client.DownloadString(steampricesUrl);
                }
                catch (WebException wex)
                {
                    // If there's a 404 error, the issue might be DLC (which is a different link)
                    if (wex.Message.Contains("404"))
                    {
                        steampricesUrl = "https://www.steamprices.com/us/dlc/" + steamGameId;
                        try
                        {
                            pricesHtml = client.DownloadString(steampricesUrl);
                        }
                        catch (WebException wex2)
                        {
                            Console.WriteLine("Exception: {0}", wex2.Message); //Replace with Logger
                            // return with a zero value
                            return "0:0";
                        }
                    }
                }
                lowestPriceData.LoadHtml(pricesHtml);
            }

            // Get the Price History data
            //    //div[@id='history']
            priceHistory = lowestPriceData.DocumentNode.SelectSingleNode("//div[@id='history']").InnerHtml.ToString();
            // For games that aren't released yet...
            if (priceHistory.Contains("No price history") || priceHistory=="")
            {
                Console.WriteLine("[DEBUG]::(GetLowestPrices) GameID: {0} - No Price Data.", steamGameId);
                return "0:0";
            }

            HtmlDocument lowestPrices = new HtmlDocument();
            lowestPrices.LoadHtml(priceHistory);
            Console.WriteLine("[DEBUG]::(GetLowestPrices) Loaded Lowest Price Data - [{0} - {1}]", steamGameId, steampricesUrl);

            // Gets us the single or double pair of prices
            //    //p[@class='nowrap']
            HtmlNodeCollection priceData;
            priceData = lowestPrices.DocumentNode.SelectNodes("//p[@class='nowrap']");
            Console.WriteLine("[DEBUG]::(GetLowestPrices) Found low price history - [{0} - {1}]", steamGameId, steampricesUrl);

            if (priceData != null)
            {
                foreach (HtmlNode price in priceData)
                {
                    HtmlNodeCollection spans = price.ChildNodes;
                    bool bRegFound = false;
                    bool bSaleFound = false;
                    foreach (HtmlNode span in spans)
                    {
                        switch (span.InnerText)
                        {
                            case "Lowest regular price:":
                                bRegFound = true;
                                break;
                            case "Lowest discount price:":
                                bSaleFound = true;
                                break;
                        }

                        if ((span.GetAttributeValue("class", "") == "price value"))
                        {
                            if (bRegFound) { lowRegPrice = span.InnerText; bRegFound = false; }
                            if (bSaleFound) { lowSalePrice = span.InnerText; bSaleFound = false; }
                        }

                    }
                }
            }

            lowRegPrice = lowRegPrice.Substring(5, lowRegPrice.Length - 5);
            if (!(String.IsNullOrEmpty(lowSalePrice)))
                lowSalePrice = lowSalePrice.Substring(5, lowSalePrice.Length - 5);
            else
                lowSalePrice = "0";

            Console.WriteLine("[DEBUG]::(GetLowestPrices) Returning price data - [{0} - {1}]", steamGameId, (lowRegPrice + ":" + lowSalePrice));
            return lowRegPrice + ":" + lowSalePrice;
        }

        public void GetLowestPriceByBatch(GameEntryObject[] geoArray, int batchLimit = 2, int sleepTime = 30)
        {
            int sleepTimeAmt = sleepTime * 1000;
            int batchCounter = 1;
            int itemCounter = 0;

            foreach (GameEntryObject gameData in geoArray)
            {
                string[] lowestPriceData = GetLowestPrices(gameData.GameID).Split(':');
                gameData.LowestRegularPrice = lowestPriceData[0]; //lowestRegPrice 
                gameData.LowestSalePrice= lowestPriceData[1]; //lowestSalePrice 
                itemCounter++;

                if (batchCounter >= batchLimit)
                {
                    Console.WriteLine("[DEBUG]::(GetLowestPriceByBatch) Batch of {1} completed, {2} of {3} processed so far. Entering pause for {0} seconds...", sleepTime.ToString(), batchLimit.ToString(), itemCounter, (geoArray.Length+1));
                    System.Threading.Thread.Sleep(sleepTimeAmt);
                    batchCounter = 1;
                }
                else
                {
                    batchCounter++;
                }
            }
        }
    }
}
