using ChainErrand.Strings;
using HarmonyLib;
using PeterHan.PLib.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

namespace ChainErrand.Patches {
   /// <summary>
   /// This class contains patches that are related with the automatic creation of chains.
   /// </summary>
   public static class AutoChain_Patches {
      public static MultiToggle autoChainToggle;

      [HarmonyPatch(typeof(TopLeftControlScreen))]
      [HarmonyPatch(nameof(TopLeftControlScreen.OnActivate))]
      public static class AddAutoChainButton_Patch {
         public static void Postfix(TopLeftControlScreen __instance) {
            if(MeterScreen.Instance.RedAlertButton.IsNullOrDestroyed())
            {
               Debug.LogWarning(Main.debugPrefix + "Could not create the AutoChain button because: Could not find the red alert button");
               return;
            }

            var autoChainButton = Util.KInstantiateUI(MeterScreen.Instance.RedAlertButton.gameObject, MeterScreen.Instance.RedAlertButton.transform.parent.gameObject, true).transform;
            autoChainButton.name = "AutoChainButton";
            autoChainButton.SetSiblingIndex(MeterScreen.Instance.RedAlertButton.transform.GetSiblingIndex() + 1);

            var icon = autoChainButton.Find("FG")?.GetComponent<Image>();
            icon.sprite = Assets.GetSprite("ce_auto_chain");

            if(autoChainButton.TryGetComponent(out autoChainToggle))
            {
               autoChainToggle.states[1].color = PUITuning.Colors.ButtonBlueStyle.activeColor;
               autoChainToggle.states[1].color_on_hover = PUITuning.Colors.ButtonBlueStyle.activeColor;

               autoChainToggle.onClick = AutoChainUtils.ToggleAutoChain;
            }
            if(autoChainButton.TryGetComponent(out ToolTip autoChainTooltip))
            {
               autoChainTooltip.WrapWidth = 340f;
               autoChainTooltip.ClearMultiStringTooltip();
               autoChainTooltip.AddMultiStringTooltip(MYSTRINGS.UI.AUTOCHAINBUTTON.TOOLTIP_HEADER, MeterScreen.Instance.ToolTipStyle_Header);
               autoChainTooltip.AddMultiStringTooltip(MYSTRINGS.UI.AUTOCHAINBUTTON.TOOLTIP_CONTENT, MeterScreen.Instance.ToolTipStyle_Property);
            }
         }
      }

      [HarmonyPatch(typeof(Vignette))]
      [HarmonyPatch(nameof(Vignette.Reset))]
      public static class AddAutoChainVignette_Patch {
         public static void Postfix(Vignette __instance) {
            if(!ModConfig.Instance.DisableAutoChainVignette && __instance.image != null && __instance.image.color == __instance.defaultColor)
            {
               if(Main.autoChainEnabled)
               {
                  __instance.SetColor(Main.autoChainVignetteColor);
               }
            }
         }
      }

      /// <summary>
      /// This patch enables the addition of errands to the same link if they were created simultaneously via a drag tool (f.e. multiple dig errands).
      /// </summary>
      [HarmonyPatch(typeof(DragTool))]
      [HarmonyPatch(nameof(DragTool.OnLeftClickDown))]
      public static class EnableErrandsBundling_Patch {
         public static void Prefix(Vector3 cursor_pos) {
            AutoChainUtils.bundleErrands = true;
            AutoChainUtils.firstBundledErrand = true;
         }
      }
      [HarmonyPatch(typeof(DragTool))]
      [HarmonyPatch(nameof(DragTool.OnLeftClickUp))]
      public static class DisableErrandsBundling_Patch {
         public static void Postfix(Vector3 cursor_pos) {
            AutoChainUtils.bundleErrands = false;// turn off bundling after the drag was complete
         }
      }
      [HarmonyPatch(typeof(DragTool))]
      [HarmonyPatch(nameof(DragTool.OnDeactivateTool))]
      public static class DisableErrandsBundling2_Patch {
         public static void Postfix(InterfaceTool new_tool) {
            AutoChainUtils.bundleErrands = false;// turn off bundling after the drag tool is deactivated
         }
      }


   }
}