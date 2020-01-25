using log4net;
using Microsoft.Win32;
using System;
using System.Diagnostics;
using System.IO;

namespace DllInjector
{
    /// <summary>
    /// Runs the Microsoft "ListDLLs.exe" to verify that the DLL injection was successful.
    /// Sometimes the injection reports as successful, but the DLL does not persist. (unloaded by anti virus?)
    /// </summary>
    public static class InjectionVerifier {
        private static readonly ILog Logger = LogManager.GetLogger(typeof(InjectionVerifier));
        // ReSharper disable once StringLiteralTypo
        private static readonly string Filename = "Listdlls.exe";

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pid"></param>
        /// <param name="dll"></param>
        /// <returns></returns>
        public static bool VerifyInjection(long pid, string dll) {
            FixRegistryNagOnListDlls();

            Logger.Info("Running ListDLLs...");

            if (File.Exists(Filename)) {
                ProcessStartInfo startInfo = new ProcessStartInfo
                {
                    FileName = Filename,
                    Arguments = $"-d {dll}",
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                };

                Process processTemp = new Process
                {
                    StartInfo = startInfo,
                    EnableRaisingEvents = true
                };

                try {
                    string spid = pid.ToString();
                    processTemp.Start();
                    while (!processTemp.StandardOutput.EndOfStream) {
                        string line = processTemp.StandardOutput.ReadLine();
                        if (line?.EndsWith(spid) == true)
                        {
                            Logger.Info("Injection successfully verified");
                            return true;
                        }
                    }
                }
                catch (Exception ex) {
                    Logger.Warn("Exception while attempting to verify injection.. " + ex.Message + ex.StackTrace);
                }
            }
            else {
                Logger.Warn("Could not find ListDLLs.exe, unable to verify successful injection.");
            }
            return false;
        }

        /// <summary>
        /// Adds registry entries to prevent ListDLL nag screens
        /// </summary>
        private static void FixRegistryNagOnListDlls()
        {
            try
            {
                RegistryKey key = Registry.CurrentUser.OpenSubKey("Software", true);

                key?.CreateSubKey("Sysinternals");
                key = key?.OpenSubKey("Sysinternals", true);

                key?.CreateSubKey("ListDLLs");
                key = key?.OpenSubKey("ListDLLs", true);

                key?.SetValue("EulaAccepted", 1);

                key?.Close();
            }
            catch (Exception ex)
            {
                Logger.Warn("Error trying to create registry keys, this is not critical.");
                Logger.Warn(ex.Message);
            }
        }
    }
}
