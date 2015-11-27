using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace WishlistCompare
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void btnLoad_Click(object sender, RoutedEventArgs e)
        {
            HtmlParser par = new HtmlParser();
            //txtDisplay.Text = par.CombineGameAndRank();
            //txtDisplay.Text = par.GetGamesAndRanksFromURL( txtWishlistURL.Text );
            dgMain.ItemsSource = GameEntryObject.GetGameData(txtWishlistURL.Text);
        }

        private void btnDebug_Click(object sender, RoutedEventArgs e)
        {
            Console.WriteLine("Testing the most awesome test of all the tests!");
        }
    }
}
