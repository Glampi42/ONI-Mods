using AdvancedFlowManagement;
using HarmonyLib;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using static ConduitFlow;
using static STRINGS.UI.SPACEARTIFACTS;

namespace AdvancedFlowManagement.Patches {
   class CrossingsUpdates_Patches {
      public static void PostProcessNetworksRebuild(ICollection<int> allUpdatedCells, ConduitFlow conduitFlow) {
         ConduitType conduit_type = conduitFlow.conduitType;

         CacheAllEndpoints(conduitFlow.networks, conduit_type, out Dictionary<int, Endpoint> oldEndpoints);

         Main.DeserializeCmpsIfNeeded(conduit_type, out bool deserialized);

         var registeredCrossings = Utils.ConduitTypeToCrossingsSet(conduit_type);
         Dictionary<int, CrossingCmp> savedCrossingCmps = new Dictionary<int, CrossingCmp>(registeredCrossings.Count);
         foreach(int crossing_cell in registeredCrossings)
         {
            CrossingCmp crossingCmp = Utils.GetCrossingCmp(crossing_cell, conduit_type);
            if(crossingCmp != null)
               savedCrossingCmps.Add(crossing_cell, crossingCmp);
         }

         if(deserialized)
            ManageDeserializedCrossings();

         Dictionary<int, int[]> previousCrossingsCells = new Dictionary<int, int[]>(registeredCrossings.Count);
         SavePreviousCrossingsCells();

         GameObject selectedObject = (SelectTool.Instance.selected?.selected ?? false) ? SelectTool.Instance.selected.gameObject : null;
         bool selectedIsNotNull = selectedObject != null;

         //----------------Deleting all PipeEndings values----------------DOWN
         foreach(CrossingCmp crossingCmp in savedCrossingCmps.Values)
         {
            crossingCmp.pipeEndings = Enumerable.Repeat(PipeEnding.Invalid, 4).ToArray();
         }
         //----------------Deleting all PipeEndings values----------------UP

         var networksCrossings = Utils.ConduitTypeToNetworksCrossings(conduit_type);
         networksCrossings.Clear();// resetting this after the rebuild(see StoreCrossingsNetwork() below)

         Dictionary<int, bool>/*crossing_cell, skipUpdate*/ checkedCrossings = new Dictionary<int, bool>();
         Dictionary<int, (string, (int, bool)[], (int, bool), bool)>/*crossing_cell, (newID, (forwards_direction, shouldBeSwitched)[], (backwards_direction, isSwitched), skipUpdate)*/ crossingsCluster =
             new Dictionary<int, (string, (int, bool)[], (int, bool), bool)>();

         foreach(int conduit_cell in allUpdatedCells)
         {
            int connectionsCount = Utils.CountConnections(conduit_cell, conduit_type, true);
            if(registeredCrossings.Contains(conduit_cell))
            {
               if(connectionsCount < 3)
               {
                  UnregisterCrossing(savedCrossingCmps[conduit_cell], Utils.GetBufferStorageCmp(savedCrossingCmps[conduit_cell]), true, selectedObject);
               }
               else if(!checkedCrossings.ContainsKey(conduit_cell))
               {
                  MonitorCrossing(conduit_cell);
               }
            }
            else if(connectionsCount > 2)
               RegisterNewCrossing(conduit_cell, conduit_type);
         }

         UpdateAllMonitoredCrossings();




         void ManageDeserializedCrossings() {
            foreach(CrossingCmp crossingCmp in savedCrossingCmps.Values)
            {
               CrossingSprite.Create(crossingCmp, true/*<--this fixes layering issues*/);
               crossingCmp.gameObject.AddComponent<CopyBuildingSettings>();
               Utils.UpdateShouldManageFlowPriorities(crossingCmp);

               crossingCmp.pipeEndings = Enumerable.Repeat(PipeEnding.Invalid, 4).ToArray();
            }
            foreach(CrossingCmp crossingCmp in savedCrossingCmps.Values)
            {
               Utils.FollowAllDirections(crossingCmp);// each crossing has to have a value for pipeEndings before executing this
            }
         }


         void SavePreviousCrossingsCells() {
            foreach(CrossingCmp crossingCmp in savedCrossingCmps.Values)
            {
               previousCrossingsCells.Add(crossingCmp.crossingCell, new int[4]);
               for(int direction = 0; direction < 4; direction++)
               {
                  PipeEnding[] pipeEndings = crossingCmp.pipeEndings;
                  previousCrossingsCells[crossingCmp.crossingCell][direction] = pipeEndings[direction].type == PipeEnding.Type.CROSSING ? pipeEndings[direction].endingCell : -1;// <--this is important
               }
            }
         }


         void MonitorCrossing(int conduit_cell) {
            //----------------Finding the cluster----------------DOWN
            crossingsCluster.Clear();
            FindConnectedCrossings(conduit_cell);
            //----------------Finding the cluster----------------UP
            if(crossingsCluster.Count > 1 || crossingsCluster.Values.First().Item2[0].Item1 != -1)// if clusterSize > 1 OR crossing has wrong flow direction(s) towards sinks/sources
            {
               if(crossingsCluster.Count > 1)// shouldn't search for legal path if only one crossing is in the cluster
               {
                  //----------------Trying to fix illegal configurations----------------DOWN
                  Dictionary<int, int>/*crossing_cell, legal_direction*/ legalPath =
                      new Dictionary<int, int>();
                  int[] temp2 = crossingsCluster.Keys.ToArray();
                  foreach(int crossing_cell in temp2)
                  {
                     var clusterEntry = crossingsCluster[crossing_cell];
                     int swappedDirectionsCount = 0;
                     foreach(var forwardsDirection in clusterEntry.Item2)
                        if(forwardsDirection.Item1 != -1)
                           swappedDirectionsCount++;
                     if(clusterEntry.Item3.Item2)
                        swappedDirectionsCount++;

                     for(int tryNum = 0; tryNum < 4; tryNum++)
                     {
                        legalPath.Clear();

                        bool acceptDeadEnds = tryNum > 1;
                        bool reverseSearchOrder = tryNum % 2 == 1;

                        if(reverseSearchOrder && swappedDirectionsCount < 2)
                           continue;// if there is only one swapped direction, it is not necessary to reverse the search order

                        if(FindLegalPath(crossing_cell, legalPath, -1, acceptDeadEnds, reverseSearchOrder))
                        {
                           // !acceptDeadEnds & !reverseSearchOrder; !acceptDeadEnds & reverseSearchOrder;
                           // acceptDeadEnds & !reverseSearchOrder; acceptDeadEnds & reverseSearchOrder(00; 01; 10; 11)
                           foreach(int crossing in legalPath.Keys)
                           {
                              //-------Unsetting the legal direction for the crossing-------DOWN
                              // by unsetting is meant that this direction will not be switched in the following code
                              var crossingEntry = crossingsCluster[crossing];
                              if(crossingEntry.Item3.Item1 == legalPath[crossing])// if it's a backwardDirection
                              {
                                 var entry = crossingEntry;
                                 entry.Item3.Item2 = false;
                                 crossingEntry = entry;
                              }
                              else// else it's a forwardDirection
                              {
                                 var entry2 = crossingEntry;
                                 int index = Array.IndexOf(crossingEntry.Item2, crossingEntry.Item2
                                     .FindOrDefault(dir => dir.Item1 == legalPath[crossing], (-1, false)));
                                 if(index != -1)
                                 {
                                    entry2.Item2[index] = (-1, false);
                                    crossingEntry = entry2;
                                 }
                              }
                              //-------Unsetting the legal direction for the crossing-------UP
                              //-------Unsetting the legal direction for the other crossing-------DOWN
                              PipeEnding pipeEnding = Utils.FollowPipe(savedCrossingCmps[crossing], legalPath[crossing]);
                              int legal_direction = Utils.CountTrailingZeros((int)pipeEnding.backwardsDirection);
                              var otherCrossingEntry = crossingsCluster[pipeEnding.endingCell];
                              if(otherCrossingEntry.Item3.Item1 == legal_direction)// if it's a backwardDirection
                              {
                                 var entry = otherCrossingEntry;
                                 entry.Item3.Item2 = false;
                                 otherCrossingEntry = entry;
                              }
                              else// else it's a forwardDirection
                              {
                                 var entry2 = otherCrossingEntry;
                                 int index = Array.IndexOf(otherCrossingEntry.Item2, otherCrossingEntry.Item2
                                     .FindOrDefault(dir => dir.Item1 == legal_direction, (-1, false)));
                                 if(index != -1)
                                 {
                                    entry2.Item2[index] = (-1, false);
                                    otherCrossingEntry = entry2;
                                 }
                              }
                              //-------Unsetting the legal direction for the other crossing-------UP
                           }
                           break;
                        }
                     }
                  }
                  //----------------Trying to fix illegal configurations----------------DOWN
               }
               //----------------Switching flow directions back----------------DOWN
               foreach(int crossing_cell in crossingsCluster.Keys)
               {
                  CrossingCmp crossingCmp = savedCrossingCmps[crossing_cell];
                  foreach((int, bool) forwardDir in crossingsCluster[crossing_cell].Item2)
                  {
                     if(forwardDir.Item2 && forwardDir.Item1 != -1)
                     {
                        Utils.FollowPipe(crossingCmp, forwardDir.Item1);// the PipeEnding is not defined at this point
                        Utils.SwitchFlowDirection(crossingCmp, (ConduitFlow.FlowDirections)(1 << forwardDir.Item1), false);
                     }
                  }
               }
               //----------------Switching flow directions back----------------UP
            }
            foreach(int crossing in crossingsCluster.Keys)
            {
               if(!checkedCrossings.ContainsKey(crossing))
                  checkedCrossings.Add(crossing, crossingsCluster[crossing].Item4);
            }
         }


         void UpdateAllMonitoredCrossings() {
            foreach(int crossing_cell in checkedCrossings.Keys)
            {
               CrossingCmp crossingCmp = savedCrossingCmps[crossing_cell];
               if(!checkedCrossings[crossing_cell])// if !skipUpdate
               {
                  Utils.FollowAllDirections(crossingCmp);
                  Utils.UpdateCrossing(crossingCmp);
               }
               else
               {
                  // things that have to be updated in either case:
                  Utils.FollowAllDirections(crossingCmp);

                  Endpoint savedEndpoint = oldEndpoints.ContainsKey(crossing_cell) ? oldEndpoints[crossing_cell] : Endpoint.Conduit;
                  Utils.TryGetRealEndpointType(crossingCmp, out Endpoint realEndpoint);
                  //--needed if a sink/source was removed above the crossing--DOWN
                  if(savedEndpoint != realEndpoint)
                  {
                     CrossingSprite.UpdateIsIllegal(crossingCmp);
                     Utils.UpdateEndpointFlowPriority(crossingCmp, savedEndpoint != Endpoint.Conduit && realEndpoint != Endpoint.Conduit);
                     Utils.UpdateShouldManageFlowPriorities(crossingCmp);
                  }
                  //--[...]--UP
               }

               if(selectedIsNotNull)
               {
                  GameObject crossing_go = crossingCmp.gameObject;
                  if(selectedObject.Equals(crossing_go))
                     DetailsScreen.Instance.Refresh(crossing_go);// Needed for the Flow Configuration SideScreen to update
               }

               // saving crossing's network(used for optimization):
               Utils.StoreCrossingsNetwork(crossingCmp);
            }
         }


         void FindConnectedCrossings(int beginningCrossingCell, int backwardsDirection = -1, char previousCrossingPreviousFlow = '_'/*some random char*/) {
            if(!savedCrossingCmps.ContainsKey(beginningCrossingCell))
               return;

            (int, bool)[] forwardsDirections = Enumerable.Repeat((-1, false), 4).ToArray();
            bool skipUpdate = false;

            CrossingCmp beginningCrossingCmp = savedCrossingCmps[beginningCrossingCell];
            string oldCrossingID = beginningCrossingCmp.crossingID;
            string newCrossingID = Utils.GetRotatedCrossingID(beginningCrossingCmp);
            crossingsCluster.Add(beginningCrossingCell, (default, forwardsDirections, default, default));// beginningCrossingCell should be added to the cluster before FindingConnectedCrossings

            if(backwardsDirection == -1 ? (newCrossingID != oldCrossingID) : (oldCrossingID.Remove(backwardsDirection, 1) != newCrossingID.Remove(backwardsDirection, 1)))
            {
               for(int direction = 0; direction < 4; direction++)
               {
                  if(direction != backwardsDirection)
                  {
                     char oldFlowDirection = oldCrossingID[direction];
                     char newFlowDirection = newCrossingID[direction];
                     if((oldFlowDirection != '0') && (newFlowDirection != '0') && (newFlowDirection != oldFlowDirection))
                     {
                        PipeEnding pipeEnding = Utils.FollowPipe(beginningCrossingCmp, direction);
                        if(pipeEnding.type == PipeEnding.Type.CROSSING)
                        {
                           if(savedCrossingCmps.ContainsKey(pipeEnding.endingCell)/*isRegistered*/ &&
                              pipeEnding.endingCell == previousCrossingsCells[beginningCrossingCell][direction] && !checkedCrossings.ContainsKey(pipeEnding.endingCell))
                           {
                              int oppositeDirection = Utils.CountTrailingZeros((int)pipeEnding.backwardsDirection);

                              bool shouldBeSwitched = !crossingsCluster.ContainsKey(pipeEnding.endingCell) || !crossingsCluster[pipeEnding.endingCell].Item2.Contains((oppositeDirection, true));
                              for(int i = 0; i < 4; i++)
                                 if(forwardsDirections[i].Item1 == -1)
                                 { forwardsDirections[i] = (direction, shouldBeSwitched); break; }
                              // save this forwardsDirection before findingConnectedCrossings

                              if(!crossingsCluster.ContainsKey(pipeEnding.endingCell))
                                 FindConnectedCrossings(pipeEnding.endingCell, oppositeDirection, oldFlowDirection);
                           }
                        }
                        else if((pipeEnding.type == PipeEnding.Type.SINK && newFlowDirection == '1') || (pipeEnding.type == PipeEnding.Type.SOURCE && newFlowDirection == '2'))
                        {
                           // correcting the wrong flow direction towards a sink/source:
                           for(int i = 0; i < 4; i++)
                              if(forwardsDirections[i].Item1 == -1)
                              { forwardsDirections[i] = (direction, true); break; }
                        }
                     }
                  }
               }
            }
            else if(crossingsCluster.Count == 1)
               skipUpdate = true;

            crossingsCluster[beginningCrossingCell] = (newCrossingID, forwardsDirections,
                (backwardsDirection, backwardsDirection != -1 && (newCrossingID[backwardsDirection] == previousCrossingPreviousFlow)), skipUpdate);
         }


         bool FindLegalPath(int beginningCrossingCell, Dictionary<int, int> legalPath, int backwardsDirection, bool acceptDeadEnds, bool reverseSearchOrder = false) {
            CrossingCmp beginningCrossingCmp = savedCrossingCmps[beginningCrossingCell];

            int[] legalDirections = new int[2] { backwardsDirection, legalPath.ContainsKey(beginningCrossingCell) ? legalPath[beginningCrossingCell] : -1 };
            int[] swappedDirections;
            if(!WillCrossingConfigurationBeLegal())
            {
               if(legalPath.ContainsKey(beginningCrossingCell))
                  return false;

               legalPath.Add(beginningCrossingCell, default);// beginningCrossingCell should be added to the path before FindingLegalPath
               if(!reverseSearchOrder)
               {
                  foreach(int direction in swappedDirections)
                  {
                     if(direction != -1 && direction != backwardsDirection)
                     {
                        PipeEnding pipeEnding = Utils.FollowPipe(beginningCrossingCmp, direction);
                        if(pipeEnding.type == PipeEnding.Type.CROSSING/*don't delete this you fool!*/)
                        {
                           // saving this direction to the legalPath ensures that it won't get swapped:
                           legalPath[beginningCrossingCell] = direction;
                           // finding out whether the next crossing will be legal if this direction won't be swapped:
                           if(FindLegalPath(pipeEnding.endingCell, legalPath, Utils.CountTrailingZeros((int)pipeEnding.backwardsDirection), acceptDeadEnds))
                           {
                              return true;
                           }
                        }
                     }
                  }
               }
               else
               {
                  // reversing direction search order to try to find a legal path that goes in a different direction(reversing is not passed to recursion):
                  for(int i = swappedDirections.Length - 1; i > -1; i--)
                  {
                     int direction = swappedDirections[i];
                     if(direction != -1 && direction != backwardsDirection)
                     {
                        PipeEnding pipeEnding = Utils.FollowPipe(beginningCrossingCmp, direction);
                        if(pipeEnding.type == PipeEnding.Type.CROSSING/*don't delete this you fool!*/)
                        {
                           // saving this direction to the legalPath ensures that it won't get swapped:
                           legalPath[beginningCrossingCell] = direction;
                           // finding out whether the next crossing will be legal if this direction won't be swapped:
                           if(FindLegalPath(pipeEnding.endingCell, legalPath, Utils.CountTrailingZeros((int)pipeEnding.backwardsDirection), acceptDeadEnds))
                           {
                              return true;
                           }
                        }
                     }
                  }
               }
               legalPath.Remove(beginningCrossingCell);
               return false;
            }
            else
               return true;


            bool WillCrossingConfigurationBeLegal() {
               string crossingID = crossingsCluster[beginningCrossingCell].Item1;
               int inputscount = 0;
               int outputscount = 0;
               if(Utils.TryGetRealEndpointType(beginningCrossingCmp, out Endpoint endpoint_type))
               {
                  if(endpoint_type == Endpoint.Sink)
                     outputscount++;
                  else
                     inputscount++;
               }

               // collecting all swappedDirections except for the ones in the legalPath(they shouldn't be swapped):
               swappedDirections = new int[4];
               for(int i = 0; i < 4; i++)
               {
                  if(i == 3 &&// 4th direction is mostly backwardsDirection
                     crossingsCluster[beginningCrossingCell].Item3.Item2 && !legalDirections.Contains(crossingsCluster[beginningCrossingCell].Item3.Item1))
                  {
                     swappedDirections[3] = crossingsCluster[beginningCrossingCell].Item3.Item1;
                     continue;
                  }

                  if(!legalDirections.Contains(crossingsCluster[beginningCrossingCell].Item2[i].Item1))
                  {
                     swappedDirections[i] = crossingsCluster[beginningCrossingCell].Item2[i].Item1;
                  }
                  else
                  {
                     swappedDirections[i] = -1;
                  }
               }

               // changing the crossingID accordingly to the swappedDirections:
               char[] array = crossingID.ToCharArray();
               for(int i = 0; i < 4; i++)
               {
                  if(swappedDirections[i] != -1)
                     array[swappedDirections[i]] = crossingID[swappedDirections[i]] == '1' ? '2' : '1';
               }
               if(!acceptDeadEnds)
               {
                  for(int i = 0; i < 4; i++)
                  {
                     if(array[i] == '1' && Utils.FollowPipe(beginningCrossingCmp, i).type == PipeEnding.Type.DEAD_END)
                        array[i] = '0';// ignoring dead ends if acceptDeadEnds == false
                  }
               }
               crossingID = new string(array);

               inputscount += crossingID.Count(ch4r => ch4r == '1');
               outputscount += crossingID.Count(ch4r => ch4r == '2');
               return inputscount > 0 && outputscount > 0;
            }
         }
      }

      [HarmonyPatch(typeof(Conduit), "OnCleanUp")]
      public static class OnConduitDestroy_Patch {
         public static void Prefix(Conduit __instance) {
            GameObject conduit_go = __instance.gameObject;
            BufferStorageCmp bufferStorageCmp = null;
            bool bufferIsDefined = false;
            if(Utils.ConduitTypeToBuffersSet(__instance.ConduitType).Contains(__instance.Cell))
            {
               // can't use the Utils' methods for getting the cmps because the conduit_go is not on the Grid anymore
               bufferStorageCmp = conduit_go.GetComponent<BufferStorageCmp>();
               bufferIsDefined = true;
               FlowPriorityManagement_Patches.ForceDiscardBuffer(__instance.Cell, bufferStorageCmp);
            }
            if(Utils.ConduitTypeToCrossingsSet(__instance.ConduitType).Contains(__instance.Cell))
            {
               UnregisterCrossing(conduit_go.GetComponent<CrossingCmp>(), bufferIsDefined ? bufferStorageCmp : conduit_go.GetComponent<BufferStorageCmp>(), false);
            }
         }
      }

      private static void RegisterNewCrossing(int crossing_cell, ConduitType conduit_type) {
         GameObject crossing_go = Utils.GetConduitGO(crossing_cell, conduit_type);
         CrossingCmp crossingCmp = crossing_go.AddComponent<CrossingCmp>();
         crossingCmp.SetFieldsToDefault(crossing_cell, conduit_type);
         lock(Utils.ConduitTypeToCrossingsLock(conduit_type))
         {
            Utils.ConduitTypeToCrossingsSet(conduit_type).Add(crossing_cell);
         }

         Utils.FollowAllDirections(crossingCmp);
         Utils.SetDefaultFlowPriorities(crossingCmp);

         CrossingSprite.Create(crossingCmp, false);

         // saving crossing's network(used for optimization):
         Utils.StoreCrossingsNetwork(crossingCmp);

         crossing_go.AddComponent<CopyBuildingSettings>();

         Overlay_Patches.UpdateAdaptBuildingEndpointToCrossing(crossingCmp, Utils.ConduitTypeToShowCrossingsBool(conduit_type));
      }

      private static void UnregisterCrossing(CrossingCmp crossingCmp, BufferStorageCmp bufferStorageCmp, bool destroyCmps, GameObject selectedObject = null) {
         if(destroyCmps)
         {
            if(bufferStorageCmp != null)
            {
               bool bufferIsEmpty = true;
               foreach(ConduitContents buffer in bufferStorageCmp.bufferStorage)
               {
                  if(buffer.element != SimHashes.Vacuum && buffer.mass > 0.0)
                  {
                     bufferIsEmpty = false;
                     break;
                  }
               }

               if(bufferIsEmpty)
               {
                  KSelectable selectable = Grid.Objects[crossingCmp.crossingCell, (int)Utils.ConduitTypeToObjectLayer(crossingCmp.conduitType)]?.GetComponent<KSelectable>();
                  if(selectable != null)
                  {
                     selectable.RemoveStatusItem(Main.bufferContentsSI, true);
                  }

                  lock(Utils.ConduitTypeToBuffersLock(bufferStorageCmp.conduitType))
                  {
                     Utils.ConduitTypeToBuffersSet(bufferStorageCmp.conduitType).Remove(bufferStorageCmp.conduitCell);
                  }
                  UnityEngine.Object.Destroy(bufferStorageCmp);
               }
            }

            CopyBuildingSettings copysettings = crossingCmp.gameObject.GetComponent<CopyBuildingSettings>();
            KObjectManager.Instance.GetOrCreateObject(crossingCmp.gameObject).GetEventSystem().Unsubscribe(493375141, CopyBuildingSettings.OnRefreshUserMenuDelegate, true);
            UnityEngine.Object.Destroy(copysettings);
         }

         CrossingSprite.Remove(crossingCmp);

         Overlay_Patches.UpdateAdaptBuildingEndpointToCrossing(crossingCmp, false);

         if(selectedObject != null && selectedObject.Equals(crossingCmp.gameObject))
            DetailsScreen.Instance.DeselectAndClose();

         lock(Utils.ConduitTypeToCrossingsLock(crossingCmp.conduitType))
         {
            Utils.ConduitTypeToCrossingsSet(crossingCmp.conduitType).Remove(crossingCmp.crossingCell);
         }
         if(destroyCmps)
            UnityEngine.Object.Destroy(crossingCmp);
      }

      private static void CacheAllEndpoints(List<Network> networks, ConduitType conduit_type, out Dictionary<int, Endpoint> oldEndpoints) {
         oldEndpoints = new Dictionary<int, Endpoint>(Utils.ConduitTypeToEndpointsDict(conduit_type));

         Utils.ConduitTypeToEndpointsDict(conduit_type).Clear();

         foreach(var network in networks)
         {
            foreach(var sink in network.network.sinks)
            {
               Utils.ConduitTypeToEndpointsDict(conduit_type).Add(sink.Cell, sink.EndpointType);
            }
            foreach(var source in network.network.sources)
            {
               Utils.ConduitTypeToEndpointsDict(conduit_type).Add(source.Cell, source.EndpointType);
            }
         }
      }
   }
}