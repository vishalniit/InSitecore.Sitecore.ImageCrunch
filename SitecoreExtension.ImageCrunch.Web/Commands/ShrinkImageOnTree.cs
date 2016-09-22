using System;
using System.Linq;
using Sitecore;
using Sitecore.Data.Items;
using Sitecore.Jobs;
using Sitecore.Shell.Framework.Commands;
using SitecoreExtension.ImageCrunch.Entities;
using SitecoreExtension.ImageCrunch.Pipelines;
using Sitecore.Diagnostics;

namespace SitecoreExtension.ImageCrunch.Web.Commands
{
    public class ShrinkImageOnTree : ShrinkImage
    {

        protected override void Shrink(object[] parameters)
        {
            Item item = parameters[0] as Item;
            MediaItem mi;
            CrunchOptions crunchOptions;
            Job job = Context.Job;
            if (item == null)
            {
                return;
            }
            else
            {
                mi = new MediaItem(item);
                crunchOptions = new CrunchOptions();
                crunchOptions.fullname = mi.Name + "." + mi.Extension;
                FillSetting objFillSetting = new FillSetting();
                TenantSetting objTennantSetting = objFillSetting.getSetting();
                if (mi.Size > objTennantSetting.MinimumKBSize)
                {
                    crunchOptions.wait = true;
                    crunchOptions.IsEnabled = objTennantSetting.IsEnabled;
                    crunchOptions.enhance = objTennantSetting.Enhance;
                    crunchOptions.lossy = objTennantSetting.Lossy;
                    crunchOptions.dev = objTennantSetting.IsDev;
                    crunchOptions.enhance = objTennantSetting.Enhance;                    
                    try
                    {
                        CrunchImage.ProcessMediaItem(mi, crunchOptions);
                        if (job != null)
                        {
                            job.Status.LogInfo(string.Format("Done: {0}", item.Paths.FullPath));
                        }
                    }
                    catch (Exception catche)
                    {
                        if (job != null)
                        {
                            job.Status.LogInfo(string.Format("{0}: {1}", catche.Message, item.Paths.FullPath));
                        }
                    }
                }
                {
                    Log.Info(string.Format("Image Size is {0} {1}", mi.Size, ",KB which is below minimum size defined in setting"), this);
                }
            }            
            foreach (var child in item.Children)
            {
                this.Shrink(new[] { child });
            }
        }
    }
}