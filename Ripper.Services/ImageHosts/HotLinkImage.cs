﻿//////////////////////////////////////////////////////////////////////////
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

using System;
using System.Collections;
using System.IO;
using System.Net;
using System.Threading;

namespace Ripper
{
    using Ripper.Core.Components;
    using Ripper.Core.Objects;

    /// <summary>
    /// Worker class to get images hosted on HotLinkImage.com
    /// </summary>
    public class HotLinkImage : ServiceTemplate
    {
        public HotLinkImage(ref string sSavePath, ref string strURL, ref string thumbURL, ref string imageName, ref int imageNumber, ref Hashtable hashtable)
            : base(sSavePath, strURL, thumbURL, imageName, imageNumber, ref hashtable)
        {
            // Add constructor logic here
        }


        protected override bool DoDownload()
        {
            var strImgURL = this.ImageLinkURL;

            if (this.EventTable.ContainsKey(strImgURL))
            {
                return true;
            }

            try
            {
                if (!Directory.Exists(this.SavePath))
                    Directory.CreateDirectory(this.SavePath);
            }
            catch (IOException ex)
            {
                // MainForm.DeleteMessage = ex.Message;
                // MainForm.Delete = true;
                return false;
            }

            var strFilePath = string.Empty;

            var CCObj = new CacheObject();
            CCObj.IsDownloaded = false;
            CCObj.FilePath = strFilePath;
            CCObj.Url = strImgURL;
            try
            {
                this.EventTable.Add(strImgURL, CCObj);
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
                else
                {
                    this.EventTable.Add(strImgURL, CCObj);
                }
            }

            var strIVPage = this.GetImageHostPage(ref strImgURL);

            if (strIVPage.Length < 10)
            {
                return false;
            }

            var strNewURL = strImgURL.Substring(0, strImgURL.IndexOf("/", 8) + 1);

            var iStartSRC = 0;
            var iEndSRC = 0;

            iStartSRC = strIVPage.IndexOf("adjusted size.</h1><script type=\"text/javascript\" src=\"");

            if (iStartSRC < 0)
            {
                return false;
            }

            iStartSRC += 55;

            iEndSRC = strIVPage.IndexOf("\"></script><br><p class=\"actions\">", iStartSRC);

            if (iEndSRC < 0)
            {
                return false;
            }

            strNewURL = strIVPage.Substring(iStartSRC, iEndSRC - iStartSRC);

            var strNewURL2 = strImgURL;

            var strNewURL3 = strNewURL2.Remove(strNewURL2.LastIndexOf(@"/") + 1)
                             + strNewURL.Substring(strNewURL.IndexOf("&loc=") + 5) + "/"
                             + strImgURL.Substring(strImgURL.IndexOf("?id=") + 4) + "."
                             + strNewURL.Substring(strNewURL.IndexOf("&ext=") + 5, 3);

            strFilePath = strImgURL.Substring(strImgURL.IndexOf("id=") + 3) + "."
                                                                            + strNewURL.Substring(
                                                                                strNewURL.IndexOf("&ext=") + 5,
                                                                                3);

            strFilePath = Path.Combine(this.SavePath, Utility.RemoveIllegalCharecters(strFilePath));

            //////////////////////////////////////////////////////////////////////////
            var NewAlteredPath = Utility.GetSuitableName(strFilePath);
            if (strFilePath != NewAlteredPath)
            {
                strFilePath = NewAlteredPath;
                ((CacheObject)this.EventTable[this.ImageLinkURL]).FilePath = strFilePath;
            }

            try
            {
                var client = new WebClient();
                client.Headers.Add("Referer: " + strNewURL);
                client.DownloadFile(strNewURL3, strFilePath);
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

            // CacheController.GetInstance().u_s_LastPic = ((CacheObject)eventTable[mstrURL]).FilePath;
            CacheController.Instance().LastPic =
                ((CacheObject)this.EventTable[this.ImageLinkURL]).FilePath = strFilePath;

            return true;
        }

        //////////////////////////////////////////////////////////////////////////
        
    }
}
