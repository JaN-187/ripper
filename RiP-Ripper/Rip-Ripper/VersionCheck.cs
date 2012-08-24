// --------------------------------------------------------------------------------------------------------------------
// <copyright file="VersionCheck.cs" company="The Watcher">
//   Copyright (c) The Watcher Partial Rights Reserved.
//  This software is licensed under the MIT license. See license.txt for details.
// </copyright>
// <summary>
//   Code Named: RiP-Ripper
//   Function  : Extracts Images posted on RiP forums and attempts to fetch them to disk.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace RiPRipper
{
    using System;
    using System.Data;
    using System.Linq;
    using System.Reflection;

    /// <summary>
    /// Comparing Version Check from online xml file with Assembly
    /// </summary>
    public class VersionCheck
    {
        /// <summary>
        /// Version String of the Online Version
        /// </summary>
        public static string OnlineVersion { get; set; }

        /// <summary>
        /// Check if Update is available
        /// </summary>
        /// <returns>
        /// Returns bool if Update is available
        /// </returns>
        public static bool UpdateAvailable()
        {
            // Proper Format: (MAJOR).(MINOR).(BUILD).(REVISION) Decimal makes it easier to judge versions
            try
            {
                Assembly ripperASM = Assembly.GetExecutingAssembly();
                double dbCurrentVersion =
                    Double.Parse(
                        string.Format(
                            "{0}.{1}.{2}.{3}",
                            ripperASM.GetName().Version.Major,
                            ripperASM.GetName().Version.Minor,
                            ripperASM.GetName().Version.Build,
                            ripperASM.GetName().Version.Revision));

                DataSet ds = new DataSet();

                ds.ReadXml("http://www.watchersnet.de/rip-ripper/ripperUpdates.xml");

                foreach (DataRow row in from DataRow row in ds.Tables["ripper"].Rows
                                        let test = row["name"].ToString()
                                        where test.Equals("RiP-Ripper")
                                        select row)
                {
                    OnlineVersion = row["version"].ToString();
                }

                double dbLatestVersion = Double.Parse(OnlineVersion);

                if (dbLatestVersion > dbCurrentVersion)
                {
                    return true;
                }
            }
            catch (Exception)
            {
                return false;
            }

            return false;
        }
    }
}
