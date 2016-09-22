using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SitecoreExtension.ImageCrunch.Entities
{
    public class CrunchOptions
    {
        #region ImageConversion
        public bool ImageConversion { get; set; }
        public format Format { get; set; }
        public string background { get; set; }
        public enum format { jpeg = 0, png = 1, gif = 2 }
        public bool keep_extension { get; set; }
        #endregion

        #region Configuration
        public string callbackurl { get; set; }
        public bool dev { get; set; }
        public string fullname { get; set; }
        public bool wait { get; set; }

        public bool IsEnabled { get; set; }
        #endregion

        #region Compression
        public bool lossy { get; set; }
        public int quality { get; set; }
        public bool webp { get; set; }
        #endregion

        #region  ImageResizing
        public bool ImageResizing { get; set; }
        public int height { get; set; }
        public int width { get; set; }
        public enum strategy { portrait = 0, landscape = 1, auto = 2, crop = 3, exact = 4, fit = 5, fill = 6 }
        public strategy Strategy { get; set; }
        public enum cropmode { top = 0, northwest = 1, northeast = 2, west = 3, east = 4, southeast = 5, southwest = 6, south = 7 }
        public cropmode CropMode { get; set; }
        public bool enhance { get; set; }
        #endregion
    }
}
