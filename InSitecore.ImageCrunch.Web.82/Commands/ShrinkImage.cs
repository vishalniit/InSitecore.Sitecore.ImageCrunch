using System;
using System.Linq;
using Sitecore.Data.Items;
using Sitecore.Shell.Applications.Dialogs.ProgressBoxes;
using Sitecore.Shell.Framework.Commands;
using InSitecore.ImageCrunch.Entities;
using InSitecore.ImageCrunch.Pipelines;
using Sitecore.Diagnostics;

namespace InSitecore.ImageCrunch.Web.Commands
{
    [Serializable]
    public class ShrinkImage : Command
    {
        public MediaItem mi { get; set; }
        public CrunchOptions crunchOptions { get; set; }

        public FillSetting objFillSetting { get; set; }
        public override void Execute(CommandContext context)
        {
            if (!context.Items.Any(t => t.Paths.IsMediaItem))
            {
                return;
            }
            objFillSetting = null;
            //Sitecore.Context.ClientPage.
            ProgressBox.Execute("Shrink Image", "Shrink Image", new ProgressBoxMethod(this.Shrink), new object[1]
            {
                context.Items[0]
            });
            //Sitecore.Jobs.JobManager.Start(new Sitecore.Jobs.JobOptions("Shrink Image", "Shrink Image", Sitecore.Context.Site.Name, new object[1] { context.Items[0] }, "Shrink"));
        }

        protected virtual void Shrink(object[] parameters)
        {
            Item item = parameters[0] as Item;

            if (item == null)
            {
                throw new Exception("Parameter 0 was not a item");
            }
            else
            {
                mi = new MediaItem(item);
                crunchOptions = new CrunchOptions();
                objFillSetting = new FillSetting();
                TenantSetting objTennantSetting = objFillSetting.getSetting(mi.MediaPath, mi.Database.Name, mi.InnerItem.Language.ToString());
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
                else
                {
                    Log.Info(string.Format("Image Size is {0} {1}", mi.Size, ",KB which is not fit in minimum & maximum size defined in setting"), this);
                }
            }
        }

        public override CommandState QueryState(CommandContext context)
        {
            if (context.Items.Any(t => t.Paths.IsMediaItem))
            {
                return CommandState.Enabled;
            }
            else
            {
                return CommandState.Disabled;
            }
        }
    }
}