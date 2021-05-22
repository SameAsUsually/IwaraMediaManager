using IwaraMediaManager.Core.ViewModels;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;

namespace IwaraMediaManager.Core.Interfaces
{
    public interface IDownloadProvider
    {
        ObservableCollection<DownloadViewModel> Downloads { get; set; }
    }
}
