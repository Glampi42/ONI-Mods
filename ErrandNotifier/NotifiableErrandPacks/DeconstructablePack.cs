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
         var targetMethod = typeof(Deconstructable).GetMethod(nameof(Deconstructable.OnSpawn), Utils.GeneralBindingFlags);
         var postfix = SymbolExtensions.GetMethodInfo(() => OnSpawnPostfix(default));
         return new([new GPatchInfo(targetMethod, null, postfix)]);
      }
      private static void OnSpawnPostfix(Deconstructable __instance) {
         __instance.Subscribe((int)GameHashes.Cancel, OnErrandCancelDelegate);
      }
      private static readonly EventSystem.IntraObjectHandler<Deconstructable> OnErrandCancelDelegate = new EventSystem.IntraObjectHandler<Deconstructable>((errand, data) => {
         if(errand.TryGetCorrespondingNotifiableErrand(out NotifiableErrand notifiableErrand, true))
         {
            notifiableErrand.Remove(false);
         }
      });

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
