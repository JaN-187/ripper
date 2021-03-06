// --------------------------------------------------------------------------------------------------------------------
// <copyright file="FastPic.cs" company="The Watcher">
//   Copyright (c) The Watcher Partial Rights Reserved.
//  This software is licensed under the MIT license. See license.txt for details.
// </copyright>
// <summary>
//   Code Named: VG-Ripper
//   Function  : Extracts Images posted on RiP forums and attempts to fetch them to disk.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace Ripper.Services.ImageHosts
{
    using System;
    using System.Collections;
    using System.IO;
    using System.Net;
    using System.Text.RegularExpressions;
    using System.Threading;

    using Ripper.Core.Components;
    using Ripper.Core.Objects;

    /// <summary>
    /// Worker class to get images from FastPic.ru
    /// </summary>
    public class FastPic : ServiceTemplate
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="FastPic"/> class.
        /// </summary>
        /// <param name="savePath">The arguments save path.</param>
        /// <param name="imageUrl">The string URL.</param>
        /// <param name="thumbURL">The thumb URL.</param>
        /// <param name="imageName">Name of the image.</param>
        /// <param name="hashtable">The authentication table.</param>
        public FastPic(ref string savePath, ref string imageUrl, ref string thumbURL, ref string imageName, ref int imageNumber, ref Hashtable hashtable)
            : base(savePath, imageUrl, thumbURL, imageName, imageNumber, ref hashtable)
        {
        }

        /// <summary>
        /// Do the Download
        /// </summary>
        /// <returns>
        /// Returns if the Image was downloaded
        /// </returns>
        protected override bool DoDownload()
        {
            var imageURL = this.ImageLinkURL;
            var filePath = string.Empty;

            if (this.EventTable.ContainsKey(imageURL))
            {
                return true;
            }

            try
            {
                if (!Directory.Exists(this.SavePath))
                {
                    Directory.CreateDirectory(this.SavePath);
                }
            }
            catch (IOException ex)
            {
                // MainForm.DeleteMessage = ex.Message;
                // MainForm.Delete = true;
                return false;
            }

            var cacheObject = new CacheObject { IsDownloaded = false, FilePath = filePath, Url = imageURL };

            try
            {
                this.EventTable.Add(imageURL, cacheObject);
            }
            catch (ThreadAbortException)
            {
                return true;
            }
            catch (Exception)
            {
                if (this.EventTable.ContainsKey(imageURL))
                {
                    return false;
                }

                this.EventTable.Add(imageURL, cacheObject);
            }

            var page = this.GetImageHostPage(ref imageURL);

            if (page.Length < 10)
            {
                ((CacheObject)this.EventTable[imageURL]).IsDownloaded = false;
                return false;
            }

            string imageDownloadURL;
            
            var match = Regex.Match(
                page,
                @"loading_img = '(?<inner>[^']*)';",
                RegexOptions.Compiled);

            if (match.Success)
            {
                imageDownloadURL = match.Groups["inner"].Value.Replace("&amp;", "&");
            }
            else
            {
                ((CacheObject)this.EventTable[imageURL]).IsDownloaded = false;
                return false;
            }

            // Set Image Name instead of using random name
            filePath = this.GetImageName(this.PostTitle, imageDownloadURL, this.ImageNumber);

            filePath = Path.Combine(this.SavePath, Utility.RemoveIllegalCharecters(filePath));

            if (!Directory.Exists(this.SavePath))
            {
                Directory.CreateDirectory(this.SavePath);
            }

            //////////////////////////////////////////////////////////////////////////
            var newAlteredPath = Utility.GetSuitableName(filePath);

            if (filePath != newAlteredPath)
            {
                filePath = newAlteredPath;
                ((CacheObject)this.EventTable[this.ImageLinkURL]).FilePath = filePath;
            }

            try
            {
                var webClient = new WebClient();
                webClient.Headers.Add($"Referer: {imageURL}");
                webClient.DownloadFile(imageDownloadURL, filePath);
            }
            catch (ThreadAbortException)
            {
                ((CacheObject)this.EventTable[imageURL]).IsDownloaded = false;
                ThreadManager.GetInstance().RemoveThreadbyId(this.ImageLinkURL);

                return true;
            }
            catch (IOException ex)
            {
                // MainForm.DeleteMessage = ex.Message;
                // MainForm.Delete = true;
                ((CacheObject)this.EventTable[imageURL]).IsDownloaded = false;
                ThreadManager.GetInstance().RemoveThreadbyId(this.ImageLinkURL);

                return true;
            }
            catch (WebException)
            {
                ((CacheObject)this.EventTable[imageURL]).IsDownloaded = false;
                ThreadManager.GetInstance().RemoveThreadbyId(this.ImageLinkURL);

                return false;
            }

            ((CacheObject)this.EventTable[this.ImageLinkURL]).IsDownloaded = true;
            CacheController.Instance().LastPic = ((CacheObject)this.EventTable[this.ImageLinkURL]).FilePath = filePath;

            return true;
        }

        //////////////////////////////////////////////////////////////////////////
    }
}