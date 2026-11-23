using HarmonyLib;
using Steamworks;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;

namespace ChainErrand.Patches {
   public class Overlay_Patches {
      [HarmonyPatch(typeof(OverlayScreen), "RegisterModes")]
      public static class RegisterChainOverlay_Patch {
         public static void Postfix() {
            if(!StatusItem.overlayBitfieldMap.ContainsKey(ChainOverlay.ID))
            {
               StatusItem.overlayBitfieldMap.Add(ChainOverlay.ID, StatusItem.StatusItemOverlays.None);
            }

            OverlayScreen.Instance.RegisterMode(new ChainOverlay());
            Main.chainOverlay = (ChainOverlay)OverlayScreen.Instance.modeInfos.GetOrDefault(ChainOverlay.ID).mode;
         }
      }

      [HarmonyPatch(typeof(ToolMenu), nameof(ToolMenu.OnOverlayChanged))]
      public static class DisableAutoChain_Patch { 
         public static void Postfix(object overlay_data) {
            if(Main.autoChainEnabled && ((Boxed<HashedString>)overlay_data).value != ChainOverlay.ID)
            {
               AutoChainUtils.ToggleAutoChain(false);
            }
         }
      }
   }
}
