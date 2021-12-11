using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace WeHateGlobalSettings
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.Title = "WeHateGlobalSettings by (Co)";

            var RobloxOld = Registry.CurrentUser.OpenSubKey("SOFTWARE\\WeHateGlobalSettings").GetValue("Old").ToString();
            var SetBack = Registry.ClassesRoot.OpenSubKey("roblox-player\\shell\\open\\command", true).GetValue("").ToString();
            var RobloxLauncherPath = RobloxOld.Split('%')[0].Split('"')[1];
            var RobloxBasePath = RobloxLauncherPath.Split(new[] { "\\Versions" }, StringSplitOptions.None)[0];
            Console.WriteLine("Looking for settings...");

            foreach (var FilePath in Directory.GetFiles(RobloxBasePath))
            {
                var FileName = Path.GetFileName(FilePath);
                if (FileName.Contains("BasicSettings"))
                {
                    Console.WriteLine(string.Format("Deleted {0}", FileName));
                    File.Delete(FilePath);
                }
            }

            var RobloxProtocol = Registry.ClassesRoot.OpenSubKey("roblox-player\\shell\\open\\command", true);
            RobloxProtocol.SetValue("", RobloxOld); // Set back for moment so roblox wont get mad

            Console.WriteLine("Launching Roblox...");
            var Roblox = new Process();
            var Settings = new ProcessStartInfo();
            Settings.UseShellExecute = true;
            Settings.FileName = RobloxLauncherPath;
            Settings.Arguments = string.Join(" ", args);
            Roblox.StartInfo = Settings;
            Roblox.Start();
            Console.WriteLine("Done!");

            Thread.Sleep(3000); // A little delay

            RobloxProtocol.SetValue("", SetBack); // Set back for next launch :)

        }
    }
}
