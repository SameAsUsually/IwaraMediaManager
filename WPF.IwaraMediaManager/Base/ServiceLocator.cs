using IwaraMediaManager.Core.Interfaces;
using IwaraMediaManager.Core.Provider;
using IwaraMediaManager.Core.ViewModels;
using Microsoft.Extensions.DependencyInjection;
using System.Windows;

namespace IwaraMediaManager.WPF.Base
{
    public class ServiceLocator
    {
        private ServiceProvider services;

        public ServiceLocator()
        {

        }

        public void Initialize()
        {
            SplashScreen splashScreen = new SplashScreen("./Images/IwaraIcon.png");
            splashScreen.Show(true, true);

            VideoProvider videoProvider = new VideoProvider();

            var collection = new ServiceCollection();
            collection.AddSingleton<IVideoProvider>(videoProvider);
            collection.AddSingleton<IDownloadProvider>(new DownloadProvider());
            collection.AddSingleton<VideoPageViewModel>();
            collection.AddScoped<MainWindowViewModel>();
            collection.AddScoped<IwaraViewerViewModel>();
            collection.AddScoped<DownloadsViewModel>();

            services = collection.BuildServiceProvider();
        }

        public MainWindowViewModel MainWindowViewModel
        {
            get { return services.GetRequiredService<MainWindowViewModel>(); }
        }

        public VideoPageViewModel VideoPageViewModel
        {
            get { return services.GetRequiredService<VideoPageViewModel>(); }
        }

        public IwaraViewerViewModel IwaraViewerViewModel
        {
            get { return services.GetRequiredService<IwaraViewerViewModel>(); }
        }

        public DownloadsViewModel DownloadViewModel
        {
            get { return services.GetRequiredService<DownloadsViewModel>(); }
        }
    }
}
