using System;
using System.IO;
using System.Runtime.CompilerServices;
using Sitecore;
using Sitecore.Data.Items;
using Sitecore.Diagnostics;
using Sitecore.Jobs;
using Sitecore.Resources.Media;
using InSitecore.ImageCrunch.Entities;
using InSitecore.ImageCrunch.Factory;
using InSitecore.ImageCrunch.Interfaces;
using InSitecoreSitecore.Common;

namespace InSitecore.ImageCrunch
{
    public class CrunchImage
    {
        public static long ProcessMediaItem(MediaItem mediaItem, CrunchOptions crunchOptions)
        {
            long resultantSize = 0;
            if (crunchOptions.IsEnabled)
            {
                if (mediaItem.MimeType == "image/jpeg" || mediaItem.MimeType == "image/pjpeg" ||
                    mediaItem.MimeType == "image/gif" || mediaItem.MimeType == "image/png")
                {
                    ICruncher cruncher = Cruncher.GetCruncher();
                    if (cruncher == null)
                    {
                        Log.Error("Could not find cruncher!", typeof(CrunchImage));
                        return resultantSize;
                    }
                    if (mediaItem.Size >= cruncher.MaxImageSize)
                    {
                        return resultantSize;
                    }
                    var mediaStream = mediaItem.GetMediaStream();
                    Job job = Context.Job;
                    try
                    {
                        CrunchResult result = cruncher.CrunchStream(mediaStream, crunchOptions);
                        
                        if (result == null)
                        {
                            Log.Error(string.Format("Could not shrink media file {0}", mediaItem.InnerItem.Paths.Path),
                                typeof(CrunchImage));
                        }
                        Sitecore.Resources.Media.Media media = MediaManager.GetMedia(mediaItem);
                        using (var stream = new MemoryStream())
                        {
                            result.FileStream.CopyTo(stream);
                            stream.Position = 0;
                            resultantSize = stream.Length;
                            media.SetStream(stream, Path.GetExtension(result.Format).TrimStart('.'));
                        }
                    }
                    catch (Exception exception)
                    {
                        if (job != null)
                        {
                            job.Status.LogError("Image could not be reduced in size");
                        }
                        Log.Error(string.Format("Image crunch failed: {0}", mediaItem.InnerItem.Paths.FullPath), exception,
                            (typeof(CrunchImage)));
                    }
                }
            }
            return resultantSize;
        }
    }
}