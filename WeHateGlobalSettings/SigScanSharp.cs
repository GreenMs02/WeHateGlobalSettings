using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.InteropServices;

public class SigScanSharp
{
    private IntPtr g_hProcess;

    private byte[] g_arrModuleBuffer;

    private IntPtr g_lpModuleBase;

    private Dictionary<string, string> g_dictStringPatterns;

    public SigScanSharp(IntPtr hProc)
    {
        g_hProcess = hProc;
        g_dictStringPatterns = new Dictionary<string, string>();
    }
    public bool SelectModule(ProcessModule targetModule)
    {
        g_lpModuleBase = targetModule.BaseAddress;
        g_arrModuleBuffer = new byte[targetModule.ModuleMemorySize];

        g_dictStringPatterns.Clear();

        return Win32.ReadProcessMemory(g_hProcess, g_lpModuleBase, g_arrModuleBuffer, targetModule.ModuleMemorySize);
    }

    private bool PatternCheck(int nOffset, byte[] arrPattern)
    {
        for (int i = 0; i < arrPattern.Length; i++)
        {
            if (arrPattern[i] == 0x0)
                continue;

            if (arrPattern[i] != this.g_arrModuleBuffer[nOffset + i])
                return false;
        }

        return true;
    }
    public IntPtr FindPattern(string szPattern)
    {
        if (g_arrModuleBuffer == null || g_lpModuleBase == IntPtr.Zero)
            throw new Exception("Selected module is null");


        byte[] arrPattern = ParsePatternString(szPattern);

        for (int nModuleIndex = 0; nModuleIndex < g_arrModuleBuffer.Length; nModuleIndex++)
        {
            if (this.g_arrModuleBuffer[nModuleIndex] != arrPattern[0])
                continue;

            if (PatternCheck(nModuleIndex, arrPattern))
            {
                return (IntPtr)(g_lpModuleBase + nModuleIndex);
            }
        }
        return IntPtr.Zero;
    }

    private byte[] ParsePatternString(string szPattern)
    {
        List<byte> patternbytes = new List<byte>();

        foreach (var szByte in szPattern.Split(' '))
            patternbytes.Add(szByte == "?" ? (byte)0x0 : Convert.ToByte(szByte, 16));

        return patternbytes.ToArray();
    }
    private static class Win32
    {
        [DllImport("kernel32.dll")]
        public static extern bool ReadProcessMemory(IntPtr hProcess, IntPtr lpBaseAddress, byte[] lpBuffer, int dwSize, int lpNumberOfBytesRead = 0);
    }
}