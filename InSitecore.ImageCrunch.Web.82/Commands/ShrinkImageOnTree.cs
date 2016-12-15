using System;
using System.Linq;
using Sitecore;
using Sitecore.Data.Items;
using Sitecore.Jobs;
using Sitecore.Shell.Framework.Commands;
using InSitecore.ImageCrunch.Entities;
using InSitecore.ImageCrunch.Pipelines;
using Sitecore.Diagnostics;
using InSitecoreSitecore.Common;
using System.Diagnostics;

namespace InSitecore.ImageCrunch.Web.Commands
{
    [Serializable]
    public class ShrinkImageOnTree : ShrinkImage
    {
        protected override void Shrink(object[] parameters)
        {
            Item item = parameters[0] as Item;
            Job job = Context.Job;
            if (crunchedStats == null)
            {
                crunchedStats = new CrunchedStats();
                crunchedStats.JobID = job.Name;
                crunchedStats.typeofExecution = CrunchedStats.TypeofExecution.CrunchbyTreeRibbonCommand;                
            }
            if (item == null)
            {
                return;
            }
            else if (InSitecoreSitecore.Common.Functions.IsMediaItem(item))
            {
                mi = new MediaItem(item);
                //This make sures that no need to get setting item recursively for each child.
                if (objFillSetting == null)
                {
                    crunchOptions = new CrunchOptions();
                    objFillSetting = new FillSetting();
                    objTennantSetting = objFillSetting.getSetting(mi.MediaPath, mi.Database.Name, mi.InnerItem.Language.ToString());
                    crunchOptions.APIKey = objTennantSetting.ApiKey;
                    crunchOptions.APISecret = objTennantSetting.ApiSecret;
                    crunchOptions.wait = true;
                    crunchOptions.IsEnabled = objTennantSetting.IsEnabled;
                    crunchOptions.enhance = objTennantSetting.Enhance;
                    crunchOptions.lossy = objTennantSetting.Lossy;
                    crunchOptions.dev = objTennantSetting.IsDev;
                    crunchOptions.enhance = objTennantSetting.Enhance;
                    crunchedStats.Database = mi.Database.Name;
                    crunchedStats.InitiatedBy = Sitecore.Context.GetUserName();
                    Log.Info(string.Format("Job Started {0} by {1} on Database {2}", crunchedStats.JobID, crunchedStats.InitiatedBy, crunchedStats.Database), this);
                }
                if (objFillSetting.isInitialised)
                {
                    crunchOptions.fullname = mi.Name + "." + mi.Extension;
                    if (mi.Size > objTennantSetting.MinimumKBSize && mi.Size < objTennantSetting.MaxImageSize)
                    {
                        try
                        {
                            crunchedStats.BeforeCrunchSize = mi.Size;
                            if (sw == null)
                                sw = new Stopwatch();
                            sw.Start();
                            crunchedStats.AfterCrunchSize = CrunchImage.ProcessMediaItem(mi, crunchOptions);
                            sw.Stop();
                            crunchedStats.TimeTaken = sw.ElapsedMilliseconds;
                            if (job != null)
                            {
                                

                                job.Status.LogInfo(string.Format("Done: {0}", item.Paths.FullPath));
                                Log.Info(string.Format("Done: {0}", item.Paths.FullPath), this);

                                job.Status.LogInfo(string.Format("Stats - Before Crunch Size: {0}", crunchedStats.BeforeCrunchSize));
                                Log.Info(string.Format("Stats - Before Crunch Size: {0}", crunchedStats.BeforeCrunchSize), this);

                                job.Status.LogInfo(string.Format("Stats - After Crunch Size: {0}", crunchedStats.AfterCrunchSize));
                                Log.Info(string.Format("Stats - After Crunch Size: {0}", crunchedStats.AfterCrunchSize), this);
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
                    else
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