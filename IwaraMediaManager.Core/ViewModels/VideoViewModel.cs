using IwaraMediaManager.Core.Base;
using IwaraMediaManager.DatabaseManager;
using IwaraMediaManager.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace IwaraMediaManager.Core.ViewModels
{
    public class VideoViewModel : ViewModelBase
    {
        private const string THUMBNAILFOLDER = "Thumbnails";
        public RelayCommand<string> OpenFolderCommand { get; private set; }

        public int Id { get; set; }
        public string Name { get; set; }
        public Artist Artist { get; set; }
        public string UrlIwara { get; set; }
        
        private bool _IsFavorite;
        public bool IsFavorite 
        {
            get 
            {
                return _IsFavorite;
            } 
            set 
            {
                _IsFavorite = value;
                RaisePropertyChanged();
            } 
        }
        public string FilePath { get; set; }

        private string thumpnailUrl;
        public string ThumbnailUrl 
        { 
            get { return thumpnailUrl; }
            set
            {
                thumpnailUrl = value;
                RaisePropertyChanged();
            } 
        }
        public string DownloadUri { get; set; }

        public DateTime FileCreated { get; set; }

        public string FileCreatedDay => FileCreated.Date.ToString();

        public VideoViewModel(Video video)
        {
            OpenFolderCommand = new RelayCommand<string>(OpenFolder, CanOpenFolder);

            Id = video.Id;
            Name = video.Name;
            Artist = video.Artist;
            UrlIwara = video.UrlIwara;
            IsFavorite = video.IsFavorite;
            FilePath = video.FilePath;
            ThumbnailUrl = video.ThumbnailUrl;
            FileCreated = video.FileCreated;
        }

        private bool CanOpenFolder(string obj)
        {
            return Directory.Exists(Path.GetDirectoryName(FilePath));
        }

        private void OpenFolder(string obj)
        {
            try
            {
                Process.Start("explorer.exe", Path.GetDirectoryName(FilePath));
            }
            catch { }
        }

        private string _ThumbnailBitmap;
        public string ThumbailBitmap
        {
            get
            {
                return _ThumbnailBitmap;
            }
            set
            {
                _ThumbnailBitmap = value;
                RaisePropertyChanged();
            }
        }

        public Task<string> GetThumbnailBitmap()
        {
            var tcs = new TaskCompletionSource<string>();

            var chars = Path.GetInvalidFileNameChars();
            var pathName = Name;
            foreach (var c in chars)
                pathName = pathName.Replace(c, '_');

            var baseDir = System.AppDomain.CurrentDomain.BaseDirectory;

            string thumpnailPath = Path.Combine(baseDir, THUMBNAILFOLDER, pathName + ".jpg");
            Directory.CreateDirectory(Path.Combine(baseDir, THUMBNAILFOLDER));
            if (!File.Exists(thumpnailPath))
            {
                var cmd = "ffmpeg  -itsoffset -4  -i " + '"' + FilePath + '"' + " -vcodec mjpeg -vframes 1 -an -f rawvideo -s 800x450 " + '"' + thumpnailPath + '"';

                var startInfo = new ProcessStartInfo
                {
                    WindowStyle = ProcessWindowStyle.Hidden,
                    FileName = "cmd.exe",
                    Arguments = "/C " + cmd,
                    CreateNoWindow = true,
                };

                var process = new Process { StartInfo = startInfo, EnableRaisingEvents = true };
                process.Exited += (sender, e) => 
                {
                    tcs.SetResult(thumpnailPath);
                    process.Dispose();
                };

                process.Start();
            }
            else
                tcs.SetResult(thumpnailPath);

            return tcs.Task;
        }
    }
}
