﻿using ControlzEx.Theming;

using FASTER.Models;

using Microsoft.AppCenter;
using Microsoft.AppCenter.Analytics;
using Microsoft.AppCenter.Crashes;
using Microsoft.ApplicationInsights;
using Microsoft.ApplicationInsights.Extensibility;

using System;
using System.Globalization;
using System.Management;
using System.Threading;
using System.Windows;

namespace FASTER
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            AprilFoolsCheck();
            
            base.OnStartup(e);

            var countryCode = RegionInfo.CurrentRegion.TwoLetterISORegionName;
            AppCenter.SetCountryCode(countryCode);
            Analytics.SetEnabledAsync(true);
            AppCenter.Start("257a7dac-e53c-4bec-b672-b6b939ed5d1e", typeof(Analytics), typeof(Crashes));

            TelemetryConfiguration configuration = TelemetryConfiguration.CreateDefault();
            configuration.InstrumentationKey = AppInsights.AzureInsightsKey;
            configuration.TelemetryInitializers.Add(new OperationCorrelationTelemetryInitializer());

            AppInsights.Client = new TelemetryClient(configuration);
            AppInsights.Client.Context.Component.Version = Functions.GetVersion();
            AppInsights.Client.Context.Device.Type = "PC";
            AppInsights.Client.Context.Device.Id = Environment.MachineName + Environment.UserDomainName;
            AppInsights.Client.Context.User.Id = Environment.MachineName + Environment.UserDomainName;
            try
            {
                using ManagementObjectSearcher searcher =
                    new ManagementObjectSearcher(
                        new SelectQuery(@"Select * from Win32_ComputerSystem"));
                //execute the query
                foreach (var o in searcher.Get())
                {
                    var process = (ManagementObject) o;
                    process.Get();
                    AppInsights.Client.Context.Device.Model = process["Model"].ToString();
                }
            }
            catch
            { AppInsights.Client.Context.Device.Model = "Unknown"; }
        }

        private void AprilFoolsCheck()
        {
            if (DateTime.Today != new DateTime(DateTime.Today.Year, 4, 1)) return;

            var themeThread = new Thread(() =>
                {
                    var r      = new Random();
                    var themes = ThemeManager.Current.ColorSchemes;
                    while (true)
                    {
                        Dispatcher.BeginInvoke(new Action(() => ThemeManager.Current.ChangeTheme(Current, themes[r.Next(themes.Count)])));
                        Thread.Sleep(5000);
                    }
                })
                { IsBackground = true };

            themeThread.Start();
        }
    }
}
