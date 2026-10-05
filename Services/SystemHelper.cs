using System;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace download_redirector.Services;

public static class SystemHelper
{
    private const uint PROCESS_MODE_BACKGROUND_BEGIN = 0x00100000;
    private const uint PROCESS_MODE_BACKGROUND_END = 0x00200000;

    [DllImport("kernel32.dll", SetLastError = true)]
    private static extern bool SetPriorityClass(IntPtr handle, uint priorityClass);

    public static void SetBackgroundPriority()
    {
        try
        {
            using var process = Process.GetCurrentProcess();
            process.PriorityClass = ProcessPriorityClass.Idle;
            SetPriorityClass(process.Handle, PROCESS_MODE_BACKGROUND_BEGIN);
        }
        catch { }
    }

    public static void SetNormalPriority()
    {
        try
        {
            using var process = Process.GetCurrentProcess();
            SetPriorityClass(process.Handle, PROCESS_MODE_BACKGROUND_END);
            process.PriorityClass = ProcessPriorityClass.Normal;
        }
        catch { }
    }
}
