using System.IO;
using Sitecore.Data.Items;
using Sitecore.Diagnostics;
using Sitecore.Pipelines.Attach;
using Sitecore.Resources.Media;
using Mindtree.ImageCrunch.Entities;

namespace Mindtree.ImageCrunch.Pipelines
{
    public class AttachFile
    {
        public void Process(AttachArgs args)
        {
            MediaItem mi = args.MediaItem;
            try
            {
                CrunchOptions crunchOptions;
                if (mi != null)
                {
                    crunchOptions = new CrunchOptions();
                    FillSetting objFillSetting = new FillSetting();
                    TenantSetting objTennantSetting = objFillSetting.getSetting(mi.MediaPath, mi.Database.Name);
                    if (mi.Size > objTennantSetting.MinimumKBSize && mi.Size < objTennantSetting.MaxImageSize)
                    {
                        crunchOptions.APIKey = objTennantSetting.ApiKey;
                        crunchOptions.APISecret = objTennantSetting.ApiSecret;
                        crunchOptions.fullname = mi.Name + "." + mi.Extension;
                        crunchOptions.IsEnabled = objTennantSetting.IsEnabled;
                        crunchOptions.wait = true;
                        crunchOptions.lossy = objTennantSetting.Lossy;
                        crunchOptions.dev = objTennantSetting.IsDev;
                        crunchOptions.enhance = objTennantSetting.Enhance;
                        CrunchImage.ProcessMediaItem(mi, crunchOptions);
                    }
                    {
                        Log.Info(string.Format("Image Size is {0} {1}", mi.Size, ",KB which is not fit in minimum & maximum size defined in setting"), this);
                    }
                }
            }
            catch (System.Exception exception)
            {
                Log.Error(string.Format("Could not shrink item {0}", mi.InnerItem.Paths.FullPath), exception, this);
            }
        }
    }
}