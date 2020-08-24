using System.Windows;

namespace Blitz_Troubleshooter_V2._3
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        readonly string Version = "2.4";
        public MainWindow()
        {
            InitializeComponent();
            w1.Title = "Blitz Troubleshooter V" + Version;
        }
        private void Button_Click(object sender, RoutedEventArgs e)
        {
            Window English = new English(Version);
            English.Show();
            Close();

        }
        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            Window German = new German(Version);
            German.Show();
            Close();
        }
        private void Button_Click_2(object sender, RoutedEventArgs e)
        {
            Window Turkish = new Turkish(Version);
            Turkish.Show();
            Close();
        }
        private void Button_Click_3(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Welcome to my Unofficial Troubleshooter for the Blitz application, if you've pressed this button it means you've probably been in the need of some information regarding the program." + "\n\n" + "There are currently 4 options to choose from in order to solve your issues" + "\n\n" + "If you'd like some detailed information regarding these options, please message me on Discord:" + "\n" + "Kubi#2468", "Information", MessageBoxButton.OK, MessageBoxImage.Information);
        }
    }
}
