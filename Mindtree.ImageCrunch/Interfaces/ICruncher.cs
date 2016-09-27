using System.IO;
using Mindtree.ImageCrunch.Entities;

namespace Mindtree.ImageCrunch.Interfaces
{
    public interface ICruncher
    {
        CrunchResult CrunchStream(Stream stream, CrunchOptions options);
        decimal MaxImageSize { get; }
    }
}