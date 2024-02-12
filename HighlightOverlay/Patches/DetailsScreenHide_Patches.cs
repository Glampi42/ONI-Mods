using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace HighlightOverlay.Patches {
   public class DetailsScreenHide_Patches {
      [HarmonyPatch(typeof(DetailsScreen), "OnSpawn")]
      public static class OnDetailsScreenClose_Patch {
         public static void Postfix(DetailsScreen __instance) {
            __instance.CloseButton.onClick -= __instance.DeselectAndClose;
            __instance.CloseButton.onClick += () => {
               if(Main.highlightMode != default && Main.highlightMode.isEnabled)
               {
                  // hiding the details screen without actually deselecting the object
                  __instance.gameObject.transform.SetPosition(new Vector3(-Screen.currentResolution.width, __instance.gameObject.transform.position.y, __instance.gameObject.transform.position.z));// look, this works okay(disactivating the screen's GO is a worse way to implement this)
                  Debug.Log($"Coordinates after change: x: {__instance.gameObject.transform.position.x}, y: {__instance.gameObject.transform.position.y}, z: {__instance.gameObject.transform.position.z}");
               }
               else
               {
                  __instance.DeselectAndClose();
               }
            };
         }
      }
   }
}
