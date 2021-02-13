using System.Windows;

namespace Blitz_Troubleshooter
{
    /// <summary>
    ///     Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            Window english = new English();
            english.Show();
            Close();
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            Window german = new German();
            german.Show();
            Close();
        }

        private void Button_Click_2(object sender, RoutedEventArgs e)
        {
            Window turkish = new Turkish();
            turkish.Show();
            Close();
        }

        private void Button_Click_3(object sender, RoutedEventArgs e)
        {
            MessageBox.Show(
                "Welcome to my Unofficial Troubleshooter for the Blitz application, if you've pressed this button it means you've probably been in the need of some information regarding the program." +
                "\n\n" + "There are currently 4 options to choose from in order to solve your issues" + "\n\n" +
                "If you'd like some detailed information regarding these options, please message me on Discord:" +
                "\n" + "Kubi#2468", "Information", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void btn4_Click(object sender, RoutedEventArgs e)
        {
            Window polish = new Polish();
            polish.Show();
            Close();
        }
    }
}