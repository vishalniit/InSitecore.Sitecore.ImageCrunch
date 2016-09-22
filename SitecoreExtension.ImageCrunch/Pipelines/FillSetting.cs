using Sitecore.Data.Items;
using SitecoreExtension.ImageCrunch.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SitecoreExtension.ImageCrunch.Pipelines
{
    public class FillSetting
    {
        public TenantSetting getSetting()
        {
            TenantSetting objSetting = null;
            Sitecore.Data.Database db = Sitecore.Data.Database.GetDatabase("master");
            if (db != null)
            {
                objSetting = new TenantSetting();
                Item settingItm = db.GetItem("/sitecore/system/Modules/ImageCore/DefaultTenant");
                if (settingItm != null)
                {
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
                        objSetting.MinimumKBSize = 50;
                }
            }
            return objSetting;
        }
    }
}
