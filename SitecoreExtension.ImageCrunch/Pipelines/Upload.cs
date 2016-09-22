using Sitecore.Data.Items;
using Sitecore.Diagnostics;
using Sitecore.Pipelines.Upload;
using SitecoreExtension.ImageCrunch.Entities;

namespace SitecoreExtension.ImageCrunch.Pipelines
{
    public class Upload : UploadProcessor
    {
        public void Process(UploadArgs args)
        {
            Assert.ArgumentNotNull((object)args, "args");

            foreach (Item uploadedItem in args.UploadedItems)
            {
                try
                {
                    CrunchOptions crunchOptions;
                    MediaItem mi = new MediaItem(uploadedItem);
                    if (mi != null)
                    {
                        crunchOptions = new CrunchOptions();
                        FillSetting objFillSetting = new FillSetting();
                        TenantSetting objTennantSetting = objFillSetting.getSetting();
                        if (mi.Size > objTennantSetting.MinimumKBSize)
                        {
                            crunchOptions.fullname = mi.Name + "." + mi.Extension;
                            //As of now API is based out of post approach
                            crunchOptions.IsEnabled = objTennantSetting.IsEnabled;
                            crunchOptions.wait = true;
                            crunchOptions.lossy = objTennantSetting.Lossy;
                            crunchOptions.dev = objTennantSetting.IsDev;
                            //Test Resizing option
                            //crunchOptions.ImageResizing = true;
                            //crunchOptions.height = 50;
                            //crunchOptions.width = 50;
                            //crunchOptions.Strategy = CrunchOptions.strategy.crop;
                            //crunchOptions.CropMode = CrunchOptions.cropmode.top;
                            crunchOptions.enhance = objTennantSetting.Enhance;
                            //Test ImageConversion option
                            //crunchOptions.ImageConversion = true;
                            //crunchOptions.Format = CrunchOptions.format.jpeg;
                            CrunchImage.ProcessMediaItem(mi, crunchOptions);
                        }
                        {
                            Log.Info(string.Format("Image Size is {0} {1}", mi.Size, ",KB which is below minimum size defined in setting"), this);
                        }
                    }
                }
                catch (System.Exception exception)
                {
                    Log.Error(string.Format("Could not shrink item {0}", uploadedItem.Paths.FullPath), exception, this);
                }
            }
        }
    }
}