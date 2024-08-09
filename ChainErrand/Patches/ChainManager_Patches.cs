using ChainErrand.ChainedErrandPacks;
using ChainErrand.ChainHierarchy;
using HarmonyLib;
using KSerialization;
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
               IEnumerable<Type> errandTypes = ChainedErrandPackRegistry.AllErrandTypes();
               foreach(var errandType in errandTypes)
               {
                  if(prefab.gameObject.TryGetComponent(errandType, out _))
                  {
                     var chainedErrand = prefab.gameObject.AddComponent(ChainedErrandPackRegistry.GetChainedErrandPack(errandType).GetChainedErrandType()) as ChainedErrand;
                     if(chainedErrand == null)
                        throw new Exception(Main.debugPrefix + $"Failed to add ChainedErrand component for errand of type {errandType}");

                     chainedErrand.enabled = false;
                  }
               }
            }
         }
      }

      [HarmonyPatch(typeof(Game), "Load")]
      public static class ClearEmptyChains_Patch {
         public static void Postfix(Deserializer deserializer) {
            SerializationUtils.CleanupEmptyDeserializedChains();
         }
      }
   }
}
