﻿using System.Collections.Generic;
using System.Threading.Tasks;
using SharpLoader.Constants;
using SharpLoader.DependencyInjection;
using SharpLoader.Models.Video;
using SharpLoader.Services.Contracts;

namespace SharpLoader.Services.Implementations
{
    public class VideoInfoService : IVideoInfoService
    {
        private static readonly Dictionary<string, IVideoInfoService> VideoInfoExtractors;

        static VideoInfoService()
        {
            VideoInfoExtractors = new Dictionary<string, IVideoInfoService>
            {
                { DomainsConstants.Vbox7, new Vbox7VideoInfoService() },
                { DomainsConstants.YouTube, new YouTubeVideoInfoService() },
                { DomainsConstants.Vimeo, null },
                { DomainsConstants.Pornhub, null },
                { DomainsConstants.XVideos, null }
            };
        }

        private readonly IUrlService urlService;

        public VideoInfoService()
        {
            urlService = DependencyResolver.Instance.Resolve<IUrlService>();
        }

        public async Task<VideoInfo> GetVideoInfo(string videoUrl)
        {
            var domain = urlService.GetDomainFromUrl(videoUrl);
            var videoInfoStrategy = VideoInfoExtractors[domain];
            var videoInfo = await videoInfoStrategy.GetVideoInfo(videoUrl);
            return  videoInfo;
        }
    }
}