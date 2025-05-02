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
         var targetMethod = typeof(Moppable).GetMethod(nameof(Moppable.OnSpawn), Utils.GeneralBindingFlags);
         var postfix = SymbolExtensions.GetMethodInfo(() => OnSpawnPostfix(default));
         return new([new GPatchInfo(targetMethod, null, postfix)]);
      }
      private static void OnSpawnPostfix(Moppable __instance) {
         __instance.Subscribe((int)GameHashes.Cancel, OnErrandCancelDelegate);
      }
      private static readonly EventSystem.IntraObjectHandler<Moppable> OnErrandCancelDelegate = new EventSystem.IntraObjectHandler<Moppable>((errand, data) => {
         if(errand.TryGetCorrespondingNotifiableErrand(out NotifiableErrand notifiableErrand, true))
         {
            notifiableErrand.Remove(false);
         }
      });

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
