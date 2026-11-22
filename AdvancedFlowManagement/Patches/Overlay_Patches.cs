using HarmonyLib;
using UnityEngine;
using System;
using System.Collections.Generic;
using UnityEngine.UI;
using AdvancedFlowManagement;
using MonoMod.Utils;

namespace AdvancedFlowManagement {
   public class Overlay_Patches {
      [HarmonyPatch(typeof(OverlayModes.ConduitMode), "Disable")]
      public static class OnDisable_Patch {
         public static void Postfix(OverlayModes.ConduitMode __instance) {
            ConduitType conduit_type = __instance is OverlayModes.LiquidConduits ? ConduitType.Liquid : ConduitType.Gas;
            if(Utils.ConduitTypeToShowCrossingsBool(conduit_type))
            {
               foreach(int crossing_cell in Utils.ConduitTypeToCrossingsSet(conduit_type))
               {
                  CrossingSprite.Hide(Utils.GetCrossingCmp(crossing_cell, conduit_type));
               }
            }
         }
      }
      //-------------------Adding&managing crossings filter in overlay legend-------------------DOWN
      [HarmonyPatch(typeof(OverlayModes.Mode), nameof(OverlayModes.Mode.GetCustomLegendData))]
      public static class AddCustomLegendEntries_Patch {
         public static void Postfix(OverlayModes.Mode __instance, ref List<LegendEntry> __result) {
            if(__instance is OverlayModes.ConduitMode)
            {
               if(OverlayModes.ConduitMode.IsViewConduitFlowEnabled)
               {
                  // display the custom legend entries when conduit flow is enabled

                  ConduitType conduit_type = __instance is OverlayModes.LiquidConduits ? ConduitType.Liquid : ConduitType.Gas;

                  List<LegendEntry> entries = new(4);

                  entries.Add(new LegendEntry(
                     Util.StripTextFormatting(MYSTRINGS.OVERLAYITEMS.JUNCTION.NAME),
                     "<b>" + MYSTRINGS.OVERLAYITEMS.JUNCTION.NAME + "</b>\n" + MYSTRINGS.OVERLAYITEMS.JUNCTION.TOOLTIP,
                     CrossingSprite.normalColor,
                     sprite: MYSPRITES.GetSprite("afm_crossingThick_ui")));
                  entries.Add(new LegendEntry(
                     Util.StripTextFormatting(MYSTRINGS.OVERLAYITEMS.JUNCTIONINPUT.NAME),
                     "<b>" + MYSTRINGS.OVERLAYITEMS.JUNCTIONINPUT.NAME + "</b>\n" + (conduit_type.Equals(ConduitType.Liquid) ? MYSTRINGS.OVERLAYITEMS.JUNCTIONINPUT.TOOLTIP_LIQUID : MYSTRINGS.OVERLAYITEMS.JUNCTIONINPUT.TOOLTIP_GAS),
                     CrossingSprite.normalColor,
                     sprite: MYSPRITES.GetSprite("afm_crossingInput_L_ui")));
                  entries.Add(new LegendEntry(
                     Util.StripTextFormatting(MYSTRINGS.OVERLAYITEMS.JUNCTIONOUTPUT.NAME),
                     "<b>" + MYSTRINGS.OVERLAYITEMS.JUNCTIONOUTPUT.NAME + "</b>\n" + (conduit_type.Equals(ConduitType.Liquid) ? MYSTRINGS.OVERLAYITEMS.JUNCTIONOUTPUT.TOOLTIP_LIQUID : MYSTRINGS.OVERLAYITEMS.JUNCTIONOUTPUT.TOOLTIP_GAS),
                     CrossingSprite.normalColor,
                     sprite: MYSPRITES.GetSprite("afm_crossingOutput_R_ui")));
                  entries.Add(new LegendEntry(
                     Util.StripTextFormatting(MYSTRINGS.OVERLAYITEMS.ILLEGALJUNCTION.NAME),
                     "<b>" + MYSTRINGS.OVERLAYITEMS.ILLEGALJUNCTION.NAME + "</b>\n" + (conduit_type.Equals(ConduitType.Liquid) ? MYSTRINGS.OVERLAYITEMS.ILLEGALJUNCTION.TOOLTIP_LIQUID : MYSTRINGS.OVERLAYITEMS.ILLEGALJUNCTION.TOOLTIP_GAS),
                     CrossingSprite.highlightedColor,
                     sprite: MYSPRITES.GetSprite("afm_crossingThick_ui")));

                  __result = entries;
               }
               else
               {
                  __result = null;
               }
            }
         }
      }

      [HarmonyPatch(typeof(OverlayLegend), "OnFiltersChanged")]
      public static class OnFiltersChanged_Patch {
         public static void Postfix(OverlayLegend __instance) {
            if(__instance.currentMode is not OverlayModes.ConduitMode)
               return;

            ConduitType conduit_type = Utils.ConduitTypeFromOverlayMode(__instance.currentMode);
            if(conduit_type.Equals(ConduitType.None))
               return;

            Utils.ConduitTypeToShowCrossingsBoolRef(conduit_type) = OverlayModes.ConduitMode.IsViewConduitFlowEnabled;
            bool shouldBeAdapted = Utils.ConduitTypeToShowCrossingsBool(conduit_type);
            foreach(int crossing_cell in Utils.ConduitTypeToCrossingsSet(conduit_type))
            {
               CrossingCmp crossingCmp = Utils.GetCrossingCmp(crossing_cell, conduit_type);
               CrossingSprite.UpdateVisibility(crossingCmp);
               UpdateAdaptBuildingEndpointToCrossing(crossingCmp, shouldBeAdapted);
            }
         }
      }
      //-------------------Adding&managing crossings filter in overlay legend-------------------UP
      //-------------------Adapting buildings' endpoints to crossings-------------------DOWN
      [HarmonyPatch(typeof(EntityCellVisualizer), "DrawUtilityIcon")]
      [HarmonyPatch(new Type[] { typeof(int), typeof(Sprite), typeof(GameObject), typeof(Color), typeof(float), typeof(bool) },
          new ArgumentType[] { ArgumentType.Normal, ArgumentType.Normal, ArgumentType.Ref, ArgumentType.Normal, ArgumentType.Normal, ArgumentType.Normal })]
      public static class AdaptBuildingEndpointToCrossing_Patch {
         public static void Postfix(int cell, Sprite icon_img, ref GameObject visualizerObj, Color tint, float scaleMultiplier, bool hideBG, BuildingCellVisualizer __instance) {
            ConduitType conduit_type = Utils.ConduitTypeFromOverlayModeID(OverlayScreen.Instance.GetMode());
            if(conduit_type == ConduitType.None)
               return;

            if(Utils.ConduitTypeToShowCrossingsBool(conduit_type) && Utils.ConduitTypeToCrossingsSet(conduit_type).Contains(cell) &&
                    Utils.TryGetRealEndpointType(Utils.GetCrossingCmp(cell, conduit_type), out Endpoint endpoint_type))
            {
               __instance.icons[visualizerObj].sprite = endpoint_type.Equals(Endpoint.Sink) ? MYSPRITES.GetSprite("afm_input_round") : MYSPRITES.GetSprite("afm_output_round");
               visualizerObj.transform.localScale = Vector3.one * 1f;
            }
         }
      }

      public static void UpdateAdaptBuildingEndpointToCrossing(CrossingCmp crossingCmp, bool shouldBeAdapted) {
         if(Utils.TryGetEndpointVisualizerObj(crossingCmp, !shouldBeAdapted, out GameObject visualizerObj, out BuildingCellVisualizer buildingCellVisualizer))
         {
            if(shouldBeAdapted)
            {
               if(Utils.TryGetRealEndpointType(crossingCmp, out Endpoint endpoint_type))
               {
                  Dictionary<GameObject, Image> icons = buildingCellVisualizer.icons;

                  //-----Deleting pulsing-----DOWN
                  if(visualizerObj.gameObject.TryGetComponent(out SizePulse sizePulse))
                     UnityEngine.Object.Destroy(sizePulse);// Needed for the size of the sprite to change(otherwise it won't)
                  //-----Deleting pulsing-----UP
                  icons[visualizerObj].sprite = endpoint_type.Equals(Endpoint.Sink) ? MYSPRITES.GetSprite("afm_input_round") : MYSPRITES.GetSprite("afm_output_round");
                  visualizerObj.transform.localScale = Vector3.one * 1f;
               }
            }
         }
      }
      //-------------------Adapting buildings' endpoints to crossings-------------------UP
   }
}
