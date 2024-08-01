using ChainErrand.ChainHierarchy;
using ChainErrand.Custom;
using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace ChainErrand.ChainedErrandPacks {
   public class MoveToPack : AChainedErrandPack<Movable, ChainedErrand_Movable> {
      public override List<GPatchInfo> OnChoreCreate_Patch() {
         var targetMethod = typeof(Movable).GetMethod("MarkForMove", Utils.GeneralBindingFlags);
         var postfix = SymbolExtensions.GetMethodInfo(() => CreatePostfix(default));

         var targetMethod2 = typeof(Movable).GetMethod("OnSplitFromChunk", Utils.GeneralBindingFlags);
         var prefix = SymbolExtensions.GetMethodInfo(() => OnSplitFromChunkPrefix(default, default));
         return [new GPatchInfo(targetMethod, null, postfix), new GPatchInfo(targetMethod2, prefix, null)];
      }
      private static void CreatePostfix(Movable __instance) {
         Debug.Log("MarkForMove");
         if(__instance.TryGetCorrespondingChainedErrand(out ChainedErrand chainedErrand) &&
            chainedErrand.chore == null)
         {
            Debug.Log("Adding chore");
            chainedErrand.ConfigureChorePrecondition();
         }
      }
      private static void OnSplitFromChunkPrefix(object data, Movable __instance) {
         Debug.Log("OnSplitFromChunkPrefix");
         Movable parentMovable = (data as Pickupable)?.GetComponent<Movable>();
         if(parentMovable != null && parentMovable.IsMarkedForMove && parentMovable.TryGetCorrespondingChainedErrand(out ChainedErrand chainedErrand))
         {
            Debug.Log("Adding chore");
            // adding the split chunk to the same chain its parent is in:
            Dictionary<GameObject, HashSet<Workable>> newErrands = new();
            newErrands.Add(parentMovable.StorageProxy.gameObject, new([__instance]));
            chainedErrand.parentLink.parentChain.CreateOrExpandLink(chainedErrand.parentLink.linkNumber, false, newErrands);
         }
      }

      public override List<GPatchInfo> OnChoreDelete_Patch() {
         return null;// the GameObject gets destroyed in either case
      }

      public override bool CollectErrands(GameObject gameObject, HashSet<Workable> errands, ref KMonoBehaviour errandReference) {
         if(gameObject.TryGetComponent(out CancellableMove cancellableMove) &&
               cancellableMove.movingObjects.Count > 0)
         {
            foreach(var movable in cancellableMove.movingObjects)
            {
               errands.Add(movable.Get());
            }
            errandReference = cancellableMove;
            return true;
         }

         return false;
      }

      public override Chore GetChoreFromErrand(Movable errand) {
         if(errand.StorageProxy?.TryGetComponent(out CancellableMove cancellableMove) ?? false)
         {
            return cancellableMove.fetchChore;
         }
         return null;
      }
   }
}
