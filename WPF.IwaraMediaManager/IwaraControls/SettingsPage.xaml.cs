using IwaraMediaManager.Core.Provider;
using IwaraMediaManager.DatabaseManager.Loader;
using IwaraMediaManager.Models;
using ModernWpf;
using System;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace IwaraMediaManager.WPF.IwaraControls
{
    /// <summary>
    /// Interaction logic for SettingsPage.xaml
    /// </summary>
    public partial class SettingsPage : UserControl
    {
        public SettingsPage()
        {
            InitializeComponent();

            var settingsLoader = new SettingsLoader();
            TextBoxUserName.Text = settingsLoader.GetSettingAsync(Setting.ACCOUNT_NAME).Result?.Value;
            TextBoxPassword.Password = settingsLoader.GetSettingAsync(Setting.ACCOUNT_PASSWORD).Result?.Value;

            NumericMaxVideosPerPage.Value = double.Parse(settingsLoader.GetSettingAsync(Setting.MAXVIDEOSPERPAGE).Result?.Value ?? "50");
            ThumpnailSizeSlider.Value = double.Parse(settingsLoader.GetSettingAsync(Setting.THUMPNAILSIZE).Result?.Value ?? "320");

            TextBoxArtistFolderPath.Text = settingsLoader.GetSettingAsync(Setting.VIDEOPATH).Result?.Value;
        }

        private async void ButtonArtistFolderPath_Click(object sender, RoutedEventArgs e)
        {
            var setting = await VideoProvider.SetVideoPathDialogAsync();

            if (setting != null)
                TextBoxArtistFolderPath.Text = $"{setting.Value} (Please restart)";
        }

        private async void Button_SaveLoginInformation(object sender, RoutedEventArgs e)
        {
            var name = TextBoxUserName.Text;
            var password = TextBoxPassword.Password;

            SettingsLoader settingsLoader = new SettingsLoader();
            await settingsLoader.SetSetting(new Setting(Setting.ACCOUNT_NAME, name));
            await settingsLoader.SetSetting(new Setting(Setting.ACCOUNT_PASSWORD, password));
        }

        private async void NumericMaxVideosPerPage_ValueChanged(ModernWpf.Controls.NumberBox sender, ModernWpf.Controls.NumberBoxValueChangedEventArgs args)
        {
            var settingsLoader = new SettingsLoader();
            await settingsLoader.SetSetting(new Setting(Setting.MAXVIDEOSPERPAGE, args.NewValue.ToString()));
        }

        private async void ThumpnailSizeSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (this.IsInitialized)
            {
                var settingsLoader = new SettingsLoader();
                await settingsLoader.SetSetting(new Setting(Setting.THUMPNAILSIZE, e.NewValue.ToString()));
            }
        }

        private async void ListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var color = ((e.AddedItems[0] as ListBoxItem).Background as SolidColorBrush).Color;
            ThemeManager.Current.AccentColor = color;

            SettingsLoader settingsLoader = new SettingsLoader();
            await settingsLoader.SetSetting(new Setting(Setting.COLORTHEME, color.ToString()));
        }
    }
}
