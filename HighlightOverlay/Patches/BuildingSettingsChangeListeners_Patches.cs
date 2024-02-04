using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HighlightOverlay.Patches {
   public class BuildingSettingsChangeListeners_Patches {
      [HarmonyPatch(typeof(ComplexFabricator), "OnSpawn")]
      public static class ComplexFabricator_OnQueueChange_Patch {
         public static void Postfix(ComplexFabricator __instance) {
            __instance.gameObject.Subscribe((int)GameHashes.FabricatorOrdersUpdated, OnFabricationQueueChange);
         }
      }
      private static void OnFabricationQueueChange(object data) {
         ComplexFabricator fabricator = (ComplexFabricator)data;
         if(SelectTool.Instance.selected != null && SelectTool.Instance.selected == fabricator.gameObject)
            Utils.UpdateHighlightMode();
      }

      [HarmonyPatch(typeof(TreeFilterable), "OnSpawn")]
      public static class TreeFilterable_OnFiltersChange_Patch {
         public static void Postfix(TreeFilterable __instance) {
            __instance.OnFilterChanged += OnFiltersChange;
         }
      }
      private static void OnFiltersChange(HashSet<Tag> newFilters) {
         Utils.UpdateHighlightMode();
      }

      [HarmonyPatch(typeof(Ownable), "OnSpawn")]
      public static class Ownable_OnAssignmentsChange_Patch {
         public static void Postfix(Ownable __instance) {
            __instance.OnAssign += OnAssignmentsChange;
         }
      }
      private static void OnAssignmentsChange(IAssignableIdentity assignables) {
         Utils.UpdateHighlightMode();
      }

      [HarmonyPatch(typeof(SingleEntityReceptacle), "CreateOrder")]
      public static class SingleEntityReceptacle_OnOrderChange_Patch {
         public static void Postfix(Tag entityTag, Tag additionalFilterTag) {
            Utils.UpdateHighlightMode();
         }
      }
      [HarmonyPatch(typeof(SingleEntityReceptacle), "CancelActiveRequest")]
      public static class SingleEntityReceptacle_OnOrderRemove_Patch {
         public static void Postfix() {
            Utils.UpdateHighlightMode();
         }
      }

      [HarmonyPatch(typeof(SingleEntityReceptacle), "OnSpawn")]
      public static class SingleEntityReceptacle_OnOccupantChange_Patch {
         public static void Postfix(SingleEntityReceptacle __instance) {
            __instance.Subscribe((int)GameHashes.OccupantChanged, OnOccupantChange);
         }
      }
      private static void OnOccupantChange(object data) {
         //GameObject newOccupant = (GameObject)data;
         Utils.UpdateHighlightMode();
      }

      [HarmonyPatch(typeof(SpiceGrinder.StatesInstance), "OnOptionSelected")]
      public static class SpiceGrinder_OnSelectedSpiceChange_Patch {
         public static void Postfix(SpiceGrinder.Option spiceOption) {
            Utils.UpdateHighlightMode();
         }
      }
   }
}
