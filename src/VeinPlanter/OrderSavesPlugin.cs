using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;

using BepInEx;
using BepInEx.Logging;
using BepInEx.Configuration;

using HarmonyLib;

using UnityEngine;
using System.Linq.Expressions;

namespace OrderSaves
{
    /// <summary>
    /// Main plugin entry point.
    /// </summary>
    [BepInPlugin(ThisAssembly.Plugin.GUID, ThisAssembly.Plugin.Name, ThisAssembly.Plugin.Version)]
    public sealed class OrderSavesPlugin : BaseUnityPlugin
    {
        Func<UIGameSaveEntry, FileInfo> _getFileInfo;
        Func<UIGameSaveEntry, int> _getIndex;

        readonly Harmony _harmony = new Harmony(ThisAssembly.Plugin.GUID);

        void Awake()
        {
            _harmony.PatchAllRecursive(typeof(Patches));

            const BindingFlags flags = BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.DeclaredOnly;

            ParameterExpression[] parameters = new[] { Expression.Parameter(typeof(UIGameSaveEntry), "entry") };

            _getFileInfo = Expression.Lambda<Func<UIGameSaveEntry, FileInfo>>(
                Expression.Field(parameters[0], typeof(UIGameSaveEntry).GetField("fileInfo", flags)),
                parameters).Compile();

            _getIndex = Expression.Lambda<Func<UIGameSaveEntry, int>>(
                Expression.Field(parameters[0], typeof(UIGameSaveEntry).GetField("index", flags)),
                parameters).Compile();
        }

        void OnEnable()
        {
            Patches.Refreshed += OnListRefreshed;
            Logger.DevLog();
        }

        void OnDisable()
        {
            Patches.Refreshed -= OnListRefreshed;
            Logger.DevLog();
        }

        void OnDestroy()
        {
            _harmony.UnpatchSelf();
        }

        void OnListRefreshed(List<UIGameSaveEntry> entries)
        {
            long token = 0;

            Logger.DevMeasureStart(ref token);

            // Sort the save list, descending order.
            entries.Sort((x, y) => y.fileDate.CompareTo(x.fileDate));

            for (int i = 0; i < entries.Count; ++i)
            {
                UIGameSaveEntry entry = entries[i];
                int num = i + 1;
                if (_getIndex(entry) != num)
                {
                    // Trigger update of UI entities
                    entry.SetEntry(num, _getFileInfo(entry));
                }
            }
            Logger.DevMeasureEnd(token);
        }

        static class Patches
        {
            public static Action<List<UIGameSaveEntry>> Refreshed;

            [HarmonyPostfix]
            [HarmonyPatch(typeof(UISaveGameWindow), nameof(UISaveGameWindow.RefreshList))]
            [HarmonyPatch(typeof(UILoadGameWindow), nameof(UILoadGameWindow.RefreshList))]
            public static void RefreshList(List<UIGameSaveEntry> ___entries)
            {
                Refreshed?.Invoke(___entries);
            }
        }
    }
}
