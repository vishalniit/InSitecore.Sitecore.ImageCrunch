using System;
using System.Collections.Generic;
using System.IO;
using System.Json;
using System.Net.Http;
using Mindtree.ImageCrunch.Entities;
using Mindtree.ImageCrunch.Interfaces;

namespace Mindtree.ImageCrunch.Kraken
{
    public class KrakenCruncher : ICruncher
    {
        public CrunchResult CrunchStream(Stream stream, CrunchOptions options)
        {
            var client = new HttpClient();
            var content = new MultipartFormDataContent();
            JsonObject parent = new JsonObject();
            JsonObject auth = null;
            #region Authentication
            ///JSON Object creation
            auth = new JsonObject
            {
                {
                            "api_key", options.APIKey
                        },
                        {
                            "api_secret", options.APISecret
                }
            };
            parent.Add("auth", auth);
            #endregion

            #region Configuration
            if (options.wait)
                parent.Add("wait", options.wait);
            else
                parent.Add("callback_url", options.callbackurl);
            if (options.dev)
                parent.Add("dev", options.dev);
            #endregion

            #region CustomImageQuality
            if (options.lossy)
            {
                parent.Add("lossy", options.lossy);
                if (options.quality != 0)
                    parent.Add("quality", options.quality);
            }
            #endregion

            JsonObject imageConversion = null;
            #region ImageConversion
            if (options.ImageConversion)
            {
                imageConversion = new JsonObject();
                switch (options.Format)
                {
                    case CrunchOptions.format.jpeg:
                        imageConversion.Add("format", "jpeg");
                        break;
                    case CrunchOptions.format.png:
                        imageConversion.Add("format", "png");
                        break;
                    case CrunchOptions.format.gif:
                        imageConversion.Add("format", "gif");
                        break;
                    default:
                        break;
                }
                if (options.background != null && options.background.Length > 0)
                    imageConversion.Add("background", options.background);
                if (options.keep_extension)
                    imageConversion.Add("keep_extension", options.keep_extension);
            }
            #endregion

            JsonObject resizevalues = null;
            #region ImageResizing
            if (options.ImageResizing)
            {
                resizevalues = new JsonObject();
                if (options.height != 0)
                    resizevalues.Add("height", options.height);
                if (options.width != 0)
                    resizevalues.Add("width", options.width);
                switch (options.Strategy)
                {
                    case CrunchOptions.strategy.portrait:
                        resizevalues.Add("strategy", "portrait");
                        break;
                    case CrunchOptions.strategy.landscape:
                        resizevalues.Add("strategy", "landscape");
                        break;
                    case CrunchOptions.strategy.auto:
                        resizevalues.Add("strategy", "auto");
                        break;
                    case CrunchOptions.strategy.crop:
                        resizevalues.Add("strategy", "crop");
                        switch (options.CropMode)
                        {
                            case CrunchOptions.cropmode.top:
                                resizevalues.Add("crop_mode", "top");
                                break;
                            case CrunchOptions.cropmode.northwest:
                                resizevalues.Add("crop_mode", "northwest");
                                break;
                            case CrunchOptions.cropmode.northeast:
                                resizevalues.Add("crop_mode", "northeast");
                                break;
                            case CrunchOptions.cropmode.west:
                                resizevalues.Add("crop_mode", "west");
                                break;
                            case CrunchOptions.cropmode.east:
                                resizevalues.Add("crop_mode", "east");
                                break;
                            case CrunchOptions.cropmode.southeast:
                                resizevalues.Add("crop_mode", "southeast");
                                break;
                            case CrunchOptions.cropmode.southwest:
                                resizevalues.Add("crop_mode", "southwest");
                                break;
                            case CrunchOptions.cropmode.south:
                                resizevalues.Add("crop_mode", "south");
                                break;
                            default:
                                resizevalues.Add("crop_mode", "center");
                                break;
                        }
                        break;
                    case CrunchOptions.strategy.exact:
                        resizevalues.Add("strategy", "exact");
                        break;
                    case CrunchOptions.strategy.fit:
                        resizevalues.Add("strategy", "fit");
                        break;
                    case CrunchOptions.strategy.fill:
                        resizevalues.Add("strategy", "fill");
                        resizevalues.Add("background", options.background);
                        break;
                    default:
                        resizevalues.Add("strategy", "auto");
                        break;
                }
                if (options.enhance)
                    resizevalues.Add("enhance", options.enhance);
            }
            #endregion

            #region WebP Compression
            if (options.webp)
            {
                parent.Add("webp", options.webp);
            }
            #endregion

            if (imageConversion != null)
            {
                parent.Add("convert", imageConversion);
            }
            if (resizevalues != null)
            {
                parent.Add("resize", resizevalues);
            }

            var stringContent = new StringContent(parent.ToString());

            content.Add(stringContent, "woop");
            var streamContent = new StreamContent(stream);
            //streamContent.Headers.ContentType = new MediaTypeHeaderValue("image/jpeg");
            content.Add(streamContent, "file ", options.fullname);

            HttpResponseMessage postAsync = client.PostAsync("https://api.kraken.io/v1/upload", content).Result;

            Response jsonResult = null;
            if (postAsync.IsSuccessStatusCode)
            {
                string stringResult = postAsync.Content.ReadAsStringAsync().Result;

                var dynamicResult = JsonValue.Parse(stringResult);
                if (dynamicResult.ContainsKey("success") && dynamicResult["success"].ReadAs(false))
                {
                    jsonResult = postAsync.Content.ReadAsAsync<Response>().Result;
                }
                else
                {
                    throw new Exception(string.Format("Error: {0}", dynamicResult.GetValue("error").ReadAs<string>()));
                }
            }
            else
            {
                return null;
            }

            HttpResponseMessage httpResponseMessage = client.GetAsync(jsonResult.Dest, HttpCompletionOption.ResponseHeadersRead).Result;

            if (!httpResponseMessage.IsSuccessStatusCode)
            {
                return null;
            }

            Stream result = httpResponseMessage.Content.ReadAsStreamAsync().Result;

            var returnObject = new Entities.CrunchResult();
            returnObject.FileStream = result;
            returnObject.Format = Path.GetExtension(jsonResult.Dest);

            return returnObject;
        }

        public decimal MaxImageSize
        {
            get
            {
                return int.MaxValue;
            }
        }
    }


}