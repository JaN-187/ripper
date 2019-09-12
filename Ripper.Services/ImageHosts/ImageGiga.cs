//////////////////////////////////////////////////////////////////////////
// Code Named: VG-Ripper
// Function  : Extracts Images posted on RiP forums and attempts to fetch
//			   them to disk.
//
// This software is licensed under the MIT license. See license.txt for
// details.
// 
// Copyright (c) The Watcher
// Partial Rights Reserved.
// 
//////////////////////////////////////////////////////////////////////////
// This file is part of the RiP Ripper project base.

namespace Ripper.Services.ImageHosts
{
    using System;
    using System.Collections;
    using System.IO;
    using System.Net;
    using System.Threading;

    using Ripper.Core.Components;
    using Ripper.Core.Objects;

    /// <summary>
    /// Worker class to get images from ImageGiga.com
    /// </summary>
    public class ImageGiga : ServiceTemplate
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ImageGiga"/> class.
        /// </summary>
        /// <param name="sSavePath">
        /// The s save path.
        /// </param>
        /// <param name="strURL">
        /// The str url.
        /// </param>
        /// <param name="hTbl">
        /// The h tbl.
        /// </param>
        public ImageGiga(ref string sSavePath, ref string strURL, ref string thumbURL, ref string imageName, ref int imageNumber, ref Hashtable hashtable)
            : base(sSavePath, strURL, thumbURL, imageName, imageNumber, ref hashtable)
        {
        }

        /// <summary>
        /// Do the Download
        /// </summary>
        /// <returns>
        /// Return if Downloaded or not
        /// </returns>
        protected override bool DoDownload()
        {
            var strImgURL = this.ImageLinkURL;

            if (this.EventTable.ContainsKey(strImgURL))
            {
                return true;
            }

            var strFilePath = string.Empty;

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

            var ccObj = new CacheObject { IsDownloaded = false, FilePath = strFilePath, Url = strImgURL };

            try
            {
                this.EventTable.Add(strImgURL, ccObj);
            }
            catch (ThreadAbortException)
            {
                return true;
            }
            catch (Exception)
            {
                if (this.EventTable.ContainsKey(strImgURL))
                {
                    return false;
                }

                this.EventTable.Add(strImgURL, ccObj);
            }

            const string StartSrc = "<script type=\"text/javascript\" src=\"imgiga/showimg";

            var sPage = this.GetImageHostPage(ref strImgURL);

            if (sPage.Length < 10)
            {
                return false;
            }

            var iStartSrc = sPage.IndexOf(StartSrc);

            if (iStartSrc < 0)
            {
                return false;
            }

            iStartSrc += StartSrc.Length;

            var iEndSrc = sPage.IndexOf("\"", iStartSrc);

            if (iEndSrc < 0)
            {
                return false;
            }

            var sScriptUrl = strImgURL;

            sScriptUrl = sScriptUrl.Remove(sScriptUrl.IndexOf(@"img.php"));

            sScriptUrl += $"imgiga/showimg{sPage.Substring(iStartSrc, iEndSrc - iStartSrc)}";

            var sPage2 = this.GetImageHostPage(ref sScriptUrl);

            const string StartSrc2 = "href=\"";

            var iStartSrc2 = sPage2.IndexOf(StartSrc2);

            if (iStartSrc2 < 0)
            {
                return false;
            }

            iStartSrc2 += StartSrc2.Length;

            var iEndSrc2 = sPage2.IndexOf("\"", iStartSrc2);

            var strNewURL = sPage2.Substring(iStartSrc2, iEndSrc2 - iStartSrc2);

            strFilePath = strNewURL;

            if (strFilePath.Contains(".jpg"))
            {
                strFilePath = strFilePath.Remove(strFilePath.IndexOf(".jpg") + 4);
            }
            else if (strFilePath.Contains(".jpeg"))
            {
                strFilePath = strFilePath.Remove(strFilePath.IndexOf(".jpeg") + 5);
            }

            strFilePath = strFilePath.Substring(strFilePath.LastIndexOf("/") + 1);

            strFilePath = Path.Combine(this.SavePath, Utility.RemoveIllegalCharecters(strFilePath));

            //////////////////////////////////////////////////////////////////////////
            var newAlteredPath = Utility.GetSuitableName(strFilePath);

            if (strFilePath != newAlteredPath)
            {
                strFilePath = newAlteredPath;
                ((CacheObject)this.EventTable[this.ImageLinkURL]).FilePath = strFilePath;
            }

            strFilePath = Utility.CheckPathLength(strFilePath);
            ((CacheObject)this.EventTable[this.ImageLinkURL]).FilePath = strFilePath;

            try
            {
                var client = new WebClient();
                client.Headers.Add("Referer: " + strImgURL);
                client.Headers.Add("User-Agent: Mozilla/5.0 (Windows; U; Windows NT 5.2; en-US; rv:1.7.10) Gecko/20050716 Firefox/1.0.6");
                client.DownloadFile(strNewURL, strFilePath);
                client.Dispose();
            }
            catch (ThreadAbortException)
            {
                ((CacheObject)this.EventTable[strImgURL]).IsDownloaded = false;
                ThreadManager.GetInstance().RemoveThreadbyId(this.ImageLinkURL);

                return true;
            }
            catch (IOException ex)
            {
                // MainForm.DeleteMessage = ex.Message;
                // MainForm.Delete = true;
                ((CacheObject)this.EventTable[strImgURL]).IsDownloaded = false;
                ThreadManager.GetInstance().RemoveThreadbyId(this.ImageLinkURL);

                return true;
            }
            catch (WebException)
            {
                ((CacheObject)this.EventTable[strImgURL]).IsDownloaded = false;
                ThreadManager.GetInstance().RemoveThreadbyId(this.ImageLinkURL);

                return false;
            }

            ((CacheObject)this.EventTable[this.ImageLinkURL]).IsDownloaded = true;
            CacheController.Instance().LastPic = ((CacheObject)this.EventTable[this.ImageLinkURL]).FilePath = strFilePath;

            return true;
        }

        //////////////////////////////////////////////////////////////////////////
        
    }
}