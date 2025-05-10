using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ErrandNotifier.Patches {
   /// <summary>
   /// This class contains patches responsible for finishing the NotifierToolMenu.
   /// </summary>
   public class Menu_Patches {
      [HarmonyPatch(typeof(NotificationScreen), "OnPrefabInit")]
      public static class AddNotificationTypeToggles_Patch {
         public static void Postfix() {
            if(NotifierToolMenu.Instance != null)
            {
               Prefabs.RunAfterPrefabsInit(() => {
                  NotifierToolMenu.Instance.AddTypeSelectionButtons();// should run this here because the NotificationScreen.Instance exists here
               }, nameof(Prefabs.OutlinedCheckboxPrefab));
            }
         }
      }
   }
}
