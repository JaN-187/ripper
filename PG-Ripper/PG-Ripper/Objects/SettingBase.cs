﻿//////////////////////////////////////////////////////////////////////////
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


namespace PGRipper.Objects
{
    /// <summary>
    /// The Settings Base
    /// </summary>
    public class SettingBase : object
    {
        /// <summary>
        /// Current Forum Url Setting
        /// </summary>
        public string sForumUrl { get; set; }


        /// <summary>
        /// "Clipboard Watch" Setting
        /// </summary>
        public bool bClipBWatch { get; set; }

        /// <summary>
        /// Create Folder for Every Post Setting
        /// </summary>
        public bool bSubDirs { get; set; }

        /// <summary>
        /// Create New Folder for Every Thread Setting
        /// </summary>
        public bool bDownInSepFolder { get; set; }

        /// <summary>
        /// Automatically presses Thank You Button Setting
        /// </summary>
        public bool bAutoThank { get; set; }

        /// <summary>
        /// Save PostIDs for Checking Setting
        /// </summary>
        public bool bSavePids { get; set; }

        /// <summary>
        /// Show Tray PopUps Setting
        /// </summary>
        public bool bShowPopUps { get; set; }

        /// <summary>
        /// Show Complete all Downloads PopUp Setting
        /// </summary>
        public bool bShowCompletePopUp { get; set; }

        /// <summary>
        /// "Always on Top" Setting
        /// </summary>
        public bool bTopMost { get; set; }

        /// <summary>
        /// min. Image Count for Thanks Setting
        /// </summary>
        public int iMinImageCount { get; set; }

        /// <summary>
        /// Max. Multi. Downloads Setting
        /// </summary>
        public int iThreadLimit { get; set; }

        /// <summary>
        /// The Downloadfolder Location
        /// </summary>
        public string sDownloadFolder { get; set; }

        /// <summary>
        /// "Download Options" Setting
        /// </summary>
        public string sDownloadOptions { get; set; }

        /// <summary>
        /// Current Language Setting
        /// </summary>
        public string sLanguage { get; set; }

        /// <summary>
        /// Username
        /// </summary>
        public string sUser { get; set; }

        /// <summary>
        /// User Password
        /// </summary>
        public string sPass { get; set; }

        /// <summary>
        /// Window Position Left
        /// </summary>
        public int iWindowLeft { get; set; }

        /// <summary>
        /// Window Position Top
        /// </summary>
        public int iWindowTop { get; set; }

        /// <summary>
        /// Window Width
        /// </summary>
        public int iWindowWidth { get; set; }

        /// <summary>
        /// Window Height
        /// </summary>
        public int iWindowHeight { get; set; }
    }
}