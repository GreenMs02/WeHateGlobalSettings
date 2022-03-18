using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Setup
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.Title = "We Hate AC Roblox Invention by (Co)";
            Console.WriteLine("Configuring... (Made by (Co))");
            if (Registry.CurrentUser.OpenSubKey("SOFTWARE\\WeHateGlobalSettings") == null)
            {
                Registry.CurrentUser.CreateSubKey("SOFTWARE\\WeHateGlobalSettings");
                var ClassesRoot = Registry.ClassesRoot;
                var Key = ClassesRoot.OpenSubKey("roblox-player\\shell\\open\\command", true);
                var Old = Key.GetValue("").ToString();

                Registry.CurrentUser.OpenSubKey("SOFTWARE\\WeHateGlobalSettings", true).SetValue("Old", Old);
                Key.SetValue("", string.Format("\"{0}\" %1", Path.GetFullPath("WeHateGlobalSettings.exe")));

                Console.WriteLine("Setup is complete!");
            }
            else
            {
                Console.WriteLine("Do you want uninstall? Type Y to uninstall");
                var IsUninstall = Console.ReadLine().ToLower().Contains("y"); // just incase they type yes lol
                if (IsUninstall)
                {
                    Registry.ClassesRoot.OpenSubKey("roblox-player\\shell\\open\\command", true).SetValue("", Registry.CurrentUser.OpenSubKey("SOFTWARE\\WeHateGlobalSettings").GetValue("Old").ToString());
                    Registry.CurrentUser.DeleteSubKey("SOFTWARE\\WeHateGlobalSettings");
                    Console.WriteLine("Done!");
                }
            }

            Thread.Sleep(2000);
        }
    }
}
