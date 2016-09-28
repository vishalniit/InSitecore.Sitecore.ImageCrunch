﻿using System.Linq;
using Sitecore.Data.Items;
using Sitecore.Diagnostics;
using Sitecore.IO;
using Sitecore.Resources.Media;
using Sitecore.SecurityModel;
using Sitecore.Web;
using Sitecore.Pipelines.Upload;
using System.Web;
using Sitecore.Zip;

namespace Mindtree.ImageCrunch.Pipelines
{
    /// <summary>
    /// Saves the uploaded files.
    /// </summary>
    public class Save : UploadProcessor
    {
        /// <summary>
        /// Runs the processor.
        /// </summary>
        /// <param name="args">The arguments.</param>
        /// <exception cref="T:System.Exception"><c>Exception</c>.</exception>
        public void Process(UploadArgs args)
        {
            Assert.ArgumentNotNull(args, "args");
            for (int i = 0; i < args.Files.Count; i++)
            {
                HttpPostedFile httpPostedFile = args.Files[i];
                if (!string.IsNullOrEmpty(httpPostedFile.FileName))
                {
                    try
                    {
                        bool flag = UploadProcessor.IsUnpack(args, httpPostedFile);
                        if (args.FileOnly)
                        {
                            if (flag)
                            {
                                Save.UnpackToFile(args, httpPostedFile);
                            }
                            else
                            {
                                string filename = this.UploadToFile(args, httpPostedFile);
                                if (i == 0)
                                {
                                    args.Properties["filename"] = FileHandle.GetFileHandle(filename);
                                }
                            }
                        }
                        else
                        {
                            MediaUploader mediaUploader = new MediaUploader
                            {
                                File = httpPostedFile,
                                Unpack = flag,
                                Folder = args.Folder,
                                Versioned = args.Versioned,
                                Language = args.Language,
                                AlternateText = args.GetFileParameter(httpPostedFile.FileName, "alt"),
                                Overwrite = args.Overwrite,
                                FileBased = args.Destination == UploadDestination.File
                            };
                            System.Collections.Generic.List<MediaUploadResult> list;
                            using (new SecurityDisabler())
                            {
                                list = mediaUploader.Upload();
                            }
                            Log.Audit(this, "Upload: {0}", new string[]
                            {
                                httpPostedFile.FileName
                            });
                            foreach (MediaUploadResult current in list)
                            {
                                this.ProcessItem(args, current.Item, current.Path);
                            }
                        }
                    }
                    catch (System.Exception exception)
                    {
                        Log.Error("Could not save posted file: " + httpPostedFile.FileName, exception, this);
                        throw;
                    }
                }
            }
        }
        /// <summary>
        /// Processes the item.
        /// </summary>
        /// <param name="args">The arguments.</param>
        /// <param name="mediaItem">The media item.</param>
        /// <param name="path">The path.</param>
        private void ProcessItem(UploadArgs args, MediaItem mediaItem, string path)
        {
            Assert.ArgumentNotNull(args, "args");
            Assert.ArgumentNotNull(mediaItem, "mediaItem");
            Assert.ArgumentNotNull(path, "path");
            if (args.Destination == UploadDestination.Database)
            {
                Log.Info("Media Item has been uploaded to database: " + path, this);
            }
            else
            {
                Log.Info("Media Item has been uploaded to file system: " + path, this);
            }
            args.UploadedItems.Add(mediaItem.InnerItem);
        }
        /// <summary>
        /// Unpacks to file.
        /// </summary>
        /// <param name="args">The arguments.</param>
        /// <param name="file">The file.</param>
        private static void UnpackToFile(UploadArgs args, HttpPostedFile file)
        {
            Assert.ArgumentNotNull(args, "args");
            Assert.ArgumentNotNull(file, "file");
            string filename = FileUtil.MapPath(TempFolder.GetFilename("temp.zip"));
            file.SaveAs(filename);
            using (ZipReader zipReader = new ZipReader(filename))
            {
                foreach (ZipEntry current in zipReader.Entries)
                {
                    string text = FileUtil.MakePath(args.Folder, current.Name, '\\');
                    if (current.IsDirectory)
                    {
                        System.IO.Directory.CreateDirectory(text);
                    }
                    else
                    {
                        if (!args.Overwrite)
                        {
                            text = FileUtil.GetUniqueFilename(text);
                        }
                        System.IO.Directory.CreateDirectory(System.IO.Path.GetDirectoryName(text));
                        lock (FileUtil.GetFileLock(text))
                        {
                            FileUtil.CreateFile(text, current.GetStream(), true);
                        }
                    }
                }
            }
        }
        /// <summary>
        /// Uploads to file.
        /// </summary>
        /// <param name="args">The arguments.</param>
        /// <param name="file">The file.</param>
        /// <returns>The name of the uploaded file</returns>
        private string UploadToFile(UploadArgs args, HttpPostedFile file)
        {
            Assert.ArgumentNotNull(args, "args");
            Assert.ArgumentNotNull(file, "file");
            string text = FileUtil.MakePath(args.Folder, System.IO.Path.GetFileName(file.FileName), '\\');
            if (!args.Overwrite)
            {
                text = FileUtil.GetUniqueFilename(text);
            }
            file.SaveAs(text);
            Log.Info("File has been uploaded: " + text, this);
            return Assert.ResultNotNull<string>(text);
        }
    }

}
