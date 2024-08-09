using AdvancedFlowManagement.Patches;
using AdvancedFlowManagement;
using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Linq;
using System.Reflection;
using System.Text;
using static AdvancedFlowManagement.Main;
using System.Collections.Specialized;
using static ConduitFlow;
using static AdvancedFlowManagement.CrossingCmp;
using HarmonyLib;
using System.Linq.Expressions;

namespace AdvancedFlowManagement {
   static class Utils {
      public static void UpdateCrossing(CrossingCmp crossingCmp) {
         string oldID = crossingCmp.crossingID;
         crossingCmp.crossingID = GetRotatedCrossingID(crossingCmp);
         CrossingSprite.Update(crossingCmp);

         for(sbyte direction = 0; direction < 5; direction++)
         {
            char newFlowDirection = GetFlowDirection(crossingCmp, direction);
            if(newFlowDirection == '0')
            {
               StoreFlowPriority(crossingCmp, direction, -1);
            }
            else if(GetFlowPriority(crossingCmp, direction) == -1)
            {
               StoreFlowPriority(crossingCmp, direction, DefaultFlowPriority(crossingCmp, direction));
            }
            else if(oldID != null)
            {
               bool flowDirChanged = newFlowDirection != GetFlowDirection(crossingCmp, oldID, direction);
               if(flowDirChanged)
               {
                  StoreFlowPriority(crossingCmp, direction, DefaultFlowPriority(crossingCmp, direction));
               }
            }
         }
         UpdateShouldManageFlowPriorities(crossingCmp);
      }
      public static void UpdateCrossingDirection(CrossingCmp crossingCmp, sbyte direction) {
         char[] id = crossingCmp.crossingID.ToCharArray();
         byte crossing_outputs = (byte)ConduitTypeToConduitFlow(crossingCmp.conduitType).GetPermittedFlow(crossingCmp.crossingCell);
         char previousDir = id[direction];
         id[direction] = IsBitSet(crossing_outputs, 1 << direction) ? '2' : '1';
         crossingCmp.crossingID = new string(id);
         CrossingSprite.Update(crossingCmp);

         char newFlowDirection = id[direction];
         if(GetFlowPriority(crossingCmp, direction) == -1 || newFlowDirection != previousDir)
         {
            StoreFlowPriority(crossingCmp, direction, DefaultFlowPriority(crossingCmp, direction));
         }

         UpdateShouldManageFlowPriorities(crossingCmp);
      }

      public static void StoreCrossingsNetwork(CrossingCmp crossingCmp) {
         ConduitFlow conduitFlow = ConduitTypeToConduitFlow(crossingCmp.conduitType);
         if(conduitFlow.GetConduit(crossingCmp.crossingCell).idx == -1)
         {
            return;// yeah, that happens...
         }

         var networksCrossings = ConduitTypeToNetworksCrossings(crossingCmp.conduitType);
         int network_id = conduitFlow.GetNetwork(conduitFlow.GetConduit(crossingCmp.crossingCell)).id;
         if(!networksCrossings.ContainsKey(network_id))
            networksCrossings.Add(network_id, new List<int>());
         networksCrossings[network_id].Add(crossingCmp.crossingCell);
      }

      public static void StoreFlowPriority(CrossingCmp crossingCmp, sbyte direction, sbyte flowPriority) {
         (List<sbyte>, sbyte) cluster = crossingCmp.flowPriorities.FirstOrDefault(value => value.Item1.Contains(direction));
         if(cluster != default)
         {
            cluster.Item1.Remove(direction);
            if(cluster.Item1.Count == 0)
               crossingCmp.flowPriorities.Remove(cluster);
         }
         if((cluster = (GetFlowDirection(crossingCmp, direction) == '2' ? crossingCmp.flowPriorities.LastOrDefault(value => value.Item2 == flowPriority) :
            crossingCmp.flowPriorities.FirstOrDefault(value => value.Item2 == flowPriority)/*search begins from the end for outputs, from the beginning for inputs*/)) != default &&
            GetFlowDirection(crossingCmp, direction) == GetFlowDirection(crossingCmp, cluster.Item1[0]))
         {
            cluster.Item1.Add(direction);
         }
         else
         {
            crossingCmp.flowPriorities.Add((new List<sbyte> { direction }, flowPriority));
            // sorting the priorities (undefined connections - negative priority)(inputs - lowest to highest priority)(outputs - lowest to highest priority):
            crossingCmp.flowPriorities.Sort((value1, value2) => (value1.Item2 + (GetFlowDirection(crossingCmp, value1.Item1[0]) == '2' ? 10 : 0)) -
               (value2.Item2 + (GetFlowDirection(crossingCmp, value2.Item1[0]) == '2' ? 10 : 0)));
         }
      }
      public static void StoreBufferContents(BufferStorageCmp bufferStorageCmp, ConduitContents bufferContents, int index) {
         bufferContents.ConsolidateMass();
         bufferStorageCmp.bufferStorage[index] = bufferContents;
      }
      public static void StoreInFlowMngmnt(CrossingCmp crossingCmp, byte[] flowOrder, bool forOutputs) {
         if(forOutputs)
         {
            if(crossingCmp.outputsFlowManagement != default)
            {
               crossingCmp.outputsFlowManagement.Item1 = flowOrder;
            }
            else
            {
               crossingCmp.outputsFlowManagement = (flowOrder, default);
            }
         }
         else
         {
            if(crossingCmp.inputsFlowManagement != default)
            {
               crossingCmp.inputsFlowManagement.Item1 = flowOrder;
            }
            else
            {
               crossingCmp.inputsFlowManagement = (flowOrder, default, default, default);
            }
         }
      }
      public static void StoreInOutFlowMngmnt(CrossingCmp crossingCmp, byte checkedDirectionsCount) {
         if(crossingCmp.outputsFlowManagement != default)
         {
            crossingCmp.outputsFlowManagement.Item2 = checkedDirectionsCount;
         }
         else
         {
            crossingCmp.outputsFlowManagement = (default, checkedDirectionsCount);
         }
      }
      public static void StoreInInFlowMngmnt(CrossingCmp crossingCmp, byte[] local_flowOrder) {
         if(crossingCmp.inputsFlowManagement != default)
         {
            crossingCmp.inputsFlowManagement.Item2 = local_flowOrder;
         }
         else
         {
            crossingCmp.inputsFlowManagement = (default, local_flowOrder, default, default);
         }
      }
      public static void StoreInInFlowMngmnt(CrossingCmp crossingCmp, byte occuredFlowsCount) {
         if(crossingCmp.inputsFlowManagement != default)
         {
            crossingCmp.inputsFlowManagement.Item3 = occuredFlowsCount;
         }
         else
         {
            crossingCmp.inputsFlowManagement = (default, default, occuredFlowsCount, default);
         }
      }
      public static void StoreInInFlowMngmnt(CrossingCmp crossingCmp, bool endpointFlowOccured) {
         if(crossingCmp.inputsFlowManagement != default)
         {
            crossingCmp.inputsFlowManagement.Item4 = endpointFlowOccured;
         }
         else
         {
            crossingCmp.inputsFlowManagement = (default, default, default, endpointFlowOccured);
         }
      }
      public static void ClearInFlowDictionary(CrossingCmp crossingCmp) {
         if(crossingCmp.inputsFlowManagement != default)
         {
            crossingCmp.inputsFlowManagement.Item2 = default;
            crossingCmp.inputsFlowManagement.Item3 = default;
            crossingCmp.inputsFlowManagement.Item4 = default;
         }
      }
      public static void ClearOutFlowDictionary(CrossingCmp crossingCmp) {
         if(crossingCmp.outputsFlowManagement != default)
         {
            crossingCmp.outputsFlowManagement.Item2 = default;
         }
      }

      public static int CountConnections(int conduit_cell, ConduitType conduit_type, bool includeEndpoint) {
         if(ConduitTypeToConduitFlow(conduit_type).GetConduit(conduit_cell).idx == -1)
            return 0;

         int connectionscount = 0;
         if(includeEndpoint && TryGetRealEndpointType(FakeCrossingCmp(conduit_cell, conduit_type), out _))
         {
            connectionscount++;
         }
         connectionscount += CountSetBits((int)ConduitTypeToUtilityNetworkManager(conduit_type).GetConnections(conduit_cell, true));
         return connectionscount;
      }
      //---------------------------EndpointType stuff---------------------------DOWN
      public static bool TryGetVisualEndpoint(CrossingCmp fakeCrossingCmp, out GameObject building_go) {
         return (building_go = Grid.Objects[fakeCrossingCmp.crossingCell, (int)(fakeCrossingCmp.conduitType == ConduitType.Liquid ? ObjectLayer.LiquidConduitConnection : ObjectLayer.GasConduitConnection)]) != null;
      }
      public static bool TryGetVisualEndpointType(CrossingCmp fakeCrossingCmp, out Endpoint endpoint_type) {
         endpoint_type = Endpoint.Conduit;// = no type
         if(!TryGetVisualEndpoint(fakeCrossingCmp, out GameObject building_go))
            return false;

         BuildingCellVisualizer cellVisualizer = building_go?.GetComponent<BuildingCellVisualizer>();
         if(cellVisualizer != null)
         {
            if(fakeCrossingCmp.crossingCell == cellVisualizer.building.GetUtilityInputCell())
            {
               endpoint_type = Endpoint.Sink;
               return true;
            }
            else if(fakeCrossingCmp.crossingCell == cellVisualizer.building.GetUtilityOutputCell())
            {
               endpoint_type = Endpoint.Source;
               return true;
            }
         }

         return false;
      }

      public static bool TryGetRealEndpointType(CrossingCmp fakeCrossingCmp, out Endpoint endpoint_type) {
         endpoint_type = Endpoint.Conduit;

         if(ConduitTypeToEndpointsDict(fakeCrossingCmp.conduitType).ContainsKey(fakeCrossingCmp.crossingCell))
         {
            endpoint_type = ConduitTypeToEndpointsDict(fakeCrossingCmp.conduitType)[fakeCrossingCmp.crossingCell];
            return true;
         }

         return false;
      }
      //---------------------------EndpointType stuff---------------------------UP
      //--------------ConduitType-Everything Converting--------------DOWN
      public static HashSet<int> ConduitTypeToCrossingsSet(ConduitType conduit_type) {
         return (conduit_type == ConduitType.Liquid) ? Main.crossings_liquid : Main.crossings_gas;
      }
      public static HashSet<int> ConduitTypeToBuffersSet(ConduitType conduit_type) {
         return (conduit_type == ConduitType.Liquid) ? Main.buffers_liquid : Main.buffers_gas;
      }
      public static bool ConduitTypeToShowCrossingsBool(ConduitType conduit_type) {
         return (conduit_type == ConduitType.Liquid) ? Main.showCrossings_Liquid : Main.showCrossings_Gas;
      }
      public static ref bool ConduitTypeToShowCrossingsBoolRef(ConduitType conduit_type) {
         if(conduit_type == ConduitType.Liquid)
            return ref Main.showCrossings_Liquid;
         return ref Main.showCrossings_Gas;
      }
      public static ObjectLayer ConduitTypeToObjectLayer(ConduitType conduit_type) {
         return conduit_type == ConduitType.Liquid ? ObjectLayer.LiquidConduit : ObjectLayer.GasConduit;
      }
      public static HashedString ConduitTypeToOverlayModeID(ConduitType conduit_type) {
         return (conduit_type == ConduitType.Liquid) ? OverlayModes.LiquidConduits.ID : OverlayModes.GasConduits.ID;
      }
      public static ConduitFlow ConduitTypeToConduitFlow(ConduitType conduit_type) {
         return (conduit_type == ConduitType.Liquid) ? Game.Instance.liquidConduitFlow : Game.Instance.gasConduitFlow;
      }
      public static UtilityNetworkManager<FlowUtilityNetwork, Vent> ConduitTypeToUtilityNetworkManager(ConduitType conduit_type) {
         return (conduit_type == ConduitType.Liquid) ? Game.Instance.liquidConduitSystem : Game.Instance.gasConduitSystem;
      }
      public static HashSet<int> ConduitTypeToCustomFlow(ConduitType conduit_type) {
         return (conduit_type == ConduitType.Liquid) ? Main.customFlowConduits_liquid : Main.customFlowConduits_gas;
      }
      public static Dictionary<int, List<int>> ConduitTypeToNetworksCrossings(ConduitType conduit_type) {
         return (conduit_type == ConduitType.Liquid) ? Main.crossingsNetworks_liquid : Main.crossingsNetworks_gas;
      }
      public static Dictionary<int, Endpoint> ConduitTypeToEndpointsDict(ConduitType conduit_type) =>
         conduit_type == ConduitType.Liquid ? Main.endpoints_liquid : Main.endpoints_gas;
      public static object ConduitTypeToCrossingsLock(ConduitType conduit_type) =>
         conduit_type == ConduitType.Liquid ? Main.lockCrossings_liquid : Main.lockCrossings_gas;
      public static object ConduitTypeToBuffersLock(ConduitType conduit_type) =>
         conduit_type == ConduitType.Liquid ? Main.lockBuffers_liquid : Main.lockBuffers_gas;

      public static ConduitType ConduitTypeFromOverlayMode(OverlayModes.Mode mode) {
         if(mode is OverlayModes.LiquidConduits)
            return ConduitType.Liquid;
         return mode is OverlayModes.GasConduits ? ConduitType.Gas : ConduitType.None;
      }
      public static ConduitType ConduitTypeFromOverlayModeID(HashedString id) {
         if(id.Equals(OverlayModes.LiquidConduits.ID))
         {
            return ConduitType.Liquid;
         }
         else if(id.Equals(OverlayModes.GasConduits.ID))
         {
            return ConduitType.Gas;
         }
         else
         {
            return ConduitType.None;
         }
      }
      //--------------ConduitType-Everything Converting--------------UP

      public static bool TryGetEndpointVisualizerObj(CrossingCmp crossingCmp, bool resetToDefaults, out GameObject visualizerObj, out BuildingCellVisualizer buildingCellVisualizer) {
         visualizerObj = null;
         buildingCellVisualizer = null;

         if(!Utils.TryGetVisualEndpoint(crossingCmp, out GameObject building_go))
            return false;
         buildingCellVisualizer = building_go.GetComponent<BuildingCellVisualizer>();
         if(buildingCellVisualizer == null)
            return false;

         if(TryGetVisualEndpointType(crossingCmp, out Endpoint endpoint_type))
         {
            if(endpoint_type.Equals(Endpoint.Sink))
            {
               visualizerObj = buildingCellVisualizer.ports.FirstOrDefault(port => (crossingCmp.conduitType == ConduitType.Liquid && port.type == EntityCellVisualizer.Ports.LiquidIn) ||
               (crossingCmp.conduitType == ConduitType.Gas && port.type == EntityCellVisualizer.Ports.GasIn))?.visualizer;
            }
            else if(endpoint_type.Equals(Endpoint.Source))
            {
               visualizerObj = buildingCellVisualizer.ports.FirstOrDefault(port => (crossingCmp.conduitType == ConduitType.Liquid && port.type == EntityCellVisualizer.Ports.LiquidOut) ||
               (crossingCmp.conduitType == ConduitType.Gas && port.type == EntityCellVisualizer.Ports.GasOut))?.visualizer;
            }
            if(visualizerObj == null)
               return false;

            if(resetToDefaults)
            {
               BuildingCellVisualizerResources resources = buildingCellVisualizer.Resources;
               Dictionary<GameObject, Image> icons = buildingCellVisualizer.icons;

               switch(crossingCmp.conduitType)
               {
                  case ConduitType.Liquid:
                     if(endpoint_type.Equals(Endpoint.Sink))
                     {
                        icons[visualizerObj].sprite = resources.liquidInputIcon;
                     }
                     else if(endpoint_type.Equals(Endpoint.Source))
                     {
                        icons[visualizerObj].sprite = resources.liquidOutputIcon;
                     }
                     break;
                  case ConduitType.Gas:
                     if(endpoint_type.Equals(Endpoint.Sink))
                     {
                        icons[visualizerObj].sprite = resources.gasInputIcon;
                     }
                     else if(endpoint_type.Equals(Endpoint.Source))
                     {
                        icons[visualizerObj].sprite = resources.gasOutputIcon;
                     }
                     break;
                  default:
                     break;
               }
               visualizerObj.transform.localScale = Vector3.one * 1.5f;
            }
         }
         return visualizerObj;
      }

      //--------------Crossings' flow directions--------------DOWN
      public static PipeEnding FollowPipe(CrossingCmp crossingCmp, int initialDirection) {
         if(initialDirection == -1)
            return PipeEnding.Invalid;// just for safety
         PipeEnding savedPipeEnding = crossingCmp.pipeEndings[initialDirection];
         if(savedPipeEnding.type != PipeEnding.Type.NOT_SET)
            return savedPipeEnding;

         ConduitFlow.FlowDirections initialDirection_URLD = (ConduitFlow.FlowDirections)(1 << initialDirection);
         ConduitFlow conduitFlow = crossingCmp.conduitType.Equals(ConduitType.Liquid) ? Game.Instance.liquidConduitFlow : Game.Instance.gasConduitFlow;

         PipeEnding pipeEnding = PipeEnding.Invalid;
         int pipeLength = 0;
         int current_idx = conduitFlow.GetConduit(crossingCmp.crossingCell).idx;
         if(current_idx == -1)
            return PipeEnding.Invalid;// just for safety

         int next_idx = conduitFlow.soaInfo.GetConduitFromDirection(current_idx, initialDirection_URLD).idx;
         ConduitFlow.FlowDirections previousDirectionOpposite = ConduitFlow.Opposite(initialDirection_URLD);

         if(next_idx == -1)
         {
            pipeEnding.type = PipeEnding.Type.DEAD_END;
            pipeEnding.endingCell = crossingCmp.crossingCell;
            pipeEnding.pipeLength = 0;
            pipeEnding.backwardsDirection = previousDirectionOpposite;
         }
         else
         {
            do
            {
               if(conduitFlow.soaInfo.GetCell(next_idx) == crossingCmp.crossingCell)// if this pipe ends at the same crossing
               {
                  pipeEnding.type = PipeEnding.Type.CROSSING;
                  pipeEnding.endingCell = crossingCmp.crossingCell;
                  pipeEnding.pipeLength = pipeLength;
                  pipeEnding.backwardsDirection = previousDirectionOpposite;
                  //-----------Setting the other direction-----------DOWN
                  ref PipeEnding otherPipeEnding = ref crossingCmp.pipeEndings[CountTrailingZeros((byte)previousDirectionOpposite)];
                  otherPipeEnding.type = PipeEnding.Type.CROSSING;
                  otherPipeEnding.endingCell = crossingCmp.crossingCell;
                  pipeEnding.pipeLength = pipeLength;
                  otherPipeEnding.backwardsDirection = initialDirection_URLD;
                  //-----------Setting the other direction-----------UP
                  break;
               }
               int nextCell = conduitFlow.soaInfo.GetCell(next_idx);
               int connectionsCount = CountConnections(nextCell, crossingCmp.conduitType, true);
               if(connectionsCount > 2)// if isCrossing
               {
                  pipeEnding.type = PipeEnding.Type.CROSSING;
                  pipeEnding.endingCell = nextCell;
                  pipeEnding.pipeLength = pipeLength;
                  pipeEnding.backwardsDirection = previousDirectionOpposite;
                  //-----------Setting the other direction-----------DOWN
                  if(TryGetCrossingCmp(nextCell, crossingCmp.conduitType, out CrossingCmp otherCrossingCmp))// if isOtherCrossingRegistered
                  {
                     ref PipeEnding otherPipeEnding = ref otherCrossingCmp.pipeEndings[CountTrailingZeros((byte)previousDirectionOpposite)];
                     otherPipeEnding.type = PipeEnding.Type.CROSSING;
                     otherPipeEnding.endingCell = crossingCmp.crossingCell;
                     otherPipeEnding.pipeLength = pipeLength;
                     otherPipeEnding.backwardsDirection = initialDirection_URLD;
                  }
                  //-----------Setting the other direction-----------UP
               }
               else if(connectionsCount > 1)// if !isDeadEnd
               {
                  if(TryGetRealEndpointType(FakeCrossingCmp(nextCell, crossingCmp.conduitType), out Endpoint endpoint_type))
                  {
                     pipeEnding.type = endpoint_type == Endpoint.Sink ? PipeEnding.Type.SINK : PipeEnding.Type.SOURCE;
                     pipeEnding.endingCell = nextCell;
                     pipeEnding.pipeLength = ++pipeLength;
                     pipeEnding.backwardsDirection = previousDirectionOpposite;
                  }
                  else// else continue following the pipe
                  {
                     int previous_idx = current_idx;
                     current_idx = next_idx;
                     next_idx = GetNextIdx(current_idx, previous_idx, conduitFlow, out FlowDirections dirToNextIdx);

                     if(next_idx == -1)
                     {
                        pipeEnding.type = PipeEnding.Type.DEAD_END;
                        pipeEnding.endingCell = conduitFlow.soaInfo.GetCell(current_idx);
                        pipeEnding.pipeLength = pipeLength;
                        pipeEnding.backwardsDirection = previousDirectionOpposite;
                        break;
                     }

                     previousDirectionOpposite = ConduitFlow.Opposite(dirToNextIdx);

                     pipeLength++;
                  }
               }
               else
               {
                  pipeEnding.type = PipeEnding.Type.DEAD_END;
                  pipeEnding.endingCell = nextCell;
                  pipeEnding.pipeLength = ++pipeLength;
                  pipeEnding.backwardsDirection = previousDirectionOpposite;
               }
            } while(pipeEnding.type == PipeEnding.Type.NOT_SET);
         }

         crossingCmp.pipeEndings[initialDirection] = pipeEnding;
         return pipeEnding;
      }

      public static void FollowAllDirections(CrossingCmp crossingCmp) {
         // calculates & stores the PipeEndings for each direction for a crossing (if not defined already):
         for(int direction = 0; direction < 4; direction++)
            FollowPipe(crossingCmp, direction);
      }

      public static void SwitchFlowDirection(CrossingCmp crossingCmp, ConduitFlow.FlowDirections direction_URLD, bool updateOtherCrossing) {
         ConduitFlow conduitFlow = crossingCmp.conduitType.Equals(ConduitType.Liquid) ? Game.Instance.liquidConduitFlow : Game.Instance.gasConduitFlow;
         bool inwardsFlowDirection = IsBitSet((byte)conduitFlow.soaInfo.GetPermittedFlowDirections(conduitFlow.GetConduit(crossingCmp.crossingCell).idx), (byte)direction_URLD);

         int current_idx = conduitFlow.GetConduit(crossingCmp.crossingCell).idx;
         if(inwardsFlowDirection)
         {
            conduitFlow.soaInfo.SetPermittedFlowDirections(current_idx, conduitFlow.soaInfo.GetPermittedFlowDirections(current_idx) & ~direction_URLD);
            conduitFlow.soaInfo.SetTargetFlowDirection(current_idx, ConduitFlow.FlowDirections.None);
         }
         else
         {
            conduitFlow.soaInfo.SetPermittedFlowDirections(current_idx, conduitFlow.soaInfo.GetPermittedFlowDirections(current_idx) | direction_URLD);
            conduitFlow.soaInfo.SetSrcFlowDirection(current_idx, ConduitFlow.FlowDirections.None);
            conduitFlow.soaInfo.SetPullDirection(current_idx, ConduitFlow.FlowDirections.None);
         }

         int previous_idx;
         int next_idx = conduitFlow.soaInfo.GetConduitFromDirection(current_idx, direction_URLD).idx;
         ConduitFlow.FlowDirections previousDirectionOpposite = ConduitFlow.Opposite(direction_URLD);

         int endingCell = FollowPipe(crossingCmp, CountTrailingZeros((byte)direction_URLD)).endingCell;
         bool reachedEnd;
         do
         {
            if(next_idx == -1)
               return;// no more pipes

            reachedEnd = conduitFlow.soaInfo.GetCell(next_idx) == endingCell;

            if(!reachedEnd)
            {
               // normal conduit:
               previous_idx = current_idx;
               current_idx = next_idx;
               next_idx = GetNextIdx(current_idx, previous_idx, conduitFlow, out ConduitFlow.FlowDirections directionToNextIdx);
               if(inwardsFlowDirection)
               {
                  conduitFlow.soaInfo.SetPermittedFlowDirections(current_idx, previousDirectionOpposite);
                  conduitFlow.soaInfo.SetTargetFlowDirection(current_idx, FlowDirections.None);
                  conduitFlow.soaInfo.SetSrcFlowDirection(current_idx, FlowDirections.None);
                  conduitFlow.soaInfo.SetPullDirection(current_idx, FlowDirections.None);
               }
               else
               {
                  conduitFlow.soaInfo.SetPermittedFlowDirections(current_idx, directionToNextIdx);
                  conduitFlow.soaInfo.SetTargetFlowDirection(current_idx, FlowDirections.None);
                  conduitFlow.soaInfo.SetSrcFlowDirection(current_idx, FlowDirections.None);
                  conduitFlow.soaInfo.SetPullDirection(current_idx, FlowDirections.None);
               }
               previousDirectionOpposite = ConduitFlow.Opposite(directionToNextIdx);
               continue;
            }

            int connectionsCount = CountConnections(conduitFlow.soaInfo.GetCell(next_idx), crossingCmp.conduitType, true);

            if(connectionsCount > 2)// if isCrossing
            {
               if(inwardsFlowDirection)
               {
                  conduitFlow.soaInfo.SetPermittedFlowDirections(next_idx, conduitFlow.soaInfo.GetPermittedFlowDirections(next_idx) | previousDirectionOpposite);
                  conduitFlow.soaInfo.SetSrcFlowDirection(next_idx, ConduitFlow.FlowDirections.None);
                  conduitFlow.soaInfo.SetPullDirection(next_idx, ConduitFlow.FlowDirections.None);
               }
               else
               {
                  conduitFlow.soaInfo.SetPermittedFlowDirections(next_idx, conduitFlow.soaInfo.GetPermittedFlowDirections(next_idx) & ~previousDirectionOpposite);
                  conduitFlow.soaInfo.SetTargetFlowDirection(next_idx, ConduitFlow.FlowDirections.None);
               }

               if(updateOtherCrossing)
                  UpdateCrossingDirection(GetCrossingCmp(conduitFlow.soaInfo.GetCell(next_idx), crossingCmp.conduitType), (sbyte)CountTrailingZeros((byte)previousDirectionOpposite));
            }
            else// deadEnd OR sink OR source
            {
               if(inwardsFlowDirection)
               {
                  conduitFlow.soaInfo.SetPermittedFlowDirections(next_idx, previousDirectionOpposite);
                  conduitFlow.soaInfo.SetSrcFlowDirection(next_idx, ConduitFlow.FlowDirections.None);
                  conduitFlow.soaInfo.SetPullDirection(next_idx, ConduitFlow.FlowDirections.None);
               }
               else
               {
                  conduitFlow.soaInfo.SetPermittedFlowDirections(next_idx, ConduitFlow.FlowDirections.None);
                  conduitFlow.soaInfo.SetTargetFlowDirection(next_idx, ConduitFlow.FlowDirections.None);
               }
            }
         } while(!reachedEnd);
      }

      public static int GetNextIdx(int currentIdx, int previousIdx, ConduitFlow conduitFlow, out ConduitFlow.FlowDirections directionToNextIdx) {
         ConduitFlow.ConduitConnections connections = conduitFlow.soaInfo.GetConduitConnections(currentIdx);
         if(connections.down != -1 && connections.down != previousIdx)
         {
            directionToNextIdx = ConduitFlow.FlowDirections.Down;
            return connections.down;
         }
         if(connections.left != -1 && connections.left != previousIdx)
         {
            directionToNextIdx = ConduitFlow.FlowDirections.Left;
            return connections.left;
         }
         if(connections.up != -1 && connections.up != previousIdx)
         {
            directionToNextIdx = ConduitFlow.FlowDirections.Up;
            return connections.up;
         }
         if(connections.right != -1 && connections.right != previousIdx)
         {
            directionToNextIdx = ConduitFlow.FlowDirections.Right;
            return connections.right;
         }
         directionToNextIdx = ConduitFlow.FlowDirections.None;
         return -1;
      }

      public static char GetFlowDirection(CrossingCmp crossingCmp, sbyte direction) {
         if(direction == 4)
         {
            return !TryGetRealEndpointType(crossingCmp, out Endpoint endpoint_type) ? '0' : (endpoint_type == Endpoint.Sink ? '2' : '1');
         }
         return crossingCmp.crossingID[direction];
      }
      public static char GetFlowDirection(CrossingCmp crossingCmp, string crossingID, sbyte direction) {
         if(direction == 4)
         {
            return !TryGetRealEndpointType(crossingCmp, out Endpoint endpoint_type) ? '0' : (endpoint_type == Endpoint.Sink ? '2' : '1');
         }
         return crossingID[direction];
      }

      public static sbyte OppositeDirection(sbyte direction_URLD) {
         switch(direction_URLD)
         {
            case 0:
               return 3;
            case 1:
               return 2;
            case 2:
               return 1;
            case 3:
            default:
               return 0;
         }
      }

      public static UtilityConnections URLD_To_DURL_Single(FlowDirections direction) {
         switch(direction)
         {
            case FlowDirections.Down:
               return UtilityConnections.Down;
            case FlowDirections.Up:
               return UtilityConnections.Up;
            case FlowDirections.Left:
               return UtilityConnections.Left;
            case FlowDirections.Right:
               return UtilityConnections.Right;
            default:
               return 0x0;
         }
      }

      public static FlowDirections DURL_To_URLD_Single(UtilityConnections direction) {
         switch(direction)
         {
            case UtilityConnections.Down:
               return FlowDirections.Down;
            case UtilityConnections.Up:
               return FlowDirections.Up;
            case UtilityConnections.Left:
               return FlowDirections.Left;
            case UtilityConnections.Right:
               return FlowDirections.Right;
            default:
               return FlowDirections.None;
         }
      }
      //--------------Crossings' flow directions--------------UP
      //--------------Crossings' flow priorities--------------DOWN
      public static void SetDefaultFlowPriorities(CrossingCmp crossingCmp) {
         string crossing_id = crossingCmp.crossingID;
         for(sbyte direction = 0; direction < 4; direction++)
         {
            StoreFlowPriority(crossingCmp, direction, crossing_id[direction] == '0' ? (sbyte)-1 : DefaultFlowPriority(crossingCmp, direction));
         }
         StoreFlowPriority(crossingCmp, 4, DefaultFlowPriority(crossingCmp, 4));
      }

      public static sbyte DefaultFlowPriority(CrossingCmp crossingCmp, int direction) {
         if(direction == 4)
         {
            return (sbyte)(!TryGetRealEndpointType(crossingCmp, out Endpoint endpoint_type) ? -1 : (endpoint_type == Endpoint.Sink ? 2 : 0));
         }
         else
         {
            return 1;
         }
      }

      public static sbyte GetFlowPriority(CrossingCmp fakeCrossingCmp, sbyte direction_outwards) {
         if(!IsCrossingRegistered(fakeCrossingCmp))
            return -1;
         List<(List<sbyte>, sbyte)> flowPriorities = fakeCrossingCmp.flowPriorities;
         (List<sbyte>, sbyte) cluster = flowPriorities.FirstOrDefault(value => value.Item1.Contains(direction_outwards));
         if(cluster == default)
            return -1;
         return cluster.Item2;
      }

      public static (List<sbyte>, sbyte) GetFlowPriorityCluster(CrossingCmp crossingCmp, sbyte direction_outwards) {
         List<(List<sbyte>, sbyte)> flowPriorities = crossingCmp.flowPriorities;
         (List<sbyte>, sbyte) cluster = flowPriorities.FirstOrDefault(value => value.Item1.Contains(direction_outwards));
         return cluster;
      }

      public static (List<sbyte>, sbyte) GetFlowPriorityClusterFromPriority(CrossingCmp crossingCmp, sbyte flowPriority, bool outputsFlow) {
         List<(List<sbyte>, sbyte)> flowPriorities = crossingCmp.flowPriorities;
         (List<sbyte>, sbyte) cluster = outputsFlow ? flowPriorities.LastOrDefault(clust3r => clust3r.Item2 == flowPriority) :
            flowPriorities.FirstOrDefault(clust3r => clust3r.Item2 == flowPriority);/*search begins from the end for outputs, from the beginning for inputs*/

         if(cluster == default)
            return default;
         if((outputsFlow && GetFlowDirection(crossingCmp, cluster.Item1[0]) == '2') || (!outputsFlow && GetFlowDirection(crossingCmp, cluster.Item1[0]) == '1'))
            return cluster;
         return default;
      }

      public static void UpdateEndpointFlowPriority(CrossingCmp crossingCmp, bool endpointTypeChanged) {
         if(TryGetRealEndpointType(crossingCmp, out _))
         {
            sbyte flowPriority = GetFlowPriority(crossingCmp, 4);
            if(flowPriority == -1 || endpointTypeChanged)
               StoreFlowPriority(crossingCmp, 4, DefaultFlowPriority(crossingCmp, 4));
         }
         else
         {
            StoreFlowPriority(crossingCmp, 4, -1);
         }
      }

      public static bool ConduitRequiresBuffer(CrossingCmp fakeCrossingCmp) {
         return IsCrossingRegistered(fakeCrossingCmp) && (TryGetRealEndpointType(fakeCrossingCmp, out Endpoint endpoint_type) &&
            (endpoint_type == Endpoint.Sink && fakeCrossingCmp.shouldManageOutPriorities) ||
            (endpoint_type == Endpoint.Source && fakeCrossingCmp.shouldManageInPriorities));
      }

      public static void UpdateShouldManageFlowPriorities(CrossingCmp crossingCmp) {
         string crossing_id = crossingCmp.crossingID;
         List<sbyte> inputsPriorities = new List<sbyte>();
         List<sbyte> outputsPriorities = new List<sbyte>();
         for(sbyte direction = 0; direction < 4; direction++)
         {
            if(crossing_id[direction] == '2')
            {
               outputsPriorities.Add(GetFlowPriority(crossingCmp, direction));
            }
            else if(crossing_id[direction] == '1')
            {
               inputsPriorities.Add(GetFlowPriority(crossingCmp, direction));
            }
         }

         bool shouldManageIn = false;
         bool shouldManageOut = false;
         if(inputsPriorities.Count > 1)
         {
            for(int i = 0; i < inputsPriorities.Count - 1; i++)
            {
               if(inputsPriorities[i] != inputsPriorities[i + 1])
               {
                  shouldManageIn = true;// inputs have different flow priorities
                  break;
               }
            }
         }
         if(outputsPriorities.Count > 1)
         {
            for(int i = 0; i < outputsPriorities.Count - 1; i++)
            {
               if(outputsPriorities[i] != outputsPriorities[i + 1])
               {
                  shouldManageOut = true;// outputs have different flow priorities
                  break;
               }
            }
         }
         sbyte flowPriority = GetFlowPriority(crossingCmp, 4);
         if(flowPriority != -1)
         {
            if(TryGetRealEndpointType(crossingCmp, out Endpoint endpoint_type))
            {
               if(endpoint_type == Endpoint.Sink)
               {
                  if(outputsPriorities.Count > 0 && flowPriority <= outputsPriorities[0])
                  {
                     shouldManageOut = true;// sink has a lower/same flow priority as all outputs
                  }
               }
               else
               {
                  if(inputsPriorities.Count > 0 && flowPriority >= inputsPriorities[0])
                  {
                     shouldManageIn = true;// source has a higher/same flow priority as all inputs
                  }
               }
            }
         }
         if(!shouldManageIn)
         {
            crossingCmp.inputsFlowManagement = default;
         }
         if(!shouldManageOut)
         {
            crossingCmp.outputsFlowManagement = default;
         }
         crossingCmp.shouldManageInPriorities = shouldManageIn;
         crossingCmp.shouldManageOutPriorities = shouldManageOut;

         FlowPriorityManagement_Patches.SaveCustomFlowConduits(crossingCmp.crossingCell, ConduitTypeToConduitFlow(crossingCmp.conduitType));
      }
      //--------------Crossings' flow priorities--------------UP
      //--------------Crossings and their IDs--------------DOWN
      public static Sprite GetSpriteForCrossing(CrossingCmp crossingCmp, out Quaternion rotation) {
         string unrotated_crossing_id = ToUnrotatedCrossingID(crossingCmp.crossingID, out byte rotation_CCW);

         rotation = Quaternion.AngleAxis(rotation_CCW * 90f, Vector3.forward);
         return MYSPRITES.GetSprite("afm_crossing_" + unrotated_crossing_id);
      }

      public static string GetRotatedCrossingID(CrossingCmp crossingCmp) {
         ConduitFlow conduitFlow = ConduitTypeToConduitFlow(crossingCmp.conduitType);
         byte crossing_connections = (byte)ConduitTypeToUtilityNetworkManager(crossingCmp.conduitType).GetConnections(crossingCmp.crossingCell, true);
         byte crossing_outputs = (byte)conduitFlow.GetPermittedFlow(crossingCmp.crossingCell);
         DURL_To_URLD(ref crossing_connections);

         StringBuilder crossing_id = new StringBuilder(4);
         for(int i = 0; i < 4; i++)
         {
            if(IsBitSet(crossing_connections, 0x1))
            {
               if(IsBitSet(crossing_outputs, 0x1))
               {
                  crossing_id.Append("2");
               }
               else
               {
                  crossing_id.Append("1");
               }
            }
            else
            {
               crossing_id.Append("0");
            }

            crossing_connections >>= 1;
            crossing_outputs >>= 1;
         }
         return crossing_id.ToString();
      }

      public static int CountCrossingConnections(byte crossing_id) {
         int connections = 0;
         for(int i = 0; i < 4; i++)
         {
            if(crossing_id % 3 != 0)
               connections++;
            crossing_id /= 3;
         }
         return connections;
      }

      public static string ToUnrotatedCrossingID(string rotated_crossing_id, out byte rotation_CCW) {
         (string, byte) tuple = Main.allCrossingIDs[rotated_crossing_id];
         rotation_CCW = tuple.Item2;
         return tuple.Item1;
      }
      //--------------Crossings and their IDs--------------UP
      //--------------GameObjects' Components--------------DOWN
      public static GameObject GetConduitGO(int conduit_cell, ConduitType conduit_type) => Grid.Objects[conduit_cell, (int)Utils.ConduitTypeToObjectLayer(conduit_type)];
      public static bool TryGetConduitGO(int conduit_cell, ConduitType conduit_type, out GameObject conduitGO) {
         conduitGO = GetConduitGO(conduit_cell, conduit_type);
         return conduitGO != null;
      }

      public static CrossingCmp GetCrossingCmp(int crossing_cell, ConduitType conduit_type) => GetConduitGO(crossing_cell, conduit_type)?.GetComponent<CrossingCmp>();
      public static BufferStorageCmp GetBufferStorageCmp(CrossingCmp fakeCrossingCmp) => GetBufferStorageCmp(fakeCrossingCmp.crossingCell, fakeCrossingCmp.conduitType);
      public static BufferStorageCmp GetBufferStorageCmp(int conduit_cell, ConduitType conduit_type) => GetConduitGO(conduit_cell, conduit_type)?.GetComponent<BufferStorageCmp>();

      public static bool TryGetCrossingCmp(int crossing_cell, ConduitType conduit_type, out CrossingCmp crossingCmp) {
         crossingCmp = null;
         if(!ConduitTypeToCrossingsSet(conduit_type).Contains(crossing_cell))
            return false;

         crossingCmp = GetCrossingCmp(crossing_cell, conduit_type);
         return true;
      }
      public static bool TryGetBufferStorageCmp(CrossingCmp fakeCrossingCmp, out BufferStorageCmp bufferStorageCmp) {
         return TryGetBufferStorageCmp(fakeCrossingCmp.crossingCell, fakeCrossingCmp.conduitType, out bufferStorageCmp);
      }
      public static bool TryGetBufferStorageCmp(int conduit_cell, ConduitType conduit_type, out BufferStorageCmp bufferStorageCmp) {
         bufferStorageCmp = null;
         if(!ConduitTypeToBuffersSet(conduit_type).Contains(conduit_cell))
            return false;

         bufferStorageCmp = GetBufferStorageCmp(conduit_cell, conduit_type);
         return true;
      }

      public static bool IsCrossingRegistered(CrossingCmp fakeCrossingCmp) => fakeCrossingCmp.crossingID != default;

      public static CrossingCmp FakeCrossingCmp(int conduit_cell, ConduitType conduit_type) => new CrossingCmp(conduit_cell, conduit_type);

      public static CrossingCmp GetOrFakeCrossingCmp(int conduit_cell, ConduitType conduit_type) {
         if(TryGetCrossingCmp(conduit_cell, conduit_type, out CrossingCmp crossingCmp))
            return crossingCmp;
         return FakeCrossingCmp(conduit_cell, conduit_type);
      }

      public static BufferStorageCmp CreateOrGetBufferStorage(CrossingCmp crossingCmp, float MaxMass) {
         BufferStorageCmp bufferStorageCmp;
         if(!TryGetBufferStorageCmp(crossingCmp, out bufferStorageCmp))
         {
            bufferStorageCmp = crossingCmp.gameObject.AddComponent<BufferStorageCmp>();
            bufferStorageCmp.conduitCell = crossingCmp.crossingCell;
            bufferStorageCmp.conduitType = crossingCmp.conduitType;
            bufferStorageCmp.bufferStorage = Enumerable.Repeat(ConduitContents.Empty, 2).ToArray();

            lock(ConduitTypeToBuffersLock(crossingCmp.conduitType))
            {
               ConduitTypeToBuffersSet(crossingCmp.conduitType).Add(bufferStorageCmp.conduitCell);
            }
         }

         bufferStorageCmp.bufferMaxMass = MaxMass;// if a conduit gets replaced

         return bufferStorageCmp;
      }
      //--------------GameObjects' Components--------------UP
      //--------------Extensions--------------DOWN
      public static T FindOrDefault<T>(this T[] array, Func<T, bool> predicate, T defaultValue) {
         foreach(T current in array)
         {
            if(predicate(current))
            {
               return current;
            }
         }
         return defaultValue;
      }
      public static T FindOrDefault<T>(this IEnumerable<T> array, Func<T, bool> predicate, T defaultValue) {
         foreach(T current in array)
         {
            if(predicate(current))
            {
               return current;
            }
         }
         return defaultValue;
      }

      public static DictionaryEntry FirstOrDefault(this ListDictionary dictionary, Func<DictionaryEntry, bool> predicate, DictionaryEntry defaultValue) {
         IEnumerator enumerator = dictionary.GetEnumerator();
         while(enumerator.MoveNext())
         {
            if(predicate((DictionaryEntry)enumerator.Current))
            {
               return (DictionaryEntry)enumerator.Current;
            }
         }
         return defaultValue;
      }
      public static DictionaryEntry FirstOrDefault(this ListDictionary dictionary, DictionaryEntry defaultValue) {
         IEnumerator enumerator = dictionary.GetEnumerator();
         if(enumerator.MoveNext())
         {
            return (DictionaryEntry)enumerator.Current;
         }
         return defaultValue;
      }

      public static D GetOrDefault<T, D>(this Dictionary<T, D> dictionary, T key, D defaultValue) {
         if(dictionary.ContainsKey(key))
         {
            return dictionary[key];
         }
         else
         {
            return defaultValue;
         }
      }

      public static void AddRange<T>(this HashSet<T> set, HashSet<T> another) {
         foreach(T element in another)
            set.Add(element);
      }

      public static void RemoveRange(this ListDictionary dictionary, Func<DictionaryEntry, bool> predicate) {
         List<DictionaryEntry> temp = new List<DictionaryEntry>(dictionary.Cast<DictionaryEntry>());
         foreach(DictionaryEntry entry in temp)
         {
            if(predicate(entry))
               dictionary.Remove(entry.Key);
         }
      }

      public static List<(List<sbyte>, sbyte)> DeepClone(this List<(List<sbyte>, sbyte)> list) {
         if(list == default)
            return default;

         var newList = new List<(List<sbyte>, sbyte)>(list.Count);
         foreach(var item in list)
         {
            var newTuple = (new List<sbyte>(item.Item1), item.Item2);
            newList.Add(newTuple);
         }
         return newList;
      }
      //--------------Extensions--------------UP
      //--------------Bit Masks--------------DOWN
      public static void DURL_To_URLD(ref byte bitMask) {
         bool d_Set = IsBitSet(bitMask, 0x8);
         bitMask = (byte)(((bitMask & ~0x8) << 1) + (d_Set ? 1 : 0));
      }

      public static int CountSetBits(int bitMask) {
         int count = 0;
         while(bitMask > 0)
         {
            count += bitMask & 1;
            bitMask >>= 1;
         }
         return count;
      }

      public static int CountTrailingZeros(int bit) {
         int temp = bit;
         int count = 0;
         if(temp != 0)
         {
            while((temp & 1) == 0)
            {
               count++;
               temp >>= 1;
            }
            return count;
         }
         else
         {
            return 8 * sizeof(int);
         }
      }

      public static bool IsBitSet(int bitMask, int bit) => (bitMask & bit) != 0;
      //--------------Bit Masks--------------UP

      public static FieldInfo GetFieldInfo<TClass, TField>(Expression<Func<TClass, TField>> expr) {
         return ((MemberExpression)expr.Body).Member as FieldInfo;
      }
   }
}
