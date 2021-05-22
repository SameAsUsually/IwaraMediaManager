using IwaraMediaManager.WPF.IwaraControls;
using System;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;

namespace IwaraMediaManager.Wpf.Views
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private const int WINDOW_MAX_BORDER_THICKNESS = 8;
        private const int TABINDEX_VIDEOSPAGE = 0;
        private const int TABINDEX_IWARAVIEWER = 1;
        private const int TABINDEX_DOWNLOADS = 2;
        private const int TABINDEX_SETTINGS = 3;
        private int oldTabItemIndex;

        private WindowState oldWindowState;

        public MainWindow()
        {
            InitializeComponent();

            this.StateChanged += MainWindow_StateChanged;

            VideoPage.NavigateToVideoPageRequested += VideoPage_NavigateToVideoPageRequested;
            VideoPage.IwaraPlayer.FullscreenRequested += IwaraViewer_FullscreenRequested;
            IwaraViewer.FullscreenRequested += IwaraViewer_FullscreenRequested;
        }

        private void MainWindow_StateChanged(object sender, EventArgs e)
        {
            if (this.WindowState == WindowState.Maximized)
                this.BorderThickness = new Thickness(WINDOW_MAX_BORDER_THICKNESS);
            else if (this.WindowState == WindowState.Normal)
                this.BorderThickness = new Thickness(0);
        }

        private void MainWindow_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
                this.DragMove();
        }

        private void IwaraViewer_FullscreenRequested(object sender, bool e)
        {
            using (Dispatcher.DisableProcessing())
            {
                TabControl.Visibility = Visibility.Collapsed;
                if (e)
                {
                    SetIwaraViewerFullScreenControls(e);
                    SetVideosPageFullScreenControls(e);
                    oldWindowState = this.WindowState;
                    this.WindowState = WindowState.Maximized;
                    this.BorderThickness = new Thickness(0);

                    foreach (var item in TabControl.Items)
                        (item as TabItem).Visibility = Visibility.Collapsed;

                    TabStripHeaderStackPanel.Visibility = Visibility.Collapsed;
                    MenuGrid.Visibility = Visibility.Collapsed;
                }
                else
                {
                    SetIwaraViewerFullScreenControls(e);
                    SetVideosPageFullScreenControls(e);
                    this.WindowState = oldWindowState;

                    if (this.WindowState == WindowState.Maximized)
                        this.BorderThickness = new Thickness(WINDOW_MAX_BORDER_THICKNESS);

                    foreach (var item in TabControl.Items)
                        if (item is TabItem tabItem && tabItem != SettingsTabItem)
                            tabItem.Visibility = Visibility.Visible;

                    TabStripHeaderStackPanel.Visibility = Visibility.Visible;
                    MenuGrid.Visibility = Visibility.Visible;
                }
                TabControl.Visibility = Visibility.Visible;
            }

            ((Storyboard)FindResource("FadeIn")).Begin(this);
        }

        private void SetIwaraViewerFullScreenControls(bool e)
        {
            if (e)
            {
                IwaraViewer.MainGrid.ColumnDefinitions[1].MinWidth = 0;
                IwaraViewer.MainGrid.ColumnDefinitions[1].Width = new GridLength(0);
                IwaraViewer.MainGrid.RowDefinitions[0].Height = new GridLength(0);
            }
            else
            {
                IwaraViewer.MainGrid.ColumnDefinitions[1].MinWidth = 550;
                IwaraViewer.MainGrid.ColumnDefinitions[1].Width = new GridLength(1, GridUnitType.Star);
                IwaraViewer.MainGrid.RowDefinitions[0].Height = new GridLength(40);
            }
        }

        private GridLength oldGridLenght;
        private void SetVideosPageFullScreenControls(bool e)
        {
            if (e)
            {
                oldGridLenght = VideoPage.MainGrid.ColumnDefinitions[0].Width;
                VideoPage.MainGrid.ColumnDefinitions[0].Width = new GridLength(0);
                VideoPage.VideoDetailGrid.Margin = new Thickness(0);
            }
            else
            {
                VideoPage.MainGrid.ColumnDefinitions[0].Width = oldGridLenght;
                VideoPage.VideoDetailGrid.Margin = new Thickness(10);
            }
        }

        private async void VideoPage_NavigateToVideoPageRequested(object sender, string e)
        {
            if (IwaraViewer.IsInitialized && IwaraViewer.IwaraWebView.IsInitialized)
            {
                TabControl.SelectedIndex = 1;
                await IwaraViewer.IwaraWebView.EnsureCoreWebView2Async(null);
                IwaraViewer.IwaraWebView.CoreWebView2.Navigate(e);
            }
        }

        private void SettingsButton_Click(object sender, RoutedEventArgs e)
        {
            var currentlyVisible = TabControl.SelectedIndex == TABINDEX_SETTINGS;

            if (currentlyVisible)
            {
                TabControl.SelectedIndex = oldTabItemIndex;
            }
            else
            {
                oldTabItemIndex = TabControl.SelectedIndex;
                TabControl.SelectedIndex = TABINDEX_SETTINGS;
            }
        }

        private void TabControl_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            foreach (var item in e.AddedItems)
                if (!(item is TabItem))
                    return;

            if (SettingsButton.IsChecked.Value && TabControl.SelectedIndex != TABINDEX_SETTINGS)
                SettingsButton.IsChecked = false;

            var storyboard = ((Storyboard)this.FindResource("FadeIn"));

            if (TabControl.SelectedIndex == TABINDEX_VIDEOSPAGE)
            {
                Storyboard.SetTarget(storyboard, VideoPage);
                LoadNewBackGroundImage(VideoPage.VideoDetailsBackgroundImage, "SettingsPage", 500);
            }
            else if (TabControl.SelectedIndex == TABINDEX_IWARAVIEWER)
            {
                Storyboard.SetTarget(storyboard, IwaraViewer);
                LoadNewBackGroundImage(IwaraViewer.IwaraViewerBackgroundImage, "SettingBackgrounds", 1080);
            }
            else if (TabControl.SelectedIndex == TABINDEX_DOWNLOADS)
            {
                Storyboard.SetTarget(storyboard, DownloadsPage);
                LoadNewBackGroundImage(DownloadsPage.DownloadsBackgroundImage, "SettingBackgrounds", 1080);
            }
            else if (TabControl.SelectedIndex == TABINDEX_SETTINGS)
            {
                Storyboard.SetTarget(storyboard, SettingsPage);
                LoadNewBackGroundImage(SettingsPage.SettingsBackgroundImage, "SettingBackgrounds", 1080);
            }

            storyboard.Begin(this);
        }

        public void LoadNewBackGroundImage(Image image, string settingsFolder, int decodePixelHeight)
        {
            var files = Directory.GetFiles(Path.Combine(Directory.GetCurrentDirectory(), "Images", settingsFolder));
            var index = new Random().Next(0, files.Count() - 1);

            var bitmap = new BitmapImage();
            bitmap.BeginInit();
            bitmap.CacheOption = BitmapCacheOption.OnLoad;
            bitmap.UriSource = new Uri(files[index]);
            bitmap.DecodePixelHeight = decodePixelHeight;
            bitmap.EndInit();

            image.Source = bitmap;
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }

        private void MaximiseButton_Click(object sender, RoutedEventArgs e)
        {
            if (this.WindowState == WindowState.Normal)
                this.WindowState = WindowState.Maximized;
            else
                this.WindowState = WindowState.Normal;
        }

        private void MinimizeButton_Click(object sender, RoutedEventArgs e)
        {
            this.WindowState = WindowState.Minimized;
        }
    }
}
