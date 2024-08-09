using ChainErrand.ChainHierarchy;
using ChainErrand.Strings;
using HarmonyLib;
using KSerialization;
using PeterHan.PLib.Core;
using PeterHan.PLib.PatchManager;
using PeterHan.PLib.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;
using static STRINGS.MISC.STATUSITEMS;

namespace ChainErrand.Patches {
   public class Main_Patches {
      [HarmonyPatch(typeof(Assets), "OnPrefabInit")]
      public static class Assets_OnPrefabInit_Patch {
         public static void Postfix() {
            Utils.SaveSpriteToAssets("ce_icon_action_chain");

            MYSPRITES.SaveSprite("ce_create_chain");
            MYSPRITES.SaveSprite("ce_create_link");
            MYSPRITES.SaveSprite("ce_delete_chain");
            MYSPRITES.SaveSprite("ce_delete_link");
         }
      }

      [HarmonyPatch(typeof(PlayerController), "OnPrefabInit")]
      public static class PlayerController_OnPrefabInit_Patch {
         public static void Postfix(PlayerController __instance) {
            // Create list so that new tool can be appended at the end
            var interfaceTools = new List<InterfaceTool>(__instance.tools);
            var chainTool = new GameObject(nameof(ChainTool));
            Main.chainTool = chainTool.AddComponent<ChainTool>();
            // Reparent tool to the player controller, then enable/disable to load it
            chainTool.transform.SetParent(__instance.gameObject.transform);
            chainTool.SetActive(true);
            chainTool.SetActive(false);
            PUtil.LogDebug(Main.debugPrefix + "Created ChainTool");
            // Add tool to tool list
            interfaceTools.Add(Main.chainTool);
            __instance.tools = interfaceTools.ToArray();
         }
      }

      [HarmonyPatch(typeof(ToolMenu), "CreateBasicTools")]
      public class ToolMenu_CreateBasicTools_Patch {
         public static void Postfix(ToolMenu __instance) {
            int insertionIndex = -1;
            for(int i = 0; i < __instance.basicTools.Count; i++)
            {
               if(__instance.basicTools[i].text == STRINGS.UI.TOOLS.PRIORITIZE.NAME)
               {
                  insertionIndex = i + 1;
                  break;
               }
            }
            if(insertionIndex == -1)
            {
               Debug.LogWarning(Main.debugPrefix + "Prioritize tool couldn't be found; inserting chain tool at the end of tools");
               insertionIndex = __instance.basicTools.Count;
            }

            __instance.basicTools.Insert(insertionIndex, ToolMenu.CreateToolCollection(MYSTRINGS.UI.TOOLS.CHAIN.NAME, "ce_icon_action_chain", Main.chainTool_binding.GetKAction(), nameof(ChainTool),
                    MYSTRINGS.UI.TOOLTIPS.CHAINBUTTON, true));
         }
      }

      /// <summary>
		/// Applied to ToolMenu to add the tool list, as the number of tools exceeded
		/// the limit of the base game tool menu (Clay please!)
		/// </summary>
		[HarmonyPatch(typeof(ToolMenu), "OnPrefabInit")]
      public static class ToolMenu_OnPrefabInit_Patch {
         internal static void Postfix() {
            ChainToolMenu.CreateInstance();
         }
      }
      [HarmonyPatch(typeof(Game), "DestroyInstances")]
      public static class Game_DestroyInstances_Patch {
         internal static void Postfix() {
            PUtil.LogDebug(Main.debugPrefix + "Destroying ChainToolMenu");
            ChainToolMenu.DestroyInstance();
            Prefabs.DestroyPrefabs();
         }
      }

      [HarmonyPatch(typeof(Game), "OnDestroy")]
      public static class OnGameDestroy_Patch {
         public static void Postfix(Game __instance) {
            ChainsContainer.Clear();
            ChainNumberPrefab.DestroyPrefab();
         }
      }
   }
}
