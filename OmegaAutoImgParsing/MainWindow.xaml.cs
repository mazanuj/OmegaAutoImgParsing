using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Windows;
using Microsoft.Win32;
using OmegaAutoImgParsing.Properties;
using OmegaParsingLib;

namespace OmegaAutoImgParsing
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow
    {
        private static string CsvPath { get; set; }
        public static ObservableCollection<string> DataItemsLog { get; set; }

        public MainWindow()
        {
            DataContext = this;
            DataItemsLog = new ObservableCollection<string>();

            Informer.OnResultStr +=
                async result =>
                    await Application.Current.Dispatcher.BeginInvoke(
                        new Action(() => DataItemsLog.Insert(0, result)));

            InitializeComponent();
            //Height = SystemParameters.WorkArea.Height;
        }

        private void LaunchOmegaAutoImgParsingOnGitHub(object sender, RoutedEventArgs e)
        {
            Process.Start("https://github.com/mazanuj/OmegaAutoImgParsing/");
        }

        private async void ButtonBase_OnClick(object sender, RoutedEventArgs e)
        {
            ButtonIsEnable(false);
            await Task.Run(async () =>
            {
                await Initialize.Parse(CsvPath, Settings.Default.UserName, Settings.Default.UserPass);
            });
            ButtonIsEnable(true);
        }

        private void ButtonCsv_OnClick(object sender, RoutedEventArgs e)
        {
            ButtonIsEnable(false);
            var sfd = new OpenFileDialog()
            {
                Filter = "CSV (*.csv)|*.csv",
                InitialDirectory = AppDomain.CurrentDomain.BaseDirectory,
                RestoreDirectory = true
            };

            if (sfd.ShowDialog() == false)
                return;

            CsvPath = sfd.FileName;
            ButtonIsEnable(true);
        }

        private void ButtonIsEnable(bool value)
        {
            ButtonStart.IsEnabled = value;
            ButtonXls.IsEnabled = value;
        }
    }
}