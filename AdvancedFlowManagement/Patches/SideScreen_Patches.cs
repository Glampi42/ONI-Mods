using HarmonyLib;
using PeterHan.PLib.Core;
using PeterHan.PLib.PatchManager;
using PeterHan.PLib.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace AdvancedFlowManagement.Patches {
   class SideScreen_Patches {
      [HarmonyPatch(typeof(DetailsScreen), "OnPrefabInit")]
      public static class AddSideScreen_Patch {
         public static void Postfix() => PUIUtils.AddSideScreenContent<FlowConfigurationSideScreen>();
      }
   }
}
