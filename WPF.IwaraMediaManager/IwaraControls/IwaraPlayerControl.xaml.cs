using IwaraMediaManager.Core.Base;
using IwaraMediaManager.Core.ViewModels;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using System.Timers;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace IwaraMediaManager.WPF.IwaraControls
{
    /// <summary>
    /// Interaction logic for IwaraPlayerControl.xaml
    /// </summary>
    public partial class IwaraPlayerControl : UserControl
    {
        public RelayCommand<object> NextVideoCommand
        {
            get { return (RelayCommand<object>)GetValue(NextVideoCommandProperty); }
            set { SetValue(NextVideoCommandProperty, value); }
        }

        public static readonly DependencyProperty NextVideoCommandProperty =
            DependencyProperty.Register("NextVideoCommand", typeof(RelayCommand<object>), typeof(IwaraPlayerControl), new PropertyMetadata(null));

        public RelayCommand<object> PreviousVideoCommand
        {
            get { return (RelayCommand<object>)GetValue(PreviousVideoCommandProperty); }
            set { SetValue(PreviousVideoCommandProperty, value); }
        }

        public static readonly DependencyProperty PreviousVideoCommandProperty =
            DependencyProperty.Register("PreviousVideoCommand", typeof(RelayCommand<object>), typeof(IwaraPlayerControl), new PropertyMetadata(null));

        public event EventHandler<bool> FullscreenRequested;

        public bool AutoPlayMedia { get; set; }

        private DispatcherTimer _VideoProgressTimer;
        private Timer _VolumeDisappearTimer = new Timer(500);
        private Timer _ShowPlayerControlsTimer = new Timer(1000);
        private bool _Dragging = false;
        private bool _IsFullscreen = false;

        private MediaState GetMediaState(MediaElement myMedia)
        {
            FieldInfo hlp = typeof(MediaElement).GetField("_helper", BindingFlags.NonPublic | BindingFlags.Instance);
            object helperObject = hlp.GetValue(myMedia);
            FieldInfo stateField = helperObject.GetType().GetField("_currentState", BindingFlags.NonPublic | BindingFlags.Instance);
            MediaState state = (MediaState)stateField.GetValue(helperObject);
            return state;
        }

        public VideoViewModel SelectedVideoViewModel
        {
            get { return (VideoViewModel)GetValue(SelectedVideoViewModelProperty); }
            set { SetValue(SelectedVideoViewModelProperty, value); }
        }

        public static readonly DependencyProperty SelectedVideoViewModelProperty =
            DependencyProperty.Register("SelectedVideoViewModel", typeof(VideoViewModel), typeof(IwaraPlayerControl), new PropertyMetadata(null));

        public IwaraPlayerControl()
        {
            InitializeComponent();
            IwaraMediaElement.Volume = 0.01;

            _VideoProgressTimer = new DispatcherTimer();
            _VideoProgressTimer.Interval = TimeSpan.FromMilliseconds(600);
            _VideoProgressTimer.Tick += new EventHandler(ProgressTimer_Tick);
            _VolumeDisappearTimer.Elapsed += VolumeDisappearTimer_Elapsed;

            _ShowPlayerControlsTimer.Elapsed += (sender, e) =>
            {
                Dispatcher.Invoke(() => PlayerControlsGrid.Visibility = Visibility.Hidden);
                _ShowPlayerControlsTimer.Stop();
            };
        }

        private void ProgressTimer_Tick(object sender, EventArgs e)
        {
            if (!_Dragging)
                slProgress.Value = IwaraMediaElement.Position.TotalSeconds;
        }

        private void btPlay_Click(object sender, RoutedEventArgs e)
        {
            var mediaState = GetMediaState(IwaraMediaElement);

            if (mediaState == MediaState.Play)
            {
                PlayIcon.Kind = MahApps.Metro.IconPacks.PackIconBoxIconsKind.RegularPlay;
                IwaraMediaElement.Pause();
            }
            else
            {
                PlayIcon.Kind = MahApps.Metro.IconPacks.PackIconBoxIconsKind.RegularPause;
                IwaraMediaElement.Play();
            }
        }

        private void PlayerBorder_Click(object sender, MouseButtonEventArgs e)
        {
            btPlay_Click(sender, null);
        }

        private void tbtVolume_Click(object sender, RoutedEventArgs e)
        {
            slVolume.Visibility = Visibility.Visible;
        }

        private void tbtVolume_MouseEnter(object sender, MouseEventArgs e)
        {
            slVolume.Visibility = Visibility.Visible;

            if (_VolumeDisappearTimer.Enabled)
                _VolumeDisappearTimer.Stop();
        }

        private void MouseLeave_FadeOut(object sender, MouseEventArgs e)
        {
            _VolumeDisappearTimer.Start();
        }

        private void MouseEnter_StopFadeOut(object sender, MouseEventArgs e)
        {
            _VolumeDisappearTimer.Stop();
        }

        private void VolumeDisappearTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            var timer = sender as Timer;
            timer.Stop();
            Dispatcher.Invoke(() => { slVolume.Visibility = Visibility.Collapsed; });
        }

        private void FullscreenButton_Click(object sender, RoutedEventArgs e)
        {
            _IsFullscreen = !_IsFullscreen;
            FullscreenRequested?.Invoke(this, _IsFullscreen);
        }

        private void Grid_MouseMove(object sender, MouseEventArgs e)
        {
            if (_ShowPlayerControlsTimer.Enabled)
                _ShowPlayerControlsTimer.Stop();

            PlayerControlsGrid.Visibility = Visibility.Visible;

            _ShowPlayerControlsTimer.Start();
        }

        private void slProgress_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            IwaraMediaElement.Position = TimeSpan.FromSeconds(slProgress.Value);
        }

        private void slProgress_PreviewMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            IwaraMediaElement.Position = TimeSpan.FromSeconds(slProgress.Value);

            _VideoProgressTimer.Start();
            _Dragging = false;
        }

        private void slProgress_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            _VideoProgressTimer.Stop();
            _Dragging = true;
        }

        private void IwaraMediaElement_MediaOpenened(object sender, RoutedEventArgs e)
        {
            if (IwaraMediaElement.NaturalDuration.HasTimeSpan)
            {
                var ts = IwaraMediaElement.NaturalDuration.TimeSpan;
                slProgress.Maximum = ts.TotalSeconds;
                slProgress.SmallChange = 1;
                slProgress.LargeChange = Math.Min(10, ts.Seconds / 10);

                _VideoProgressTimer.Start();
            }
        }

        private void IwaraMediaElement_Loaded(object sender, RoutedEventArgs e)
        {
            if (AutoPlayMedia)
                IwaraMediaElement.Play();
        }

        private void IwaraMediaElement_MediaEnded(object sender, RoutedEventArgs e)
        {
            if (btAutoNextVideoEnabled.IsChecked.GetValueOrDefault() && NextVideoCommand != null)
                NextVideoCommand.Execute(null);
        }
    }
}
