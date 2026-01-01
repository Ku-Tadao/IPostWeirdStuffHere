using System.Configuration;
using System;
using System.IO;
using System.Windows;
using BlitzTroubleshooter.Configuration;
using BlitzTroubleshooter.Services;
using BlitzTroubleshooter.ViewModels;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Serilog;
using System.Globalization;
using System.Linq;
using System.Threading;
using BlitzTroubleshooter.Views;
using System.Diagnostics;

namespace BlitzTroubleshooter
{
    public partial class App : Application
    {
        private readonly ServiceProvider _serviceProvider;
        public static IConfiguration Configuration { get; private set; } = null!;

        public App()
        {
            this.ShutdownMode = ShutdownMode.OnExplicitShutdown;

            var assembly = System.Reflection.Assembly.GetExecutingAssembly();
            // Resource name pattern: Namespace.Folder.Filename (folders use dots)
            // Since it's in the root of the project, it should be Namespace.Filename
            using var stream = assembly.GetManifestResourceStream("BlitzTroubleshooter.appsettings.json");

            var builder = new ConfigurationBuilder();
            
            if (stream != null)
            {
                builder.AddJsonStream(stream);
            }
            
            Configuration = builder.Build();

            Log.Logger = new LoggerConfiguration()
                .ReadFrom.Configuration(Configuration)
                .CreateLogger();

            var services = new ServiceCollection();

            services.AddLogging(loggingBuilder =>
            {
                loggingBuilder.ClearProviders();
                loggingBuilder.AddSerilog(dispose: true);
            });

            services.Configure<BlitzConfiguration>(Configuration.GetSection("Blitz"));

            services.AddHttpClient();
            services.AddSingleton<IBlitzService, BlitzService>();

            services.AddSingleton<MainViewModel>();
            services.AddSingleton<MainWindow>();

            _serviceProvider = services.BuildServiceProvider();
        }

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            var languagePicker = new LanguagePickerWindow();

            bool? pickerResult = languagePicker.ShowDialog();

            if (pickerResult == true && !string.IsNullOrEmpty(languagePicker.SelectedCultureName))
            {
                SetLanguage(languagePicker.SelectedCultureName);

                MainWindow mainWindowInstance = null;
                try
                {
                    mainWindowInstance = _serviceProvider.GetRequiredService<MainWindow>();
                }
                catch (Exception exService)
                {
                    Log.Fatal(exService, "Failed to resolve MainWindow service.");
                    Shutdown();
                    return;
                }

                Application.Current.MainWindow = mainWindowInstance;

                try
                {
                    mainWindowInstance.Show();
                }
                catch (Exception exShow)
                {
                    Log.Fatal(exShow, "Failed to show MainWindow.");
                    Shutdown();
                    return;
                }
            }
            else
            {
                Log.Warning("Language selection was not confirmed. Application will shut down.");
                Shutdown();
                return;
            }
        }

        public static void SetLanguage(string cultureName)
        {
            if (string.IsNullOrEmpty(cultureName))
            {
                Log.Warning("Culture name is null or empty, defaulting to en-US.");
                cultureName = "en-US";
            }

            try
            {
                var culture = new CultureInfo(cultureName);
                Thread.CurrentThread.CurrentCulture = culture;
                Thread.CurrentThread.CurrentUICulture = culture;
                Log.Information("Application UI culture set to: {CultureName}", cultureName);
            }
            catch (CultureNotFoundException ex)
            {
                Log.Error(ex, "Culture not found: {CultureName}. Defaulting to en-US.", cultureName);
                var culture = new CultureInfo("en-US");
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
                Log.Debug("Removed existing language dictionary: {SourceUri}", existingLangDict.Source.OriginalString);
            }
            else
            {
            }

            ResourceDictionary newLangDict = new ResourceDictionary();
            string resourcePath;

            if (cultureName.Equals("en-US", StringComparison.OrdinalIgnoreCase))
            {
                resourcePath = "Resources/Localization/StringResource.xaml";
            }
            else
            {
                resourcePath = $"Resources/Localization/StringResource.{cultureName}.xaml";
            }

            try
            {
                newLangDict.Source = new Uri(resourcePath, UriKind.RelativeOrAbsolute);
                currentDictionaries.Add(newLangDict);
                Log.Information("Successfully loaded language resource: {ResourcePath}", resourcePath);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Failed to load language resource file: {ResourcePath}. Attempting to load default English.", resourcePath);
                try
                {
                    var englishDict = currentDictionaries.FirstOrDefault(d => d.Source != null && d.Source.OriginalString.EndsWith("StringResource.xaml"));
                    if (englishDict == null)
                    {
                        var defaultLangDict = new ResourceDictionary
                        {
                            Source = new Uri("Resources/Localization/StringResource.xaml", UriKind.RelativeOrAbsolute)
                        };
                        currentDictionaries.Add(defaultLangDict);
                        Log.Information("Fallback: Loaded default English language dictionary (StringResource.xaml).");
                    }
                    else
                    {
                    }
                }
                catch (Exception fallbackEx)
                {
                    Log.Fatal(fallbackEx, "CRITICAL: Failed to load default English language resource file as fallback. UI text may be missing.");
                }
            }
        }

        protected override void OnExit(ExitEventArgs e)
        {
            Log.CloseAndFlush();
            base.OnExit(e);
        }
    }
}