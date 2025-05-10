using ErrandNotifier.Custom;
using ErrandNotifier.NotificationsHierarchy;
using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace ErrandNotifier.NotifiableErrandPacks {
   public class DiggablePack : ANotifiableErrandPack<Diggable, NotifiableErrand_Diggable> {
      public override List<GPatchInfo> OnChoreDelete_Patch() {
         var targetMethod = typeof(CancellableDig).GetMethod(nameof(CancellableDig.OnCancel), Utils.GeneralBindingFlags);
         var postfix = SymbolExtensions.GetMethodInfo(() => OnCancelPostfix(default));
         return new([new GPatchInfo(targetMethod, null, postfix)]);
      }
      private static void OnCancelPostfix(CancellableDig __instance) {
         if(__instance.TryGetComponent(out Diggable diggable) && diggable.TryGetCorrespondingNotifiableErrand(out NotifiableErrand notifiableErrand))
         {
            notifiableErrand.Remove(false);
         }
      }

      public override bool CollectErrands(GameObject gameObject, HashSet<Workable> errands, ref KMonoBehaviour errandReference) {
         if(gameObject.TryGetComponent(out Diggable diggable))
         {
            errands.Add(diggable);
            return true;
         }

         return false;
      }
   }
}
