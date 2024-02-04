using AdvancedFlowManagement;
using HarmonyLib;
using PeterHan.PLib.UI;
using STRINGS;
using System;
using System.Collections.Generic;
using System.IO.Pipes;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using static STRINGS.UI.TOOLS;

namespace AdvancedFlowManagement.Patches {
   class CopyCrossingConfiguration_Patches {
      private static HashSet<(UtilityNetwork, ConduitFlow)> dirtyNetworks = new HashSet<(UtilityNetwork, ConduitFlow)>();

      [HarmonyPatch(typeof(CopyBuildingSettings), "ApplyCopy")]
      public static class OnCopyConfiguration_Patch {
         public static bool Prefix(int targetCell, GameObject sourceGameObject) {
            if(sourceGameObject.TryGetComponent(out CrossingCmp sourceCmp))
            {
               GameObject targetCrossing = Grid.Objects[targetCell, (int)Utils.ConduitTypeToObjectLayer(sourceCmp.conduitType)];
               if(targetCrossing != null && targetCrossing.TryGetComponent(out CrossingCmp targetCmp))
               {
                  string sourceID = sourceCmp.crossingID;
                  string targetID = targetCmp.crossingID;

                  for(int i = 0; i < 4; i++)
                  {
                     if((sourceID[i] == '0') != (targetID[i] == '0'))
                        return false;// crossings don't have the same connections
                  }
                  bool[] switchDirections = new bool[4];
                  for(int i = 0; i < 4; i++)
                  {
                     if(sourceID[i] != '0' && sourceID[i] != targetID[i])// if the flow directions are different
                     {
                        PipeEnding pipeEnding = Utils.FollowPipe(targetCmp, i);
                        switchDirections[i] = pipeEnding.type == PipeEnding.Type.CROSSING && pipeEnding.endingCell != targetCmp.crossingCell;
                        if(switchDirections[i] == false)
                           return false;// if it is impossible to flip at least one direction, no changes will be applied
                     }
                  }

                  for(int direction = 0; direction < 4; direction++)
                  {
                     if(switchDirections[direction])
                     {
                        Utils.SwitchFlowDirection(targetCmp, (ConduitFlow.FlowDirections)(1 << direction), true);
                     }
                  }
                  targetCmp.flowPriorities = sourceCmp.flowPriorities.DeepClone();
                  Utils.UpdateCrossing(targetCmp);

                  ConduitFlow conduitFlow = Utils.ConduitTypeToConduitFlow(targetCmp.conduitType);
                  dirtyNetworks.Add((conduitFlow.GetNetwork(conduitFlow.GetConduit(targetCmp.crossingCell)), conduitFlow));

                  PopFXManager.Instance.SpawnFX(PopFXManager.Instance.sprite_Plus, (string)UI.COPIED_SETTINGS, targetCrossing.transform, new Vector3(0.0f, 0.5f, 0.0f));
               }
               return false;
            }
            return true;
         }
      }

      [HarmonyPatch(typeof(DragTool), "OnLeftClickUp")]
      public static class OnCopyConfigurationsComplete_Patch {
         public static void Prefix(Vector3 cursor_pos, DragTool __instance, out bool __state) {
            __state = false;

            if(!(__instance is CopySettingsTool) || !__instance.dragging)
               return;

            DragTool.Mode mode = __instance.GetMode();
            if(mode != DragTool.Mode.Box && mode != DragTool.Mode.Line || __instance.areaVisualizer == null)
               return;

            __state = true;
         }

         public static void Postfix(Vector3 cursor_pos, bool __state) {
            if(__state)
            {
               foreach(var network in dirtyNetworks)
               {
                  FlowPriorityManagement_Patches.RecalculateUpdateOrder(network.Item1, network.Item2);
               }

               dirtyNetworks.Clear();
            }
         }
      }
   }
}
