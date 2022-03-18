using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace WeHateGlobalSettings
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.Title = "WeHateAC by (Co)";

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

            Process[] Processes;
            while ((Processes = Process.GetProcessesByName("RobloxPlayerBeta")).Length < 1) // Wait for RobloxPlayerBeta.exe
                Thread.Sleep(100); // Dont put much delay so the bypass would works!

            Roblox = Processes.First();
            SigScanSharp Sigscan = new SigScanSharp(Roblox.Handle);
            Sigscan.SelectModule(Roblox.MainModule);

            // I convert C++ code to C# code, All credits goes to iivillian & 0x90 & gogo
            IntPtr Check = traceRelativeCall(Roblox.Handle, Sigscan.FindPattern("E8 ? ? ? ? 83 78 10 ? 75 41"));
            IntPtr Flag = ReadMemory<IntPtr>(Roblox.Handle, Check + 0x22);
            WriteMemory(Roblox.Handle, Flag, 0xEAC);

            Thread.Sleep(2000); // A little delay again

            RobloxProtocol.SetValue("", SetBack); // Set back for next launch :)
        }

        private static IntPtr traceRelativeCall(IntPtr Handle, IntPtr call)
        {
            return IntPtr.Add(ReadMemory<IntPtr>(Handle, call + 1), (call.ToInt32() + 5));
        }


        private static int WrittenBytes = 0;
        private static void WriteMemory(IntPtr Handle, IntPtr Address, object Value)
        {
            var buffer = StructureToByteArray(Value);

            WriteProcessMemory(Handle, Address, buffer, (uint)buffer.Length, ref WrittenBytes);
        }

        public static T ReadMemory<T>(IntPtr Handle, IntPtr address) where T : struct
        {
            var ByteSize = Marshal.SizeOf(typeof(T));

            var buffer = new byte[ByteSize];

            ReadProcessMemory(Handle, address, buffer, buffer.Length, ref WrittenBytes);

            return ByteArrayToStructure<T>(buffer);
        }

        [DllImport("kernel32.dll", SetLastError = true)]
        public static extern bool WriteProcessMemory(IntPtr hProcess, IntPtr lpBaseAddress, byte[] lpBuffer, uint nSize, ref int lpNumberOfBytesWritten);


        [DllImport("kernel32.dll", SetLastError = true)]
        public static extern bool ReadProcessMemory(IntPtr hProcess, IntPtr lpBaseAddress, byte[] lpBuffer, int dwSize, ref int lpNumberOfBytesRead);

        private static byte[] StructureToByteArray(object obj)
        {
            var length = Marshal.SizeOf(obj);

            var array = new byte[length];

            var pointer = Marshal.AllocHGlobal(length);

            Marshal.StructureToPtr(obj, pointer, true);
            Marshal.Copy(pointer, array, 0, length);
            Marshal.FreeHGlobal(pointer);

            return array;
        }
        private static T ByteArrayToStructure<T>(byte[] bytes) where T : struct
        {
            var handle = GCHandle.Alloc(bytes, GCHandleType.Pinned);
            try
            {
                return (T)Marshal.PtrToStructure(handle.AddrOfPinnedObject(), typeof(T));
            }
            finally
            {
                handle.Free();
            }
        }
    }
}
