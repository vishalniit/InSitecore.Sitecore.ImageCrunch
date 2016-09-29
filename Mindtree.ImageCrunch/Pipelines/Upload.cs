using Sitecore.Data.Items;
using Sitecore.Diagnostics;
using Sitecore.Pipelines.Upload;
using Mindtree.ImageCrunch.Entities;

namespace Mindtree.ImageCrunch.Pipelines
{
    public class Upload : UploadProcessor
    {
        public void Process(UploadArgs args)
        {
            Assert.ArgumentNotNull((object)args, "args");
            if (args != null && args.UploadedItems != null && args.UploadedItems[0] != null)
            {
                CrunchOptions crunchOptions;
                FillSetting objFillSetting = new FillSetting();
                MediaItem mi = args.UploadedItems[0];
                TenantSetting objTennantSetting = objFillSetting.getSetting(mi.MediaPath, mi.Database.Name, mi.InnerItem.Language.ToString());
                crunchOptions = new CrunchOptions();
                crunchOptions.APIKey = objTennantSetting.ApiKey;
                crunchOptions.APISecret = objTennantSetting.ApiSecret;
                crunchOptions.IsEnabled = objTennantSetting.IsEnabled;
                crunchOptions.wait = true;
                crunchOptions.lossy = objTennantSetting.Lossy;
                crunchOptions.dev = objTennantSetting.IsDev;
                crunchOptions.enhance = objTennantSetting.Enhance;
                foreach (Item uploadedItem in args.UploadedItems)
                {
                    try
                    {
                        mi = new MediaItem(uploadedItem);
                        //checking maximum & minimum size condition defined for the tenant
                        if (mi != null && mi.Size > objTennantSetting.MinimumKBSize && mi.Size < objTennantSetting.MaxImageSize)
                        {
                            crunchOptions.fullname = mi.Name + "." + mi.Extension;
                            //As of now API is based out of post approach
                            CrunchImage.ProcessMediaItem(mi, crunchOptions);
                        }
                        {
                            Log.Info(string.Format("Image Size is {0} {1}", mi.Size, ",KB which is not fit in minimum & maximum size defined in setting"), this);
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
}