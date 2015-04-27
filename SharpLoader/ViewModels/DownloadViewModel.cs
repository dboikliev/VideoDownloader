﻿using System;
using System.Net;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using SharpLoader.Commands;
using SharpLoader.Models;
using SharpLoader.Models.Downloader;
using SharpLoader.Services;
using SharpLoader.DependencyInjection;
using SharpLoader.Models.Video;
using SharpLoader.Services.Contracts;

namespace SharpLoader.ViewModels
{
    public class DownloadViewModel : ViewModelBase
    {
        private long progress;
        private BitmapImage thumbnail;
        private string speed;
        private string title;
        private string duration;

        public string Title
        {
            get { return title; }
            set
            {
                title = value;
                OnPropertyChanged("Title");
            }
        }

        public long Progress
        {
            get
            {
                return progress;
            }
            set
            {
                progress = value;
                OnPropertyChanged("Progress");
            }
        }

        public BitmapImage Thumbnail
        {
            get
            {
                return thumbnail;
            }
            set
            {
                thumbnail = value;
                OnPropertyChanged("Thumbnail");
            }
        }

        public string Duration
        {
            get { return duration; }
            set
            {
                duration = value;
                OnPropertyChanged("Duration");
            }
        }

        public string Speed
        {
            get
            {
                return speed;
            }
            set
            {
                speed = string.Format("{0} MB/s", value);
                OnPropertyChanged("Speed");
            }
        }

        private readonly IVideoInfoService videoInfoService;
        private readonly IDownloaderService downloaderService;

        public DownloadViewModel()
        {
            ServicePointManager.DefaultConnectionLimit = int.MaxValue;

            videoInfoService = DependencyResolver.Instance.Resolve<IVideoInfoService>();
            downloaderService = DependencyResolver.Instance.Resolve<IDownloaderService>();

            downloaderService.ProgressUpdated += OnDownloaderProgressUpdated;
            downloaderService.SpeedUpdated += OnSpeedUpdated;
        }

        public async Task<VideoInfo> Initialize(string videoUrl)
        {
            var videoInfo = await videoInfoService.GetVideoInfo(videoUrl);
            Thumbnail = videoInfo.Thumbnail;
            Title = videoInfo.Title;
            Duration = TimeSpan.FromSeconds(videoInfo.DurationInSeconds).ToString();
            return videoInfo;
        }

        public void Download(VideoInfo videoInfo, string downloadLocation)
        {
            downloaderService.BeginDownload(videoInfo, downloadLocation);
        }

        void OnDownloaderProgressUpdated(object sender, ProgressUpdatedEventArgs e)
        {
            Progress = e.Progress;
        }

        private void OnSpeedUpdated(object sender, SpeedUpdatedEventArgs e)
        {
            Speed = e.Speed.ToString("0.00");
        }
    }
}
