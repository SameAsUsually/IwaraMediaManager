using IwaraMediaManager.Core.Base;
using IwaraMediaManager.Core.Interfaces;
using IwaraMediaManager.Core.ViewModels;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;

namespace IwaraMediaManager.Core.ViewModels
{
    public class DownloadsViewModel : ViewModelBase
    {
        public IDownloadProvider DownloadProvider { get; }
        
        public DownloadsViewModel(IDownloadProvider downloadProvider)
        {
            DownloadProvider = downloadProvider;
        }
    }
}
