using IwaraMediaManager.DatabaseManager;
using IwaraMediaManager.DatabaseManager.Loader;
using IwaraMediaManager.Models;
using IwaraMediaManager.Wpf.Views;
using IwaraMediaManager.WPF.Base;
using Microsoft.EntityFrameworkCore;
using System.Drawing;
using System.Windows;
using System.Windows.Media;

namespace WPF.IwaraMediaManager
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        private void Application_Startup(object sender, StartupEventArgs e)
        {
            this.ShutdownMode = ShutdownMode.OnExplicitShutdown;
            
            using (DataBaseContext dbContext = new DataBaseContext())
            {
                dbContext.Database.Migrate();
            }

            var settingsLoader = new SettingsLoader();
            string savedTheme = settingsLoader.GetSettingAsync(Setting.COLORTHEME).Result?.Value;

            if (savedTheme != null)
            {
                ModernWpf.ThemeManager.Current.ApplicationTheme = ModernWpf.ApplicationTheme.Dark;

                try
                {
                    var color = ColorTranslator.FromHtml(savedTheme);
                    ModernWpf.ThemeManager.Current.AccentColor = System.Windows.Media.Color.FromArgb(color.A, color.R, color.G, color.B);
                }
                catch { }
            }
            else
            {
                ModernWpf.ThemeManager.Current.ApplicationTheme = ModernWpf.ApplicationTheme.Dark;
                ModernWpf.ThemeManager.Current.AccentColor = Colors.SteelBlue;
            }


            var serviceLocator = this.FindResource("ServiceLocator") as ServiceLocator;
            serviceLocator.Initialize();
            
            var mainWindow = new MainWindow();
            mainWindow.FontFamily = new System.Windows.Media.FontFamily("Segoe UI");
            mainWindow.Show();

            this.ShutdownMode = ShutdownMode.OnLastWindowClose;
        }
    }
}
