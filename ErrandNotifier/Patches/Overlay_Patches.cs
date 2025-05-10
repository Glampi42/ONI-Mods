using ErrandNotifier;
using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;

namespace ErrandNotifier.Patches {
   public class Overlay_Patches {
      [HarmonyPatch(typeof(OverlayScreen), "RegisterModes")]
      public static class RegisterNotifierOverlay_Patch {
         public static void Postfix() {
            if(!StatusItem.overlayBitfieldMap.ContainsKey(NotifierOverlay.ID))
            {
               StatusItem.overlayBitfieldMap.Add(NotifierOverlay.ID, StatusItem.StatusItemOverlays.None);
            }

            OverlayScreen.Instance.RegisterMode(new NotifierOverlay());
            Main.notifierOverlay = (NotifierOverlay)OverlayScreen.Instance.modeInfos.GetOrDefault(NotifierOverlay.ID).mode;
         }
      }
   }
}
