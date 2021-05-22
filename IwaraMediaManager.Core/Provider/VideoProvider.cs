using IwaraMediaManager.Core.Interfaces;
using IwaraMediaManager.DatabaseManager.Loader;
using IwaraMediaManager.Models;
using Microsoft.WindowsAPICodePack.Dialogs;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Forms;

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

            return new List<Video>();
        }

        private async Task<Setting> GetVideoPathSettings()
        {
            SettingsLoader settingsLoader = new SettingsLoader();
            var videoPathSetting = await settingsLoader.GetSettingAsync(Setting.VIDEOPATH);

            if (videoPathSetting == null)
            {
                var msgBox = System.Windows.MessageBox.Show("First time starting Iwara Media Manager", "Please select a video folder.");
                
                videoPathSetting = await SetVideoPathDialogAsync();
            }

            return videoPathSetting;
        }

        public static async Task<Setting> SetVideoPathDialogAsync()
        {
            SettingsLoader settingsLoader = new SettingsLoader();
            Setting videoPathSetting = null;

            using (var dialog = new CommonOpenFileDialog
            {
                IsFolderPicker = true
            })
            {
                CommonFileDialogResult result = dialog.ShowDialog();

                if (result == CommonFileDialogResult.Ok)
                {
                    videoPathSetting = new Setting(Setting.VIDEOPATH, dialog.FileName);
                    await settingsLoader.SetSettingAsync(videoPathSetting);
                }
            }

            return videoPathSetting;
        }
    }
}
