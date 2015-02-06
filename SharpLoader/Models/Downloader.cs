﻿using System;
using System.IO;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using SharpLoader.Models.VideoInfo;

namespace SharpLoader.Models
{
    /// <summary>
    /// Contains the logic for downloading a video and gives information about the download progress and speed. 
    /// </summary>
    class Downloader
    {
        private long totalDownloadedBytes;
        private long currentVideoSize;
        private long bytesDownloadedPerSecond;

        public static event EventHandler<ProgressUpdatedEventArgs> ProgressUpdated;
        public static event EventHandler<SpeedUpdatedEventArgs> SpeedUpdated;

        private static void OnProgressUpdated(ProgressUpdatedEventArgs e)
        {
            var temp = Interlocked.CompareExchange(ref ProgressUpdated, null, null);
            if (temp != null)
            {
                temp(typeof(Downloader), e);
            }
        }

        private static void OnSpeedUpdated(SpeedUpdatedEventArgs e)
        {
            var temp = Interlocked.CompareExchange(ref SpeedUpdated, null, null);
            if (temp != null)
            {
                SpeedUpdated(typeof(Downloader), e);
            }
        }

        public void DownloadFile(VideoInfoBase video, string downloadLocation, CancellationToken token)
        {
            totalDownloadedBytes = 0;
            currentVideoSize = video.VideoSize;

            const int millisecondsInSecond = 1000;
            const int dueTime = 0;
            using (var timer = new Timer(UpdateSpeed, null, dueTime, millisecondsInSecond))
            {
                File.Create(downloadLocation).Dispose();
                foreach (var segmentGroup in video.Segments)
                {
                    Parallel.ForEach(segmentGroup, segment =>
                    {
                        token.ThrowIfCancellationRequested();
                        DownloadSegment(video.DownloadUrl, downloadLocation, segment);
                    });
                }
            }
        }

        private void DownloadSegment(string videoUrl, string downloadLocation, VideoFileSegment segment)
        {
            var request = (HttpWebRequest)WebRequest.Create(videoUrl);
            request.AddRange(segment.Start, segment.End);
            using (var response = (HttpWebResponse)request.GetResponse())
            {
                using (var stream = response.GetResponseStream())
                {
                    using (var fileStream = File.Open(downloadLocation, FileMode.Open, FileAccess.ReadWrite, FileShare.ReadWrite))
                    {
                        fileStream.Position = segment.Start;
                        using (var reader = new BinaryReader(stream))
                        {
                            var data = reader.ReadBytes((int)segment.Length);

                            totalDownloadedBytes += data.Length;
                            UpdateProgress();

                            using (var writer = new BinaryWriter(fileStream))
                            {
                                writer.Write(data);
                                bytesDownloadedPerSecond += data.Length;
                            }
                        }
                    }
                }
            }
        }

        private void UpdateSpeed(object obj)
        {
            var speedArgs = new SpeedUpdatedEventArgs((bytesDownloadedPerSecond / 1024D / 1024D));
            OnSpeedUpdated(speedArgs);
            bytesDownloadedPerSecond = 0;
        }

        private void UpdateProgress()
        {
            var progressArgs = new ProgressUpdatedEventArgs();
            progressArgs.Progress = (int)(100.0 * totalDownloadedBytes / currentVideoSize);
            OnProgressUpdated(progressArgs);
        }
    }
}
