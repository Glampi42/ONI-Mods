using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using static StateMachine;

namespace HighlightOverlay.Patches {
   public class DetailsScreenHide_Patches {

      [HarmonyPatch(typeof(DetailsScreen), "OnSpawn")]
      public static class OnDetailsScreenClose_Patch {
         public static void Postfix() {
            DetailsScreen screen = DetailsScreen.Instance;
            screen.CloseButton.onClick -= screen.DeselectAndClose;
            screen.CloseButton.onClick += () => {
               if(Main.highlightMode != default && Main.highlightMode.isEnabled)
               {
                  // hiding the details screen without deselecting the object:
                  screen.gameObject.SetActive(false);
               }
               else
               {
                  screen.DeselectAndClose();
               }
            };
         }
      }

      [HarmonyPatch(typeof(KScreenManager), "OnKeyUp")]
      public static class RightClickScreenCloseFix_Patch {
         public static void Prefix(KButtonEvent e) {
            if(e.IsAction(Action.MouseRight) && SelectTool.Instance?.selected != null && !(DetailsScreen.Instance?.gameObject.activeSelf ?? true))// if something is selected, but the details screen is disabled
            {
               DetailsScreen screen = DetailsScreen.Instance;
               if(screen.isEditing || screen.target == null || !((PlayerController.Instance.dragAction != Action.MouseRight || !PlayerController.Instance.dragging) && e.IsAction(Action.MouseRight)))
                  return;

               DetailsScreen.Instance.gameObject.SetActive(true);// needed to close the screen when right clicking
            }
         }
      }
   }
}
