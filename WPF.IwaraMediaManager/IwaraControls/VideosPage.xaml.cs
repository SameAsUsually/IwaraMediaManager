using IwaraMediaManager.Core.ViewModels;
using IwaraMediaManager.WPF.Views;
using System;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Timers;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;

namespace IwaraMediaManager.WPF.IwaraControls
{
    /// <summary>
    /// Interaction logic for VideosPage.xaml
    /// </summary>
    public partial class VideosPage
    {
        public event EventHandler<string> NavigateToVideoPageRequested;

        public VideosPage()
        {
            Initialized += VideosPage_Initialized;
            InitializeComponent();
        }

        private async void VideosPage_Initialized(object sender, EventArgs e)
        {
            var vm = DataContext as VideoPageViewModel;
            vm.RequestScrollToFirstItem += VideoList.ScrollToSelectedItem;
            await vm.InitializeAsync();
        }

        private void Button_GoToVideoUrl(object sender, RoutedEventArgs e)
        {
            var vm = DataContext as VideoPageViewModel;
            NavigateToVideoPageRequested?.Invoke(this, vm.SelectedVideo.UrlIwara);
        }

        private void Button_GoToArtistPage(object sender, RoutedEventArgs e)
        {
            var artist = (VideoList.VideoListBox.SelectedItem as VideoViewModel).Artist;
            var url = $"https://ecchi.iwara.tv/users/{artist.Name.Replace("_", string.Empty).Replace(Environment.NewLine, "-")}";
            NavigateToVideoPageRequested?.Invoke(this, url);
        }

        public VideoPlayerWindow VideoPlayerWindow;

        private void OpenVideo(object sender, MouseButtonEventArgs e)
        {
            if (VideoPlayerWindow == null)
            {
                VideoPlayerWindow = new VideoPlayerWindow();
                VideoPlayerWindow.Owner = Application.Current.MainWindow;
                //VideoPlayerWindow.ShowTitleBar = false;
                VideoPlayerWindow.Closed += (x, y) =>
                {
                    VideoPlayerWindow = null;
                    Application.Current.MainWindow.Show();
                };

                VideoPlayerWindow.Show();
                Application.Current.MainWindow.Hide();
            }
        }

        private void ButtonClick_OpenVideo(object sender, RoutedEventArgs e)
        {
            OpenVideo(this, null);
        }

        private void VideoSearchAutoSuggestBox_TextChanged(ModernWpf.Controls.AutoSuggestBox sender, ModernWpf.Controls.AutoSuggestBoxTextChangedEventArgs args)
        {
            if (args.Reason == ModernWpf.Controls.AutoSuggestionBoxTextChangeReason.UserInput)
            {
                var vm = this.DataContext as VideoPageViewModel;
                vm.AutoSuggestTextInputCommand.Execute(sender.Text);
            }
        }

        private void VideoSearchAutoSuggestBox_QuerySubmitted(ModernWpf.Controls.AutoSuggestBox sender, ModernWpf.Controls.AutoSuggestBoxQuerySubmittedEventArgs args)
        {
            var vm = this.DataContext as VideoPageViewModel;
            vm.AutoSuggestSubmitCommand.Execute(args.QueryText);
        }

        private void VideoSearchAutoSuggestBox_LostFocus(object sender, RoutedEventArgs e)
        {
            var vm = this.DataContext as VideoPageViewModel;
            vm.AutoSuggestValues.Clear();
        }
    }
}
