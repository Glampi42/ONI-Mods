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
      public override List<GPatchInfo> OnChoreCreate_Patch() {
         var targetMethod = typeof(Deconstructable).GetMethod("QueueDeconstruction", Utils.GeneralBindingFlags, null, [typeof(bool)], null);
         var postfix = SymbolExtensions.GetMethodInfo(() => CreatePostfix(default, default));
         return [new GPatchInfo(targetMethod, null, postfix)];
      }
      private static void CreatePostfix(bool userTriggered, Deconstructable __instance) {
         if(__instance.TryGetCorrespondingNotifiableErrand(out NotifiableErrand chainedErrand))
         {
            //chainedErrand.ConfigureChorePrecondition(__instance.chore);
         }
      }

      public override List<GPatchInfo> OnChoreDelete_Patch() {
         var targetMethod = typeof(Deconstructable).GetMethod("CancelDeconstruction", Utils.GeneralBindingFlags);
         var postfix = SymbolExtensions.GetMethodInfo(() => DeletePostfix(default));
         return new([new GPatchInfo(targetMethod, null, postfix)]);
      }
      private static void DeletePostfix(Deconstructable __instance) {
         if(__instance.TryGetCorrespondingNotifiableErrand(out NotifiableErrand chainedErrand))
         {
            chainedErrand.Remove(true);
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

      public override Chore GetChoreFromErrand(Deconstructable errand) {
         return errand.chore;
      }
   }
}
