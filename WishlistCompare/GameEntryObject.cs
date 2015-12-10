using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
//added
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Globalization;
using System.Runtime.CompilerServices;

namespace WishlistCompare
{
    public class GameEntryObject : INotifyPropertyChanged
    {
        #region Properties
        private string _name;
        private int _rank;
        private int _gameID;
        private decimal _originalPrice = 0.0m;
        private decimal _salePrice = 0.0m;
        private decimal _salePct = -0;
        private decimal _lowestRegPrice = 0.0m;
        private decimal _lowestSalePrice = 0.0m;
        private CultureInfo culture = new CultureInfo(CultureInfo.CurrentCulture.Name, false);
        private NumberStyles style_sales = NumberStyles.Number | NumberStyles.AllowCurrencySymbol;
        private NumberStyles style_pct = NumberStyles.Number | NumberStyles.AllowLeadingSign | NumberStyles.AllowTrailingSign;
        private ObservableCollection<GameEntryObject> _collectedData;

        /// <summary>
        /// Name of the game.
        /// </summary>
        public string Name
        {
            get { return _name; }
            set
            {
                _name = value;
                RaisePropertyChanged();
            }
        }
        /// <summary>
        /// Rank of the game in the wishlist.
        /// </summary>
        public string Rank
        {
            get { return _rank.ToString(); }
            set
            {
                if (!(Int32.TryParse(value, out _rank)))
                    _rank = -1;
                RaisePropertyChanged();
            }
        }
        public string GameID
        {
            get { return _gameID.ToString(); }
            set
            {
                if (!(Int32.TryParse(value, out _gameID)))
                    _gameID = -1;
                RaisePropertyChanged();
            }
        }
        /// <summary>
        /// The original price of the game.
        /// </summary>
        public string OriginalPrice
        {
            get { return _originalPrice.ToString("C2"); }
            set
            {
                if (!(Decimal.TryParse(value, style_sales, culture, out _originalPrice)))
                    _originalPrice = -1.0m;
                RaisePropertyChanged();
            }
        }
        /// <summary>
        /// The price at which the game is on sale for (this is the current/final price if the game is on sale).
        /// </summary>
        public string SalePrice
        {
            get { return _salePrice.ToString("C2"); }
            set
            {
                if (!(Decimal.TryParse(value, style_sales, culture, out _salePrice)))
                    _salePrice = -1.0m;
                RaisePropertyChanged();
            }
        }
        /// <summary>
        /// The sales percent of the Game Object. Expects a decimal to convert (i.e. 25% should be input as .25).
        /// </summary>
        public string SalePercent
        {
            get { return _salePct.ToString("P"); }
            set
            {
                if (!(Decimal.TryParse(value, style_pct, culture, out _salePct)))
                    _salePct = -1;
                RaisePropertyChanged();
            }
        }
        /// <summary>
        /// The lowest regular price that was found from SteamPrices.com
        /// </summary>
        public string LowestRegularPrice
        {
            get { return _lowestRegPrice.ToString("C2"); }
            set
            {
                if (!(Decimal.TryParse(value, style_sales, culture, out _lowestRegPrice)))
                    _lowestRegPrice = -1.0m;
                RaisePropertyChanged();
            }
        }
        /// <summary>
        /// The lowest sale price that was found from SteamPrices.com
        /// </summary>
        public string LowestSalePrice
        {
            get { return _lowestSalePrice.ToString("C2"); }
            set
            {
                if (!(Decimal.TryParse(value, style_sales, culture, out _lowestSalePrice)))
                    _lowestSalePrice = -1.0m;
                RaisePropertyChanged();
            }
        }
        /// <summary>
        /// Observable collection of GameEntryObjects; only populated after running the async method to gather data.
        /// </summary>
        public ObservableCollection<GameEntryObject> CollectedGameData
        {
            get { return _collectedData; }
            set
            {
                _collectedData = value;
                RaisePropertyChanged();
            }
        }
        #endregion
        #region EventHandler
        public event PropertyChangedEventHandler PropertyChanged;
        private void RaisePropertyChanged([CallerMemberName] string caller = "")
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(caller));
        }
        #endregion
        #region Methods
        //public void GetGameData(string url) // Leaving this here for when I figure out what is wrong with the async call
        public static ObservableCollection<GameEntryObject> GetGameData(string url)
        {
            var gameData = new ObservableCollection<GameEntryObject>();
            HtmlParser par = new HtmlParser();

            // Call method to get data
            List<string> rawData = par.GetWishlistGameData(url);

            // Parse the data and put it in the object
            foreach (string raw in rawData)
            {
                //  {0}   |   {1}   |      {2}     |    {3}   |     {4}    |      {5}      |      {6}       |   {7}
                //gameName, gameRank, originalPrice, salePrice, salePercent, lowestRegPrice, lowestSalePrice, gameID - separated by '|'
                string[] gameObjData = raw.Split('|');
                gameData.Add(new GameEntryObject() { Name = gameObjData[0], Rank = gameObjData[1], OriginalPrice = gameObjData[2], SalePrice = gameObjData[3],
                    SalePercent = gameObjData[4], LowestRegularPrice = gameObjData[5], LowestSalePrice = gameObjData[6], GameID = gameObjData[7]});
            }
            
            // Return data to caller
            //CollectedGameData = gameData;
            return gameData;
        }
        public async void GetGameDataAsync(string url)
        {
            await Task.Run( () => GetGameData(url) );
        }
        #endregion
    }
}
