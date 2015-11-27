using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
//added
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Collections.ObjectModel;

namespace WishlistCompare
{
    public class GameEntryObject : INotifyPropertyChanged
    {
        #region Properties
        private string _name;
        private decimal _originalPrice = 0.0m;
        private decimal _salePrice = 0.0m;
        private int _salePct = 0;
        private decimal _lowestRegPrice = 0.0m;
        private decimal _lowestSalePrice = 0.0m;

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
        /// The original price of the game.
        /// </summary>
        public string OriginalPrice
        {
            get { return _originalPrice.ToString("C2"); }
            set
            {
                if (!(Decimal.TryParse(value, out _originalPrice)))
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
                if (!(Decimal.TryParse(value, out _salePrice)))
                    _salePrice = -1.0m;
                RaisePropertyChanged();
            }
        }
        /// <summary>
        /// Gets/Sets the sales percent of the Game Object. Expects a decimal to convert (i.e. 25% should be input as .25).
        /// </summary>
        public string SalePercent
        {
            get { return _salePct.ToString("P"); }
            set
            {
                if (!(Int32.TryParse(value, out _salePct)))
                    _salePct = -1;
                RaisePropertyChanged();
            }
        }
        /// <summary>
        /// Not yet implemented
        /// </summary>
        public string LowestRegularPrice
        {
            get { return _lowestRegPrice.ToString("C2"); }
            set
            {
                if (!(Decimal.TryParse(value, out _lowestRegPrice)))
                    _lowestRegPrice = -1.0m;
                RaisePropertyChanged();
            }
        }
        /// <summary>
        /// Not yet implemented
        /// </summary>
        public string LowestSalePrice
        {
            get { return _lowestSalePrice.ToString("C2"); }
            set
            {
                if (!(Decimal.TryParse(value, out _lowestSalePrice)))
                    _lowestSalePrice = -1.0m;
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
    }
}
