using ErrandNotifier.NotifiableErrandPacks;
using ErrandNotifier.Custom;
using HarmonyLib;
using Newtonsoft.Json.Bson;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using ErrandNotifier.NotificationsHierarchy;

namespace ErrandNotifier.NotifiableErrandPacks {
   public class ConstructablePack : ANotifiableErrandPack<Constructable, NotifiableErrand_Constructable> {
      public override List<GPatchInfo> OnChoreDelete_Patch() {
         var targetMethod = typeof(Constructable).GetMethod(nameof(Constructable.OnSpawn), Utils.GeneralBindingFlags);
         var postfix = SymbolExtensions.GetMethodInfo(() => OnSpawnPostfix(default));
         return new([new GPatchInfo(targetMethod, null, postfix)]);
      }
      private static void OnSpawnPostfix(Constructable __instance) {
         __instance.Subscribe((int)GameHashes.Cancel, OnErrandCancelDelegate);
      }
      private static readonly EventSystem.IntraObjectHandler<Constructable> OnErrandCancelDelegate = new EventSystem.IntraObjectHandler<Constructable>((errand, data) => {
         if(errand.TryGetCorrespondingNotifiableErrand(out NotifiableErrand notifiableErrand, true))
         {
            notifiableErrand.Remove(false);
         }
      });


      public override bool CollectErrands(GameObject gameObject, HashSet<Workable> errands, ref KMonoBehaviour errandReference) {
         if(gameObject.TryGetComponent(out Constructable constructable))
         {
            errands.Add(constructable);
            return true;
         }

         return false;
      }
   }
}
