using HarmonyLib;
using HighlightOverlay.Strings;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace HighlightOverlay.Patches {
   public class Overlay_Patches {
      [HarmonyPatch(typeof(OverlayMenu), "InitializeToggles")]
      public static class RegisterHighlightOverlay_Patch {
         public static void Postfix(List<KIconToggleMenu.ToggleInfo> ___overlayToggleInfos) {
            KIconToggleMenu.ToggleInfo toggleInfo = new OverlayMenu.OverlayToggleInfo(MYSTRINGS.UI.OVERLAYS.HIGHLIGHTMODE.BUTTON, "ho_overlayicon",
               HighlightMode.ID, "", Action.Overlay14, MYSTRINGS.UI.TOOLTIPS.HIGHLIGHTOVERLAYSTRING, MYSTRINGS.UI.OVERLAYS.HIGHLIGHTMODE.BUTTON);

            ___overlayToggleInfos.Add(toggleInfo);
         }
      }

      [HarmonyPatch(typeof(OverlayScreen), "RegisterModes")]
      public static class FormallyRegisterHighlightOverlay_Patch {
         public static void Postfix() {
            if(!StatusItem.overlayBitfieldMap.ContainsKey(HighlightMode.ID))
            {
               StatusItem.overlayBitfieldMap.Add(HighlightMode.ID, StatusItem.StatusItemOverlays.None);
            }

            OverlayScreen.Instance.RegisterMode(new HighlightMode());
            Main.highlightMode = (HighlightMode)OverlayScreen.Instance.modeInfos.GetOrDefault(HighlightMode.ID).mode;
         }
      }

      [HarmonyPatch(typeof(OverlayLegend), "OnSpawn")]
      public static class CreateOverlayLegend_Patch {
         public static void Postfix(OverlayLegend __instance) {
            System.Action createOverlayLegend = () => {
               HighlightOverlayDiagram.InitPrefab();

               OverlayLegend.OverlayInfo overlayInfo = new OverlayLegend.OverlayInfo() {
                  name = Strings.MYSTRINGS.UI.OVERLAYS.HIGHLIGHTMODE.NAME,
                  mode = HighlightMode.ID,
                  infoUnits = new List<OverlayLegend.OverlayInfoUnit>() {
                  new OverlayLegend.OverlayInfoUnit(Assets.GetSprite("ho_overlayicon"), "If you see this, something went wrong", Color.white, Color.white)
               },// infoUnits can't be null, because otherwise the diagram won't be displayed
                  isProgrammaticallyPopulated = true,
                  diagrams = new List<GameObject>() { HighlightOverlayDiagram.diagramPrefab }
               };

               __instance.overlayInfoList.Add(overlayInfo);
            };

            Prefabs.RunAfterPrefabsInit(createOverlayLegend, nameof(Prefabs.LabelPrefab), nameof(Prefabs.CheckboxPrefab), nameof(Prefabs.FilterTogglePrefab));
         }
      }
      [HarmonyPatch(typeof(OverlayLegend), "PopulateOverlayInfoUnits")]
      public static class ConfigureDiagram_Patch {
         public static void Postfix(OverlayLegend.OverlayInfo overlayInfo, bool isRefresh, OverlayLegend __instance) {
            if(isRefresh || overlayInfo.mode != HighlightMode.ID)
               return;

            Utils.GetHighlightOverlayDiagram().ConfigureDiagramExceptOptions();
            Utils.UpdateHighlightDiagramOptions();// ConfigureDiagramOptions() is ran here
         }
      }

      [HarmonyPatch(typeof(SimDebugView), "OnPrefabInit")]
      public static class SetTilesColor_Patch {
         public static void Postfix(SimDebugView __instance) {
            __instance.getColourFuncs.Add(HighlightMode.ID, new Func<SimDebugView, int, Color>(GetCellColor));
         }
      }
      private static Color GetCellColor(SimDebugView simDebugView, int cell) {
         if(cell == Main.selectedCell || cell == Main.selectedTile.Item2)
            return Main.selectedCellHighlightColor;

         if(Main.tileColors[cell] != Main.blackBackgroundColor)
            return Main.tileColors[cell];

         return Main.cellColors[cell];
      }

      [HarmonyPatch(typeof(GridSettings), "Reset")]
      public static class OnGridChange_Patch {
         public static void Postfix(int width, int height) {
            Main.cellColors = new Color[Grid.CellCount];
            Main.tileColors = new Color[Grid.CellCount];
            Utils.SetDefaultCellColors();
         }
      }
   }
}
