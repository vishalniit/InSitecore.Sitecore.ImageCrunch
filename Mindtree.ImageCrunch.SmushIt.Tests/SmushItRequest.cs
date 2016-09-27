using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;
using Mindtree.ImageCrunch.Entities;

namespace Mindtree.ImageCrunch.SmushIt.Tests
{
    [TestFixture]
    public class SmushItRequest
    {
        [Test]
        public void CreateRequest()
        {
            var assembly = Assembly.GetExecutingAssembly();
            var resourceName = "Mindtree.ImageCrunch.SmushIt.Tests.TestResources.testPicture.jpg";

            using (Stream stream = assembly.GetManifestResourceStream(resourceName))
            {
                var memoryStream = new MemoryStream();
                stream.CopyTo(memoryStream);
                memoryStream.Position = 0;
                var cruncher = new SmushItCruncher();
                CrunchOptions crunchOptions;
                crunchOptions = new CrunchOptions();
                CrunchResult request = cruncher.CrunchStream(memoryStream, crunchOptions);
            }
        }
    }
}