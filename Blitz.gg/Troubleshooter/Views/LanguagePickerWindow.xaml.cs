using BlitzTroubleshooter.Models;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Input;
using System.Linq;
using System.Diagnostics;

namespace BlitzTroubleshooter.Views
{
    public partial class LanguagePickerWindow : Window
    {
        public List<LanguageItem> AvailableLanguages { get; set; }
        public string SelectedCultureName { get; private set; }

        public LanguagePickerWindow()
        {
            InitializeComponent();

            AvailableLanguages = new List<LanguageItem>
            {
                new LanguageItem { DisplayName = "English", CultureCode = "en-US" },
                new LanguageItem { DisplayName = "Deutsch", CultureCode = "de-DE" },
                new LanguageItem { DisplayName = "Français", CultureCode = "fr-FR" },
                new LanguageItem { DisplayName = "Polski", CultureCode = "pl-PL" },
                new LanguageItem { DisplayName = "Português", CultureCode = "pt-PT" },
                new LanguageItem { DisplayName = "Türkçe", CultureCode = "tr-TR" },
                new LanguageItem { DisplayName = "Русский", CultureCode = "ru-RU" }
            };

            DataContext = this;

            if (AvailableLanguages.Any())
            {
                LanguageComboBox.SelectedItem = AvailableLanguages.FirstOrDefault();
            }
            else
            {
            }
        }

        private void ConfirmButton_Click(object sender, RoutedEventArgs e)
        {

            if (LanguageComboBox.SelectedItem is LanguageItem selectedLanguage)
            {
                SelectedCultureName = selectedLanguage.CultureCode;
                DialogResult = true;
                Close();
            }
            else
            {
                MessageBox.Show("Please select a language to continue.", "Language Required", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        protected override void OnClosing(System.ComponentModel.CancelEventArgs e)
        {
            base.OnClosing(e);
            if (DialogResult != true)
            {
                DialogResult = false;
            }
            else
            {
            }
        }

        private void Window_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
                this.DragMove();
        }
    }
}