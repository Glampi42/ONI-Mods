using HarmonyLib;
using HighlightOverlay.Components;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace HighlightOverlay.Patches {
   public class TintManager_Patches {
      [HarmonyPatch(typeof(Assets), "OnPrefabInit")]
      public static class AddTintManager_Patch {
         public static void Postfix() {
            foreach(var prefab in Assets.Prefabs)
            {
               if(prefab.TryGetComponent(out KBatchedAnimController _))
               {
                  prefab.gameObject.AddOrGet<TintManagerCmp>();
               }
            }
         }
      }
   }
}
