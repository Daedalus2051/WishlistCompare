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
            WishlistCompare.ViewModel.GameEntryViewModel gevm = new ViewModel.GameEntryViewModel();
            //HtmlParser par = new HtmlParser();
            //GameEntryObject gameData = new GameEntryObject();
            
            //txtDisplay.Text = par.CombineGameAndRank();
            //txtDisplay.Text = par.GetGamesAndRanksFromURL( txtWishlistURL.Text );
            
            //dgMain.ItemsSource = GameEntryObject.GetGameData(txtWishlistURL.Text);
            
            //gameData.GetGameDataAsync(txtWishlistURL.Text);
            //dgMain.ItemsSource = gameData.CollectedGameData;

            gevm.PopulateData(txtWishlistURL.Text);
            Application.Current.Dispatcher.BeginInvoke(new Action(() => dgMain.ItemsSource = gevm.gameObjectData));

        }

        private void btnDebug_Click(object sender, RoutedEventArgs e)
        {
            Console.WriteLine("Testing the most awesome test of all the tests!");
            /*
            GameEntryObject test = (GameEntryObject)dgMain.SelectedItem;
            MessageBox.Show(String.Format("Name: {0}\nID: {1}", test.Name, test.GameID)); */
        }
    }
}
