using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using System.Reflection;
using ErrandNotifier.Custom;
using ErrandNotifier.NotificationsHierarchy;

namespace ErrandNotifier.NotifiableErrandPacks {
   public class DeconstructablePack : ANotifiableErrandPack<Deconstructable, NotifiableErrand_Deconstructable> {
      public override List<GPatchInfo> OnChoreDelete_Patch() {
         var targetMethod = typeof(Deconstructable).GetMethod(nameof(Deconstructable.CancelDeconstruction), Utils.GeneralBindingFlags);
         var postfix = SymbolExtensions.GetMethodInfo(() => DeletePostfix(default));
         return new([new GPatchInfo(targetMethod, null, postfix)]);
      }
      private static void DeletePostfix(Deconstructable __instance) {
         if(__instance.TryGetCorrespondingNotifiableErrand(out NotifiableErrand notifiableErrand))
         {
            notifiableErrand.Remove(false);
         }
      }

      public override bool CollectErrands(GameObject gameObject, HashSet<Workable> errands, ref KMonoBehaviour errandReference) {
         if(gameObject.TryGetComponent(out Deconstructable deconstructable) &&
            deconstructable.IsMarkedForDeconstruction())
         {
            errands.Add(deconstructable);
            return true;
         }

         return false;
      }
   }
}
