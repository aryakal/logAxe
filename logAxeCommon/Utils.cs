//=====================================================================================================================
// Source : https://github.com/aryakal/
//--------------------------------------------------------------------------------------------------------------------
//=====================================================================================================================

using System;
using System.IO;
using System.Diagnostics;
using System.Threading;
using System.Runtime.InteropServices;
using Newtonsoft.Json;

using libALogger;

namespace logAxeCommon
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

         var ret = size==0 ? "" :$"{size:.00} {unit}";
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
      //public static string AppMemGapSize(double size)
      //{
      //   return GetHumanSize(GetAppMemSize().Memory - size);
      //}
      //public static AppSize GetAppMemSizeStablized()
      //{
      //   ClearAllGCMemory();
      //   Thread.Sleep(1000);
      //   return GetAppMemSize();
      //}
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
      public static string ConvertLineToStr(LogLine line, string timeFormat = "yyyy-MM-dd hh:mm:ss.fff")
      {
         var logType = line.LogType.ToString().Substring(0, 1);
         var timeStamp = line.TimeStamp.ToString(timeFormat);
         var logText = line.Msg.Replace("\n", "");
         return $"{logType}, {timeStamp}, {logText}";
      }
      public static ConfigUI ReadConfigFile(string configFilePath, ILibALogger logger = null, bool createFileIfNotPresent = true)
      {
         if (!File.Exists(configFilePath))
         {
            if (createFileIfNotPresent)
            {
               var config = new ConfigUI();
               logger?.Error($@"File not found creating file @ {configFilePath}");
               File.WriteAllText(configFilePath, JsonConvert.SerializeObject(config, Formatting.Indented));
            }
            else {
               throw new Exception($"File not found {configFilePath}");
            }
         };
         return JsonConvert.DeserializeObject<ConfigUI>(File.ReadAllText(configFilePath));
      }

      public static string ToJson<T>(T val, bool formatted = true)
      {
         return JsonConvert.SerializeObject(val, formatted ? Formatting.Indented : Formatting.None);
      }

      public static T FromJson<T>(string data) {
         return JsonConvert.DeserializeObject<T>(data);
      }
      
   }

   public class CmdLines
   {
      [JsonProperty("s")]
      public int StartLine { get; set; }
      [JsonProperty("l")]
      public int Length { get; set; }

   }



   
}
