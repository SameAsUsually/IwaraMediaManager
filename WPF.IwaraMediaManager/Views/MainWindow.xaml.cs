using IwaraMediaManager.WPF.IwaraControls;
using System;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Interop;
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
        private const int FULLHD_HEIGHT = 1080;
        private const int SMALL_HEIGHT = 500;
        private const string FOLDER_SETTINGSBACKGROUNDS = "SettingBackgrounds";
        private const string FOLDER_SETTINGSPAGE = "SettingsPage";
        private int oldTabItemIndex;

        private WindowState oldWindowState;
        private GridLength oldGridLenght;
        private bool doRestoreIfMove;

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        static extern bool GetCursorPos(out POINT lpPoint);

        [DllImport("user32.dll")]
        private static extern IntPtr MonitorFromWindow(IntPtr handle, uint flags);
        
        [DllImport("user32.dll")]
        private static extern bool GetMonitorInfo(IntPtr hMonitor, ref MONITORINFO lpmi);

        public MainWindow()
        {
            InitializeComponent();

            VideoPage.NavigateToVideoPageRequested += VideoPage_NavigateToVideoPageRequested;
            VideoPage.IwaraPlayer.FullscreenRequested += IwaraViewer_FullscreenRequested;
            IwaraViewer.FullscreenRequested += IwaraViewer_FullscreenRequested;

            MenuGrid.MouseDown += Titlebar_MouseDown;
            MenuGrid.PreviewMouseLeftButtonUp += Titlebar_PreviewMouseLeftButtonUp;
            MenuGrid.PreviewMouseMove += Titlebar_PreviewMouseMove;
        }

        private void Titlebar_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton != MouseButton.Left)
                return;

            // On doubleclick
            if (e.ClickCount == 2)
            {
                // If allowed, switch window state
                if ((ResizeMode == ResizeMode.CanResize) || (ResizeMode == ResizeMode.CanResizeWithGrip))
                {
                    if (this.WindowState == WindowState.Normal)
                        this.WindowState = WindowState.Maximized;
                    else
                        this.WindowState = WindowState.Normal;
                }

                return;
            }
            // Allow restore on move if maximized
            else if (WindowState == WindowState.Maximized)
            {
                doRestoreIfMove = true;
                return;
            }

            this.DragMove();

        }

        private void Titlebar_PreviewMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            // Disable restore on move
            doRestoreIfMove = false;
        }

        private void Titlebar_PreviewMouseMove(object sender, MouseEventArgs e)
        {
            // Only do, if restore on move is allowed
            if (doRestoreIfMove)
            {
                doRestoreIfMove = false;

                // Calculate values
                double percentHorizontal = e.GetPosition(this).X / ActualWidth;
                double targetHorizontal = RestoreBounds.Width * percentHorizontal;

                double percentVertical = e.GetPosition(this).Y / ActualHeight;
                double targetVertical = RestoreBounds.Height * percentVertical;

                // Force set window state
                WindowState = WindowState.Normal;

                // Get current mouse position
                POINT lMousePosition;
                GetCursorPos(out lMousePosition);

                // Set window location relative to current mouse cursor. This simulates the moving of the window
                Left = lMousePosition.X - targetHorizontal;
                Top = lMousePosition.Y - targetVertical;

                DragMove();
            }
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
                    //this.BorderThickness = new Thickness(0);

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

                    //if (this.WindowState == WindowState.Maximized)
                        //this.BorderThickness = new Thickness(WINDOW_MAX_BORDER_THICKNESS);

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
                LoadNewBackGroundImage(VideoPage.VideoDetailsBackgroundImage, FOLDER_SETTINGSPAGE, SMALL_HEIGHT);
            }
            else if (TabControl.SelectedIndex == TABINDEX_IWARAVIEWER)
            {
                Storyboard.SetTarget(storyboard, IwaraViewer);
                LoadNewBackGroundImage(IwaraViewer.IwaraViewerBackgroundImage, FOLDER_SETTINGSBACKGROUNDS, FULLHD_HEIGHT);
            }
            else if (TabControl.SelectedIndex == TABINDEX_DOWNLOADS)
            {
                Storyboard.SetTarget(storyboard, DownloadsPage);
                LoadNewBackGroundImage(DownloadsPage.DownloadsBackgroundImage, FOLDER_SETTINGSBACKGROUNDS, FULLHD_HEIGHT);
            }
            else if (TabControl.SelectedIndex == TABINDEX_SETTINGS)
            {
                Storyboard.SetTarget(storyboard, SettingsPage);
                LoadNewBackGroundImage(SettingsPage.SettingsBackgroundImage, FOLDER_SETTINGSBACKGROUNDS, FULLHD_HEIGHT);
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




        #region maximize logic
        private const int WM_GETMINMAXINFO = 0x0024;
        private const uint MONITOR_DEFAULTTONEAREST = 0x00000002;

        protected override void OnSourceInitialized(EventArgs e)
        {
            base.OnSourceInitialized(e);
            ((HwndSource)PresentationSource.FromVisual(this)).AddHook(HookProc);
        }

        public IntPtr HookProc(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
        {
            if (msg == WM_GETMINMAXINFO)
            {
                // We need to tell the system what our size should be when maximized. Otherwise it will cover the whole screen,
                // including the task bar.
                MINMAXINFO mmi = (MINMAXINFO)Marshal.PtrToStructure(lParam, typeof(MINMAXINFO));

                // Adjust the maximized size and position to fit the work area of the correct monitor
                IntPtr monitor = MonitorFromWindow(hwnd, MONITOR_DEFAULTTONEAREST);

                if (monitor != IntPtr.Zero)
                {
                    MONITORINFO monitorInfo = new MONITORINFO();
                    monitorInfo.cbSize = Marshal.SizeOf(typeof(MONITORINFO));
                    GetMonitorInfo(monitor, ref monitorInfo);
                    RECT rcWorkArea = monitorInfo.rcWork;
                    RECT rcMonitorArea = monitorInfo.rcMonitor;
                    mmi.ptMaxPosition.X = Math.Abs(rcWorkArea.Left - rcMonitorArea.Left);
                    mmi.ptMaxPosition.Y = Math.Abs(rcWorkArea.Top - rcMonitorArea.Top);
                    mmi.ptMaxSize.X = Math.Abs(rcWorkArea.Right - rcWorkArea.Left);
                    mmi.ptMaxSize.Y = Math.Abs(rcWorkArea.Bottom - rcWorkArea.Top);
                }

                Marshal.StructureToPtr(mmi, lParam, true);
            }
             
            return IntPtr.Zero;
        }

        [Serializable]
        [StructLayout(LayoutKind.Sequential)]
        public struct RECT
        {
            public int Left;
            public int Top;
            public int Right;
            public int Bottom;

            public RECT(int left, int top, int right, int bottom)
            {
                this.Left = left;
                this.Top = top;
                this.Right = right;
                this.Bottom = bottom;
            }
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct MONITORINFO
        {
            public int cbSize;
            public RECT rcMonitor;
            public RECT rcWork;
            public uint dwFlags;
        }

        [Serializable]
        [StructLayout(LayoutKind.Sequential)]
        public struct POINT
        {
            public int X;
            public int Y;

            public POINT(int x, int y)
            {
                this.X = x;
                this.Y = y;
            }
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct MINMAXINFO
        {
            public POINT ptReserved;
            public POINT ptMaxSize;
            public POINT ptMaxPosition;
            public POINT ptMinTrackSize;
            public POINT ptMaxTrackSize;
        }
        #endregion
    }
}
