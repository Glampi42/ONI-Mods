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
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using static STRINGS.MISC.STATUSITEMS;

namespace ChainErrand.Patches {
   public class Main_Patches {
      [HarmonyPatch(typeof(Assets), "OnPrefabInit")]
      public static class Assets_OnPrefabInit_Patch {
         public static void Postfix() {
            var fonts = Utils.ImportFontsFromModAssets();
            ChainNumberPrefab.graystroke_outline = fonts.FirstOrDefault(font => font.name == "Glampi Graystroke");
            ChainNumberPrefab.graystroke_outline_italic = fonts.FirstOrDefault(font => font.name == "Glampi Graystroke Italic");

            Utils.SaveSpriteToAssets("ce_icon_action_chain");
            Utils.SaveSpriteToAssets("ce_auto_chain");

            MYSPRITES.SaveSprite("ce_create_chain");
            MYSPRITES.SaveSprite("ce_create_link");
            MYSPRITES.SaveSprite("ce_delete_chain");
            MYSPRITES.SaveSprite("ce_delete_link");

            Main.autoChainNotification = new(MYSTRINGS.UI.AUTOCHAINNOTIFICATION.NAME, NotificationType.Neutral,
               (List<Notification> List, object data) => MYSTRINGS.UI.AUTOCHAINNOTIFICATION.TOOLTIP, expires: false,
               custom_click_callback: new Notification.ClickCallback((object data) => AutoChainUtils.ToggleAutoChain()));
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
            Main.autoChainEnabled = false;
         }
      }

      [HarmonyPatch(typeof(Game), "OnSpawn")]
      public static class DebugPatch1_Patch {
         public static void Postfix() {
            TMP_FontAsset outline = Localization.GetFont("GRAYSTROKE OUTLINE SDF");
            DumpFontInfo(outline);

            TMP_FontAsset normal = Localization.GetFont("GRAYSTROKE REGULAR SDF");
            DumpFontInfo(normal);
         }
         private static void DumpFontInfo(TMP_FontAsset origFont) {
            var sb = new StringBuilder();
            sb.AppendLine("==== TMP_FontAsset Dump ====");
            sb.AppendLine($"Name: {origFont.name}");

            sb.AppendLine("---- Face Info ----");
            sb.AppendLine($"Scale: {origFont.fontInfo.Scale}");
            sb.AppendLine($"Line Height: {origFont.fontInfo.LineHeight}");
            sb.AppendLine($"Ascent Line: {origFont.fontInfo.Ascender}");
            sb.AppendLine($"Cap Line: {origFont.fontInfo.CapHeight}");
            sb.AppendLine($"Mean Line: {origFont.fontInfo.CenterLine}");
            sb.AppendLine($"Baseline: {origFont.fontInfo.Baseline}");
            sb.AppendLine($"Descent Line: {origFont.fontInfo.Descender}");
            sb.AppendLine($"Underline Offset: {origFont.fontInfo.Underline}");
            sb.AppendLine($"Underline Thickness: {origFont.fontInfo.UnderlineThickness}");
            sb.AppendLine($"Strikethrough Offset: {origFont.fontInfo.strikethrough}");
            sb.AppendLine($"Superscript Offset: {origFont.fontInfo.SuperscriptOffset}");
            sb.AppendLine($"Superscript Size: {origFont.fontInfo.SubSize}");
            sb.AppendLine($"Subscript Offset: {origFont.fontInfo.SubscriptOffset}");
            sb.AppendLine($"Subscript Size: {origFont.fontInfo.SubSize}");
            sb.AppendLine($"Tab Width: {origFont.fontInfo.TabWidth}");

            sb.AppendLine("---- Generation Settings ----");
            sb.AppendLine($"Sampling Point Size: {origFont.fontInfo.PointSize}");
            sb.AppendLine($"Padding: {origFont.fontInfo.Padding}");
            sb.AppendLine($"Atlas width: {origFont.atlas.width}");
            sb.AppendLine($"Atlas height: {origFont.atlas.height}");

            sb.AppendLine("---- Font Weights ----");
            for(int i = 0; i < origFont.fontWeights.Length; i++)
            {
               sb.AppendLine($"Regular Typeface: {origFont.fontWeights[i].regularTypeface?.name}");
               sb.AppendLine($"Italic Typeface: {origFont.fontWeights[i].italicTypeface?.name}");
            }
            sb.AppendLine($"Spacing Offset: {origFont.normalSpacingOffset}");
            sb.AppendLine($"Bold Spacing: {origFont.boldSpacing}");
            sb.AppendLine($"Tab Multiple: {origFont.tabSize}");

            sb.AppendLine("====== End Dump ======");

            Debug.Log(sb.ToString());
         }
      }
   }
}
