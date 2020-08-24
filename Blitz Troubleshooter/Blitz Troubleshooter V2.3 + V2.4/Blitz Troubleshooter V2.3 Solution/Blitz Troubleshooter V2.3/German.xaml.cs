using System;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Windows;

namespace Blitz_Troubleshooter_V2._3
{
    /// <summary>
    /// Interaction logic for German.xaml
    /// </summary>
    public partial class German : Window
    {
        public German(string version)
        {
            InitializeComponent();

            w1.Title = "Blitz Troubleshooter V" + version;
        }

        public void DownloadVSRDST()
        {
            using (var client = new WebClient())
            {

                try
                {
                    client.DownloadFile("https://aka.ms/vs/16/release/vc_redist.x86.exe", System.IO.Path.GetTempPath() + "vc_redist.x86.exe");
                }
                catch (Exception e)
                {
                    MessageBox.Show(e.Message);
                    System.Windows.Application.Current.Shutdown();
                }
                finally
                {
                    Process.Start(System.IO.Path.GetTempPath() + "vc_redist.x86.exe");
                }
            }

        }
        public void KillBlitz()
        {
            foreach (Process proc in Process.GetProcessesByName("Blitz"))
            {
                proc.Kill();
            }

        }
        public void AppdataBlitz()
        {
            var path = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "\\Blitz";
            if (Directory.Exists(path))
            {
                Directory.Delete(path, true);
            }
        }
        public void Uninstall()
        {
            var paths = new String[] {
        Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData) + "\\blitz-updater",
        Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData) + "\\Programs\\blitz-core",
        Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "\\Blitz",
        Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "\\blitz-core",
        Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "\\Blitz-helpers",
        Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData) + "\\Programs\\Blitz"
      };

            foreach (var path in paths)
            {
                if (Directory.Exists(path))
                {
                    Directory.Delete(path, true);
                }
            }

            MessageBox.Show("Blitz wurde deinstalliert ", " Blitz Deinstallation", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        public void CleanReinstall()
        {
            var paths = new String[] {
        Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData) + "\\blitz-updater",
        Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData) + "\\Programs\\blitz-core",
        Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "\\Blitz",
        Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "\\blitz-core",
        Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "\\Blitz-helpers",
        Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData) + "\\Programs\\Blitz"
      };

            foreach (var path in paths)
            {
                if (Directory.Exists(path))
                {
                    Directory.Delete(path, true);
                }
            }

            System.Threading.Thread.Sleep(5000);

            MessageBox.Show("Bitte warten Sie, während sich Blitz vollständig neu installiert ", " Blitz Clean Neuinstallation", MessageBoxButton.OK, MessageBoxImage.Information);
            using (var client = new WebClient())
            {
                try
                {
                    client.DownloadFile("https://blitz.gg/download/win", System.IO.Path.GetTempPath() + "Blitz.exe");
                }
                catch (Exception e)
                {
                    MessageBox.Show(e.Message);
                    System.Windows.Application.Current.Shutdown();
                }
                finally
                {
                    Process.Start(System.IO.Path.GetTempPath() + "Blitz.exe");
                }
            }
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            KillBlitz();
            System.Threading.Thread.Sleep(1000);
            CleanReinstall();
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            KillBlitz();
            DownloadVSRDST();
            MessageBox.Show("Fahren Sie mit dem anderen Fenster fort und beenden Sie die Installation. Klicken Sie auf OK, um fortzufahren. ", " Vc_redist.x86 Installation", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void Button_Click_2(object sender, RoutedEventArgs e)
        {
            KillBlitz();
            System.Threading.Thread.Sleep(1000);
            AppdataBlitz();
            MessageBox.Show("Cache erfolgreich geleert");
        }
        private void Button_Click_4(object sender, RoutedEventArgs e)
        {
            KillBlitz();
            System.Threading.Thread.Sleep(1000);
            Uninstall();
        }
    }
}