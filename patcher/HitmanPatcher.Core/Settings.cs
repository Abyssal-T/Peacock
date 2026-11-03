using System;
using System.Collections.Generic;
using System.IO;

namespace HitmanPatcher
{
    public class Settings
    {
        public MemoryPatcher.Options patchOptions;
        public bool startInTray;
        public bool minimizeToTray;
        public bool darkModeEnabled;
        public List<string> trayDomains;
        public bool devOverrideEntitlements;
        public List<string> devSelectedProducts;

        public Settings()
        {
            // Default settings
            patchOptions = new MemoryPatcher.Options
            {
                CustomConfigDomain = "127.0.0.1",
                UseHttp = true,
                DisableCertPinning = true,
                AlwaysSendAuthHeader = true,
                SetCustomConfigDomain = true,
                EnableDynamicResources = true,
                DisableForceOfflineOnFailedDynamicResources = true,
            };
            darkModeEnabled = false;
            startInTray = false;
            minimizeToTray = false;
            trayDomains = new List<string>();
            devOverrideEntitlements = false;
            devSelectedProducts = new List<string>();
        }

        private static string GetSavePath()
        {
            if (!Directory.Exists(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + @"\PeacockProject"))
            {
                Directory.CreateDirectory(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + @"\PeacockProject");
            }

            string appData = Environment.GetFolderPath(Environment
                .SpecialFolder
                .ApplicationData);

            string folder = $@"{appData}\PeacockProject\";
            string config1 = folder + "peacock_patcher.conf";
            string config2 = folder + "peacock_patcher2.conf";

            if (File.Exists(config1))
            {
                File.Delete(config1);
            }

            return config2;
        }

        // The server (components/devEntitlements.ts) reads this file from its working directory,
        // which is the folder PeacockPatcher.exe lives in. Deleting it turns the override off.
        private const string DevEntitlementsFileName = "devEntitlements.txt";

        private void WriteDevEntitlementsFile()
        {
            try
            {
                string path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, DevEntitlementsFileName);

                if (devOverrideEntitlements)
                {
                    File.WriteAllLines(path, devSelectedProducts);
                }
                else if (File.Exists(path))
                {
                    File.Delete(path);
                }
            }
            catch (Exception ex)
            {
                // Dev testing only - never stop the patcher over this, but don't fail silently either.
                System.Diagnostics.Debug.WriteLine("Could not update " + DevEntitlementsFileName + ": " + ex.Message);
            }
        }

        public void SaveToFile()
        {
            List<string> lines = new List<string>();
            lines.Add(string.Format("CustomConfigDomain={0}", patchOptions.CustomConfigDomain));
            lines.Add(string.Format("UseHttp={0}", patchOptions.UseHttp));
            lines.Add(string.Format("DisableForceDynamicResources={0}", patchOptions.DisableForceOfflineOnFailedDynamicResources));
			lines.Add(string.Format("DarkModeEnabled={0}", darkModeEnabled));
            lines.Add(string.Format("startInTray={0}", startInTray));
            lines.Add(string.Format("minToTray={0}", minimizeToTray));
            lines.Add(string.Format("devOverrideEntitlements={0}", devOverrideEntitlements));

            foreach (string product in devSelectedProducts)
            {
                lines.Add(string.Format("devProduct={0}", product));
            }

            foreach (string domain in trayDomains)
            {
                lines.Add(string.Format("trayDomain={0}", domain));
            }

            File.WriteAllLines(GetSavePath(), lines);
            WriteDevEntitlementsFile();
        }

        public static Settings GetFromFile()
        {
            Settings result = new Settings();
            if (File.Exists(GetSavePath()))
            {
                string[] lines = File.ReadAllLines(GetSavePath());
                if (lines.Length == 1)
                {
                    result.patchOptions.CustomConfigDomain = lines[0];
                }
                else
                {
                    foreach (string line in lines)
                    {
                        if (line == "")
                            continue;

                        string[] linecontents = line.Split(new char[] { '=' }, 2);
                        if (linecontents.Length < 2)
                            continue;

                        switch (linecontents[0])
                        {
                            case "CustomConfigDomain":
                                result.patchOptions.CustomConfigDomain = linecontents[1];
                                break;
                            case "UseHttp":
                                if (bool.TryParse(linecontents[1], out bool useHttp)) result.patchOptions.UseHttp = useHttp;
                                break;
                            case "DisableForceDynamicResources":
                                if (bool.TryParse(linecontents[1], out bool disableForce)) result.patchOptions.DisableForceOfflineOnFailedDynamicResources = disableForce;
                                break;
							case "DarkModeEnabled":
								if (bool.TryParse(linecontents[1], out bool dark)) result.darkModeEnabled = dark;
								break;
                            case "startInTray":
                                if (bool.TryParse(linecontents[1], out bool tray)) result.startInTray = tray;
                                break;
                            case "minToTray":
                                if (bool.TryParse(linecontents[1], out bool minTray)) result.minimizeToTray = minTray;
                                break;
                            case "devOverrideEntitlements":
                                if (bool.TryParse(linecontents[1], out bool devOverride)) result.devOverrideEntitlements = devOverride;
                                break;
                            case "devProduct":
                                result.devSelectedProducts.Add(linecontents[1]);
                                break;
                            case "trayDomain":
                                result.trayDomains.Add(linecontents[1]);
                                break;
                        }
                    }
                }
            }
            return result;
        }
    }
}
