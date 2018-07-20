﻿using System;
using System.IO;
using System.Windows;
using Sakuno.ING.Bootstrap;

namespace Sakuno.ING
{
    public static class Program
    {
        [STAThread]
        public static void Main(string[] args)
        {
            Directory.CreateDirectory("data");

            Bootstraper.InitializeFromAssemblyNames
            (
                typeof(FrameworkElement),
                Array.Empty<string>(),
                "Sakuno.ING.Core.Listener",
                "Sakuno.ING.Data",
                "Sakuno.ING.Data.Desktop",
                "Sakuno.ING.Localization.Embed",
                "Sakuno.ING.Game.Logger",
                "Sakuno.ING.Game.Logger.Migrators",
                "Sakuno.ING.Game.Models",
                "Sakuno.ING.Game.Provider",
                "Sakuno.ING.Settings.Common",
                "Sakuno.ING.Shell.Desktop",
                "Sakuno.ING.Timing.NTP",
                "Sakuno.ING.ViewModels.Logging",
                "Sakuno.ING.Views.Desktop.Base",
                "Sakuno.ING.Views.Desktop.Common",
                "Sakuno.ING.Views.Desktop.Logging",
                "Sakuno.ING.Views.Desktop.Homeport",
                "Sakuno.ING.Views.Desktop.Settings"
            );

            Bootstraper.Startup();
        }
    }
}
