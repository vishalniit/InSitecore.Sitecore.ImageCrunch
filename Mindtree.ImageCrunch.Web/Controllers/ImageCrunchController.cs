using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Sitecore.Data.Items;
using Sitecore.Data;
using Sitecore.Configuration;
using Sitecore.Resources.Media;
using Sitecore.Data.Managers;
using System.Drawing;
using Sitecore.Controllers.Results;
using System.Collections.Specialized;
using System.IO;
using Sitecore.Diagnostics;
using Mindtree.ImageCrunch.Entities;
using Mindtree.ImageCrunch.Pipelines;
using Mindtree.ImageCrunch;

namespace Sitecore.Controllers
{    
    public class ImageCrunchController : Controller
    {
        CrunchOptions crunchOptions;
        FillSetting objFillSetting;
        TenantSetting objTennantSetting;
        
        public JsonResult Upload(string database, string destinationUrl)
        {
            SitecoreViewModelResult result;
            try
            {
                return this.DoUpload(database, destinationUrl);
            }
            catch (Exception ex)
            {
                Log.Error(ex.Message, ex, (object)this);
                SitecoreViewModelResult sitecoreViewModelResult = new SitecoreViewModelResult();
                List<ErrorItem> errorItemList = new List<ErrorItem>();
                result = new SitecoreViewModelResult();
                errorItemList.Add(new ErrorItem("Exception", ex.Message, "Internal Server Error"));
                ((dynamic)result.Result).errorItems = errorItemList;
            }
            return (JsonResult)result;
        }
        private static bool ValidateDestination(string database, string destinationUrl, SitecoreViewModelResult result)
        {
            List<ErrorItem> errorItemList = new List<ErrorItem>();
            bool flag = true;
            Database database1 = ClientHost.Databases.ContentDatabase;
            if (!string.IsNullOrEmpty(database))
                database1 = Factory.GetDatabase(database);
            Item obj1 = database1.GetItem(destinationUrl);
            if (obj1 == null || !obj1.Access.CanCreate())
            {
                errorItemList.Add(new ErrorItem("destination", destinationUrl, ClientHost.Globalization.Translate("You do not have permission to upload files to the currently selected folder.")));
                flag = false;
            }
            if (!flag)
            {
                ((dynamic)result.Result).errorItems = errorItemList;
            }
            return flag;
        }

        private string ParseDestinationUrl(string destinationUrl)
        {
            if (!destinationUrl.EndsWith("/"))
                destinationUrl += "/";
            return destinationUrl;
        }
        private static bool ValidateFile(HttpPostedFileBase file, SitecoreViewModelResult result)
        {
            List<ErrorItem> errorItemList = new List<ErrorItem>();
            int contentLength = file.ContentLength;
            bool flag = true;
            if ((long)contentLength > Settings.Media.MaxSizeInDatabase)
            {
                errorItemList.Add(new ErrorItem("size", contentLength.ToString(), string.Format(ClientHost.Globalization.Translate("The file exceeds the maximum size ({0})."), (object)Settings.Media.MaxSizeInDatabase)));
                flag = false;
            }
            if (!flag)
            {
                ((dynamic)result.Result).errorItems = errorItemList;
            }
            return flag;
        }
        private JsonResult DoUpload(string database, string destinationUrl)
        {
            if (string.IsNullOrEmpty(destinationUrl))
                destinationUrl = "/sitecore/media library";
            List<UploadedFileItem> uploadedFileItemList = new List<UploadedFileItem>();
            SitecoreViewModelResult result = new SitecoreViewModelResult();
            if (!ImageCrunchController.ValidateDestination(database, destinationUrl, result))
                return (JsonResult)result;
            objFillSetting = new FillSetting();
            crunchOptions = new CrunchOptions();
            foreach (string file1 in (NameObjectCollectionBase)this.Request.Files)
            {
                HttpPostedFileBase file2 = this.Request.Files[file1];
                if (file2 != null)
                {
                    string withoutExtension = Path.GetFileNameWithoutExtension(file2.FileName);
                    if (!string.IsNullOrEmpty(this.Request.Form["name"]))
                        withoutExtension = this.Request.Form["name"];
                    string str = ItemUtil.ProposeValidItemName(withoutExtension, "default");
                    string empty = string.Empty;
                    if (!string.IsNullOrEmpty(this.Request.Form["alternate"]))
                        empty = this.Request.Form["alternate"];
                    Database database1 = Context.ContentDatabase;
                    if (!string.IsNullOrEmpty(database))
                        database1 = Factory.GetDatabase(database);
                    if (database1 == null)
                        database1 = Context.ContentDatabase;
                    MediaCreatorOptions options1 = new MediaCreatorOptions()
                    {
                        AlternateText = empty,
                        Database = database1,
                        FileBased = Settings.Media.UploadAsFiles,
                        IncludeExtensionInItemName = Settings.Media.IncludeExtensionsInItemNames,
                        KeepExisting = true,
                        Language = LanguageManager.DefaultLanguage,
                        Versioned = Settings.Media.UploadAsVersionableByDefault,
                        Destination = this.ParseDestinationUrl(destinationUrl) + str
                    };
                    if (!ImageCrunchController.ValidateFile(file2, result))
                        return (JsonResult)result;
                    Item fromStream = MediaManager.Creator.CreateFromStream(file2.InputStream, "/upload/" + file2.FileName, options1);
                    if (!string.IsNullOrEmpty(this.Request.Form["description"]))
                    {
                        fromStream.Editing.BeginEdit();
                        fromStream["Description"] = this.Request.Form["description"];
                        fromStream.Editing.EndEdit();
                    }
                    MediaItem mediaItem = new MediaItem(fromStream);
                    ///Code to Shrunk the Media Item begin
                    objTennantSetting = objFillSetting.getSetting(mediaItem.MediaPath, mediaItem.Database.Name, mediaItem.InnerItem.Language.ToString());
                    crunchOptions.APIKey = objTennantSetting.ApiKey;
                    crunchOptions.APISecret = objTennantSetting.ApiSecret;
                    crunchOptions.IsEnabled = objTennantSetting.IsEnabled;
                    crunchOptions.wait = true;
                    crunchOptions.lossy = objTennantSetting.Lossy;
                    crunchOptions.dev = objTennantSetting.IsDev;
                    crunchOptions.enhance = objTennantSetting.Enhance;
                    try
                    {
                        //checking maximum & minimum size condition defined for the tenant
                        if (mediaItem != null && mediaItem.Size > objTennantSetting.MinimumKBSize && mediaItem.Size < objTennantSetting.MaxImageSize)
                        {
                            crunchOptions.fullname = mediaItem.Name + "." + mediaItem.Extension;
                            //As of now API is based out of post approach
                            CrunchImage.ProcessMediaItem(mediaItem, crunchOptions);
                        }
                        else
                        {
                            Log.Info(string.Format("Image Size is {0} {1}", mediaItem.Size, ",KB which is not fit in minimum & maximum size defined in setting"), this);
                        }
                    }
                    catch (System.Exception exception)
                    {
                        Log.Error(string.Format("Could not shrink item {0}", mediaItem.InnerItem.Paths.FullPath), exception, this);
                    }
                    ///Code to shrunk media item end
                    MediaUrlOptions options2 = new MediaUrlOptions(130, 130, false)
                    {
                        Thumbnail = true,
                        BackgroundColor = Color.Transparent,
                        Database = mediaItem.Database
                    };
                    string mediaUrl = MediaManager.GetMediaUrl(mediaItem, options2);
                    uploadedFileItemList.Add(new UploadedFileItem(fromStream.Name, fromStream.ID.ToString(), fromStream.ID.ToShortID().ToString(), mediaUrl));
                }
            }
                    ((dynamic)result.Result).uploadedFileItems = uploadedFileItemList;
            return (JsonResult)result;
        }
    }
}