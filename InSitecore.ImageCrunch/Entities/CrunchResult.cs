using System.IO;

namespace InSitecore.ImageCrunch.Entities
{
    public class CrunchResult
    {
        public string Format { get; set; }

        public Stream FileStream { get; set; }
    }
}