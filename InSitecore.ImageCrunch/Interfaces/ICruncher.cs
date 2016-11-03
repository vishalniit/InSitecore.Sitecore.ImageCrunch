using System.IO;
using InSitecore.ImageCrunch.Entities;

namespace InSitecore.ImageCrunch.Interfaces
{
    public interface ICruncher
    {
        CrunchResult CrunchStream(Stream stream, CrunchOptions options);
        decimal MaxImageSize { get; }
    }
}