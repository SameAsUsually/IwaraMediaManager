using IwaraMediaManager.Core.ViewModels;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace IwaraMediaManager.WPF.IwaraControls
{
    /// <summary>
    /// Interaction logic for VideoListControl.xaml
    /// </summary>
    public partial class VideoListControl : UserControl
    {
        public ICollectionView VideoCollectionView
        {
            get { return (ICollectionView)GetValue(VideoCollectionViewProperty); }
            set { SetValue(VideoCollectionViewProperty, value); }
        }

        public static readonly DependencyProperty VideoCollectionViewProperty =
            DependencyProperty.Register("VideoCollectionView", typeof(ICollectionView), typeof(VideoListControl), new PropertyMetadata(null));

        public VideoViewModel SelectedVideo
        {
            get { return (VideoViewModel)GetValue(SelectedVideoProperty); }
            set { SetValue(SelectedVideoProperty, value); }
        }

        public static readonly DependencyProperty SelectedVideoProperty =
            DependencyProperty.Register("SelectedVideo", typeof(VideoViewModel), typeof(VideoListControl), new PropertyMetadata(null));

        public int ThumpnailWidth
        {
            get { return (int)GetValue(ThumpnailWidthProperty); }
            set { SetValue(ThumpnailWidthProperty, value); }
        }

        public static readonly DependencyProperty ThumpnailWidthProperty =
            DependencyProperty.Register("ThumpnailWidth", typeof(int), typeof(VideoListControl), new PropertyMetadata(null));


        public VideoListControl()
        {
            InitializeComponent();
        }

        public void ScrollToSelectedItem(object sender, EventArgs e)
        {
            VideoListBox.UpdateLayout();
            VideoListBox.ScrollIntoView(VideoListBox.SelectedItem);
        }

        private void VideoListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ScrollToSelectedItem(null, null);
        }
    }
}
