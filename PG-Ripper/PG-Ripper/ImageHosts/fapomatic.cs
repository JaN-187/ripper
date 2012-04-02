//////////////////////////////////////////////////////////////////////////
// Code Named: PG-Ripper
// Function  : Extracts Images posted on PG forums and attempts to fetch
//			   them to disk.
//
// This software is licensed under the MIT license. See license.txt for
// details.
// 
// Copyright (c) The Watcher 
// Partial Rights Reserved.
// 
//////////////////////////////////////////////////////////////////////////
// This file is part of the PG-Ripper project base.

using System;
using System.Collections;
using System.Web;
using System.Net;
using System.IO;
using System.Text;
using System.Threading;

namespace PGRipper
{
    using PGRipper.Objects;

    /// <summary>
	/// Worker class to get images from fapomatic.com
	/// </summary>
	public class fapomatic : ServiceTemplate
	{
		public fapomatic(ref string sSavePath, ref string strURL, ref Hashtable hTbl)
			: base( sSavePath, strURL, ref hTbl )
		{
		}

		protected override bool DoDownload()
		{
		
			string strImgURL = mstrURL;

			if (eventTable.ContainsKey(strImgURL))	
			{
				return true;;
			}

			string strFilePath = string.Empty;

			
			strFilePath = strImgURL.Substring(  strImgURL.IndexOf( "f=" ) + 2 );

            try
            {
                if (!Directory.Exists(mSavePath))
                    Directory.CreateDirectory(mSavePath);
            }
            catch (IOException ex)
            {
                MainForm.DeleteMessage = ex.Message;
                 MainForm.Delete = true;

                return false;
            }

            if (mSavePath.Contains("/"))
                strFilePath = mSavePath + "/" + Utility.RemoveIllegalCharecters(strFilePath); //strFilePath;
            else
                strFilePath = mSavePath + "\\" + Utility.RemoveIllegalCharecters(strFilePath); //strFilePath;

			CacheObject CCObj = new CacheObject();
			CCObj.IsDownloaded = false;
			CCObj.FilePath = strFilePath ;
			CCObj.Url = strImgURL;
			try
			{
				eventTable.Add(strImgURL, CCObj);
			}
			catch (ThreadAbortException)
			{
				return true;
			}
			catch(System.Exception)
			{

				if (eventTable.ContainsKey(strImgURL))	
				{
					return false;
				}
				else
				{
					eventTable.Add(strImgURL, CCObj);
				}
			}
			
			int iLocStart = strImgURL.IndexOf( "loc=" );
			if (iLocStart < 0)
			{
				return false;
			}
			iLocStart += 4;

            int iNameStart = strImgURL.IndexOf("f=");
            if (iNameStart < 0)
            {
                return false;
            }
            iNameStart += 2;

            string strNewURL = string.Format("http://fapomatic.com/{0}/{1}",
                strImgURL.Substring(iLocStart, strImgURL.IndexOf("&", iLocStart) - iLocStart),
                strImgURL.Substring(iNameStart));
			
			//////////////////////////////////////////////////////////////////////////
			HttpWebRequest lHttpWebRequest;
			HttpWebResponse lHttpWebResponse;
			Stream lHttpWebResponseStream;

			string NewAlteredPath = Utility.GetSuitableName(strFilePath);
			if (strFilePath != NewAlteredPath)
			{
				strFilePath = NewAlteredPath;
				((CacheObject)eventTable[mstrURL]).FilePath = strFilePath;
			}

			//FileStream lFileStream = new FileStream(strFilePath, FileMode.Create);

			
			//int bytesRead;

			try
			{
				lHttpWebRequest = (HttpWebRequest)WebRequest.Create(strNewURL);

				lHttpWebRequest.UserAgent = "Mozilla/5.0 (Windows; U; Windows NT 5.2; en-US; rv:1.7.10) Gecko/20050716 Firefox/1.0.6";
				lHttpWebRequest.Headers.Add("Accept-Language: en-us,en;q=0.5");
				lHttpWebRequest.Headers.Add("Accept-Encoding: gzip,deflate");
				lHttpWebRequest.Headers.Add("Accept-Charset: ISO-8859-1,utf-8;q=0.7,*;q=0.7");
				lHttpWebRequest.Referer = strImgURL;
				lHttpWebRequest.Accept = "image/png,*/*;q=0.5";
				lHttpWebRequest.KeepAlive = true;
				
				lHttpWebResponse = (HttpWebResponse)lHttpWebRequest.GetResponse();
				lHttpWebResponseStream = lHttpWebRequest.GetResponse().GetResponseStream();
				
				if (lHttpWebResponse.ContentType.IndexOf("image") < 0)
				{
					//lFileStream.Close();
					return false;
				}

				lHttpWebResponseStream.Close();

				System.Net.WebClient client = new WebClient();
				client.Headers.Add("Accept-Language: en-us,en;q=0.5");
				client.Headers.Add("Accept-Encoding: gzip,deflate");
				client.Headers.Add("Accept-Charset: ISO-8859-1,utf-8;q=0.7,*;q=0.7");
				client.Headers.Add("Referer: " + strImgURL );
				client.Headers.Add("User-Agent: Mozilla/5.0 (Windows; U; Windows NT 5.2; en-US; rv:1.7.10) Gecko/20050716 Firefox/1.0.6");
				client.DownloadFile(strNewURL, strFilePath);

				/*do
				{
					// Read up to 1000 bytes into the bytesRead array.
					bytesRead = lHttpWebResponseStream.Read(byteBuffer, 0, 999);
					lFileStream.Write(byteBuffer, 0, bytesRead);
				}while(bytesRead > 0);

				lHttpWebResponseStream.Close();*/

            }
            catch (ThreadAbortException)
            {
                ((CacheObject)eventTable[strImgURL]).IsDownloaded = false;
                ThreadManager.GetInstance().RemoveThreadbyId(mstrURL);

                return true;
            }
            catch (IOException ex)
            {
                MainForm.DeleteMessage = ex.Message;
                MainForm.Delete = true;

                ((CacheObject)eventTable[strImgURL]).IsDownloaded = false;
                ThreadManager.GetInstance().RemoveThreadbyId(mstrURL);

                return true;
            }
            catch (WebException)
            {
                ((CacheObject)eventTable[strImgURL]).IsDownloaded = false;
                ThreadManager.GetInstance().RemoveThreadbyId(mstrURL);

                return false;
            }

            
			//((CacheObject)eventTable[strImgURL]).IsDownloaded = true;
            ((CacheObject)eventTable[mstrURL]).IsDownloaded = true;
			CacheController.GetInstance().uSLastPic = ((CacheObject)eventTable[strImgURL]).FilePath;

			return true;
		}

	}
}
