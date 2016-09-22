using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SitecoreExtension.ImageCrunch.Entities
{
    public class TenantSetting
    {
        public bool IsDev { get; set; }

        public bool Lossy { get; set; }

        public bool Enhance { get; set; }

        public bool IsEnabled { get; set; }

        public double MinimumKBSize { get; set; }
    }
}
