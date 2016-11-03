using Sitecore.Data.Items;
using InSitecore.ImageCrunch.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InSitecore.ImageCrunch.Pipelines
{
    public class FillSetting
    {
        private static string _ImageCorePath;
        public static string ImageCorePath
        {
            get
            {
                if (string.IsNullOrWhiteSpace(_ImageCorePath))
                {
                    _ImageCorePath = Sitecore.Configuration.Settings.GetSetting("ImageCoreItemRootPath");

                }
                return _ImageCorePath;
            }
        }
        public bool isInitialised { get; set; }
        public FillSetting()
        {
            isInitialised = false;
        }
        private Item GetSettingItem(Item itmRootSettingPath, string path)
        {
            Item itm = null;
            //this to make sure that global setting is picked up
            string newpath = "/Media Library" + path;
            if (itmRootSettingPath != null)
            {
                var items = from p in itmRootSettingPath.Children
                            where newpath.Contains("/" + MindtreeSitecore.Common.Functions.GetItem(p.Fields["SiteMediaFolderPath"].Value, itmRootSettingPath.Database.Name, itmRootSettingPath.Language.ToString()).DisplayName + "/")
                            select p;
                //this to make sure that only regional setting is effective if present
                if (items.Count<Item>() > 1)
                {
                    items = from p in items
                            where path.Contains(p.DisplayName)
                            select p;
                    itm = items.FirstOrDefault<Item>();
                }
                else
                    itm = items.FirstOrDefault<Item>();
            }
            return itm;
        }

        public TenantSetting getSetting(string path = "", string database = "master", string language = "en")
        {
            TenantSetting objSetting = null;
            ///sitecore/system/Modules/ImageCore/DefaultTenant
            //MindtreeSitecore.Common.Functions.GetDatabase()
            Item settingItm = MindtreeSitecore.Common.Functions.GetItemPath(ImageCorePath, database, language);
            settingItm = GetSettingItem(settingItm, path);
            MindtreeSitecore.Common.Functions.ClearSitecoreCacheofItem(database, settingItm.ID);
            if (settingItm != null)
            {
                objSetting = new TenantSetting();
                string strDev = settingItm.Fields["IsDev"].Value;
                if (strDev != null && strDev.Length > 0)
                {
                    objSetting.IsDev = strDev.Equals("1", StringComparison.InvariantCultureIgnoreCase);
                }
                else
                    objSetting.IsDev = false;
                string Lossy = settingItm.Fields["Lossy"].Value;
                if (Lossy != null && Lossy.Length > 0)
                {
                    objSetting.Lossy = Lossy.Equals("1", StringComparison.InvariantCultureIgnoreCase);
                }
                else
                    objSetting.Lossy = true;
                string Enhance = settingItm.Fields["Enhance"].Value;
                if (Enhance != null && Enhance.Length > 0)
                {
                    objSetting.Enhance = Enhance.Equals("1", StringComparison.InvariantCultureIgnoreCase);
                }
                else
                    objSetting.Enhance = false;
                string isEnabled = settingItm.Fields["IsEnabled"].Value;
                if (isEnabled != null && isEnabled.Length > 0)
                {
                    objSetting.IsEnabled = isEnabled.Equals("1", StringComparison.InvariantCultureIgnoreCase);
                }
                else
                    objSetting.IsEnabled = false;

                string minimumKBSize = settingItm.Fields["MinimumImageSizeKB"].Value;
                if (minimumKBSize != null && minimumKBSize.Length > 0)
                {
                    //Converting into Bytes
                    objSetting.MinimumKBSize = Convert.ToDouble(minimumKBSize) * 1000;
                }
                else
                    objSetting.MinimumKBSize = (50 * 1000);

                string maxImageSizeKB = settingItm.Fields["MaxImageSizeKB"].Value;
                if (maxImageSizeKB != null && maxImageSizeKB.Length > 0)
                {
                    //Converting into Bytes
                    objSetting.MaxImageSize = Convert.ToDouble(maxImageSizeKB) * 1000;
                }
                else
                    objSetting.MaxImageSize = (50 * 1000);

                string krakenApiKey = settingItm.Fields["KrakenApiKey"].Value;
                if (krakenApiKey != null && krakenApiKey.Length > 0)
                {
                    objSetting.ApiKey = krakenApiKey;
                }
                else
                    objSetting.ApiKey = "";

                string krakenApiSecret = settingItm.Fields["KrakenApiSecret"].Value;
                if (krakenApiSecret != null && krakenApiSecret.Length > 0)
                {
                    objSetting.ApiSecret = krakenApiSecret;
                }
                else
                    objSetting.ApiSecret = "";

                isInitialised = true;
            }

            return objSetting;
        }
    }
}
