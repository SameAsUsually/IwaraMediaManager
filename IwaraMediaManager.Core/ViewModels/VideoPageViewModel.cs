using IwaraMediaManager.Core.Base;
using IwaraMediaManager.Core.Interfaces;
using IwaraMediaManager.DatabaseManager;
using IwaraMediaManager.DatabaseManager.Loader;
using IwaraMediaManager.Models;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;
using System.Windows.Threading;
using System.Xml.Linq;

namespace IwaraMediaManager.Core.ViewModels
{
    public class VideoPageViewModel : ViewModelBase
    {
        public event EventHandler RequestScrollToFirstItem;

        private IVideoProvider _VideoProvider;
        public int VideosPerPage = 50;

        public RelayCommand<int?> RefreshVideosCommand { get; private set; }
        public RelayCommand<int?> NextPageCommand { get; private set; }
        public RelayCommand<int?> LastPageCommand { get; private set; }
        public RelayCommand<int?> SortAlphabeticlyCommand { get; private set; }
        public RelayCommand<int?> SortAfterDateCommand { get; private set; }
        public RelayCommand<int?> ToggleSortOrderCommand { get; private set; }
        public RelayCommand<object> NextVideoCommand { get; private set; }
        public RelayCommand<object> PreviousVideoCommand { get; private set; }
        public RelayCommand<object> AutoSuggestSubmitCommand { get; private set; }
        public RelayCommand<string> AutoSuggestTextInputCommand { get; private set; }
        public RelayCommand<string> FilterFavoritesCommand { get; private set; }
        public RelayCommand<object> SetVideoFavoriteCommand { get; private set; }

        public ICollectionView VideoCollectionView { get; set; }
        public ConcurrentBag<VideoViewModel> Videos { get; set; } = new ConcurrentBag<VideoViewModel>();
        public List<VideoViewModel> FilteredVideos { get; set; } = new List<VideoViewModel>();
        public ObservableCollection<VideoViewModel> CurrentPageVideos { get; set; } = new ObservableCollection<VideoViewModel>();
        public ObservableCollection<string> AutoSuggestValues { get; set; } = new ObservableCollection<string>();
        public ObservableCollection<VideoSortOptionViewModel> VideoSortOptions { get; set; } = new ObservableCollection<VideoSortOptionViewModel>();

        private int _thumpnailSize = 320;
        public int ThumpnailSize
        {
            get { return _thumpnailSize; }
            set
            {
                _thumpnailSize = value;
                RaisePropertyChanged();
            }
        }

        private int _CurrentPage;
        public int CurrentPage
        {
            get { return _CurrentPage; }
            set
            {
                _CurrentPage = value;
                RaisePropertyChanged();
            }
        }

        private int _MaxPage;
        public int MaxPage
        {
            get { return _MaxPage; }
            set
            {
                _MaxPage = value;
                RaisePropertyChanged();
            }
        }


        private VideoViewModel _SelectedVideo;
        public VideoViewModel SelectedVideo
        {
            get { return _SelectedVideo; }
            set
            {
                _SelectedVideo = value;
                RaisePropertyChanged();
            }
        }

        private VideoSortOptionViewModel _SelectedVideoSortOption;
        public VideoSortOptionViewModel SelectedVideoSortOption
        {
            get { return _SelectedVideoSortOption; }
            set
            {
                _SelectedVideoSortOption = value;
                RaisePropertyChanged();

                value.ApplyVideoGroupOption(null);
            }
        }

        private string _AutoSuggestBoxInput;
        public string AutoSuggestBoxInput
        {
            get { return _AutoSuggestBoxInput; }
            set 
            { 
                _AutoSuggestBoxInput = value;
                RaisePropertyChanged();
            }
        }

        private bool _FavoriteFilterOn;
        public bool FavoriteFilterOn
        {
            get { return _FavoriteFilterOn; }
            set 
            { 
                _FavoriteFilterOn = value;
                RaisePropertyChanged();

                AutoSuggestBoxInput = "";
                FilterVideos(null);
            }
        }

        public VideoPageViewModel(IVideoProvider videoProvider)
        {
            _VideoProvider = videoProvider;
            videoProvider.VideoAdded += VideoProvider_VideoAdded;

            CurrentPage = 0;
            MaxPage = 0;

            VideoSortOptions.Add(new VideoSortOptionViewModel
            {
                IconKind = MahApps.Metro.IconPacks.PackIconBoxIconsKind.RegularUser,
                Name = "Artist",
                VideoSortOption = VideoSortOptionViewModel.VideoSortOptionKind.Artist,
                SortedAscending = true,
                ApplyVideoGroupOption = GroupAfterArtist
            });
            VideoSortOptions.Add(new VideoSortOptionViewModel
            {
                IconKind = MahApps.Metro.IconPacks.PackIconBoxIconsKind.RegularCalendar,
                Name = "Date",
                VideoSortOption = VideoSortOptionViewModel.VideoSortOptionKind.Date,
                ApplyVideoGroupOption = GroupAfterDate
            });
            VideoSortOptions.Add(new VideoSortOptionViewModel
            {
                IconKind = MahApps.Metro.IconPacks.PackIconBoxIconsKind.RegularSortAZ,
                Name = "Alphabet",
                VideoSortOption = VideoSortOptionViewModel.VideoSortOptionKind.Alphabet,
                ApplyVideoGroupOption = GroupAfterAlphabet
            });
            SelectedVideoSortOption = VideoSortOptions[0];

            RefreshVideosCommand = new RelayCommand<int?>(RefreshVideos);
            NextPageCommand = new RelayCommand<int?>(NextPage, CanGoToNextPage);
            LastPageCommand = new RelayCommand<int?>(LastPage, CanGoToLastPage);
            SortAfterDateCommand = new RelayCommand<int?>(GroupAfterDate);
            ToggleSortOrderCommand = new RelayCommand<int?>(ToggleSortOrder);
            NextVideoCommand = new RelayCommand<object>(NextVideo);
            PreviousVideoCommand = new RelayCommand<object>(PreviousVideo);
            AutoSuggestSubmitCommand = new RelayCommand<object>(FilterVideos);
            AutoSuggestTextInputCommand = new RelayCommand<string>(AutoSuggestTextInput);
            SetVideoFavoriteCommand = new RelayCommand<object>(SaveIsFavoriteVideo);

            SelectedVideo = new VideoViewModel(new Video());

            VideoCollectionView = CollectionViewSource.GetDefaultView(CurrentPageVideos);
        }

        private void ToggleSortOrder(int? obj)
        {
            SelectedVideoSortOption.SortedAscending = !SelectedVideoSortOption.SortedAscending;
            ApplySortOrder();
        }

        private void ApplySortOrder()
        {
            switch (SelectedVideoSortOption.VideoSortOption)
            {
                case VideoSortOptionViewModel.VideoSortOptionKind.Artist:
                    if (SelectedVideoSortOption.SortedAscending)
                        FilteredVideos = FilteredVideos.OrderBy(x => x.Artist.Name).ThenBy(x => x.Name).ToList();
                    else
                        FilteredVideos = FilteredVideos.OrderByDescending(x => x.Artist.Name).ThenByDescending(x => x.Name).ToList();

                    VideoCollectionView.SortDescriptions.Clear();
                    VideoCollectionView.SortDescriptions.Add(new SortDescription("Artist.Name", SelectedVideoSortOption.SortedAscending ? ListSortDirection.Ascending : ListSortDirection.Descending));
                    VideoCollectionView.SortDescriptions.Add(new SortDescription("Name", SelectedVideoSortOption.SortedAscending ? ListSortDirection.Ascending : ListSortDirection.Descending));
                    break;
                case VideoSortOptionViewModel.VideoSortOptionKind.Date:
                    if (SelectedVideoSortOption.SortedAscending)
                        FilteredVideos = FilteredVideos.OrderBy(x => x.FileCreated).ToList();
                    else
                        FilteredVideos = FilteredVideos.OrderByDescending(x => x.FileCreated).ToList();

                    VideoCollectionView.SortDescriptions.Clear();
                    VideoCollectionView.SortDescriptions.Add(new SortDescription("FileCreated", SelectedVideoSortOption.SortedAscending ? ListSortDirection.Ascending : ListSortDirection.Descending));
                    break;
                case VideoSortOptionViewModel.VideoSortOptionKind.Alphabet:
                    if (SelectedVideoSortOption.SortedAscending)
                        FilteredVideos = FilteredVideos.OrderBy(x => x.Name).ToList();
                    else
                        FilteredVideos = FilteredVideos.OrderByDescending(x => x.Name).ToList();

                    VideoCollectionView.SortDescriptions.Clear();
                    VideoCollectionView.SortDescriptions.Add(new SortDescription("Name", SelectedVideoSortOption.SortedAscending ? ListSortDirection.Ascending : ListSortDirection.Descending));
                    break;
                default:
                    break;

            }

            SetCurrentPage();
        }

        private void AutoSuggestTextInput(object o)
        {
            var input = o as string;
            if (!String.IsNullOrWhiteSpace(input))
            {
                var fitleredVideosSuggestions = Videos.Where(x => x.Artist.Name.ToLower().Contains(input.ToLower())).Select(x => x.Artist.Name);
                fitleredVideosSuggestions.Concat(Videos.Where(x => x.Name.ToLower().Contains(input.ToLower())).Select(x => x.Name));
                fitleredVideosSuggestions = fitleredVideosSuggestions.Distinct();
                AutoSuggestValues.Clear();

                foreach (var suggestion in fitleredVideosSuggestions)
                    AutoSuggestValues.Add(suggestion);
            }
            else
            {
                AutoSuggestValues.Clear();
                AutoSuggestValues.Add("No results found.");
            }
        }

        private void FilterVideos(object o)
        {
            var str = AutoSuggestBoxInput;
            bool filterAfterText = !string.IsNullOrWhiteSpace(str);
            
            if (filterAfterText)
            {
                FilteredVideos = Videos.Where(x => x.Artist.Name.ToLower().Contains(str) || x.Name.ToLower().Contains(str)).ToList();
                AutoSuggestValues.Clear();
            }
            
            if (FavoriteFilterOn)
                FilteredVideos = FilteredVideos.Where(x => x.IsFavorite).ToList();

            if (!filterAfterText && !FavoriteFilterOn)
                FilteredVideos = Videos.ToList();

            ApplySortOrder();
        }

        private void SaveIsFavoriteVideo(object obj)
        {
            using (var db = new DataBaseContext())
            {
                var vid = db.Videos.FirstOrDefault(x => x.Name == SelectedVideo.Name && x.Artist.Name == SelectedVideo.Artist.Name);

                if (vid != null)
                    vid.IsFavorite = SelectedVideo.IsFavorite;

                db.SaveChanges();
            }
        }

        private async void RefreshVideos(int? obj)
        {
            var settingsLoader = new SettingsLoader();
            var settingsVideosPerPage = await settingsLoader.GetSettingAsync(Setting.MAXVIDEOSPERPAGE);
            var settingsThumpnailsize = await settingsLoader.GetSettingAsync(Setting.THUMPNAILSIZE);

            if (settingsVideosPerPage != null)
            {
                VideosPerPage = int.Parse(settingsVideosPerPage.Value);

                int maxPage = FilteredVideos.Count / VideosPerPage;

                if (CurrentPage > maxPage)
                    CurrentPage = maxPage;
            }

            if (settingsThumpnailsize != null)
            {
                ThumpnailSize = int.Parse(settingsThumpnailsize.Value);
            }

            SetCurrentPage();
        }

        private void PreviousVideo(object obj)
        {
            if (obj is int tabIndex && tabIndex != 0)
                return;

            VideoCollectionView.MoveCurrentToPrevious();

            if (VideoCollectionView.IsCurrentBeforeFirst)
                VideoCollectionView.MoveCurrentToLast();
        }

        private void NextVideo(object obj)
        {
            if (obj is int tabIndex && tabIndex != 0)
                return;

            VideoCollectionView.MoveCurrentToNext();

            if (VideoCollectionView.IsCurrentAfterLast)
                VideoCollectionView.MoveCurrentToFirst();
        }

        private void GroupAfterArtist(int? obj)
        {
            if (VideoCollectionView == null)
                return;

            CurrentPage = 0;

            VideoCollectionView.GroupDescriptions.Clear();
            VideoCollectionView.GroupDescriptions.Add(new PropertyGroupDescription("Artist.Name"));
            ApplySortOrder();
        }

        private void GroupAfterDate(int? obj)
        {
            if (VideoCollectionView == null)
                return;

            CurrentPage = 0;

            VideoCollectionView.GroupDescriptions.Clear();
            VideoCollectionView.GroupDescriptions.Add(new PropertyGroupDescription("FileCreated.Date"));
            ApplySortOrder();
        }

        private void GroupAfterAlphabet(int? obj)
        {
            if (VideoCollectionView == null)
                return;

            CurrentPage = 0;

            VideoCollectionView.GroupDescriptions.Clear();
            VideoCollectionView.GroupDescriptions.Add(new PropertyGroupDescription("Name", new FirstLetterConverter()));
            ApplySortOrder();
        }

        private void SetCurrentPage()
        {
            // Depends on beeing sorted
            var page = FilteredVideos.Skip(VideosPerPage * CurrentPage).Take(VideosPerPage);

            foreach (var video in page.Where(x => !CurrentPageVideos.Contains(x)).ToArray())
                CurrentPageVideos.Add(video);

            VideoCollectionView.Filter = x => page.Contains(x);

            if (!CurrentPageVideos.Contains(SelectedVideo))
            {
                VideoCollectionView.MoveCurrentToFirst();
                RequestScrollToFirstItem?.Invoke(this, null);
            }

            MaxPage = FilteredVideos.Count / VideosPerPage;

            if (CurrentPage > MaxPage)
                CurrentPage = MaxPage; 
        }

        public async Task InitializeAsync()
        {
            var settingsLoader = new SettingsLoader();
            VideosPerPage = int.Parse((await settingsLoader.GetSettingAsync(Setting.MAXVIDEOSPERPAGE))?.Value ?? "50");
            ThumpnailSize = int.Parse((await settingsLoader.GetSettingAsync(Setting.THUMPNAILSIZE))?.Value ?? "320");

            var videos = await _VideoProvider.GetVideos();
            videos = videos.OrderBy(x => x.Artist.Name).ThenBy(x => x.Name).ToList();
            foreach (var video in videos)
            {
                var vidVM = new VideoViewModel(video);
                Videos.Add(vidVM);
            }

            FilteredVideos = Videos.ToList();

            // Add all videos to CurrentPageVideos, page filtering is done via SourceCollectionView.Filter
            foreach (var video in Videos)
                CurrentPageVideos.Add(video);

            VideoCollectionView.MoveCurrentToFirst();

            MaxPage = Videos.Count / VideosPerPage;

            GroupAfterArtist(null);

            foreach (var video in Videos)
                video.ThumbailBitmap = await video.GetThumbnailBitmap().ConfigureAwait(false);
        }

        private async void VideoProvider_VideoAdded(object sender, Video e)
        {
            if (Videos.Any(x => x.FilePath == e.FilePath))
                return;
            
            var videoVM = new VideoViewModel(e);
            Videos.Add(videoVM);
            FilteredVideos.Add(videoVM);

            SetCurrentPage();
            videoVM.ThumbailBitmap = await videoVM.GetThumbnailBitmap().ConfigureAwait(false);
        }

        private bool CanGoToLastPage(int? obj)
        {
            return CurrentPage > 0;
        }

        private bool CanGoToNextPage(int? obj)
        {
            return CurrentPage < MaxPage;
        }

        private void NextPage(int? obj)
        {
            CurrentPage++;
            SetCurrentPage();
        }

        private void LastPage(int? obj)
        {
            CurrentPage--;
            SetCurrentPage();
        }
    }
}
