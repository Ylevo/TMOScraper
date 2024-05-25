using Microsoft.Extensions.DependencyInjection;
using System;
using TMOScrapper.Core;
using Polly.Extensions;
using Microsoft.Extensions.Logging;
using TMOScrapper.Utils;
using TMOScrapper.Properties;

namespace TMOScrapper
{
    internal static class Program
    {
        /// <summary>
        ///  The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            // To customize application configuration such as set high DPI settings or default font,
            // see https://aka.ms/applicationconfiguration.
            ApplicationConfiguration.Initialize();
            var services = new ServiceCollection();
            ConfigureServices(services);
            if (Settings.Default.UpgradeRequired)
            {
                Settings.Default.Upgrade();
                Settings.Default.UpgradeRequired = false;
                Settings.Default.Save();
            }
            using (ServiceProvider serviceProvider = services.BuildServiceProvider())
            {
                var mainForm = serviceProvider.GetRequiredService<MainForm>();
                Application.Run(mainForm);
            }
        }

        private static void ConfigureServices(ServiceCollection services)
        {
            services.AddScoped<MainForm>();
            services.AddTransient<ScrapperHandler>();
            services.AddTransient<Scrapper>();
            services.AddTransient<HtmlAgilityPack.HtmlDocument>();
            services.AddTransient<CancellationTokenSource>();
        }
    }
}