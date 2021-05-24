using MahApps.Metro.Controls;
using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace IwaraMediaManager.WPF.Views
{
    /// <summary>
    /// Interaction logic for VideoPlayerWindow.xaml
    /// </summary>
    public partial class VideoPlayerWindow : Window
    {
        private RotateTransform openVideoListButtonTransform;
        private WindowState nonFullscreenWindowState;

        public VideoPlayerWindow()
        {
            InitializeComponent();
            this.MouseDown += VideoPlayerWindow_MouseDown;
            IwaraPlayer.FullscreenRequested += IwaraPlayer_FullscreenRequested;

            openVideoListButtonTransform = new RotateTransform();
            OpenFlyoutButton.RenderTransform = openVideoListButtonTransform;
            OpenFlyoutButton.RenderTransformOrigin = new Point(.5, .5);
        }

        private void VideoPlayerWindow_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
                this.DragMove();
        }

        private void IwaraPlayer_FullscreenRequested(object sender, bool e)
        {
            if (this.WindowState == WindowState.Maximized)
            {
                this.WindowState = nonFullscreenWindowState;
                this.WindowStyle = WindowStyle.ToolWindow;

            }
            else
            {
                nonFullscreenWindowState = this.WindowState;
                this.WindowStyle = WindowStyle.None;
                this.WindowState = WindowState.Maximized;
            }
        }

        private void OpenFlyoutButton_Click(object sender, RoutedEventArgs e)
        {
            VideosFlyout.IsOpen = !VideosFlyout.IsOpen;

            DoubleAnimation anim3 = new DoubleAnimation(VideosFlyout.IsOpen ? 0 : 180, VideosFlyout.IsOpen ? 180 : 360, TimeSpan.FromSeconds(.2));
            openVideoListButtonTransform.BeginAnimation(RotateTransform.AngleProperty, anim3);
        }

        private void CloseWindowButton_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}
