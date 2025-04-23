using ErrandNotifier.NotificationsHierarchy;
using ErrandNotifier.Strings;
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

namespace ErrandNotifier.Patches {
   public class Main_Patches {
      [HarmonyPatch(typeof(Assets), "OnPrefabInit")]
      public static class Assets_OnPrefabInit_Patch {
         public static void Postfix() {
            Utils.SaveSpriteToAssets("en_icon_action_notifier");

            MYSPRITES.SaveSprite("en_create_notification");
            MYSPRITES.SaveSprite("en_add_errand");
            MYSPRITES.SaveSprite("en_delete_notification");
            MYSPRITES.SaveSprite("en_remove_errand");
         }
      }

      [HarmonyPatch(typeof(PlayerController), "OnPrefabInit")]
      public static class PlayerController_OnPrefabInit_Patch {
         public static void Postfix(PlayerController __instance) {
            // Create list so that new tool can be appended at the end
            var interfaceTools = new List<InterfaceTool>(__instance.tools);
            var notifierTool = new GameObject(nameof(NotifierTool));
            Main.notifierTool = notifierTool.AddComponent<NotifierTool>();
            // Reparent tool to the player controller, then enable/disable to load it
            notifierTool.transform.SetParent(__instance.gameObject.transform);
            notifierTool.SetActive(true);
            notifierTool.SetActive(false);
            PUtil.LogDebug(Main.debugPrefix + "Created NotifierTool");
            // Add tool to tool list
            interfaceTools.Add(Main.notifierTool);
            __instance.tools = interfaceTools.ToArray();
         }
      }

      [HarmonyPatch(typeof(ToolMenu), "CreateBasicTools")]
      public class ToolMenu_CreateBasicTools_Patch {
         public static void Postfix(ToolMenu __instance) {
            int insertionIndex = __instance.basicTools.Count;

            __instance.basicTools.Insert(insertionIndex, ToolMenu.CreateToolCollection(MYSTRINGS.UI.TOOLS.NOTIFIER.NAME, "en_icon_action_notifier", Main.notifierTool_binding.GetKAction(), nameof(NotifierTool),
                    MYSTRINGS.UI.TOOLTIPS.NOTIFIERBUTTON, false));
         }
      }

      /// <summary>
		/// Applied to ToolMenu to add the tool list, as the number of tools exceeded
		/// the limit of the base game tool menu (Clay please!)
		/// </summary>
		[HarmonyPatch(typeof(ToolMenu), "OnPrefabInit")]
      public static class ToolMenu_OnPrefabInit_Patch {
         internal static void Postfix() {
            NotifierToolMenu.CreateInstance();
         }
      }
      [HarmonyPatch(typeof(Game), "DestroyInstances")]
      public static class Game_DestroyInstances_Patch {
         internal static void Postfix() {
            PUtil.LogDebug(Main.debugPrefix + "Destroying NotifierToolMenu");
            NotifierToolMenu.DestroyInstance();
            Prefabs.DestroyPrefabs();
         }
      }

      [HarmonyPatch(typeof(Game), "OnDestroy")]
      public static class OnGameDestroy_Patch {
         public static void Postfix(Game __instance) {
            NotificationsContainer.Clear();
            UISymbolPrefab.DestroyPrefab();
         }
      }
   }
}
