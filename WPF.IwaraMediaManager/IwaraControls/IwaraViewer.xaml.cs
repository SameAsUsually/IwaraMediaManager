using IwaraMediaManager.DatabaseManager.Loader;
using IwaraMediaManager.Models;
using Microsoft.Web.WebView2.Core;
using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using System.Web;
using System.Windows;
using System.Windows.Controls;

namespace IwaraMediaManager.WPF.IwaraControls
{
    /// <summary>
    /// Interaction logic for IwaraViewer.xaml
    /// </summary>
    public partial class IwaraViewer
    {
        public event EventHandler<bool> FullscreenRequested;
        private bool _IsFullscreen = false;
        private string httpCss;

        public Video CurrentVideo
        {
            get { return (Video)GetValue(CurrentVideoProperty); }
            set { SetValue(CurrentVideoProperty, value); }
        }

        // Using a DependencyProperty as the backing store for CurrentVideo.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty CurrentVideoProperty =
            DependencyProperty.Register("CurrentVideo", typeof(Video), typeof(IwaraViewer), new PropertyMetadata(null));

        public IwaraViewer()
        {
            InitializeComponent();
            InitializeAsync();
        }

        async void InitializeAsync()
        {
            string cssFilePath = Path.Combine(Directory.GetCurrentDirectory(), "IwaraCSS.css");
            string css = await File.ReadAllTextAsync(cssFilePath);
            httpCss = HttpUtility.JavaScriptStringEncode(css);

            await IwaraWebView.EnsureCoreWebView2Async(null);

            IwaraWebView.NavigationCompleted += IwaraWebView_NavigationCompletedSetHeight;

            await IwaraWebView.CoreWebView2.AddScriptToExecuteOnDocumentCreatedAsync(
@"document.addEventListener('DOMContentLoaded', function() { 
    var styleSheet = document.createElement('style');
    styleSheet.type = 'text/css';
    styleSheet.innerText = '" + httpCss + @"';
    document.head.appendChild(styleSheet);
})");
            
            //IwaraWebView.CoreWebView2.NavigationCompleted += (object y, CoreWebView2NavigationCompletedEventArgs x) => { IwaraWebView.Opacity = 1; };
            IwaraWebView.CoreWebView2.ContainsFullScreenElementChanged += CoreWebView2_ContainsFullScreenElementChanged;
            IwaraWebView.BringIntoView();

            string script = 
@"if (window.location.href.startsWith('https://ecchi.iwara.tv/user/login')) 
{
	document.getElementById('edit-name').value = globalThis.userAccount;
    document.getElementById('edit-pass').value = globalThis.userPassword;
};";

            await IwaraWebView.CoreWebView2.AddScriptToExecuteOnDocumentCreatedAsync(@"document.addEventListener('DOMContentLoaded', function() { " + script + @"})");
        }

        private void IwaraWebView_NavigationCompletedSetHeight(object sender, CoreWebView2NavigationCompletedEventArgs e)
        {
            IwaraWebView.Height = double.NaN;
            IwaraWebView.NavigationCompleted -= IwaraWebView_NavigationCompletedSetHeight;
        }

        private void CoreWebView2_ContainsFullScreenElementChanged(object sender, object e)
        {
            _IsFullscreen = !_IsFullscreen;
            FullscreenRequested?.Invoke(WebvViewBorder, _IsFullscreen);
        }

        private async void IwaraWebView_ContentLoading(object sender, Microsoft.Web.WebView2.Core.CoreWebView2ContentLoadingEventArgs e)
        {
            btDownload.IsEnabled = false;
            await SetVideo();

            var settingsLoader = new SettingsLoader();
            var account = await settingsLoader.GetSettingAsync(Setting.ACCOUNT_NAME);
            var password = await settingsLoader.GetSettingAsync(Setting.ACCOUNT_PASSWORD);

            if (account != null)
                await IwaraWebView.CoreWebView2.ExecuteScriptAsync($"globalThis.userAccount = '{account.Value}';");
            if (password != null)
                await IwaraWebView.CoreWebView2.ExecuteScriptAsync($"globalThis.userPassword = '{password.Value}';");
        }


        private async Task SetVideo()
        {
            if (IwaraWebView.CoreWebView2.Source.StartsWith("https://ecchi.iwara.tv/videos/", StringComparison.OrdinalIgnoreCase))
            {
                string downloadPath = null;
                do
                {
                    downloadPath = await IwaraWebView.CoreWebView2.ExecuteScriptAsync("document.getElementById('download-options').querySelector('a').href");
                    downloadPath = downloadPath.Trim('"');
                } while (downloadPath == null || downloadPath == "null");

                var videoTitle = await IwaraWebView.CoreWebView2.ExecuteScriptAsync("document.getElementsByClassName('title')[0].innerText");
                videoTitle = videoTitle.Trim('"');
                var artistName = await IwaraWebView.CoreWebView2.ExecuteScriptAsync("document.getElementsByClassName('username')[0].innerText");
                artistName = artistName.Trim('"');

                var thumpnailPath = await IwaraWebView.CoreWebView2.ExecuteScriptAsync("document.getElementById('video-player').getAttribute('poster')");
                thumpnailPath = thumpnailPath.Trim('"');
                thumpnailPath = $"https:{thumpnailPath}";

                var artist = await new ArtistLoader().GetDbArtistByName(artistName);

                if (artist == null)
                    artist = new Artist() { Name = artistName };

                CurrentVideo = new Video() { Artist = artist, Name = videoTitle, UrlIwara = IwaraWebView.CoreWebView2.Source, DownloadUri = downloadPath, ThumbnailUrl = thumpnailPath };

                btDownload.IsEnabled = true;
            }
            else
                CurrentVideo = new Video() { Name = "Navigation Page" };
        }

        private void btBack_Click(object sender, RoutedEventArgs e)
        {
            if (IwaraWebView.CanGoBack)
                IwaraWebView.GoBack();
        }

        private void btForward_Click(object sender, RoutedEventArgs e)
        {
            if (IwaraWebView.CanGoForward)
                IwaraWebView.GoForward();
        }

        private void btCopy_Click(object sender, RoutedEventArgs e)
        {
            Clipboard.SetText(IwaraWebView.CoreWebView2.Source);
        }
        
        private void tbAddress_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == System.Windows.Input.Key.Return && tbAddress.Text.Contains("iwara.tv"))
            {
                IwaraWebView.CoreWebView2.Navigate(tbAddress.Text);
            }
        }
    }
}
