using System.Windows;

namespace Blitz_Troubleshooter_V2._3
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


        private void Button_Click(object sender, RoutedEventArgs e)
        {
            Window English = new English();
            English.Show();
            Close();

        }
        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            Window German = new German();
            German.Show();
            Close();
        }
        private void Button_Click_2(object sender, RoutedEventArgs e)
        {
            Window Turkish = new Turkish();
            Turkish.Show();
            Close();
        }
        private void Button_Click_3(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Welcome to my Unofficial Troubleshooter for the Blitz application, if you've pressed this button it means you've probably been in the need of some information regarding the program." + "\n\n" + "There are currently 4 options to choose from in order to solve your issues" + "\n\n" + "If you'd like some detailed information regarding these options, please message me on Discord:" + "\n" + "Kubi#2468", "Information", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void btn4_Click(object sender, RoutedEventArgs e)
        {
            Window Polish = new Polish();
            Polish.Show();
            Close();
        }
    }
}
