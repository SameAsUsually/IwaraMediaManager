using IwaraMediaManager.Core.Interfaces;
using IwaraMediaManager.DatabaseManager.Loader;
using IwaraMediaManager.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace IwaraMediaManager.Core.Provider
{
    public class VideoProvider : IVideoProvider
    {
        public event EventHandler<Video> VideoAdded;

        public void NotifyNewVideo(Video e)
        {
            VideoAdded?.Invoke(this, e);
        }

        public async Task<List<Video>> GetVideos()
        {
            var pathSetting = await GetVideoPathSettings();
            var artistLoader = new ArtistLoader();

            if (pathSetting != null)
            {
                List<Artist> artists = artistLoader.GetArtists(pathSetting.Value).Result;
                var videos = artists.SelectMany(x => x.Videos);

                return new List<Video>(videos);
            }

            return null;
        }

        private async Task<Setting> GetVideoPathSettings()
        {
            SettingsLoader settingsLoader = new SettingsLoader();
            var videoPathSetting = await settingsLoader.GetSettingAsync(Setting.VIDEOPATH);

            if (videoPathSetting == null)
                await SetVideoPathDialogAsync();

            return videoPathSetting;
        }

        public static async Task<Setting> SetVideoPathDialogAsync()
        {
            SettingsLoader settingsLoader = new SettingsLoader();
            Setting videoPathSetting = null;

            MessageBox.Show("Please select your video folder.");

            var folderPicker = new Ookii.Dialogs.Wpf.VistaFolderBrowserDialog();
            var result = folderPicker.ShowDialog();

            if (result.HasValue && result.Value)
            {
                videoPathSetting = new Setting(Setting.VIDEOPATH, folderPicker.SelectedPath);
                await settingsLoader.SetSetting(videoPathSetting);
            }

            return videoPathSetting;
        }
    }
}
