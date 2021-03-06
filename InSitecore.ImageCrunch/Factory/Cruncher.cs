﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Sitecore.Reflection;
using InSitecore.ImageCrunch.Entities;
using InSitecore.ImageCrunch.Interfaces;

namespace InSitecore.ImageCrunch.Factory
{
    public abstract class Cruncher : ICruncher
    {
        public static ICruncher GetCruncher()
        {
            var providerObject = (ICruncher)Sitecore.Configuration.Settings.GetProviderObject("ImageCruncher", typeof(ICruncher));

            return providerObject;
        }
        
        public abstract Entities.CrunchResult CrunchStream(Stream stream, Entities.CrunchOptions options);        

        public abstract decimal MaxImageSize { get; set; }
    }
}
