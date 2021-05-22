using IwaraMediaManager.Core.Base;
using IwaraMediaManager.Core.Interfaces;
using IwaraMediaManager.Core.ViewModels;
using IwaraMediaManager.DatabaseManager.Loader;
using IwaraMediaManager.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace IwaraMediaManager.Core.ViewModels
{
    public class IwaraViewerViewModel : ViewModelBase
    {
        public ObservableCollection<Artist> AuthorAutoSuggestValues { get; set; }
        
        public IDownloadProvider DownloadProvider { get; }
        public IVideoProvider _VideoProvider { get; }
        public RelayCommand<string> ChangeAddressCommand { get; private set; }
        public RelayCommand<Video> DownloadCommand { get; private set; }

        private string _WebViewAdress;
        public string WebViewAdress
        {
            get { return _WebViewAdress; }
            set
            {
                _WebViewAdress = value;
                RaisePropertyChanged();
            }
        }

        private bool _AllowDownload;
        public bool AllowDownload
        {
            get { return _AllowDownload; }
            set 
            { 
                _AllowDownload = value;
                RaisePropertyChanged();
            }
        }


        public IwaraViewerViewModel(IDownloadProvider downloadProvider, IVideoProvider videoProvider)
        {
            ChangeAddressCommand = new RelayCommand<string>(ChangeAddress);
            DownloadCommand = new RelayCommand<Video>(DownloadVideo);

            WebViewAdress = "https://ecchi.iwara.tv/";

            DownloadProvider = downloadProvider;
            _VideoProvider = videoProvider;
        }

        private async void DownloadVideo(Video video)
        {
            if (video == null)
                return;
            
            AllowDownload = false;

            SettingsLoader settingsLoader = new SettingsLoader();
            var videoPathSetting = await settingsLoader.GetSettingAsync(Setting.VIDEOPATH);

            var chars = Path.GetInvalidFileNameChars();

            var artistName = video.Artist.Name;
            foreach (var c in chars)
                artistName = artistName.Replace(c, '_');

            video.Artist.FolderPath = Path.Combine(videoPathSetting.Value, artistName);

            var vidName = video.Name;
            foreach (var c in chars)
                vidName = vidName.Replace(c, '_');

            video.FilePath = Path.Combine(video.Artist.FolderPath, $"{vidName}.mp4");

            DownloadViewModel download = new DownloadViewModel();
            download.FinishedDownloading += Download_VideoFinishedDownloading;
            DownloadProvider.Downloads.Add(download);
            await download.StartDownload(video);
        }

        private async void Download_VideoFinishedDownloading(object sender, Video e)
        {
            SettingsLoader settingsLoader = new SettingsLoader();
            var videoPathSetting = await settingsLoader.GetSettingAsync(Setting.VIDEOPATH);

            e.FileCreated = File.GetCreationTime(e.FilePath);
            e.Artist.Videos.Add(e);

            ArtistLoader artistLoader = new ArtistLoader();
            await artistLoader.SetArtistAsync(e.Artist);

            _VideoProvider.NotifyNewVideo(e);
        }

        private void ChangeAddress(string obj)
        {
            WebViewAdress = obj;
        }
    }
}
