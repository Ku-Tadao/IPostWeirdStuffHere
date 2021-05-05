using System.Windows;
using Blitz_Troubleshooter.Languages.English;
using Blitz_Troubleshooter.Languages.German;
using Blitz_Troubleshooter.Languages.Polish;
using Blitz_Troubleshooter.Languages.Portuguese;
using Blitz_Troubleshooter.Languages.Turkish;

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
            W1.Title = "Version 2.7";
        }

        private void Btn_English(object sender, RoutedEventArgs e)
        {
            Window english = new English();
            english.Show();
            Close();
        }

        private void Btn_German(object sender, RoutedEventArgs e)
        {
            Window german = new German();
            german.Show();
            Close();
        }

        private void Btn_Turkish(object sender, RoutedEventArgs e)
        {
            Window turkish = new Turkish();
            turkish.Show();
            Close();
        }

        private void Btn_Information(object sender, RoutedEventArgs e)
        {
            MessageBox.Show(
                "Welcome to my Unofficial Troubleshooter for the Blitz application, if you've pressed this button it means you've probably been in the need of some information regarding the program." +
                "\n\n" + "There are currently 5 options to choose from in order to solve your issues" + "\n\n" +
                "If you'd like some detailed information regarding these options, please message me on Discord:" +
                "\n" + "Ku Tadao#8642", "Information", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void Btn_Polish(object sender, RoutedEventArgs e)
        {
            Window polish = new Polish();
            polish.Show();
            Close();
        }

        private void Btn_Portuguese(object sender, RoutedEventArgs e)
        {
            Window portuguese = new Portuguese();
            portuguese.Show();
            Close();
        }
    }
}