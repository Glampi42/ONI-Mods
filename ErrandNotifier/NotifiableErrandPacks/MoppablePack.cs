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
   public class MoppablePack : ANotifiableErrandPack<Moppable, NotifiableErrand_Moppable> {
      public override List<GPatchInfo> OnChoreDelete_Patch() {
         var targetMethod = typeof(Moppable).GetMethod(nameof(Moppable.OnCancel), Utils.GeneralBindingFlags);
         var postfix = SymbolExtensions.GetMethodInfo(() => OnCancelPostfix(default));
         return new([new GPatchInfo(targetMethod, null, postfix)]);
      }
      private static void OnCancelPostfix(Moppable __instance) {
         if(__instance.TryGetCorrespondingNotifiableErrand(out NotifiableErrand notifiableErrand))
         {
            notifiableErrand.Remove(false);
         }
      }

      public override bool CollectErrands(GameObject gameObject, HashSet<Workable> errands, ref KMonoBehaviour errandReference) {
         if(gameObject.TryGetComponent(out Moppable moppable))
         {
            errands.Add(moppable);
            return true;
         }

         return false;
      }
   }
}
