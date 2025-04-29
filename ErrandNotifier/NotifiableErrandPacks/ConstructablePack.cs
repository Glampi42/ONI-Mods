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
         var targetMethod = typeof(Constructable).GetMethod(nameof(Constructable.OnCancel), Utils.GeneralBindingFlags);
         var postfix = SymbolExtensions.GetMethodInfo(() => OnCancelPostfix(default, default));
         return new([new GPatchInfo(targetMethod, null, postfix)]);
      }
      private static void OnCancelPostfix(object data, Constructable __instance) {
         if(__instance.TryGetCorrespondingNotifiableErrand(out NotifiableErrand notifiableErrand))
         {
            notifiableErrand.Remove(false);
         }
      }


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
