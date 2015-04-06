﻿using System.Collections.ObjectModel;
using System.Windows.Input;
using SharpLoader.Commands;
using SharpLoader.Services;
using SharpLoader.DependencyInjection;

namespace SharpLoader.ViewModels
{
    public class AllDownloadsViewModel : ViewModelBase
    {
        public ObservableCollection<DownloadViewModel> Downloads { get; private set; }
        public ICommand BeginDownload { get; private set; }

        private readonly IDialogService dialogService;
        private readonly IUrlService urlService;
        private readonly INotificationService notificationService;

        public AllDownloadsViewModel()
        {
            Downloads = new ObservableCollection<DownloadViewModel>();

            BeginDownload = new RelayCommandWithParameter<string>(InitializeDownload, CanInitializeDownload);

            dialogService = DependencyResolver.Instance.Resolve<IDialogService>();
            urlService = DependencyResolver.Instance.Resolve<IUrlService>();
            notificationService = DependencyResolver.Instance.Resolve<INotificationService>();
        }

        private void InitializeDownload(string videoUrl)
        {
            if (!ValidateUrl(videoUrl))
            {
                return;
            }

            var downloadLocation = dialogService.ShowSaveFileDialog();
            if (!ValidateDownloadLocation(downloadLocation))
            {
                return;
            }

            var downloader = new DownloadViewModel();
            Downloads.Add(downloader);
            downloader.Download(videoUrl, downloadLocation);
        }

        private bool ValidateUrl(string videoUrl)
        {
            if (!urlService.IsValidUrl(videoUrl))
            {
                notificationService.ShowErrorNotification(string.Format("{0} is not a valid url.", videoUrl));
                return false;
            }
            return true;
        }

        private bool ValidateDownloadLocation(string downloadLocation)
        {
            if (string.IsNullOrEmpty(downloadLocation))
            {
                notificationService.ShowErrorNotification("Invalid download location.");
                return false;
            }
            return true;
        }

        private bool CanInitializeDownload(string videoUrl)
        {
            return true;
        }
    }
}