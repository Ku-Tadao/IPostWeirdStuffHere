using System.Windows;
using Microsoft.Win32;

namespace Blitz_Troubleshooter_2._0
{ 
    public partial class TroubleshootPage : Window
    {
        private readonly int _hashcodegiven;
        private string _optionchosen;

        private void Hashresolver()
        {
            switch (_hashcodegiven)
            {
                case 16246551:
                    _optionchosen = "Blitz";
                    break;
                case 53180767:
                    _optionchosen = "League of Legends";
                    break;
                case 12674872:
                    _optionchosen = "League of Legends";
                    break;
                case 20031746:
                    _optionchosen = "Valorant";
                    break;
                case 11958757:
                    _optionchosen = "Fortnite";
                    break;
                case 29135240:
                    _optionchosen = "Apex Legends";
                    break;
                case 11144211:
                    _optionchosen = "Counter Strike: Global Offensive";
                    break;

            }
        }

        public TroubleshootPage(int hashcodegiven)
        {
            _hashcodegiven = hashcodegiven;
            InitializeComponent();
            testlabelbruv.Content = _hashcodegiven;
            Installationpathgrabber();
        }

        private string Installationpathgrabber()
        {
            var registry_key = @"SOFTWARE\Microsoft\Windows\CurrentVersion\Explorer\FeatureUsage\AppSwitched";
            using (var key = Registry.CurrentUser.OpenSubKey(registry_key))
            {
                foreach (var subkeyName in key.GetValueNames())
                {
                    if (subkeyName.Contains(_optionchosen) && subkeyName.Contains(".exe"))
                        MessageBox.Show(subkeyName);
                        return subkeyName;

                }
            }

            return "";

        }
    }
}
