using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace HighlightOverlay.Patches {
   public class Main_Patches {
      [HarmonyPatch(typeof(Db), "Initialize")]
      public static class Db_Initialize_Patch {
         public static void Postfix() {
            ShouldHighlightCases.CasesUtils.ValidateCasesMethods();
            ShouldHighlightCases.CasesUtils.RegisterCases();
         }
      }

      [HarmonyPriority(Priority.Low)]// in case any mod adds anything to assets here
      [HarmonyPatch(typeof(Assets), "OnPrefabInit")]
      public static class Assets_OnPrefabInit_Patch {
         public static void Postfix() {
            Utils.SaveSpriteToAssets("ho_overlayicon");

            Main.CacheHighlightFilters();

            Main.CacheCrittersMorphs();

            Main.CacheElementsAggregateStates();
            Main.CacheElementsSublimationElement();
            Main.CacheElementsTransitionElements();
            Main.CacheElementsTransitionOreElements();

            Main.CacheBuildingsHighlightOptions();
            Main.CachePlantsHighlightOptions();
            Main.CacheCrittersHighlightOptions();

            Main.CacheObjectIDs();// cache this after everything else(it uses data cached above)
         }
      }

      [HarmonyPatch(typeof(Game), "OnPrefabInit")]
      public static class Game_OnPrefabInit_Patch {
         public static void Postfix(Game __instance) {
            __instance.Subscribe((int)GameHashes.PauseChanged, OnGamePausedChanged);
            __instance.Subscribe((int)GameHashes.ActiveWorldChanged, OnActiveWorldChanged);
         }
      }
      private static void OnGamePausedChanged(object pause) {
         bool isPaused = Boxed<bool>.Unbox(pause);
         Utils.UpdateHighlightDiagramOptions();
         if(!isPaused && !ModConfig.Instance.AllowNotPaused && Main.highlightMode != default && !Main.highlightMode.isEnabled && !Main.highlightMode.dataIsClear)
         {
            Main.highlightMode.ClearAllData(true, true);
         }
      }
      private static void OnActiveWorldChanged(object data) {
         //Tuple<int, int> worlds = data as Tuple<int, int>;
         //int newWorldIdx = worlds.first;
         //int oldWorldIdx = worlds.second;
         Utils.UpdateHighlightMode(true);
      }

      //[HarmonyPatch(typeof(ToolTipScreen), "ConfigureTooltip")]
      //public static class Debug_Patch {
      //   public static void Postfix(ToolTipScreen __instance) {
      //      Debug.Log("ToolTip_ConfigureTooltip");
      //      Debug.Log("Current ToolTip: ");
      //      if(__instance.tooltipSetting?.multiStringToolTips == null)
      //      {
      //         Debug.Log("NULL");
      //      }
      //      else
      //      {
      //         foreach(string line in __instance.tooltipSetting.multiStringToolTips)
      //         {
      //            Debug.Log(line);
      //         }
      //      }
      //      Debug.Log("ToolTip owner: " + (__instance.tooltipSetting?.gameObject?.name ?? "NULL"));
      //      Debug.Log($"PosX: {(__instance.tooltipSetting?.gameObject?.transform.GetPosition().x.ToString() ?? "NULL")}, PosY: {(__instance.tooltipSetting?.gameObject?.transform.GetPosition().y.ToString() ?? "NULL")}");
      //   }
      //}
   }
}
