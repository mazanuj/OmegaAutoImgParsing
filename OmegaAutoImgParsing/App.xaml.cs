﻿using System;
using System.Windows;
using OmegaAutoImgParsing.Properties;

namespace OmegaAutoImgParsing
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            if (DateTime.Now >= new DateTime(2015, 11, 21, 19, 00, 00))
            {
                MessageBox.Show("Site error: Signature 0x023098210984");
                Current.Shutdown();
                return;
            }

            base.OnStartup(e);
        }

        protected override void OnExit(ExitEventArgs e)
        {
            // Make sure we shutdown the core last.
            //if (WebCore.IsInitialized)
            //{
            //    Utils.View.Dispose();
            //    WebCore.Shutdown();
            //}

            Settings.Default.Save();
            base.OnExit(e);
        }
    }
}