using IwaraMediaManager.Core.Base;
using IwaraMediaManager.Models;
using System;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Threading.Tasks;
using System.Windows;

namespace IwaraMediaManager.Core.ViewModels
{
    public class DownloadViewModel : ViewModelBase
    {
        private WebClient _DownloadWebClient { get; set; } = new WebClient();
        
        public RelayCommand<string> OpenFolderCommand { get; private set; }

        public event EventHandler<Video> FinishedDownloading;

        public Video Video { get; set; }

        private double _Progress;
        public double Progress
        {
            get { return _Progress; }
            set
            {
                _Progress = value;
                RaisePropertyChanged();
            }
        }

        private double _MB;
        public double MB
        {
            get { return _MB; }
            set
            {
                _MB = value;
                RaisePropertyChanged();
            }
        }

        private double _MaxMB;
        public double MaxMB
        {
            get { return _MaxMB; }
            set
            {
                _MaxMB = value;
                RaisePropertyChanged();
            }
        }

        public string ThumbnailUrl { get; set; }

        public DownloadViewModel()
        {
            this.OpenFolderCommand = new RelayCommand<string>(OpenFolder, CanOpenFolder);
        }

        private bool CanOpenFolder(string obj)
        {
            return Video != null
                && Directory.Exists(Video.Artist.FolderPath);
        }

        private void OpenFolder(string obj)
        {
            try
            {
                Process.Start("explorer.exe", Video.Artist.FolderPath);
            }
            catch { }
        }

        public async Task StartDownload(Video video)
        {
            Video = video;

            _DownloadWebClient.DownloadProgressChanged += new DownloadProgressChangedEventHandler(client_DownloadProgressChanged);
            _DownloadWebClient.DownloadFileCompleted += new AsyncCompletedEventHandler(client_DownloadFileCompleted);

            Directory.CreateDirectory(Video.Artist.FolderPath);

            await _DownloadWebClient.DownloadFileTaskAsync(new Uri(Video.DownloadUri), Video.FilePath);
        }

        private void client_DownloadFileCompleted(object sender, AsyncCompletedEventArgs e)
        {
            if (e.Error == null)
            {
                FinishedDownloading?.Invoke(sender, Video);
            }
            else
            {
                MessageBox.Show(e.Error.Message);
            }
        }

        private void client_DownloadProgressChanged(object sender, DownloadProgressChangedEventArgs e)
        {
            double bytesIn = double.Parse(e.BytesReceived.ToString());
            MB = bytesIn * 0.000001;
            double totalBytes = double.Parse(e.TotalBytesToReceive.ToString());
            MaxMB = totalBytes * 0.000001;
            Progress = bytesIn / totalBytes * 100;
        }
    }
}
