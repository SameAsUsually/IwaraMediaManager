using IwaraMediaManager.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;
using System.Threading.Tasks;

namespace IwaraMediaManager.Core.Interfaces
{
    public interface IVideoProvider
    {
        public Task<List<Video>> GetVideos();

        public event EventHandler<Video> VideoAdded;

        void NotifyNewVideo(Video e);
    }
}
