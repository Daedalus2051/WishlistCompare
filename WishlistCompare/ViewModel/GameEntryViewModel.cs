using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
// Added 
using System.Collections.ObjectModel;

namespace WishlistCompare.ViewModel
{
    class GameEntryViewModel
    {
        public ObservableCollection<GameEntryObject> gameObjectData = new ObservableCollection<GameEntryObject>();
        public ObservableCollection<GameEntryObject> gameObjectData2 = new ObservableCollection<GameEntryObject>();
        public HtmlParser hParser = new HtmlParser();

        public void GetWishlistGames(string wishlistUrl)
        {
            List<string> gameData = hParser.GetWishlistGameData(wishlistUrl, false);
            GameEntryObject[] gameDataArray = new GameEntryObject[gameData.Count];
            int aryCount = 0;

            // Parse the data and put it in the object
            foreach (string raw in gameData)
            {
                //  {0}   |   {1}   |      {2}     |    {3}   |     {4}    |      {5}      |      {6}       |   {7}
                //gameName, gameRank, originalPrice, salePrice, salePercent, lowestRegPrice, lowestSalePrice, gameID - separated by '|'
                string[] gameObjData = raw.Split('|');

                gameDataArray[aryCount] = new GameEntryObject();
                gameDataArray[aryCount].Name = gameObjData[0];
                gameDataArray[aryCount].Rank = gameObjData[1];
                gameDataArray[aryCount].OriginalPrice = gameObjData[2];
                gameDataArray[aryCount].SalePrice = gameObjData[3];
                gameDataArray[aryCount].SalePercent = gameObjData[4];
                gameDataArray[aryCount].LowestRegularPrice = gameObjData[5];
                gameDataArray[aryCount].LowestSalePrice = gameObjData[6];
                gameDataArray[aryCount].GameID = gameObjData[7];
                Console.WriteLine("Game[{1}] {0} added.", gameDataArray[aryCount].Name, aryCount);
                aryCount++;
            }

            hParser.GetLowestPriceByBatch(gameDataArray);

            foreach (GameEntryObject geo in gameDataArray)
            {
                gameObjectData2.Add(new GameEntryObject()
                {
                    Name = geo.Name,
                    Rank = geo.Rank,
                    OriginalPrice = geo.OriginalPrice,
                    SalePrice = geo.SalePrice,
                    SalePercent = geo.SalePercent,
                    LowestRegularPrice = geo.LowestRegularPrice,
                    LowestSalePrice = geo.LowestSalePrice,
                    GameID = geo.GameID
                });
            }
            Console.WriteLine("GetWishlist method complete");
        }

        public void LoadDatagrid()
        {
            foreach (var x in gameObjectData2)
            {
                gameObjectData.Add(x);
            }
        }


        public async void PopulateData(string wishlistURL)
        {
            await Task.Run(() => GetWishlistGames(wishlistURL));
        }
    }
}
