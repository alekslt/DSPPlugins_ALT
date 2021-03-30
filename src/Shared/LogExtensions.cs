using HarmonyLib;

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;


namespace BepInEx.Logging
{
    static class LogExtensions
    {
        [Conditional("VERBOSE_LOG")]
        public static void DevLog(this ManualLogSource log, [CallerMemberName] string message = null)
        {
            log.LogInfo(message);
        }

        [Conditional("VERBOSE_LOG")]
        public static void DevLog<T>(this ManualLogSource log, T[] array, Func<T, string> transform, [CallerMemberName] string message = null)
        {
            log.LogInfo(message);
            log.LogInfo(typeof(T).FullName + "[" + array.Length.ToString() + "]:");
            for (int i = 0; i < array.Length; ++i)
            {
                log.LogInfo("  " + transform(array[i]));
            }
            log.LogInfo("");
        }

        [Conditional("VERY_VERBOSE_LOG")]
        public static void DevLog(this ManualLogSource log, Harmony harmony)
        {
            foreach (MethodBase method in harmony.GetPatchedMethods())
            {
                Patches patches = Harmony.GetPatchInfo(method);
                log.DevLog(string.Format("{0}::{1} patchers:", method.DeclaringType.FullName, method.Name));

                log.DevLog(string.Join(" ", patches.Owners.ToArray()));
                LogPatchSegment(log, method, patches.Prefixes, "Prefixes:");
                LogPatchSegment(log, method, patches.Transpilers, "Transpilers:");
                LogPatchSegment(log, method, patches.Postfixes, "Postfixes:");
                LogPatchSegment(log, method, patches.Finalizers, "Postfixes:");
            }
        }

        [Conditional("VERY_VERBOSE_LOG")]
        static void LogPatchSegment(ManualLogSource log, MethodBase original, ReadOnlyCollection<Patch> patches, string header)
        {
            if (patches.Count == 0)
            {
                return;
            }

            log.LogInfo(header);
            for (int i = 0; i < patches.Count; ++i)
            {
                Patch patch = patches[i];

                log.DevLog(patch.GetMethod(original).FullDescription());
                log.DevLog(string.Format("[{0}] {1} {2}", patch.owner, patch.index.ToString(), patch.priority.ToString()));
            }
        }

        [Conditional("VERBOSE_LOG")]
        public static void DevMeasureStart(this ManualLogSource log, ref long token, [CallerMemberName] string ctx = null)
        {
            MeasureStart(log, ref token, ctx);
        }

        public static void MeasureStart(this ManualLogSource log, ref long token, [CallerMemberName] string ctx = null)
        {
            token = Stopwatch.GetTimestamp();
            log.LogInfo(ctx + " start:");
        }

        [Conditional("VERBOSE_LOG")]
        public static void DevMeasureEnd(this ManualLogSource log, long token, [CallerMemberName] string ctx = null)
        {
            MeasureEnd(log, token, ctx);
        }

        public static void MeasureEnd(this ManualLogSource log, long token, [CallerMemberName] string ctx = null)
        {
            long elapsed = Stopwatch.GetTimestamp() - token;
            double scale = (double)Stopwatch.Frequency / TimeSpan.TicksPerSecond;

            log.LogInfo(ctx + " end: " + TimeSpan.FromTicks((long)(elapsed / scale)).ToString());
        }
    }
}
