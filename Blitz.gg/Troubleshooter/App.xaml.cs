using System;
using System.IO;
using System.Windows;
using BlitzTroubleshooter.Configuration;
using BlitzTroubleshooter.Services;
using BlitzTroubleshooter.ViewModels;
using Newtonsoft.Json;
using System.Globalization;
using System.Linq;
using System.Threading;
using BlitzTroubleshooter.Views;
//using System.Diagnostics; // No longer used directly here

namespace BlitzTroubleshooter
{
    public partial class App : Application
    {
        public static BlitzConfiguration AppConfig { get; private set; }

        // private readonly ServiceProvider _serviceProvider; // DI removed
        // public static IConfiguration Configuration { get; private set; } = null!; // Configuration removed

        public App()
        {
            this.ShutdownMode = ShutdownMode.OnExplicitShutdown;

            // Configuration = new ConfigurationBuilder()
            //     .SetBasePath(Directory.GetCurrentDirectory())
            //     .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
            //     .Build();

            // Log.Logger = new LoggerConfiguration()
            //     .ReadFrom.Configuration(Configuration)
            //     .CreateLogger();

            // var services = new ServiceCollection();

            // services.AddLogging(loggingBuilder =>
            // {
            //     loggingBuilder.ClearProviders();
            //     loggingBuilder.AddSerilog(dispose: true);
            // });

            // services.Configure<BlitzConfiguration>(Configuration.GetSection("Blitz")); // Deferred

            // services.AddHttpClient(); // HttpClient will be managed manually or by a simpler mechanism
            // services.AddSingleton<IBlitzService, BlitzService>(); // Manual instantiation now

            // services.AddSingleton<MainViewModel>(); // Manual instantiation now
            // services.AddSingleton<MainWindow>(); // Manual instantiation now

            // _serviceProvider = services.BuildServiceProvider(); // DI removed
        }
        private void LoadConfiguration()
        {
            try
            {
                string configFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "appsettings.json");
                if (File.Exists(configFilePath))
                {
                    string json = File.ReadAllText(configFilePath);
                    // Assuming appsettings.json has a top-level object, e.g., "Settings" or "Blitz"
                    // If BlitzConfiguration directly maps to the root, then:
                    // AppConfig = JsonConvert.DeserializeObject<BlitzConfiguration>(json);

                    // If appsettings.json has structure like { "Blitz": { ... } }
                    var rootObject = JsonConvert.DeserializeObject<RootConfiguration>(json);
                    if (rootObject != null)
                    {
                        AppConfig = rootObject.Blitz;
                    }

                    if (AppConfig == null) // Handle case where "Blitz" key might be missing or deserialization results in null
                    {
                        AppConfig = new BlitzConfiguration(); // Load defaults or handle error
                        MessageBox.Show("Blitz configuration section not found or is empty in appsettings.json. Loading default configuration.", "Configuration Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
                    }
                }
                else
                {
                    AppConfig = new BlitzConfiguration(); // Load defaults
                    MessageBox.Show("appsettings.json not found. Loading default configuration.", "Configuration Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                }
            }
            catch (Exception ex)
            {
                AppConfig = new BlitzConfiguration(); // Load defaults on error
                MessageBox.Show(string.Format("Error loading configuration: {0} Loading default configuration.", ex.Message), "Configuration Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);
            LoadConfiguration();

            var languagePicker = new LanguagePickerWindow();

            bool? pickerResult = languagePicker.ShowDialog();

            if (pickerResult == true && !string.IsNullOrEmpty(languagePicker.SelectedCultureName))
            {
                SetLanguage(languagePicker.SelectedCultureName);

                // Manual instantiation, now passing AppConfig to BlitzService
                var blitzService = new BlitzService(new System.Net.Http.HttpClient(), AppConfig);
                var mainViewModel = new MainViewModel(blitzService); // Logger placeholder was already removed
                var mainWindowInstance = new MainWindow(mainViewModel);

                // try
                // {
                //     mainWindowInstance = _serviceProvider.GetRequiredService<MainWindow>();
                // }
                // catch (Exception exService)
                // {
                //     // Log.Fatal(exService, "Failed to resolve MainWindow service.");
                //     MessageBox.Show("Failed to start application. Error: " + exService.Message, "Startup Error", MessageBoxButton.OK, MessageBoxImage.Error);
                //     Shutdown();
                //     return;
                // }

                Application.Current.MainWindow = mainWindowInstance;

                try
                {
                    mainWindowInstance.Show();
                }
                catch (Exception exShow)
                {
                    // Log.Fatal(exShow, "Failed to show MainWindow.");
                    MessageBox.Show("Failed to show main window. Error: " + exShow.Message, "Startup Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    Shutdown();
                    return;
                }
            }
            else
            {
                // Log.Warning("Language selection was not confirmed. Application will shut down.");
                MessageBox.Show("Language selection was not confirmed. Application will shut down.", "Startup Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                Shutdown();
                return;
            }
        }

        public static void SetLanguage(string cultureName)
        {
            if (string.IsNullOrEmpty(cultureName))
            {
                // Log.Warning("Culture name is null or empty, defaulting to en-US.");
                cultureName = "en-US"; // Default to en-US
            }

            try
            {
                var culture = new CultureInfo(cultureName);
                Thread.CurrentThread.CurrentCulture = culture;
                Thread.CurrentThread.CurrentUICulture = culture;
                // Log.Information("Application UI culture set to: {CultureName}", cultureName);
            }
            catch (CultureNotFoundException /*ex*/)
            {
                // Log.Error(ex, "Culture not found: {CultureName}. Defaulting to en-US.", cultureName);
                var culture = new CultureInfo("en-US"); // Fallback to en-US
                Thread.CurrentThread.CurrentCulture = culture;
                Thread.CurrentThread.CurrentUICulture = culture;
                cultureName = "en-US";
            }

            var currentDictionaries = Application.Current.Resources.MergedDictionaries;
            var existingLangDict = currentDictionaries
                .FirstOrDefault(d => d.Source != null &&
                                     (d.Source.OriginalString.EndsWith("StringResource.xaml") ||
                                      d.Source.OriginalString.Contains("StringResource.")));

            if (existingLangDict != null)
            {
                currentDictionaries.Remove(existingLangDict);
                // Log.Debug("Removed existing language dictionary: {SourceUri}", existingLangDict.Source.OriginalString);
            }
            // else
            // {
            //     // Log.Debug("No existing language dictionary found to remove.");
            // }

            ResourceDictionary newLangDict = new ResourceDictionary();
            string resourcePath;

            if (cultureName.Equals("en-US", StringComparison.OrdinalIgnoreCase))
            {
                resourcePath = "Resources/Localization/StringResource.xaml";
            }
            else
            {
                // Simplified for C# 7.3 compatibility if it was using interpolated strings in a complex way before
                resourcePath = string.Format("Resources/Localization/StringResource.{0}.xaml", cultureName);
            }

            try
            {
                newLangDict.Source = new Uri(resourcePath, UriKind.RelativeOrAbsolute);
                currentDictionaries.Add(newLangDict);
                // Log.Information("Successfully loaded language resource: {ResourcePath}", resourcePath);
            }
            catch (Exception /*ex*/)
            {
                // Log.Error(ex, "Failed to load language resource file: {ResourcePath}. Attempting to load default English.", resourcePath);
                try
                {
                    // Attempt to load default English if specific one fails or was already removed.
                    var englishDict = currentDictionaries.FirstOrDefault(d => d.Source != null && d.Source.OriginalString.EndsWith("StringResource.xaml"));
                    if (englishDict == null) // Only add if not already present
                    {
                        var defaultLangDict = new ResourceDictionary
                        {
                            Source = new Uri("Resources/Localization/StringResource.xaml", UriKind.RelativeOrAbsolute)
                        };
                        currentDictionaries.Add(defaultLangDict);
                        // Log.Information("Fallback: Loaded default English language dictionary (StringResource.xaml).");
                    }
                    // else
                    // {
                    //     // Log.Debug("Default English dictionary already present.");
                    // }
                }
                catch (Exception /*fallbackEx*/)
                {
                    // Log.Fatal(fallbackEx, "CRITICAL: Failed to load default English language resource file as fallback. UI text may be missing.");
                     MessageBox.Show("CRITICAL: Failed to load any language resources. UI text may be missing.", "Resource Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        protected override void OnExit(ExitEventArgs e)
        {
            // Log.CloseAndFlush(); // Serilog removed
            base.OnExit(e);
        }
    }

    // Helper class to match the structure of appsettings.json for deserialization
    internal class RootConfiguration
    {
        public BlitzConfiguration Blitz { get; set; }
        // Add other top-level configuration sections here if any (e.g., Serilog)
    }
}