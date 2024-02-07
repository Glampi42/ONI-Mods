using HarmonyLib;
using HighlightOverlay;
using HighlightOverlay.Structs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace HighlightOverlay.Patches {
   public class SelectTool_Patches {
      [HarmonyPatch(typeof(SelectTool), "Select")]
      public static class OnSelectObject_Patch {
         public static void Postfix(KSelectable new_selected, bool skipSound) {
            GameObject oldSelected = Main.selectedObj;
            int oldSelectedCell = Main.selectedCell;
            GameObject oldSelectedTile = Main.selectedTile;
            SaveSelectedObject();

            Utils.UpdateHighlightDiagramOptions();
            Utils.UpdateHighlightMode();
            Utils.UpdateHighlightOfSelectedObject(oldSelected, oldSelectedCell, oldSelectedTile);
         }
      }

      private static void SaveSelectedObject() {
         GameObject selected = SelectTool.Instance.selected?.gameObject;
         if(selected != null)
         {
            if(selected.TryGetComponent(out CellSelectionObject cell))
            {
               Main.selectedObjProperties = new ObjectProperties(cell.element);
               Main.selectedObj = null;
               Main.selectedCell = Grid.PosToCell(selected);
               Main.selectedTile = null;
            }
            else if(Utils.IsObjectValidForHighlight(selected, out PrimaryElement primaryElement))
            {
               bool isTile = Utils.IsTile(selected, out _);

               Main.selectedObjProperties = new ObjectProperties(primaryElement);
               Main.selectedObj = isTile ? null : selected;
               Main.selectedCell = -1;
               Main.selectedTile = isTile ? selected : null;
            }
         }
         else
         {
            Main.selectedObjProperties = default;
            Main.selectedObj = null;
            Main.selectedCell = -1;
            Main.selectedTile = null;
         }
      }
   }
}
