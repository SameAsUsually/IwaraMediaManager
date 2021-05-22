using IwaraMediaManager.Core.Interfaces;
using IwaraMediaManager.Core.ViewModels;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;

namespace IwaraMediaManager.Core.Provider
{
    public class DownloadProvider : IDownloadProvider
    {
        public ObservableCollection<DownloadViewModel> Downloads { get; set; } = new ObservableCollection<DownloadViewModel>();
    }
}
