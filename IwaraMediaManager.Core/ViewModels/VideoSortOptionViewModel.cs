using IwaraMediaManager.Core.Base;
using MahApps.Metro.IconPacks;
using System;

namespace IwaraMediaManager.Core.ViewModels
{
    public class VideoSortOptionViewModel : ViewModelBase
    {
        public enum VideoSortOptionKind
        {
            Artist,
            Date,
            Alphabet
        }

        public VideoSortOptionKind VideoSortOption { get; set; }

        private PackIconBoxIconsKind _IconKind;
        public PackIconBoxIconsKind IconKind
        {
            get { return _IconKind; }
            set
            {
                _IconKind = value;
                RaisePropertyChanged();
            }
        }

        private string _Name;
        public string Name
        {
            get { return _Name; }
            set
            {
                _Name = value;
                RaisePropertyChanged();
            }
        }

        private bool _SortedAscending;
        public bool SortedAscending
        {
            get { return _SortedAscending; }
            set
            {
                _SortedAscending = value;
                RaisePropertyChanged();
            }
        }

        public Action<int?> ApplyVideoGroupOption { get; set; }
    }
}
