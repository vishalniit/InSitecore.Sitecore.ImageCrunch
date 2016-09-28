using System;
using System.Linq;
using Sitecore;
using Sitecore.Data.Items;
using Sitecore.Jobs;
using Sitecore.Shell.Framework.Commands;
using Mindtree.ImageCrunch.Entities;
using Mindtree.ImageCrunch.Pipelines;
using Sitecore.Diagnostics;

namespace Mindtree.ImageCrunch.Web.Commands
{
    public class ShrinkImageOnTree : ShrinkImage
    {
        CrunchOptions crunchOptions;
        FillSetting objFillSetting;
        TenantSetting objTennantSetting;
        MediaItem mi;
        protected override void Shrink(object[] parameters)
        {
            Item item = parameters[0] as Item;
            Job job = Context.Job;
            if (item == null)
            {
                return;
            }
            else if (MindtreeSitecore.Common.Functions.IsMediaItem(item))
            {
                mi = new MediaItem(item);
                //This make sures that no need to get setting item recursively for each child.
                if (objFillSetting == null)
                {
                    crunchOptions = new CrunchOptions();
                    objFillSetting = new FillSetting();
                    objTennantSetting = objFillSetting.getSetting();
                    crunchOptions.APIKey = objTennantSetting.ApiKey;
                    crunchOptions.APISecret = objTennantSetting.ApiSecret;
                    crunchOptions.wait = true;
                    crunchOptions.IsEnabled = objTennantSetting.IsEnabled;
                    crunchOptions.enhance = objTennantSetting.Enhance;
                    crunchOptions.lossy = objTennantSetting.Lossy;
                    crunchOptions.dev = objTennantSetting.IsDev;
                    crunchOptions.enhance = objTennantSetting.Enhance;
                }
                if (objFillSetting.isInitialised)
                {
                    crunchOptions.fullname = mi.Name + "." + mi.Extension;
                    if (mi.Size > objTennantSetting.MinimumKBSize && mi.Size < objTennantSetting.MaxImageSize)
                    {
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
                        Log.Info(string.Format("Image Size is {0} {1}", mi.Size, ",KB which is not fit in minimum & maximum size defined in setting"), this);
                    }
                }
            }
            foreach (var child in item.Children)
            {
                this.Shrink(new[] { child });
            }
        }
    }
}