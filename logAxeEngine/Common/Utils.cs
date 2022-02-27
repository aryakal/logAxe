//=====================================================================================================================
// Source : https://github.com/aryakal/
//--------------------------------------------------------------------------------------------------------------------
//=====================================================================================================================

using System;
using System.Diagnostics;
using System.Threading;
using System.Runtime.InteropServices;

namespace logAxeEngine.Common
{
    public static class Utils
    {
        [DllImport("kernel32.dll", EntryPoint = "SetProcessWorkingSetSize", ExactSpelling = true, CharSet = CharSet.Ansi, SetLastError = true)]
        private static extern int SetProcessWorkingSetSize(IntPtr process, int minimumWorkingSetSize, int maximumWorkingSetSize);
        public static string GetHumanSize(double size, bool showInt = false, bool pad = true)
        {
            string unit = "bt";
            if (size > (1000 * 1000 * 1000))
            {
                unit = "GB";
                size = size / (1024 * 1024 * 1024);
            }
            else if (size > (1000 * 1000))
            {
                unit = "MB";
                size = size / (1024 * 1024);
            }
            else if (size > (1000))
            {
                unit = "KB";
                size = size / (1024);
            }

            var ret = $"{size:.00} {unit}";
            if (showInt)
            {
                ret = $"{(int)size} {unit}";
            }

            return pad ? ret.PadLeft(9, ' ') : ret;
        }
        public static AppSize GetAppMemSize()
        {
            return new AppSize();
        }
        public static string AppMemGapSize(double size)
        {
            return GetHumanSize(GetAppMemSize().Memory - size);
        }
        public static AppSize GetAppMemSizeStablized()
        {
            ClearAllGCMemory();
            Thread.Sleep(1000);
            return GetAppMemSize();
        }
        public static string ClearAllGCMemory()
        {
            var prev = GetAppMemSize();
            //https://stackoverflow.com/questions/888280/garbage-collection-does-not-reduce-current-memory-usage-in-release-mode-why
            GC.Collect();
            GC.WaitForPendingFinalizers();
            SetProcessWorkingSetSize(Process.GetCurrentProcess().Handle, -1, -1);
            return $"ClearAllGCMemory from {prev} -> {GetAppMemSize()}";
        }
        public static string Percentage(double value, double maxValue)
        {
            var per = (value / maxValue) * 100;
            return $"{per:.00}%".PadLeft(7, ' ');
        }
    }
}
