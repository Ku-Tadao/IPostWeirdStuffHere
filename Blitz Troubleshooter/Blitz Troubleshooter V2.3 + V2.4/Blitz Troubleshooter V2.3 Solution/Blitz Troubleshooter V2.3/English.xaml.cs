using System;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Windows;

namespace Blitz_Troubleshooter_V2._3
{
    /// <summary>
    /// Interaction logic for English.xaml
    /// </summary>
    public partial class English : Window
    {
        public English(string version)
        {
            InitializeComponent();

            w1.Title = "Blitz Troubleshooter V" + version;
        }

        public void DownloadVSRDST()
        {


            using (var client = new WebClient())
            {

                client.DownloadFile("https://aka.ms/vs/16/release/vc_redist.x86.exe", System.IO.Path.GetTempPath() + "vc_redist.x86.exe");
            }



        }
        public void KillBlitz()
        {
            try
            {
                foreach (Process proc in Process.GetProcessesByName("Blitz"))
                {
                    proc.Kill();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }
        public void AppdataBlitz()
        {
            try
            {
                var path = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "\\Blitz";
                Directory.Delete(path, true);

            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message);
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

            MessageBox.Show("Blitz has been uninstalled", "Blitz Uninstallation", MessageBoxButton.OK, MessageBoxImage.Information);
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

            try
            {
                foreach (var path in paths)
                {
                    if (Directory.Exists(path))
                    {
                        Directory.Delete(path, true);
                    }
                }
            }
            catch { }
            finally
            {


                System.Threading.Thread.Sleep(5000);



                MessageBox.Show("Please wait while Blitz completely reinstalls itself", "Blitz Clean Reinstallation", MessageBoxButton.OK, MessageBoxImage.Information);
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

        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            //fix all commmon mistakes
            KillBlitz();
            System.Threading.Thread.Sleep(1000);
            CleanReinstall();


        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            KillBlitz();
            DownloadVSRDST();
            Process.Start(System.IO.Path.GetTempPath() + "vc_redist.x86.exe");
            MessageBox.Show("Please proceed to the other Window, and finish the installation, press the OK button to continue.", "vc_redist.x86 installation", MessageBoxButton.OK, MessageBoxImage.Information);

        } /*Done */

        private void Button_Click_2(object sender, RoutedEventArgs e)
        {
            //Fix Cache issues
            KillBlitz();
            System.Threading.Thread.Sleep(1000);
            AppdataBlitz();
            MessageBox.Show("Cache sucessfully cleared");
        } /*Done */



        private void Button_Click_4(object sender, RoutedEventArgs e)
        {
            KillBlitz();
            System.Threading.Thread.Sleep(1000);
            Uninstall();
        }


    }
}
