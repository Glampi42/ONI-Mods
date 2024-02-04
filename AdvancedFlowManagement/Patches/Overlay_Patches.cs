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
      [HarmonyPatch(typeof(OverlayModes.LiquidConduits), MethodType.Constructor)]
      public class AddCrossingsFilter_Liquid_Patch {
         public static void Postfix(OverlayModes.LiquidConduits __instance) {
            AddCrossingsFilter(__instance);
         }
      }
      [HarmonyPatch(typeof(OverlayModes.GasConduits), MethodType.Constructor)]
      public class AddCrossingsFilter_Gas_Patch {
         public static void Postfix(OverlayModes.GasConduits __instance) {
            AddCrossingsFilter(__instance);
         }
      }
      private static void AddCrossingsFilter(OverlayModes.ConduitMode instance) {
         Dictionary<string, ToolParameterMenu.ToggleState> filters = new Dictionary<string, ToolParameterMenu.ToggleState>
             {
                    {
                        "SHOWCROSSINGS",
                        ToolParameterMenu.ToggleState.On
                    },
                    {
                        "HIDECROSSINGS",
                        ToolParameterMenu.ToggleState.Off
                    }
                };
         if(instance.legendFilters == null)
            instance.legendFilters = filters;
         else
            instance.legendFilters.AddRange(filters);
      }

      [HarmonyPatch(typeof(OverlayLegend), "OnSpawn")]
      public static class EnableCrossingsFilter_Patch {
         public static void Postfix(OverlayLegend __instance) {
            foreach(ConduitType conduit_type in Main.allConduitTypes)
            {
               OverlayLegend.OverlayInfo overlayInfo = __instance.overlayInfoList.Find(info => info.mode == Utils.ConduitTypeToOverlayModeID(conduit_type));

               OverlayLegend.OverlayInfoUnit crossing_infoUnit1 = new OverlayLegend.OverlayInfoUnit(MYSPRITES.GetSprite("afm_crossingThick_ui"), Util.StripTextFormatting(MYSTRINGS.OVERLAYITEMS.JUNCTION.NAME), CrossingSprite.normalColor, Color.white);
               crossing_infoUnit1.tooltip = "<b>" + MYSTRINGS.OVERLAYITEMS.JUNCTION.NAME + "</b>\n" +
                       MYSTRINGS.OVERLAYITEMS.JUNCTION.TOOLTIP;

               OverlayLegend.OverlayInfoUnit crossing_infoUnit2 = new OverlayLegend.OverlayInfoUnit(MYSPRITES.GetSprite("afm_crossingInput_L_ui"), Util.StripTextFormatting(MYSTRINGS.OVERLAYITEMS.JUNCTIONINPUT.NAME), CrossingSprite.normalColor, Color.white);
               crossing_infoUnit2.tooltip = "<b>" + MYSTRINGS.OVERLAYITEMS.JUNCTIONINPUT.NAME + "</b>\n" +
                       (conduit_type.Equals(ConduitType.Liquid) ? MYSTRINGS.OVERLAYITEMS.JUNCTIONINPUT.TOOLTIP_LIQUID : MYSTRINGS.OVERLAYITEMS.JUNCTIONINPUT.TOOLTIP_GAS);

               OverlayLegend.OverlayInfoUnit crossing_infoUnit3 = new OverlayLegend.OverlayInfoUnit(MYSPRITES.GetSprite("afm_crossingOutput_R_ui"), Util.StripTextFormatting(MYSTRINGS.OVERLAYITEMS.JUNCTIONOUTPUT.NAME), CrossingSprite.normalColor, Color.white);
               crossing_infoUnit3.tooltip = "<b>" + MYSTRINGS.OVERLAYITEMS.JUNCTIONOUTPUT.NAME + "</b>\n" +
                       (conduit_type.Equals(ConduitType.Liquid) ? MYSTRINGS.OVERLAYITEMS.JUNCTIONOUTPUT.TOOLTIP_LIQUID : MYSTRINGS.OVERLAYITEMS.JUNCTIONOUTPUT.TOOLTIP_GAS);

               OverlayLegend.OverlayInfoUnit crossing_infoUnit4 = new OverlayLegend.OverlayInfoUnit(MYSPRITES.GetSprite("afm_crossingThick_ui"), Util.StripTextFormatting(MYSTRINGS.OVERLAYITEMS.ILLEGALJUNCTION.NAME), CrossingSprite.highlightedColor, Color.white);
               crossing_infoUnit4.tooltip = "<b>" + MYSTRINGS.OVERLAYITEMS.ILLEGALJUNCTION.NAME + "</b>\n" +
                       (conduit_type.Equals(ConduitType.Liquid) ? MYSTRINGS.OVERLAYITEMS.ILLEGALJUNCTION.TOOLTIP_LIQUID : MYSTRINGS.OVERLAYITEMS.ILLEGALJUNCTION.TOOLTIP_GAS);

               overlayInfo.infoUnits.AddRange(new OverlayLegend.OverlayInfoUnit[] { crossing_infoUnit1, crossing_infoUnit2, crossing_infoUnit3, crossing_infoUnit4 });
               overlayInfo.isProgrammaticallyPopulated = true;// Needed for the crossings filter to appear
            }
         }
      }
      [HarmonyPatch(typeof(OverlayLegend), "PopulateGeneratedLegend")]
      public static class DevCodeBugFix_Patch {
         public static void Postfix(OverlayLegend __instance) {
            if(__instance.currentMode is OverlayModes.ConduitMode)
            {
               __instance.activeUnitsParent.SetActive(true);// Needed for the infoUnits to appear
            }
         }
      }

      [HarmonyPatch(typeof(OverlayLegend), "OnFiltersChanged")]
      public static class OnFiltersChanged_Patch {
         public static void Prefix(OverlayLegend __instance) {
            ConduitType conduit_type = Utils.ConduitTypeFromOverlayMode(__instance.currentMode);
            if(conduit_type.Equals(ConduitType.None))
               return;

            Utils.ConduitTypeToShowCrossingsBoolRef(conduit_type) = __instance.currentMode.legendFilters.ContainsKey("SHOWCROSSINGS") && __instance.currentMode.legendFilters["SHOWCROSSINGS"] == ToolParameterMenu.ToggleState.On;
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
      [HarmonyPatch(typeof(BuildingCellVisualizer), "DrawUtilityIcon")]
      [HarmonyPatch(new Type[] { typeof(int), typeof(Sprite), typeof(GameObject), typeof(Color), typeof(Color), typeof(float), typeof(bool) },
          new ArgumentType[] { ArgumentType.Normal, ArgumentType.Normal, ArgumentType.Ref, ArgumentType.Normal, ArgumentType.Normal, ArgumentType.Normal, ArgumentType.Normal })]
      public static class AdaptBuildingEndpointToCrossing_Patch {
         public static void Postfix(int cell, Sprite icon_img, ref GameObject visualizerObj, Color tint, Color connectorColor, float scaleMultiplier, bool hideBG, BuildingCellVisualizer __instance) {
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
