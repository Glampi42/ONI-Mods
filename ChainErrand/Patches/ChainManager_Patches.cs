using ChainErrand.ChainHierarchy;
using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace ChainErrand.Patches {
   /// <summary>
   /// This class contains patches that create, update and monitor the existing chains as well as serialize them.
   /// </summary>
   public class ChainManager_Patches {
      /// <summary>
      /// Adding the ChainedErrand component to the prefabs.
      /// </summary>
      [HarmonyPatch(typeof(Assets), "OnPrefabInit")]
      [HarmonyPriority(Priority.Low)]
      public static class AddChainedErrandCmp_Patch {
         public static void Postfix() {
            foreach(var prefab in Assets.Prefabs)
            {
               HashSet<Workable> errands;
               if((errands = RequiredChainedErrands(prefab.gameObject)).Count > 0)
               {
                  foreach(var errand in errands)
                  {
                     Debug.Log($"Adding ChainedErrand for go {prefab.gameObject.name}, errand {errand.GetType()}");
                     var chainedErrand = prefab.gameObject.AddComponent(Utils.ChainedErrandTypeFromErrand(errand)) as ChainedErrand;
                     if(chainedErrand == null)
                        throw new Exception(Main.debugPrefix + $"Failed to add ChainedErrand component for errand of type {errand.GetType()}");

                     chainedErrand.enabled = false;
                  }
               }
            }
         }
      }
      /// <summary>
      /// Collects all errands attached to the specified GameObject that need to have a related ChainedErrand component.
      /// </summary>
      /// <param name="gameObject">The GameObject</param>
      /// <returns>The errands.</returns>
      private static HashSet<Workable> RequiredChainedErrands(GameObject gameObject) {
         HashSet<Workable> errands = new();

         TryAdd(gameObject.GetComponent<Constructable>());
         TryAdd(gameObject.GetComponent<Deconstructable>());
         TryAdd(gameObject.GetComponent<EmptyConduitWorkable>());
         TryAdd(gameObject.GetComponent<Diggable>());
         TryAdd(gameObject.GetComponent<Moppable>());
         TryAdd(gameObject.GetComponent<Movable>());

         return errands;

         void TryAdd(Workable errand) {
            if(errand != null)
               errands.Add(errand);
         }
      }
   }
}
