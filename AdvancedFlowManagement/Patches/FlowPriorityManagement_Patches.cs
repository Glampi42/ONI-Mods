using Database;
using HarmonyLib;
using Klei;
using KMod;
using MonoMod.Utils;
using STRINGS;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Linq;
using System.Reflection.Emit;
using System.Text;
using System.Reflection;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Assertions.Must;
using static ConduitFlow;
using Mono.Cecil.Cil;

namespace AdvancedFlowManagement.Patches {
   public class FlowPriorityManagement_Patches {
      public static void RecalculateUpdateOrder(UtilityNetwork utilityNetwork, ConduitFlow conduitFlow) {
         Network network = conduitFlow.networks.FindOrDefault(ntwrk => ntwrk.network == utilityNetwork, default);
         if(network.network == null)
            throw new Exception(Main.debugPrefix + $"Network {utilityNetwork.id} could not be found inside of ConduitFlow.networks list");

         RecalculateUpdateOrder(new List<Network> { network }, conduitFlow);
      }
      public static void RecalculateUpdateOrder(IList<Network> networks, ConduitFlow conduitFlow) {
         foreach(Network network in networks)
         {
            if(network.cells.Count > 0 && Utils.ConduitTypeToNetworksCrossings(conduitFlow.conduitType).ContainsKey(network.network.id) &&
               Utils.ConduitTypeToNetworksCrossings(conduitFlow.conduitType)[network.network.id].Count != 0)
            {
               UpdateOrderManager manager = new UpdateOrderManager(conduitFlow);
               manager.Calculate(network);
            }
         }
      }

      public class UpdateOrderManager {

         public UpdateOrderManager(ConduitFlow conduitFlow) {
            this.conduitFlow = conduitFlow;
         }

         private readonly ConduitFlow conduitFlow;

         public void Calculate(Network network) {
            List<int> crossings = Utils.ConduitTypeToNetworksCrossings(conduitFlow.conduitType)[network.network.id];
            Dictionary<int, CrossingCmp> cachedCrossingCmps = new Dictionary<int, CrossingCmp>(crossings.Count);
            foreach(int crossing_cell in crossings)
               cachedCrossingCmps.Add(crossing_cell, Utils.GetCrossingCmp(crossing_cell, conduitFlow.conduitType));

            //-----------------Adding up all cells-----------------DOWN
            Dictionary<int, bool[]> checkedDirections = new Dictionary<int, bool[]>();
            int cellsCount = 0;
            CountCellsRecursive(crossings[0]);

            if(cellsCount != network.cells.Count)
               return;// yeah, that happens...
            //-----------------Adding up all cells-----------------UP
            //-----------------Finding dead-end-crossings-----------------DOWN
            HashSet<int> deadEndCrossings = new HashSet<int>();
            foreach(int crossing in crossings)
            {
               bool isDeadEndCrossing = true;

               byte permittedFlow = (byte)conduitFlow.GetPermittedFlow(crossing);
               CrossingCmp crossingCmp = cachedCrossingCmps[crossing];
               for(int direction = 0; direction < 4; direction++)
               {
                  if(!Utils.IsBitSet(permittedFlow, 1 << direction))
                     continue;

                  PipeEnding pipeEnding = crossingCmp.pipeEndings[direction];
                  if(pipeEnding.type == PipeEnding.Type.CROSSING)
                  {
                     isDeadEndCrossing = false;
                     break;
                  }
               }

               if(isDeadEndCrossing)
                  deadEndCrossings.Add(crossing);
            }
            //-----------------Finding dead-end-crossings-----------------UP
            //---------------Sorting conduits---------------DOWN
            network.cells.Clear();

            HashSet<int> visitedCrossings = new HashSet<int>();
            checkedDirections.Clear();// reusing this Dict
            foreach(int deadEndCrossing in deadEndCrossings)
            {
               SortConnectionsRecursive(deadEndCrossing);
            }
            //---------------Sorting conduits---------------UP
            //---------------Sorting conduits---------------DOWN
            foreach(int leftoverCrossing in crossings)
            {
               // sorting crossings that weren't sorted yet(if there are any):
               if(!visitedCrossings.Contains(leftoverCrossing))
                  SortConnectionsRecursive(leftoverCrossing);
            }
            //---------------Sorting conduits---------------UP



            void CountCellsRecursive(int crossing_cell) {
               cellsCount++;
               checkedDirections.Add(crossing_cell, new bool[4]);
               CrossingCmp crossingCmp = cachedCrossingCmps[crossing_cell];
               for(int direction = 0; direction < 4; direction++)
               {
                  PipeEnding pipeEnding = crossingCmp.pipeEndings[direction];
                  if(pipeEnding.type != PipeEnding.Type.CROSSING || !checkedDirections.ContainsKey(pipeEnding.endingCell) ||
                     !checkedDirections[pipeEnding.endingCell][Utils.CountTrailingZeros((byte)pipeEnding.backwardsDirection)])
                  {
                     cellsCount += pipeEnding.pipeLength;

                     checkedDirections[crossing_cell][direction] = true;
                  }
                  if(pipeEnding.type == PipeEnding.Type.CROSSING && !checkedDirections.ContainsKey(pipeEnding.endingCell))
                  {
                     CountCellsRecursive(pipeEnding.endingCell);
                  }
               }
            }

            void SortConnectionsRecursive(int crossing_cell) {
               CrossingCmp crossingCmp = cachedCrossingCmps[crossing_cell];
               SOAInfo soaInfo = conduitFlow.soaInfo;

               // forwards directions that lead to some pipeEnding other than another crossing:
               byte permittedFlowDirections = (byte)conduitFlow.GetPermittedFlow(crossing_cell);
               for(int direction = 0; direction < 4; direction++)
               {
                  if(!Utils.IsBitSet(permittedFlowDirections, 1 << direction))
                     continue;

                  PipeEnding pipeEnding = crossingCmp.pipeEndings[direction];
                  if(pipeEnding.type != PipeEnding.Type.CROSSING && pipeEnding.pipeLength != 0)
                  {
                     network.cells.Add(pipeEnding.endingCell);

                     int previous_idx = conduitFlow.GetConduit(pipeEnding.endingCell).idx;
                     int current_idx = soaInfo.GetConduitFromDirection(previous_idx, pipeEnding.backwardsDirection).idx;
                     while(soaInfo.GetCell(current_idx) != crossing_cell)
                     {
                        network.cells.Add(soaInfo.GetCell(current_idx));

                        int temp_idx = current_idx;
                        current_idx = Utils.GetNextIdx(current_idx, previous_idx, conduitFlow, out _);
                        previous_idx = temp_idx;
                     }
                  }
               }

               // adding the crossing cell itself to the list:
               network.cells.Add(crossing_cell);

               // backwards directions:
               int[] directions = { 0, 1, 2, 3 };
               Array.Sort(directions, (dir1, dir2) => Utils.GetFlowPriority(crossingCmp, (sbyte)dir2) -
               Utils.GetFlowPriority(crossingCmp, (sbyte)dir1));// sorting directions from highest to lowest priority

               FlowDirections checkedLoopDirections = FlowDirections.None;
               List<int> crossingsToRecurse = new List<int>(4);
               for(int l = 0; l < 4; l++)
               {
                  int direction = directions[l];
                  if(crossingCmp.crossingID[direction] == '1')// if isInwardsDirection
                  {
                     PipeEnding pipeEnding = crossingCmp.pipeEndings[direction];
                     if(pipeEnding.type != PipeEnding.Type.CROSSING)
                     {
                        if(pipeEnding.pipeLength != 0)
                        {
                           int previous_idx = conduitFlow.GetConduit(crossing_cell).idx;
                           int current_idx = soaInfo.GetConduitFromDirection(previous_idx, (FlowDirections)(1 << direction)).idx;
                           do
                           {
                              network.cells.Add(soaInfo.GetCell(current_idx));

                              int temp_idx = current_idx;
                              current_idx = Utils.GetNextIdx(current_idx, previous_idx, conduitFlow, out _);
                              previous_idx = temp_idx;
                           } while(soaInfo.GetCell(previous_idx) != pipeEnding.endingCell);
                        }
                     }
                     else
                     {
                        if(pipeEnding.endingCell == crossing_cell)
                        {
                           if((checkedLoopDirections & pipeEnding.backwardsDirection) != 0)
                              continue;

                           checkedLoopDirections |= (FlowDirections)(1 << direction);
                           // a loop leading to the same crossing:
                           var utilityNetworkMngr = Utils.ConduitTypeToUtilityNetworkManager(conduitFlow.conduitType);

                           int previous_idx = conduitFlow.GetConduit(crossing_cell).idx;
                           int current_idx = soaInfo.GetConduitFromDirection(previous_idx, (FlowDirections)(1 << direction)).idx;
                           Utils.GetNextIdx(current_idx, previous_idx, conduitFlow, out FlowDirections directionToNext);

                           // going from 1st direction:
                           while(soaInfo.GetCell(current_idx) != crossing_cell/*just for safety*/ &&
                              !Utils.IsBitSet((byte)conduitFlow.GetPermittedFlow(current_idx), (byte)directionToNext)/*!reachedMiddle*/)
                           {
                              network.cells.Add(soaInfo.GetCell(current_idx));

                              int temp_idx = current_idx;
                              current_idx = Utils.GetNextIdx(current_idx, previous_idx, conduitFlow, out directionToNext);
                              previous_idx = temp_idx;
                           }

                           int middle_idx = current_idx;

                           if(soaInfo.GetCell(middle_idx) != crossing_cell)
                           {
                              previous_idx = conduitFlow.GetConduit(crossing_cell).idx;
                              current_idx = soaInfo.GetConduitFromDirection(previous_idx, pipeEnding.backwardsDirection).idx;
                              Utils.GetNextIdx(current_idx, previous_idx, conduitFlow, out directionToNext);

                              // going from 2nd direction:
                              while(current_idx != middle_idx)
                              {
                                 network.cells.Add(soaInfo.GetCell(current_idx));

                                 int temp_idx = current_idx;
                                 current_idx = Utils.GetNextIdx(current_idx, previous_idx, conduitFlow, out directionToNext);
                                 previous_idx = temp_idx;
                              }

                              // adding middle cell:
                              network.cells.Add(soaInfo.GetCell(middle_idx));
                           }
                        }
                        else
                        {
                           if(pipeEnding.pipeLength != 0)
                           {
                              // adding conduits between this crossing and the other:
                              int previous_idx = conduitFlow.GetConduit(crossing_cell).idx;
                              int current_idx = soaInfo.GetConduitFromDirection(previous_idx, (FlowDirections)(1 << direction)).idx;

                              while(soaInfo.GetCell(current_idx) != pipeEnding.endingCell)
                              {
                                 network.cells.Add(soaInfo.GetCell(current_idx));

                                 int temp_idx = current_idx;
                                 current_idx = Utils.GetNextIdx(current_idx, previous_idx, conduitFlow, out _);
                                 previous_idx = temp_idx;
                              }
                           }

                           // managing recursion:
                           if(!visitedCrossings.Contains(pipeEnding.endingCell))
                           {
                              int followingCrossingCell = pipeEnding.endingCell;

                              byte permittedFlow = (byte)conduitFlow.GetPermittedFlow(followingCrossingCell);

                              permittedFlow &= (byte)~pipeEnding.backwardsDirection;

                              PipeEnding[] otherPipeEndings = cachedCrossingCmps[followingCrossingCell].pipeEndings;
                              for(int dir = 0; dir < 4; dir++)
                                 if(otherPipeEndings[dir].type != PipeEnding.Type.CROSSING)
                                    permittedFlow &= (byte)~(1 << dir);

                              if(checkedDirections.ContainsKey(followingCrossingCell))
                                 for(int i = 0; i < 4; i++)
                                    if(checkedDirections[followingCrossingCell][i])// if wasThisDirectionChecked
                                       permittedFlow &= (byte)~(1 << i);

                              if(permittedFlow == 0b0)// if allDirectionsWereChecked
                              {
                                 crossingsToRecurse.Add(followingCrossingCell);
                              }
                              else
                              {
                                 if(!checkedDirections.ContainsKey(followingCrossingCell))
                                    checkedDirections.Add(followingCrossingCell, new bool[4]);
                                 checkedDirections[followingCrossingCell][Utils.CountTrailingZeros((byte)pipeEnding.backwardsDirection)] = true;
                              }
                           }
                        }
                     }
                  }
               }

               visitedCrossings.Add(crossing_cell);

               foreach(int nextCrossing in crossingsToRecurse)
                  SortConnectionsRecursive(nextCrossing);
            }
         }
      }

      public static void SaveCustomFlowConduits(int central_cell, ConduitFlow conduitFlow) {
         List<int> conduit_cells = new List<int>(5);
         conduit_cells.Add(central_cell);

         Network network = default;
         if(conduitFlow.GetConduit(central_cell).idx != -1)
         {
            network = conduitFlow.networks.FindOrDefault(ntwrk => ntwrk.network == conduitFlow.GetNetwork(conduitFlow.GetConduit(central_cell)), default);
         }
         else
         {
            throw new ArgumentException(Main.debugPrefix + $"Cell {central_cell} does not have a conduit");
         }
         if(network.network == null)
            throw new Exception(Main.debugPrefix + $"Network {conduitFlow.GetNetwork(conduitFlow.GetConduit(central_cell)).id} for cell {central_cell} " +
               $"could not be found inside of ConduitFlow.networks list");

         foreach(FlowDirections direction in Main.allFlowDirections)
         {
            if(network.cells.Contains(ConduitFlow.GetCellFromDirection(central_cell, direction)))
               conduit_cells.Add(ConduitFlow.GetCellFromDirection(central_cell, direction));
         }
         SaveCustomFlowConduits(conduit_cells, conduitFlow);
      }
      public static void SaveCustomFlowConduits(IList<Network> networks, ConduitFlow conduitFlow) {
         List<int> conduit_cells = new List<int>();
         foreach(Network network in networks)
         {
            conduit_cells.AddRange(network.cells);
         }
         SaveCustomFlowConduits(conduit_cells, conduitFlow);
      }
      private static void SaveCustomFlowConduits(List<int> conduit_cells, ConduitFlow conduitFlow) {
         foreach(int conduit_cell in conduit_cells)
         {
            bool customInPriorities = false;
            bool customOutPriorities = false;

            foreach(FlowDirections direction in Main.allFlowDirections)
            {
               if(Utils.IsBitSet((byte)conduitFlow.soaInfo.GetPermittedFlowDirections(conduitFlow.GetConduit(conduit_cell).idx), (byte)direction) &&
                  Utils.TryGetCrossingCmp(ConduitFlow.GetCellFromDirection(conduit_cell, direction), conduitFlow.conduitType, out CrossingCmp crossingCmp) &&
                  crossingCmp.shouldManageInPriorities)
               {
                  // if there is a crossing with custom input priorities in this direction
                  customInPriorities = true;
                  break;
               }
            }
            if(Utils.TryGetCrossingCmp(conduit_cell, conduitFlow.conduitType, out CrossingCmp crossingCmp2) &&
               crossingCmp2.shouldManageOutPriorities)
            {
               customOutPriorities = true;
            }

            if(customInPriorities || customOutPriorities)
            {
               Utils.ConduitTypeToCustomFlow(conduitFlow.conduitType).Add(conduit_cell);
            }
            else
            {
               Utils.ConduitTypeToCustomFlow(conduitFlow.conduitType).Remove(conduit_cell);
            }
         }
      }

      [HarmonyPatch(typeof(ConduitFlow), "UpdateConduit")]
      public static class OnUpdateConduit_Patch {
         [HarmonyPriority(Priority.HigherThanNormal)]
         public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions, ILGenerator generator) {
            List<CodeInstruction> codes = new List<CodeInstruction>(instructions);

            LocalBuilder fakeCrossingCmp = generator.DeclareLocal(typeof(CrossingCmp));
            LocalBuilder bufferStorageCmp = generator.DeclareLocal(typeof(BufferStorageCmp));
            LocalBuilder bufferStorageIsDefined = generator.DeclareLocal(typeof(bool));
            LocalBuilder customFlow = generator.DeclareLocal(typeof(bool));
            LocalBuilder customInPriorities = generator.DeclareLocal(typeof(FlowDirections));
            LocalBuilder customOutPriorities = generator.DeclareLocal(typeof(bool));
            LocalBuilder conduitMass = generator.DeclareLocal(typeof(float));
            LocalBuilder shouldBreak = generator.DeclareLocal(typeof(bool));
            LocalBuilder flowOccured = generator.DeclareLocal(typeof(bool));
            LocalBuilder offeredMass = generator.DeclareLocal(typeof(float));
            LocalBuilder loop_i = generator.DeclareLocal(typeof(int));

            LocalBuilder thisMaxMass = generator.DeclareLocal(typeof(float));


            List<CodeInstruction> codesCluster = new List<CodeInstruction>();
            //--------------Fix for High Pressure Applications--------------DOWN
            // a fix required for this mod to be fully compatible with it
            codesCluster.Add(new CodeInstruction(OpCodes.Ldloc_1));// load conduit_cell
            // HPA's transpiler uses 13th local variable to get the MaxMass, not 1st:
            codesCluster.Add(new CodeInstruction(OpCodes.Stloc_S, 13));// store to "cell2"
            codesCluster.Add(new CodeInstruction(OpCodes.Ldarg_0));// load conduitFlow
            // letting HPA's transpiler replace this line with the one that gets this conduit's real MaxMass and storing the result to MaxMass:
            codesCluster.Add(new CodeInstruction(OpCodes.Ldfld, Utils.GetFieldInfo<ConduitFlow, float>(cf => cf.MaxMass)));// load thisMaxMass
            codesCluster.Add(new CodeInstruction(OpCodes.Stloc_S, thisMaxMass));
            //--------------Fix for High Pressure Applications--------------UP
            //--------------Defining local variables--------------DOWN
            // fakeCrossingCmp = Utils.GetOrFakeCrossingCmp():
            codesCluster.Add(new CodeInstruction(OpCodes.Ldloc_1));// load conduit_cell
            codesCluster.Add(new CodeInstruction(OpCodes.Ldarg_0));// load conduitFlow
            codesCluster.Add(new CodeInstruction(OpCodes.Ldfld, Utils.GetFieldInfo<ConduitFlow, ConduitType>(cf => cf.conduitType)));// load conduit_type
            codesCluster.Add(new CodeInstruction(OpCodes.Call, SymbolExtensions.GetMethodInfo(() => Utils.GetOrFakeCrossingCmp(default, default))));
            codesCluster.Add(new CodeInstruction(OpCodes.Stloc_S, fakeCrossingCmp));
            // bufferStorageCmp = null:
            codesCluster.Add(new CodeInstruction(OpCodes.Ldnull));
            codesCluster.Add(new CodeInstruction(OpCodes.Stloc_S, bufferStorageCmp));
            // bufferStorageIsDefined = false:
            codesCluster.Add(new CodeInstruction(OpCodes.Ldc_I4_0));
            codesCluster.Add(new CodeInstruction(OpCodes.Stloc_S, bufferStorageIsDefined));
            // customFlow = Utils.ConduitTypeToCustomFlow(conduit_type).Contains(conduit_cell):
            codesCluster.Add(new CodeInstruction(OpCodes.Ldarg_0));// load conduitFlow
            codesCluster.Add(new CodeInstruction(OpCodes.Ldfld, Utils.GetFieldInfo<ConduitFlow, ConduitType>(cf => cf.conduitType)));// load conduit_type
            codesCluster.Add(new CodeInstruction(OpCodes.Call, SymbolExtensions.GetMethodInfo(() => Utils.ConduitTypeToCustomFlow(default))));
            codesCluster.Add(new CodeInstruction(OpCodes.Ldloc_1));// load conduit_cell
            codesCluster.Add(new CodeInstruction(OpCodes.Callvirt, SymbolExtensions.GetMethodInfo(() => ((HashSet<int>)default).Contains(default))));
            codesCluster.Add(new CodeInstruction(OpCodes.Stloc_S, customFlow));
            // customInPriorities = FlowDirections.None:
            codesCluster.Add(new CodeInstruction(OpCodes.Ldc_I4_0));
            codesCluster.Add(new CodeInstruction(OpCodes.Stloc_S, customInPriorities));
            // customOutPriorities = false:
            codesCluster.Add(new CodeInstruction(OpCodes.Ldc_I4_0));
            codesCluster.Add(new CodeInstruction(OpCodes.Stloc_S, customOutPriorities));
            // conduitMass = 0f:
            codesCluster.Add(new CodeInstruction(OpCodes.Ldc_I4_0));
            codesCluster.Add(new CodeInstruction(OpCodes.Stloc_S, conduitMass));
            // shouldBreak = false:
            codesCluster.Add(new CodeInstruction(OpCodes.Ldc_I4_0));
            codesCluster.Add(new CodeInstruction(OpCodes.Stloc_S, shouldBreak));
            // flowOccured = false:
            codesCluster.Add(new CodeInstruction(OpCodes.Ldc_I4_0));
            codesCluster.Add(new CodeInstruction(OpCodes.Stloc_S, flowOccured));
            // offeredMass = 0f:
            codesCluster.Add(new CodeInstruction(OpCodes.Ldc_I4_0));
            codesCluster.Add(new CodeInstruction(OpCodes.Stloc_S, offeredMass));
            //--------------Defining local variables--------------UP
            //--------------Inserting TryUnswapBufferStorage--------------DOWN
            // unswapping ConduitContents-BufferStorage(for swapping process see OnNetworkUpdateFinish_Patch):
            codesCluster.Add(new CodeInstruction(OpCodes.Ldloc_S, fakeCrossingCmp));
            codesCluster.Add(new CodeInstruction(OpCodes.Ldloca_S, bufferStorageCmp));
            codesCluster.Add(new CodeInstruction(OpCodes.Ldloca_S, bufferStorageIsDefined));
            codesCluster.Add(new CodeInstruction(OpCodes.Ldarg_0));// load conduitFlow
            codesCluster.Add(new CodeInstruction(OpCodes.Ldloc_S, thisMaxMass));
            codesCluster.Add(new CodeInstruction(OpCodes.Call, SymbolExtensions.GetMethodInfo(() => TryUnswapBufferStorage(default,
               ref InstancesLibrary.bufferStorageCmp, ref InstancesLibrary.Bool, default, default))));
            //--------------Inserting TryUnswapBufferStorage--------------UP
            //--------------Inserting GetCustomPriorities--------------DOWN
            codesCluster.Add(new CodeInstruction(OpCodes.Ldloc_S, customFlow));
            codesCluster.Add(new CodeInstruction(OpCodes.Ldarg_0));// load conduitFlow
            codesCluster.Add(new CodeInstruction(OpCodes.Ldloc_S, fakeCrossingCmp));
            codesCluster.Add(new CodeInstruction(OpCodes.Ldloca_S, customInPriorities));
            codesCluster.Add(new CodeInstruction(OpCodes.Ldloca_S, customOutPriorities));
            codesCluster.Add(new CodeInstruction(OpCodes.Call, SymbolExtensions.GetMethodInfo(() => GetCustomPriorities(default, default, default,
              ref InstancesLibrary.FlowDirections, ref InstancesLibrary.Bool))));
            //--------------Inserting GetCustomPriorities--------------UP

            int computeMovableMassIndex = -1;
            for(int i = 0; i < codes.Count; i++)
            {
               if(codes[i].Calls(SymbolExtensions.GetMethodInfo(() => ((ConduitFlow)default).ComputeMovableMass(default))))
               {
                  computeMovableMassIndex = i;
                  break;
               }
            }
            if(computeMovableMassIndex == -1)
               throw new Exception(Main.debugPrefix + "ComputeMovableMass() method could not be found");

            int beforeComputeMovableMassIndex = -1;
            for(int i = computeMovableMassIndex - 1; i > -1; i--)
            {
               if(codes[i].opcode == OpCodes.Stloc_1)// store conduit_cell
               {
                  beforeComputeMovableMassIndex = i + 1;// index after the conduit_cell was defined and before grid_node was defined
                  break;
               }
            }
            if(beforeComputeMovableMassIndex == -1)
               throw new Exception(Main.debugPrefix + "Before ComputeMovableMass() index could not be found");

            codes.InsertRange(beforeComputeMovableMassIndex, codesCluster);
            computeMovableMassIndex += codesCluster.Count;// compensating for inserting the codesCluster

            codesCluster.Clear();
            //---------------Inserting CustomComputeMovableMass---------------DOWN
            codes.RemoveAt(computeMovableMassIndex);
            // already loaded arguments on the evaluation stack:
            // conduitFlow
            // grid_node
            codesCluster.Add(new CodeInstruction(OpCodes.Ldloc_S, customFlow));
            codesCluster.Add(new CodeInstruction(OpCodes.Ldloc_S, customOutPriorities));
            codesCluster.Add(new CodeInstruction(OpCodes.Ldloca_S, conduitMass));
            codesCluster.Add(new CodeInstruction(OpCodes.Call, SymbolExtensions.GetMethodInfo(() => CustomComputeMovableMass(default, default, default, default, ref InstancesLibrary.Float))));
            codes.InsertRange(computeMovableMassIndex, codesCluster);
            //---------------Inserting CustomComputeMovableMass---------------UP
            //-------------Finding the beginning of the first loop's body-------------DOWN
            // the loop inside of if(movableMass <= 0.0) block:
            int firstLoopBodyIndex = -1;
            for(int i = computeMovableMassIndex + codesCluster.Count; i < codes.Count - 2; i++)
            {
               if(codes[i].Calls(SymbolExtensions.GetMethodInfo(() => ConduitFlow.ComputeNextFlowDirection(default))))
               {
                  firstLoopBodyIndex = i + 2;// index after "flowDirections1 = ComputeNextFlowDirection();" inside of loop's body; +2 to compensate for stloc
                  break;
               }
            }
            if(firstLoopBodyIndex == -1)
               throw new Exception(Main.debugPrefix + "First loop's body could not be found");
            //-------------Finding the beginning of the first loop's body-------------UP
            codesCluster.Clear();
            //----------------Inserting ManageZeroFlowDirection----------------DOWN
            System.Reflection.Emit.Label skipManageZeroFlowLabel = generator.DefineLabel();
            codes[firstLoopBodyIndex].WithLabels(skipManageZeroFlowLabel);

            codesCluster.Add(new CodeInstruction(OpCodes.Ldloc_S, customFlow));
            codesCluster.Add(new CodeInstruction(OpCodes.Brfalse_S, skipManageZeroFlowLabel));
            codesCluster.Add(new CodeInstruction(OpCodes.Ldloc_S, customInPriorities));
            codesCluster.Add(new CodeInstruction(OpCodes.Ldloc_S, 5));// load flowDirections1
            codesCluster.Add(new CodeInstruction(OpCodes.And));
            codesCluster.Add(new CodeInstruction(OpCodes.Brfalse_S, skipManageZeroFlowLabel));// if (customInPriorities & flowDirections1 != 0)

            codesCluster.Add(new CodeInstruction(OpCodes.Ldloc_1));// load conduit_cell
            codesCluster.Add(new CodeInstruction(OpCodes.Ldarg_0));// load conduitFlow
            codesCluster.Add(new CodeInstruction(OpCodes.Ldloc_S, 5));// load flowDirections1
            codesCluster.AddRange(LoadOtherMaxMass());
            codesCluster.Add(new CodeInstruction(OpCodes.Ldloca_S, bufferStorageCmp));
            codesCluster.Add(new CodeInstruction(OpCodes.Ldloca_S, bufferStorageIsDefined));
            codesCluster.Add(new CodeInstruction(OpCodes.Call, SymbolExtensions.GetMethodInfo(() => ManageZeroFlowDirection(default, default, default, default,
               ref InstancesLibrary.bufferStorageCmp, ref InstancesLibrary.Bool))));
            codes.InsertRange(firstLoopBodyIndex, codesCluster);
            //----------------Inserting ManageZeroFlowDirection----------------UP
            //-------------Finding the ComputeNextFlowDirection-------------DOWN
            int computeNextFlowDirIndex = -1;
            for(int i = firstLoopBodyIndex + codesCluster.Count; i < codes.Count; i++)
            {
               if(codes[i].Calls(SymbolExtensions.GetMethodInfo(() => ConduitFlow.ComputeNextFlowDirection(default))))
               {
                  computeNextFlowDirIndex = i;// this one is inside of the second for loop
                  break;
               }
            }
            if(computeNextFlowDirIndex == -1)
               throw new Exception(Main.debugPrefix + "ComputeNextFlowDirection() could not be found");
            //-------------Finding the ComputeNextFlowDirection-------------UP
            codesCluster.Clear();
            //-------------Inserting CustomComputeNextFlowDirection-------------DOWN
            codes.RemoveAt(computeNextFlowDirIndex);
            codes.RemoveAt(computeNextFlowDirIndex);// removing stloc instruction
            //-------part 1-------DOWN
            // already loaded arguments on the evaluation stack:
            // flowDirections1
            codesCluster.Add(new CodeInstruction(OpCodes.Ldloc_S, customFlow));
            codesCluster.Add(new CodeInstruction(OpCodes.Ldloca_S, 3));// load ref movableMass
            codesCluster.Add(new CodeInstruction(OpCodes.Ldloc_S, conduitMass));
            codesCluster.Add(new CodeInstruction(OpCodes.Ldloc_S, customOutPriorities));
            codesCluster.Add(new CodeInstruction(OpCodes.Ldloc_S, fakeCrossingCmp));
            codesCluster.Add(new CodeInstruction(OpCodes.Ldloca_S, 2));// load ref grid_node
            codesCluster.Add(new CodeInstruction(OpCodes.Ldarg_0));// load conduitFlow
            codesCluster.Add(new CodeInstruction(OpCodes.Ldloc_S, thisMaxMass));
            codesCluster.Add(new CodeInstruction(OpCodes.Ldloca_S, shouldBreak));
            codesCluster.Add(new CodeInstruction(OpCodes.Ldloca_S, offeredMass));
            codesCluster.Add(new CodeInstruction(OpCodes.Call, SymbolExtensions.GetMethodInfo(() => CustomComputeNextFlowDirection_part1(default, default,
               ref InstancesLibrary.Float, default, default, default, ref InstancesLibrary.GridNode, default, default, ref InstancesLibrary.Bool,
               ref InstancesLibrary.Float))));
            codesCluster.Add(new CodeInstruction(OpCodes.Stloc_S, 5));// store to flowDirections1
            //-------part 1-------UP
            // after part 1 flowDirections1 is defined which is used in LoadOtherMaxMass()
            //-------part 2-------DOWN
            System.Reflection.Emit.Label skipPart2Label = generator.DefineLabel();

            codesCluster.Add(new CodeInstruction(OpCodes.Ldloc_S, customFlow));
            codesCluster.Add(new CodeInstruction(OpCodes.Brfalse_S, skipPart2Label));
            codesCluster.Add(new CodeInstruction(OpCodes.Ldloc_S, customInPriorities));
            codesCluster.Add(new CodeInstruction(OpCodes.Ldloc_S, 5));// load flowDirections1
            codesCluster.Add(new CodeInstruction(OpCodes.And));
            codesCluster.Add(new CodeInstruction(OpCodes.Brfalse_S, skipPart2Label));// if (customInPriorities & flowDirections1 != 0)

            codesCluster.Add(new CodeInstruction(OpCodes.Ldloc_S, 5));// load flowDirections1
            codesCluster.Add(new CodeInstruction(OpCodes.Ldloc_S, fakeCrossingCmp));
            codesCluster.Add(new CodeInstruction(OpCodes.Ldloca_S, bufferStorageCmp));
            codesCluster.Add(new CodeInstruction(OpCodes.Ldloca_S, bufferStorageIsDefined));
            codesCluster.Add(new CodeInstruction(OpCodes.Ldloca_S, flowOccured));
            codesCluster.Add(new CodeInstruction(OpCodes.Ldloc_S, offeredMass));
            codesCluster.Add(new CodeInstruction(OpCodes.Ldarg_0));// load conduitFlow
            codesCluster.Add(new CodeInstruction(OpCodes.Ldloca_S, 3));// load ref movableMass
            codesCluster.AddRange(LoadOtherMaxMass());
            codesCluster.Add(new CodeInstruction(OpCodes.Call, SymbolExtensions.GetMethodInfo(() => CustomComputeNextFlowDirection_part2(default, default,
               ref InstancesLibrary.bufferStorageCmp, ref InstancesLibrary.Bool, ref InstancesLibrary.Bool, default, default, ref InstancesLibrary.Float, default))));
            //-------part 2-------UP
            //-------------Inserting CustomComputeNextFlowDirection-------------UP
            //---------------Breaking if necessary---------------DOWN
            int loopEndIndex = -1;
            System.Reflection.Emit.Label breakLoopLabel = default;
            for(int i = computeNextFlowDirIndex - 1; i > -1; i--)
            {
               if(codes[i].Branches(out System.Reflection.Emit.Label? label))// branching instruction on top of the loop
               {
                  // searching the CodeInstruction that is being branched to(bottom of the loop):
                  for(int k = i + 1; k < codes.Count; k++)
                  {
                     if(codes[k].labels.Contains(label.Value))
                     {
                        loopEndIndex = k - 4;// compensating for the 4 instructions at the end of the loop that increase the loop variable

                        for(int l = k + 1; l < codes.Count - 1; l++)
                        {
                           if(codes[l].Branches(out _))// branching instruction at the end of the loop
                           {
                              breakLoopLabel = generator.DefineLabel();
                              codes[l + 1].WithLabels(breakLoopLabel);// adding label to the instruction right after the loop
                              break;
                           }
                        }
                        break;
                     }
                  }
                  break;
               }
            }
            if(loopEndIndex == -1)
               throw new Exception(Main.debugPrefix + "Loop-end index could not be found");
            if(breakLoopLabel == default)
               throw new Exception(Main.debugPrefix + "Break-loop label could not be found");

            codesCluster.Add(new CodeInstruction(OpCodes.Ldloc_S, shouldBreak).WithLabels(skipPart2Label));
            codesCluster.Add(new CodeInstruction(OpCodes.Brtrue, breakLoopLabel));// if shouldBreak -> breaking the loop
            codes.InsertRange(computeNextFlowDirIndex, codesCluster);
            loopEndIndex += codesCluster.Count;
            //---------------Breaking if necessary---------------UP
            //----------------flag1 accessing fix----------------DOWN
            int flag1Index = -1;
            for(int i = computeNextFlowDirIndex + codesCluster.Count; i < codes.Count - 1; i++)
            {
               if(codes[i].opcode == OpCodes.Ldc_I4_1 && codes[i + 1].opcode == OpCodes.Stloc_0)
               {
                  flag1Index = i;
                  break;
               }
            }
            if(flag1Index == -1)
               throw new Exception(Main.debugPrefix + "flag1 index could not be found");

            System.Reflection.Emit.Label jumpOverflag1Label = default;
            for(int i = flag1Index - 1; i > -1; i--)
            {
               if(codes[i].Branches(out System.Reflection.Emit.Label? label))
               {
                  jumpOverflag1Label = label.Value;
                  break;
               }
            }
            if(jumpOverflag1Label == default)
               throw new Exception(Main.debugPrefix + "Jump-over-flag1 label could not be found");

            codesCluster.Clear();

            codesCluster.Add(new CodeInstruction(OpCodes.Ldloc_S, customFlow));
            codesCluster.Add(new CodeInstruction(OpCodes.Ldloc_S, customInPriorities));
            codesCluster.Add(new CodeInstruction(OpCodes.Ldloc_S, 5));// load flowDirections1
            codesCluster.Add(new CodeInstruction(OpCodes.Call, SymbolExtensions.GetMethodInfo(() => CustomInPrioritiesContainDirection(default, default, default))));
            codesCluster.Add(new CodeInstruction(OpCodes.Brtrue_S, jumpOverflag1Label));
            codes.InsertRange(flag1Index, codesCluster);
            loopEndIndex += codesCluster.Count;
            // the if statement then looks like this:
            // if(srcFlowDirection != 0 && !flag2 && !CustomInPrioritiesContainDirection())
            //----------------flag1 accessing fix----------------UP

            //-------------Finding labels that branch to the end of the loop-------------DOWN
            // inside of else block of if(srcFlowDirection != 0 && !flag2 && !customInPrioritiesContainDirection())
            List<System.Reflection.Emit.Label> labelsToLoopEnd = new List<System.Reflection.Emit.Label>(2);
            int elseBlockStartIndex = -1;
            for(int i = flag1Index + codesCluster.Count; i < codes.Count; i++)
            {
               if(codes[i].labels.Contains(jumpOverflag1Label))
               {
                  elseBlockStartIndex = i;// jumpOverflag1Label jumps into the else block
                  break;
               }
            }
            if(elseBlockStartIndex == -1)
               throw new Exception(Main.debugPrefix + "Else block start index could not be found");

            int elseBlockEndIndex = loopEndIndex;
            if(elseBlockEndIndex == -1)
               throw new Exception(Main.debugPrefix + "Else block end index could not be found");

            for(int i = elseBlockStartIndex; i < elseBlockEndIndex; i++)
            {
               if(codes[i].Branches(out System.Reflection.Emit.Label? label) && codes[loopEndIndex].labels.Contains(label.Value))
               {
                  labelsToLoopEnd.Add(label.Value);
               }
            }
            //-------------Finding labels that branch to the end of the loop-------------UP

            codesCluster.Clear();

            //-------------Inserting ForceContinueUpdatingIfNeeded-------------DOWN
            // this is inserted at the end of else block of if(srcFlowDirection != 0 && !flag2 && !customInPrioritiesContainDirection()):
            CodeInstruction firstCodeInstruction = new CodeInstruction(OpCodes.Ldloc_S, customFlow);

            // redirecting the labels that branch to the end of the loop to this instruction:
            foreach(System.Reflection.Emit.Label label in labelsToLoopEnd)
            {
               codes[loopEndIndex].labels.Remove(label);
               firstCodeInstruction.labels.Add(label);
            }

            codesCluster.Add(firstCodeInstruction);
            codesCluster.Add(new CodeInstruction(OpCodes.Ldloca_S, 0));// load ref flag1
            codesCluster.Add(new CodeInstruction(OpCodes.Ldloc_S, customInPriorities));
            codesCluster.Add(new CodeInstruction(OpCodes.Ldloc_S, 5));// load flowDirections1
            codesCluster.Add(new CodeInstruction(OpCodes.Ldloc_S, flowOccured));
            codesCluster.Add(new CodeInstruction(OpCodes.Ldloc_S, offeredMass));
            codesCluster.Add(new CodeInstruction(OpCodes.Ldloc_S, 16));// load flag3
            codesCluster.Add(new CodeInstruction(OpCodes.Call, SymbolExtensions.GetMethodInfo(() => ForceContinueUpdatingIfNeeded(default, ref InstancesLibrary.Bool,
               default, default, default, default, default))));
            codesCluster.Add(new CodeInstruction(OpCodes.Brtrue, breakLoopLabel));// if shouldBreak -> breaking the loop
            codes.InsertRange(elseBlockEndIndex, codesCluster);
            //-------------Inserting ForceContinueUpdatingIfNeeded-------------UP
            codesCluster.Clear();
            //-------------Inserting TryDiscardBufferStorage-------------DOWN

            // redirecting the labels that branch to the end of the method to this instruction:
            int methodEndIndex = codes.Count - 2;// last two instructions are ldloc.0 and ret; I need ldloc.0
            if(codes[codes.Count - 1].opcode != OpCodes.Ret || (codes[codes.Count - 2].opcode != OpCodes.Ldloc_0 && codes[codes.Count - 2].opcode != OpCodes.Ldloc_S))
               throw new Exception(Main.debugPrefix + "Method end index could not be found");

            codesCluster.Add(new CodeInstruction(OpCodes.Ldloc_S, fakeCrossingCmp).WithLabels(codes[methodEndIndex].ExtractLabels()));// extracting labels also removes them from that instruction
            codesCluster.Add(new CodeInstruction(OpCodes.Ldloc_S, bufferStorageCmp));
            codesCluster.Add(new CodeInstruction(OpCodes.Ldloc_S, bufferStorageIsDefined));
            codesCluster.Add(new CodeInstruction(OpCodes.Ldarg_0));// load conduitFlow
            codesCluster.Add(new CodeInstruction(OpCodes.Ldloc_S, thisMaxMass));
            codesCluster.Add(new CodeInstruction(OpCodes.Call, SymbolExtensions.GetMethodInfo(() => TryDiscardBufferStorage(default, default, default, default, default))));
            codes.InsertRange(methodEndIndex, codesCluster);
            //-------------Inserting TryDiscardBufferStorage-------------UP

            return codes.AsEnumerable();
         }


         private static void TryUnswapBufferStorage(CrossingCmp fakeCrossingCmp, ref BufferStorageCmp bufferStorageCmp, ref bool bufferStorageIsDefined,
            ConduitFlow conduitFlow, float MaxMass) {
            if(Utils.IsCrossingRegistered(fakeCrossingCmp) && Utils.ConduitRequiresBuffer(fakeCrossingCmp))
            {
               bufferStorageCmp = Utils.CreateOrGetBufferStorage(fakeCrossingCmp, MaxMass);
               bufferStorageIsDefined = true;

               if(fakeCrossingCmp.swappedBufferStorage)
               {
                  ConduitContents tempContents = GetContentsDirectly(fakeCrossingCmp.crossingCell, conduitFlow);
                  SetContentsDirectly(fakeCrossingCmp.crossingCell, bufferStorageCmp.bufferStorage[0], conduitFlow);
                  if(bufferStorageCmp.bufferStorage[1].element != SimHashes.Vacuum)
                  {
                     ConduitContents secondBuffer = bufferStorageCmp.bufferStorage[1];
                     MoveConduitContents(ref secondBuffer, ref tempContents, secondBuffer.mass, MaxMass, out _);
                     Utils.StoreBufferContents(bufferStorageCmp, secondBuffer, 1);
                  }
                  Utils.StoreBufferContents(bufferStorageCmp, tempContents, 0);
                  fakeCrossingCmp.swappedBufferStorage = false;
               }
            }
         }

         private static void GetCustomPriorities(bool customFlow, ConduitFlow conduitFlow, CrossingCmp fakeCrossingCmp, ref FlowDirections customInPriorities, ref bool customOutPriorities) {
            if(customFlow)
            {
               customInPriorities = FlowDirections.None;
               customOutPriorities = false;

               foreach(FlowDirections direction in Main.allFlowDirections)
               {
                  if(Utils.IsBitSet((byte)conduitFlow.soaInfo.GetPermittedFlowDirections(conduitFlow.GetConduit(fakeCrossingCmp.crossingCell).idx), (byte)direction) &&
                     Utils.TryGetCrossingCmp(ConduitFlow.GetCellFromDirection(fakeCrossingCmp.crossingCell, direction), conduitFlow.conduitType, out CrossingCmp tempCmp) &&
                     tempCmp.shouldManageInPriorities)
                  {
                     // if there is a crossing with custom input priorities in this direction
                     customInPriorities |= direction;
                  }
               }
               if(Utils.IsCrossingRegistered(fakeCrossingCmp) &&
                  fakeCrossingCmp.shouldManageOutPriorities)
               {
                  customOutPriorities = true;
               }
            }
         }

         private static float CustomComputeMovableMass(ConduitFlow conduitFlow, GridNode grid_node, bool customFlow,
            bool customOutPriorities, ref float conduitMass) {
            if(customFlow)
            {
               float movableMass = GetMovableMass(grid_node, customOutPriorities, conduitFlow);
               conduitMass = movableMass;
               return movableMass;
            }
            else
            {
               return conduitFlow.ComputeMovableMass(grid_node);
            }
         }

         private static void ManageZeroFlowDirection(int conduit_cell, ConduitFlow conduitFlow, FlowDirections direction, float otherMaxMass,
            ref BufferStorageCmp bufferStorageCmp, ref bool bufferStorageIsDefined) {
            // needed for source flow to behave correctly as well as for increasing occuredFlowsCount:
            ComputeInputsFlowDistribution(ConduitFlow.GetCellFromDirection(conduit_cell, direction),
               direction, 0f, conduitFlow, otherMaxMass, ref bufferStorageCmp, ref bufferStorageIsDefined, out _);
         }

         private static FlowDirections CustomComputeNextFlowDirection_part1(FlowDirections flowDirections1, bool customFlow, ref float movableMass, float conduitMass,
            bool customOutPriorities, CrossingCmp fakeCrossingCmp, ref GridNode grid_node, ConduitFlow conduitFlow, float thisMaxMass,
             ref bool shouldBreak, ref float offeredMass) {
            if(customFlow)
            {
               movableMass = conduitMass;
               if(customOutPriorities)
               {
                  flowDirections1 = ComputeOutputsFlowDistribution(fakeCrossingCmp, ref movableMass, ref grid_node, conduitFlow, thisMaxMass);
                  if(flowDirections1 == ConduitFlow.FlowDirections.None)
                  {
                     shouldBreak = true;
                  }
               }
               else
               {
                  flowDirections1 = ConduitFlow.ComputeNextFlowDirection(flowDirections1);
               }

               offeredMass = movableMass;

               return flowDirections1;
            }
            else
            {
               return ConduitFlow.ComputeNextFlowDirection(flowDirections1);
            }
         }
         private static void CustomComputeNextFlowDirection_part2(FlowDirections flowDirections1, CrossingCmp fakeCrossingCmp,
            ref BufferStorageCmp bufferStorageCmp, ref bool bufferStorageIsDefined, ref bool flowOccured,
            float offeredMass, ConduitFlow conduitFlow, ref float movableMass, float otherMaxMass) {
            float requestedMass = ComputeInputsFlowDistribution(ConduitFlow.GetCellFromDirection(fakeCrossingCmp.crossingCell, flowDirections1),
               flowDirections1, offeredMass, conduitFlow, otherMaxMass, ref bufferStorageCmp, ref bufferStorageIsDefined, out flowOccured);

            movableMass = Mathf.Min(offeredMass, requestedMass);
         }

         private static List<CodeInstruction> LoadOtherMaxMass() {
            List<CodeInstruction> codes = new List<CodeInstruction>();

            codes.Add(new CodeInstruction(OpCodes.Ldloc_1));// load conduit_cell
            codes.Add(new CodeInstruction(OpCodes.Ldloc_S, 5));// load flowDirections1
            codes.Add(new CodeInstruction(OpCodes.Call, SymbolExtensions.GetMethodInfo(() => ConduitFlow.GetCellFromDirection(default, default))));
            // High Pressure Applications' transpiler uses 13th local variable to get the MaxMass:
            codes.Add(new CodeInstruction(OpCodes.Stloc_S, 13));// store to "cell2"
            codes.Add(new CodeInstruction(OpCodes.Ldarg_0));// load conduitFlow
            // letting HPA's transpiler replace this line with the one that gets other conduit's real MaxMass:
            codes.Add(new CodeInstruction(OpCodes.Ldfld, Utils.GetFieldInfo<ConduitFlow, float>(cf => cf.MaxMass)));// load otherMaxMass

            return codes;
         }

         private static bool CustomInPrioritiesContainDirection(bool customFlow, FlowDirections customInPriorities, FlowDirections flowDirections1) {
            if(customFlow)
            {
               return (customInPriorities & flowDirections1) != 0;
            }
            else
            {
               return false;
            }
         }

         private static bool ForceContinueUpdatingIfNeeded(bool customFlow, ref bool flag1, FlowDirections customInPriorities, FlowDirections flowDirections1,
            bool flowOccured, float offeredMass, bool flag3) {
            if(customFlow)
            {
               if((customInPriorities & flowDirections1) != 0 && !flowOccured && offeredMass > 0f && flag3)
               {
                  flag1 = true;// contents didn't flow because it wasn't it's turn to flow; should continue updating
                  return true;// should break
               }
            }
            return false;// should not break
         }

         private static void TryDiscardBufferStorage(CrossingCmp fakeCrossingCmp, BufferStorageCmp bufferStorageCmp, bool bufferStorageIsDefined,
            ConduitFlow conduitFlow, float MaxMass) {
            if(Utils.ConduitTypeToBuffersSet(fakeCrossingCmp.conduitType).Contains(fakeCrossingCmp.crossingCell))
            {
               if(!Utils.ConduitRequiresBuffer(fakeCrossingCmp))
               {
                  if(!bufferStorageIsDefined)
                     bufferStorageCmp = Utils.CreateOrGetBufferStorage(fakeCrossingCmp, MaxMass);
                  bool emptyBuffer = true;
                  for(int i = 0; i < bufferStorageCmp.bufferStorage.Length; i++)
                  {
                     ConduitContents bufferContents = bufferStorageCmp.bufferStorage[i];
                     if(bufferContents.element != SimHashes.Vacuum)
                     {
                        ConduitContents theseContents = GetContentsDirectly(fakeCrossingCmp.crossingCell, conduitFlow);
                        MoveConduitContents(ref bufferContents, ref theseContents, bufferContents.mass, MaxMass, out _);
                        Utils.StoreBufferContents(bufferStorageCmp, bufferContents, i);
                        SetContentsDirectly(fakeCrossingCmp.crossingCell, theseContents, conduitFlow);

                        if(bufferContents.mass > 0f)
                           emptyBuffer = false;
                     }
                  }
                  if(emptyBuffer)
                  {
                     KSelectable selectable = Grid.Objects[fakeCrossingCmp.crossingCell, (int)Utils.ConduitTypeToObjectLayer(fakeCrossingCmp.conduitType)]?.GetComponent<KSelectable>();
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
            }
         }

         //public static bool Prefixoooo(ConduitFlow.Conduit conduit, Dictionary<int, ConduitFlow.Sink> sinks, ConduitFlow __instance, ref bool __result, out bool __state) {
         //   ConduitFlow.SOAInfo soaInfo = __instance.soaInfo;
         //   int conduit_cell = soaInfo.GetCell(conduit.idx);
         //   CrossingCmp fakeCrossingCmp = Utils.GetOrFakeCrossingCmp(conduit_cell, __instance.conduitType);
         //   BufferStorageCmp bufferStorageCmp = null;
         //   bool bufferStorageIsDefined = false;

         //   TryUnswapBufferStorage2(fakeCrossingCmp, bufferStorageCmp, ref bufferStorageIsDefined, __instance);

         //   bool runMethod = true;
         //   if(Utils.ConduitTypeToCustomFlow(__instance.conduitType).Contains(conduit_cell))
         //   {
         //      Debug.Log("Managing custom flow priorities of " + conduit_cell);
         //      GetCustomPriorities2(__instance, fakeCrossingCmp, out List<FlowDirections> customInPriorities, out bool customOutPriorities);

         //      ConduitFlow.GridNode[] grid = __instance.grid;
         //      float thisMaxMass = __instance.thisMaxMass;
         //      //-----------------Modified original method-----------------DOWN
         //      bool flag1 = false;
         //      ConduitFlow.GridNode grid_node = grid[conduit_cell];
         //      float conduitMass = GetMovableMass(grid_node, customOutPriorities, __instance, sinks);
         //      Debug.Log("conduitMass: " + conduitMass);
         //      float movableMass = conduitMass;
         //      ConduitFlow.FlowDirections permittedFlowDirections = soaInfo.GetPermittedFlowDirections(conduit.idx);
         //      ConduitFlow.FlowDirections flowDirections1 = soaInfo.GetTargetFlowDirection(conduit.idx);
         //      if((double)movableMass <= 0.0)
         //      {
         //         // needed for source flow to behave correctly as well as for increasing occuredFlowsCount:
         //         foreach(FlowDirections direction in customInPriorities)
         //         {
         //            GetInputMassPortion(direction, 0f, out _);
         //         }

         //         for(int index = 0; index != 4; ++index)
         //         {
         //            flowDirections1 = ConduitFlow.ComputeNextFlowDirection(flowDirections1);
         //            if((permittedFlowDirections & flowDirections1) != ConduitFlow.FlowDirections.None)
         //            {
         //               ConduitFlow.Conduit conduitFromDirection = soaInfo.GetConduitFromDirection(conduit.idx, flowDirections1);
         //               Debug.Assert(conduitFromDirection.idx != -1);
         //               if((soaInfo.GetSrcFlowDirection(conduitFromDirection.idx) & ConduitFlow.Opposite(flowDirections1)) != 0)
         //                  soaInfo.SetPullDirection(conduitFromDirection.idx, flowDirections1);
         //            }
         //         }
         //      }
         //      else
         //      {
         //         for(int index = 0; index != 4; ++index)
         //         {
         //            //------------------Computing flowDirections1 & movableMass------------------DOWN
         //            movableMass = conduitMass;// resetting movableMass to the actual conduitMass
         //            if(customOutPriorities)
         //            {
         //               flowDirections1 = NextFlowDirection(ref movableMass, ref grid_node);
         //               if(flowDirections1 == ConduitFlow.FlowDirections.None)
         //               {
         //                  // no more directions to try to flow in
         //                  break;
         //               }
         //            }
         //            else
         //            {
         //               flowDirections1 = ConduitFlow.ComputeNextFlowDirection(flowDirections1);
         //            }

         //            float offeredMass = movableMass;
         //            bool flowOccured = false;

         //            if(customInPriorities.Contains(flowDirections1))
         //            {
         //               float requestedMass = GetInputMassPortion(flowDirections1, offeredMass, out flowOccured);
         //               Debug.Log("flowOccured: " + flowOccured);

         //               movableMass = Mathf.Min(offeredMass, requestedMass);
         //            }
         //            //------------------Computing flowDirections1 & movableMass------------------UP
         //            if((permittedFlowDirections & flowDirections1) != ConduitFlow.FlowDirections.None)
         //            {
         //               ConduitFlow.Conduit conduitFromDirection = soaInfo.GetConduitFromDirection(conduit.idx, flowDirections1);
         //               Debug.Assert(conduitFromDirection.idx != -1);
         //               int srcFlowDirection = (int)soaInfo.GetSrcFlowDirection(conduitFromDirection.idx);
         //               bool flag2 = ((ConduitFlow.FlowDirections)srcFlowDirection & ConduitFlow.Opposite(flowDirections1)) != 0;
         //               if(srcFlowDirection != 0 && !flag2 && !customInPriorities.Contains(flowDirections1))
         //               {
         //                  flag1 = true;
         //               }
         //               else
         //               {
         //                  int cell2 = soaInfo.GetCell(conduitFromDirection.idx);
         //                  Debug.Assert(cell2 != -1);
         //                  ConduitFlow.ConduitContents contents1 = grid[cell2].contents;
         //                  int num1 = contents1.element == SimHashes.Vacuum ? 1 : (contents1.element == grid_node.contents.element ? 1 : 0);
         //                  float effectiveCapacity = contents1.GetEffectiveCapacity(thisMaxMass);
         //                  bool flag3 = num1 != 0 && (double)effectiveCapacity > 0.0;
         //                  float num2 = Mathf.Min(movableMass, effectiveCapacity);
         //                  if(flag2 & flag3)
         //                     soaInfo.SetPullDirection(conduitFromDirection.idx, flowDirections1);
         //                  if((double)num2 > 0.0 && flag3)
         //                  {
         //                     Debug.Log("Moved mass: " + num2);
         //                     Debug.Log("...to cell: " + cell2);
         //                     soaInfo.SetTargetFlowDirection(conduit.idx, flowDirections1);
         //                     Debug.Assert((double)grid_node.contents.temperature > 0.0);
         //                     contents1.temperature = GameUtil.GetFinalTemperature(grid_node.contents.temperature, num2, contents1.temperature, contents1.mass);
         //                     contents1.AddMass(num2);
         //                     contents1.element = grid_node.contents.element;
         //                     int src1_count = (int)((double)num2 / (double)grid_node.contents.mass * (double)grid_node.contents.diseaseCount);
         //                     if(src1_count != 0)
         //                     {
         //                        SimUtil.DiseaseInfo finalDiseaseInfo = SimUtil.CalculateFinalDiseaseInfo(grid_node.contents.diseaseIdx, src1_count, contents1.diseaseIdx, contents1.diseaseCount);
         //                        contents1.diseaseIdx = finalDiseaseInfo.idx;
         //                        contents1.diseaseCount = finalDiseaseInfo.count;
         //                     }
         //                     grid[cell2].contents = contents1;
         //                     Debug.Assert((double)num2 <= (double)grid_node.contents.mass);
         //                     float amount = grid_node.contents.mass - num2;
         //                     float num3 = movableMass - num2;
         //                     if((double)amount <= 0.0)
         //                     {
         //                        Debug.Assert((double)num3 <= 0.0);
         //                        soaInfo.SetLastFlowInfo(conduit.idx, flowDirections1, ref grid_node.contents);
         //                        grid_node.contents = ConduitFlow.ConduitContents.Empty;
         //                     }
         //                     else
         //                     {
         //                        int num4 = (int)((double)amount / (double)grid_node.contents.mass * (double)grid_node.contents.diseaseCount);
         //                        Debug.Assert(num4 >= 0);
         //                        ConduitFlow.ConduitContents contents2 = grid_node.contents;
         //                        contents2.RemoveMass(amount);
         //                        contents2.diseaseCount -= num4;
         //                        grid_node.contents.RemoveMass(num2);
         //                        grid_node.contents.diseaseCount = num4;
         //                        if(num4 == 0)
         //                           grid_node.contents.diseaseIdx = byte.MaxValue;
         //                        soaInfo.SetLastFlowInfo(conduit.idx, flowDirections1, ref contents2);
         //                     }
         //                     grid[conduit_cell].contents = grid_node.contents;
         //                     flag1 = 0.0 < GetMovableMass(grid_node, customOutPriorities, __instance, sinks);
         //                     break;
         //                  }

         //                  Debug.Log($"check: {customInPriorities.Contains(flowDirections1)}, {!flowOccured}, {offeredMass > 0f}, {flag3}");
         //                  if(customInPriorities.Contains(flowDirections1) && !flowOccured && offeredMass > 0f && flag3)
         //                  {
         //                     Debug.Log("forcedContinueUpdating");
         //                     flag1 = true;// contents didn't flow because it wasn't it's turn to flow; should continue updating
         //                     break;
         //                  }
         //               }
         //            }
         //         }
         //      }
         //      ConduitFlow.FlowDirections srcFlowDirection1 = soaInfo.GetSrcFlowDirection(conduit.idx);
         //      ConduitFlow.FlowDirections pullDirection = soaInfo.GetPullDirection(conduit.idx);
         //      if(srcFlowDirection1 == ConduitFlow.FlowDirections.None || (ConduitFlow.Opposite(srcFlowDirection1) & pullDirection) != ConduitFlow.FlowDirections.None)
         //      {
         //         soaInfo.SetPullDirection(conduit.idx, ConduitFlow.FlowDirections.None);
         //         soaInfo.SetSrcFlowDirection(conduit.idx, ConduitFlow.FlowDirections.None);
         //         for(int index1 = 0; index1 != 2; ++index1)
         //         {
         //            ConduitFlow.FlowDirections flowDirections2 = srcFlowDirection1;
         //            for(int index2 = 0; index2 != 4; ++index2)
         //            {
         //               flowDirections2 = ConduitFlow.ComputeNextFlowDirection(flowDirections2);
         //               ConduitFlow.Conduit conduitFromDirection = soaInfo.GetConduitFromDirection(conduit.idx, flowDirections2);
         //               if(conduitFromDirection.idx != -1 && (soaInfo.GetPermittedFlowDirections(conduitFromDirection.idx) & ConduitFlow.Opposite(flowDirections2)) != ConduitFlow.FlowDirections.None)
         //               {
         //                  ConduitFlow.ConduitContents contents = grid[soaInfo.GetCell(conduitFromDirection.idx)].contents;
         //                  if(0.0 < (index1 == 0 ? (double)contents.movable_mass : (double)contents.mass))
         //                  {
         //                     soaInfo.SetSrcFlowDirection(conduit.idx, flowDirections2);
         //                     break;
         //                  }
         //               }
         //            }
         //            if(soaInfo.GetSrcFlowDirection(conduit.idx) != ConduitFlow.FlowDirections.None)
         //               break;
         //         }
         //      }
         //      __result = flag1;
         //      //-----------------Modified original method-----------------UP
         //      //--------------Discarding buffer storage--------------DOWN
         //      // emptying the buffer storage if it is full but unneeded anymore
         //      if(Utils.ConduitTypeToBuffersSet(fakeCrossingCmp.conduitType).Contains(fakeCrossingCmp.crossingCell))
         //      {
         //         if(!Utils.ConduitRequiresBuffer(fakeCrossingCmp))
         //         {
         //            if(!bufferStorageIsDefined)
         //               bufferStorageCmp = Utils.GetBufferStorageCmp(fakeCrossingCmp);
         //            Debug.Log("Discarding buffer 1");
         //            bool emptyBuffer = true;
         //            for(int i = 0; i < bufferStorageCmp.bufferStorage.Length; i++)
         //            {
         //               ConduitContents bufferContents = bufferStorageCmp.bufferStorage[i];
         //               if(bufferContents.element != SimHashes.Vacuum)
         //               {
         //                  ConduitContents theseContents = GetContentsDirectly(conduit_cell, __instance);
         //                  MoveConduitContents(ref bufferContents, ref theseContents, bufferContents.mass, __instance, out _);
         //                  Utils.StoreBufferContents(bufferStorageCmp, bufferContents, i);
         //                  SetContentsDirectly(conduit_cell, theseContents, __instance);

         //                  if(bufferContents.mass > 0f)
         //                     emptyBuffer = false;
         //               }
         //            }
         //            if(emptyBuffer)
         //            {
         //               KSelectable selectable = Grid.Objects[conduit_cell, (int)Utils.ConduitTypeToObjectLayer(__instance.conduitType)]?.GetComponent<KSelectable>();
         //               if(selectable != null)
         //               {
         //                  selectable.RemoveStatusItem(Main.bufferContentsSI, true);
         //                  Debug.Log("Removed buffer SI");
         //               }

         //               lock(Main.lockBuffersHashSet)
         //               {
         //                  Utils.ConduitTypeToBuffersSet(bufferStorageCmp.conduitType).Remove(bufferStorageCmp.conduitCell);
         //               }
         //               UnityEngine.Object.Destroy(bufferStorageCmp);
         //            }
         //         }
         //      }
         //      //--------------Discarding buffer storage--------------UP

         //      runMethod = false;
         //      Debug.Log("continueUpdating: " + __result);
         //      Debug.Log("---------------");
         //   }
         //   __state = runMethod;
         //   return runMethod;


         //   float GetInputMassPortion(FlowDirections direction_inwards, float offeredMass, out bool flowOccured) {
         //      Debug.Log("GetInputMassPortion");
         //      int crossing_cell = ConduitFlow.GetCellFromDirection(conduit_cell, direction_inwards);
         //      return ComputeInputsFlowDistribution(crossing_cell, direction_inwards, offeredMass, __instance, ref bufferStorageCmp, ref bufferStorageIsDefined,
         //         out flowOccured);
         //   }

         //   ConduitFlow.FlowDirections NextFlowDirection(ref float movableMass, ref GridNode grid_node) {
         //      Debug.Log("NextFlowDirection");
         //      return ComputeOutputsFlowDistribution(fakeCrossingCmp, ref movableMass, ref grid_node, __instance);
         //   }
         //}

         //public static void Postfixoooo(ConduitFlow.Conduit conduit, Dictionary<int, ConduitFlow.Sink> sinks, ConduitFlow __instance, bool __state) {
         //   if(__state)
         //   {
         //      var soaInfo = __instance.soaInfo;
         //      //--------------Discarding buffer storage 2--------------DOWN
         //      // emptying the buffer storage if it is full but unneeded anymore; this one is needed for the conduits that don't have custom flow but still have something in their buffer
         //      if(Utils.TryGetBufferStorageCmp(Utils.FakeCrossingCmp(soaInfo.GetCell(conduit.idx), __instance.conduitType), out BufferStorageCmp bufferStorageCmp))
         //      {
         //         int cell = soaInfo.GetCell(conduit.idx);
         //         CrossingCmp fakeCrossingCmp = Utils.GetOrFakeCrossingCmp(cell, __instance.conduitType);
         //         if(!Utils.ConduitRequiresBuffer(fakeCrossingCmp))
         //         {
         //            Debug.Log("Discarding buffer 2");
         //            bool emptyBuffer = true;
         //            for(int i = 0; i < bufferStorageCmp.bufferStorage.Length; i++)
         //            {
         //               ConduitContents bufferContents = bufferStorageCmp.bufferStorage[i];
         //               if(bufferContents.element != SimHashes.Vacuum)
         //               {
         //                  ConduitContents theseContents = __instance.GetContents(cell);
         //                  MoveConduitContents(ref bufferContents, ref theseContents, bufferContents.mass, __instance, out _);
         //                  Utils.StoreBufferContents(bufferStorageCmp, bufferContents, i);
         //                  __instance.SetContents(cell, theseContents);

         //                  if(bufferContents.mass > 0f)
         //                     emptyBuffer = false;
         //               }
         //            }
         //            if(emptyBuffer)
         //            {
         //               KSelectable selectable = Grid.Objects[cell, (int)Utils.ConduitTypeToObjectLayer(__instance.conduitType)]?.GetComponent<KSelectable>();
         //               if(selectable != null)
         //               {
         //                  selectable.RemoveStatusItem(Main.bufferContentsSI, true);
         //                  Debug.Log("Removed buffer SI");
         //               }

         //               lock(Main.lockBuffersHashSet)
         //               {
         //                  Utils.ConduitTypeToBuffersSet(bufferStorageCmp.conduitType).Remove(bufferStorageCmp.conduitCell);
         //               }
         //               UnityEngine.Object.Destroy(bufferStorageCmp);
         //            }
         //         }
         //      }
         //      //--------------Discarding buffer storage 2--------------UP
         //   }
         //}



         //private static void TryUnswapBufferStorage2(CrossingCmp fakeCrossingCmp, BufferStorageCmp bufferStorageCmp, ref bool bufferStorageIsDefined, ConduitFlow conduitFlow) {
         //   if(Utils.IsCrossingRegistered(fakeCrossingCmp))
         //   {
         //      if(fakeCrossingCmp.swappedBufferStorage)
         //      {
         //         Debug.Log("Unswapping buffer");
         //         ConduitContents tempContents = GetContentsDirectly(fakeCrossingCmp.crossingCell, conduitFlow);
         //         bufferStorageCmp = Utils.CreateOrGetBufferStorage(fakeCrossingCmp);// just for safety
         //         bufferStorageIsDefined = true;
         //         SetContentsDirectly(fakeCrossingCmp.crossingCell, bufferStorageCmp.bufferStorage[0], conduitFlow);
         //         Debug.Log($"contents: {bufferStorageCmp.bufferStorage[0].element}, {bufferStorageCmp.bufferStorage[0].mass}");
         //         if(bufferStorageCmp.bufferStorage[1].element != SimHashes.Vacuum)
         //         {
         //            ConduitContents secondBuffer = bufferStorageCmp.bufferStorage[1];
         //            MoveConduitContents(ref secondBuffer, ref tempContents, secondBuffer.mass, conduitFlow, out _);
         //            Utils.StoreBufferContents(bufferStorageCmp, secondBuffer, 1);
         //         }
         //         Utils.StoreBufferContents(bufferStorageCmp, tempContents, 0);
         //         Debug.Log($"bufferContents: {tempContents.element}, {tempContents.mass}");
         //         fakeCrossingCmp.swappedBufferStorage = false;
         //      }
         //   }
         //}

         //private static void GetCustomPriorities2(ConduitFlow conduitFlow, CrossingCmp fakeCrossingCmp, out List<FlowDirections> customInPriorities, out bool customOutPriorities) {
         //   customInPriorities = new List<FlowDirections>(0);
         //   customOutPriorities = false;
         //   Debug.Log("Managing custom flow priorities of " + fakeCrossingCmp.crossingCell);

         //   foreach(FlowDirections direction in Main.allFlowDirections)
         //   {
         //      if(Utils.IsBitSet((byte)conduitFlow.soaInfo.GetPermittedFlowDirections(conduitFlow.GetConduit(fakeCrossingCmp.crossingCell).idx), (byte)direction) &&
         //         Utils.TryGetCrossingCmp(ConduitFlow.GetCellFromDirection(fakeCrossingCmp.crossingCell, direction), conduitFlow.conduitType, out CrossingCmp tempCmp) &&
         //         tempCmp.shouldManageInPriorities)
         //      {
         //         // if there is a crossing with custom input priorities in this direction
         //         customInPriorities.Add(direction);
         //      }
         //   }
         //   if(Utils.IsCrossingRegistered(fakeCrossingCmp) &&
         //      fakeCrossingCmp.shouldManageOutPriorities)
         //   {
         //      customOutPriorities = true;
         //   }
         //   Debug.Log("customOutPriorities: " + customOutPriorities);
         //}
      }

      //[HarmonyPatch(typeof(GeneratedBuildings), "LoadGeneratedBuildings")]
      //public static class IgnoreFullPipe_Patch {
      //   public static void Postfix(List<System.Type> types) {
      //      foreach(BuildingDef buildingDef in Assets.BuildingDefs)
      //      {
      //         if(buildingDef.BuildingComplete.TryGetComponent(out RequireOutputs component))
      //         {
      //            component.ignoreFullPipe = true;// needed to change outputs' flow behavior
      //         }
      //      }
      //   }
      //}

      [HarmonyPatch(typeof(UpdateNetworkTask), "Finish")]
      public static class OnNetworkUpdateFinish_Patch {
         public static void Postfix(ConduitFlow conduit_flow, UpdateNetworkTask __instance) {
            if(!Utils.ConduitTypeToNetworksCrossings(conduit_flow.conduitType).ContainsKey(__instance.network.network.id))
               return;

            foreach(int crossing_cell in Utils.ConduitTypeToNetworksCrossings(conduit_flow.conduitType)[__instance.network.network.id])
            {
               CrossingCmp crossingCmp = Utils.GetCrossingCmp(crossing_cell, conduit_flow.conduitType);
               if(Utils.ConduitRequiresBuffer(crossingCmp) && Utils.TryGetBufferStorageCmp(crossingCmp, out BufferStorageCmp bufferStorageCmp))
               {
                  //-------------------Swapping ConduitContents-BufferStorage-------------------DOWN
                  // swapping ConduitContents-BufferStorage(for unswapping process see the beginning of OnUpdateConduit_Patch)
                  ConduitContents tempContents = GetContentsDirectly(crossing_cell, conduit_flow);
                  if(Utils.TryGetRealEndpointType(crossingCmp, out Endpoint endpoint_type))
                  {
                     if(endpoint_type == Endpoint.Sink)
                     {
                        SetContentsDirectly(crossing_cell, bufferStorageCmp.bufferStorage[0], conduit_flow);
                        Utils.StoreBufferContents(bufferStorageCmp, tempContents, 0);
                        crossingCmp.swappedBufferStorage = true;
                     }
                     else
                     {
                        if(bufferStorageCmp.bufferStorage[0].GetEffectiveCapacity(bufferStorageCmp.bufferMaxMass) > 0f)
                        {
                           SetContentsDirectly(crossing_cell, ConduitContents.Empty, conduit_flow);
                           Utils.StoreBufferContents(bufferStorageCmp, bufferStorageCmp.bufferStorage[0], 1);
                           Utils.StoreBufferContents(bufferStorageCmp, tempContents, 0);
                           crossingCmp.swappedBufferStorage = true;
                        }
                        else
                        {
                           SetContentsDirectly(crossing_cell, bufferStorageCmp.bufferStorage[0], conduit_flow);
                           Utils.StoreBufferContents(bufferStorageCmp, tempContents, 0);
                           crossingCmp.swappedBufferStorage = true;
                        }
                     }
                  }
                  //-------------------Swapping ConduitContents-BufferStorage-------------------UP
                  //-----------------------Adding status items-----------------------DOWN
                  // have to add status items here because doing it while the network is updating crashes the game
                  KSelectable selectable = Grid.Objects[crossing_cell, (int)Utils.ConduitTypeToObjectLayer(conduit_flow.conduitType)]?.GetComponent<KSelectable>();
                  if(selectable != null)
                  {
                     selectable.AddStatusItem(Main.bufferContentsSI, bufferStorageCmp);
                  }
                  //-----------------------Adding status items-----------------------UP
               }
               //-------------------Cleaning flow management dictionaries-------------------DOWN
               Utils.ClearInFlowDictionary(crossingCmp);
               Utils.ClearOutFlowDictionary(crossingCmp);
               //-------------------Cleaning flow management dictionaries-------------------UP
            }
         }
      }


      public static void ForceDiscardBuffer(int conduit_cell, BufferStorageCmp bufferStorageCmp) {
         foreach(ConduitContents bufferContents in bufferStorageCmp.bufferStorage)
         {
            if(bufferContents.element == SimHashes.Vacuum || bufferContents.mass <= 0.0)
               continue;

            SimMessages.AddRemoveSubstance(conduit_cell, bufferContents.element, CellEventLogger.Instance.ConduitFlowEmptyConduit, bufferContents.mass, bufferContents.temperature,
               bufferContents.diseaseIdx, bufferContents.diseaseCount);
         }

         lock(Utils.ConduitTypeToBuffersLock(bufferStorageCmp.conduitType))
         {
            Utils.ConduitTypeToBuffersSet(bufferStorageCmp.conduitType).Remove(bufferStorageCmp.conduitCell);
         }
      }

      public static void MoveConduitContents(ref ConduitContents from, ref ConduitContents to, float massToMove, float to_MaxMass, out float moved_mass) {
         int num1 = to.element == SimHashes.Vacuum ? 1 : (to.element == from.element ? 1 : 0);
         float availableMass = from.element == SimHashes.Vacuum ? 0f : from.movable_mass;
         bool canMoveMass = num1 != 0;
         moved_mass = Mathf.Min(availableMass, Mathf.Min(massToMove, to.GetEffectiveCapacity(to_MaxMass)));

         if(canMoveMass && moved_mass > 0f)
         {
            Debug.Assert((double)from.temperature > 0.0);
            to.temperature = GameUtil.GetFinalTemperature(from.temperature, moved_mass, to.temperature, to.mass);
            to.added_mass += moved_mass;
            to.element = from.element;
            int src1_count = (int)((double)moved_mass / (double)from.mass * (double)from.diseaseCount);
            if(src1_count != 0)
            {
               SimUtil.DiseaseInfo finalDiseaseInfo = SimUtil.CalculateFinalDiseaseInfo(from.diseaseIdx, src1_count, to.diseaseIdx, to.diseaseCount);
               to.diseaseIdx = finalDiseaseInfo.idx;
               to.diseaseCount = finalDiseaseInfo.count;
            }
            Debug.Assert((double)moved_mass <= (double)from.mass);
            float amount = from.mass - moved_mass;
            float num3 = availableMass - moved_mass;
            if((double)amount <= 0.0)
            {
               Debug.Assert((double)num3 <= 0.0);
               from = ConduitContents.Empty;
            }
            else
            {
               int num4 = (int)((double)amount / (double)from.mass * (double)from.diseaseCount);
               Debug.Assert(num4 >= 0);
               from.removed_mass += moved_mass;
               from.diseaseCount = num4;
               if(num4 == 0)
                  from.diseaseIdx = byte.MaxValue;
            }
         }
         else
            moved_mass = 0f;
      }
      public static void MoveConduitContents(ref ConduitContents from, ref ConduitContents to, SimHashes to_element, float massToMove, float MaxMass, out float moved_mass) {
         int num1 = to_element == SimHashes.Vacuum ? (to.element == SimHashes.Vacuum ? 1 : (to.element == from.element ? 1 : 0)) : (to_element == from.element ? 1 : 0);
         float availableMass = from.element == SimHashes.Vacuum ? 0f : from.movable_mass;
         bool canMoveMass = num1 != 0;
         moved_mass = Mathf.Min(availableMass, Mathf.Min(massToMove, to.GetEffectiveCapacity(MaxMass)));

         if(canMoveMass && moved_mass > 0f)
         {
            Debug.Assert((double)from.temperature > 0.0);
            to.temperature = GameUtil.GetFinalTemperature(from.temperature, moved_mass, to.temperature, to.mass);
            to.added_mass += moved_mass;
            to.element = from.element;
            int src1_count = (int)((double)moved_mass / (double)from.mass * (double)from.diseaseCount);
            if(src1_count != 0)
            {
               SimUtil.DiseaseInfo finalDiseaseInfo = SimUtil.CalculateFinalDiseaseInfo(from.diseaseIdx, src1_count, to.diseaseIdx, to.diseaseCount);
               to.diseaseIdx = finalDiseaseInfo.idx;
               to.diseaseCount = finalDiseaseInfo.count;
            }
            Debug.Assert((double)moved_mass <= (double)from.mass);
            float amount = from.mass - moved_mass;
            float num3 = availableMass - moved_mass;
            if((double)amount <= 0.0)
            {
               Debug.Assert((double)num3 <= 0.0);
               from = ConduitContents.Empty;
            }
            else
            {
               int num4 = (int)((double)amount / (double)from.mass * (double)from.diseaseCount);
               Debug.Assert(num4 >= 0);
               from.removed_mass += moved_mass;
               from.diseaseCount = num4;
               if(num4 == 0)
                  from.diseaseIdx = byte.MaxValue;
            }
         }
         else
            moved_mass = 0f;
      }

      private static void SetContentsDirectly(int cell, ConduitContents newContents, ConduitFlow conduitFlow) {
         if(cell == -1)
            return;
         conduitFlow.grid[cell].contents = newContents;
      }
      private static ConduitContents GetContentsDirectly(int cell, ConduitFlow conduitFlow) {
         if(cell == -1)
            return ConduitContents.Empty;
         return conduitFlow.grid[cell].contents;
      }

      private static float GetMovableMass(GridNode grid_node, bool customOutPriorities, ConduitFlow conduitFlow) {
         if(!customOutPriorities)
            return conduitFlow.ComputeMovableMass(grid_node);

         ConduitContents contents = grid_node.contents;
         if(contents.element == SimHashes.Vacuum)
            return 0f;

         return contents.movable_mass;
      }

      private static float ComputeInputsFlowDistribution(int crossing_cell, FlowDirections direction_inwards, float offeredMass, ConduitFlow conduitFlow,
         float MaxMass, ref BufferStorageCmp bufferStorageCmp_outer, ref bool bufferStorageIsDefined_outer, out bool flowOccured) {
         flowOccured = false;

         float massToFill = conduitFlow.grid[crossing_cell].contents.GetEffectiveCapacity(MaxMass);
         if(massToFill == 0f)
            return 0f;

         ConduitType conduit_type = conduitFlow.conduitType;
         CrossingCmp crossingCmp = Utils.GetCrossingCmp(crossing_cell, conduit_type);
         bool inFlowManagementIsDefined = crossingCmp.inputsFlowManagement != default;

         sbyte direction_outwards = Utils.OppositeDirection((sbyte)Utils.CountTrailingZeros((byte)direction_inwards));
         var flowPriorityCluster = Utils.GetFlowPriorityCluster(crossingCmp, direction_outwards);
         if(flowPriorityCluster == default)
            return 0f;

         byte occuredFlowsCount = inFlowManagementIsDefined ? crossingCmp.inputsFlowManagement.Item3 : (byte)0;

         // preventing flow-occuring if there are non-updated directions with higher flow priority:
         int higherPriorityDirsCount = 0;
         char flowDirection = Utils.GetFlowDirection(crossingCmp, direction_outwards);
         for(sbyte direction = 0; direction < 4; direction++)
         {
            if(Utils.GetFlowDirection(crossingCmp, direction) == flowDirection && Utils.GetFlowPriority(crossingCmp, direction) > flowPriorityCluster.Item2)
               higherPriorityDirsCount++;
         }
         if(higherPriorityDirsCount > occuredFlowsCount)
            return 0f;

         bool endpointFlowOccured = inFlowManagementIsDefined && crossingCmp.inputsFlowManagement.Item4;
         sbyte endpointFlowPriority = -1;
         if(Utils.TryGetRealEndpointType(crossingCmp, out Endpoint endpoint_type) && endpoint_type == Endpoint.Source)
         {
            endpointFlowPriority = Utils.GetFlowPriority(crossingCmp, 4);
            if(!endpointFlowOccured && endpointFlowPriority > flowPriorityCluster.Item2)// sourceFlowPriority > thisFlowPriority
            {
               MoveSourceFlowPortion(ref bufferStorageCmp_outer ,ref bufferStorageIsDefined_outer);

               if(massToFill <= 0f)
                  return 0f;
            }
         }

         float distributedMass;

         if(flowPriorityCluster.Item1.Count == 1)
         {
            // simplified flow(without flowOrder management):
            distributedMass = ComputeFlowPortion(ref flowOccured, ref bufferStorageCmp_outer, ref bufferStorageIsDefined_outer);
         }
         else
         {
            int flowPriority = flowPriorityCluster.Item2;

            byte[] flowOrder = null;
            byte[] local_flowOrder = null;
            if(inFlowManagementIsDefined)
            {
               flowOrder = crossingCmp.inputsFlowManagement.Item1;
               local_flowOrder = crossingCmp.inputsFlowManagement.Item2;
            }
            if(flowOrder == null)
            {
               flowOrder = new byte[4];// <--four different flow priorities, priority used as index
            }
            bool firstUpdate = false;
            if(local_flowOrder == null)
            {
               firstUpdate = true;

               local_flowOrder = new byte[4];// <--four different flow priorities, priority used as index
            }

            if(firstUpdate)
            {
               flowOrder[flowPriority] %= (byte)flowPriorityCluster.Item1.Count;
               local_flowOrder[flowPriority] = flowOrder[flowPriority];
               flowOrder[flowPriority]++;
            }
            else
            {
               if(local_flowOrder[flowPriority] >= flowPriorityCluster.Item1.Count)
                  local_flowOrder[flowPriority] = 0;
            }

            distributedMass = ComputeFlowPortion2(flowOrder, local_flowOrder, flowPriority, ref flowOccured, ref bufferStorageCmp_outer,
               ref bufferStorageIsDefined_outer);

            Utils.StoreInFlowMngmnt(crossingCmp, flowOrder, false);
            Utils.StoreInInFlowMngmnt(crossingCmp, local_flowOrder);
         }

         Utils.StoreInInFlowMngmnt(crossingCmp, occuredFlowsCount);
         Utils.StoreInInFlowMngmnt(crossingCmp, endpointFlowOccured);
         return distributedMass;



         float ComputeFlowPortion(ref bool flowOccured_inner, ref BufferStorageCmp bufferStorageCmp, ref bool bufferStorageIsDefined) {
            int source_cell = ConduitFlow.GetCellFromDirection(crossing_cell, ConduitFlow.Opposite(direction_inwards));
            GridNode grid_node = conduitFlow.grid[source_cell];

            ConduitContents otherContents = conduitFlow.grid[crossing_cell].contents;
            int num1 = otherContents.element == SimHashes.Vacuum ? 1 : (otherContents.element == grid_node.contents.element ? 1 : 0);
            bool canMoveMass = num1 != 0;
            float moved_mass = Mathf.Min(massToFill, offeredMass);

            float result = 0f;

            if(canMoveMass && moved_mass > 0f)
            {
               massToFill -= moved_mass;
               result = moved_mass;
            }

            occuredFlowsCount++;// indicates that this direction was updated(no matter whether any mass actually flowed)
            flowOccured_inner = true;

            if(massToFill > 0f && endpointFlowPriority != -1 && !endpointFlowOccured && endpointFlowPriority < flowPriorityCluster.Item2)// sourceFlowPriority < thisFlowPriority
            {
               if(!AreThereLowerPriorityClusters())
               {
                  MoveSourceFlowPortion(ref bufferStorageCmp, ref bufferStorageIsDefined);
               }
            }

            return result;
         }
         float ComputeFlowPortion2(byte[] flowOrder, byte[] local_flowOrder, int flowPriority, ref bool flowOccured_inner,
            ref BufferStorageCmp bufferStorageCmp, ref bool bufferStorageIsDefined) {
            if(flowPriorityCluster.Item1[local_flowOrder[flowPriority]] == 4)// sourceFlowPriority == thisFlowPriority
            {
               MoveSourceFlowPortion2(flowOrder, local_flowOrder, flowPriority, SimHashes.Vacuum/*vacuum = use the real element*/, ref bufferStorageCmp,
                  ref bufferStorageIsDefined);
            }

            float result = 0f;

            if(massToFill > 0f && flowPriorityCluster.Item1[local_flowOrder[flowPriority]] == direction_outwards)// if isThisDirection'sTurnToFlow
            {
               local_flowOrder[flowPriority]++;
               local_flowOrder[flowPriority] %= (byte)flowPriorityCluster.Item1.Count;
               int source_cell = ConduitFlow.GetCellFromDirection(crossing_cell, (FlowDirections)(1 << direction_outwards));
               GridNode grid_node = conduitFlow.grid[source_cell];

               ConduitContents otherContents = conduitFlow.grid[crossing_cell].contents;
               int num1 = otherContents.element == SimHashes.Vacuum ? 1 : (otherContents.element == grid_node.contents.element ? 1 : 0);
               bool canMoveMass = num1 != 0;
               float moved_mass = Mathf.Min(massToFill, offeredMass);

               if(canMoveMass && moved_mass > 0f)
               {
                  massToFill -= moved_mass;

                  result = moved_mass;
               }
               else if(canMoveMass)
               {
                  flowOrder[flowPriority]++;// couldn't move mass because source pipe is empty
               }

               occuredFlowsCount++;// indicates that this direction was updated(no matter whether any mass actually flowed)
               flowOccured_inner = true;

               if(massToFill > 0f)
               {
                  if(flowPriorityCluster.Item1[local_flowOrder[flowPriority]] == 4)// sourceFlowPriority == thisFlowPriority
                  {
                     MoveSourceFlowPortion2(flowOrder, local_flowOrder, flowPriority,
                        canMoveMass && moved_mass > 0f ? grid_node.contents.element : SimHashes.Vacuum, ref bufferStorageCmp, ref bufferStorageIsDefined);
                  }
                  else if(endpointFlowPriority != -1 && !endpointFlowOccured && endpointFlowPriority < flowPriorityCluster.Item2)// sourceFlowPriority < thisFlowPriority
                  {
                     if(flowPriorityCluster.Item1.Count - occuredFlowsCount <= 0 && !AreThereLowerPriorityClusters())
                     {
                        // if this direction is the last one checked in this priority cluster AND there are no clusters with lower flow priority:
                        MoveSourceFlowPortion(ref bufferStorageCmp, ref bufferStorageIsDefined);
                     }
                  }
               }
            }

            return result;
         }

         void MoveSourceFlowPortion(ref BufferStorageCmp bufferStorageCmp, ref bool bufferStorageIsDefined) {
            if(massToFill == 0f)
               return;

            if(!bufferStorageIsDefined)
            {
               bufferStorageCmp = Utils.CreateOrGetBufferStorage(crossingCmp, MaxMass);
               bufferStorageIsDefined = true;
            }

            ConduitContents sourceContents = bufferStorageCmp.bufferStorage[0];
            ConduitContents otherContents = conduitFlow.grid[crossing_cell].contents;
            MoveConduitContents(ref sourceContents, ref otherContents, massToFill, MaxMass, out float moved_mass);
            conduitFlow.grid[crossing_cell].contents = otherContents;
            Utils.StoreBufferContents(bufferStorageCmp, sourceContents, 0);

            massToFill -= moved_mass;
            endpointFlowOccured = true;

            occuredFlowsCount++;
         }
         void MoveSourceFlowPortion2(byte[] flowOrder, byte[] local_flowOrder, int flowPriority, SimHashes custom_element, ref BufferStorageCmp bufferStorageCmp,
            ref bool bufferStorageIsDefined) {
            local_flowOrder[flowPriority]++;
            local_flowOrder[flowPriority] %= (byte)flowPriorityCluster.Item1.Count;

            if(!bufferStorageIsDefined)
            {
               bufferStorageCmp = Utils.CreateOrGetBufferStorage(crossingCmp, MaxMass);
               bufferStorageIsDefined = true;
            }

            ConduitContents sourceContents = bufferStorageCmp.bufferStorage[0];
            ConduitContents otherContents = conduitFlow.grid[crossing_cell].contents;
            MoveConduitContents(ref sourceContents, ref otherContents, custom_element, massToFill, MaxMass, out float moved_mass);
            conduitFlow.grid[crossing_cell].contents = otherContents;
            Utils.StoreBufferContents(bufferStorageCmp, sourceContents, 0);

            if(moved_mass <= 0f && sourceContents.element == (custom_element == SimHashes.Vacuum ? otherContents.element : custom_element))
            {
               flowOrder[flowPriority]++;// couldn't move mass because source pipe is empty
            }

            massToFill -= moved_mass;
            endpointFlowOccured = true;

            occuredFlowsCount++;
         }

         bool AreThereLowerPriorityClusters() {
            var flowPriorities = crossingCmp.flowPriorities;
            foreach(var cluster in flowPriorities)
            {
               if(Utils.GetFlowDirection(crossingCmp, cluster.Item1[0]) == '1' && cluster.Item2 < flowPriorityCluster.Item2 &&
                  (cluster.Item1.Count > 1 || cluster.Item1[0] != 4))
                  return true;
            }
            return false;
         }
      }

      private static FlowDirections ComputeOutputsFlowDistribution(CrossingCmp crossingCmp, ref float movableMass, ref GridNode grid_node, ConduitFlow conduitFlow,
         float MaxMass) {
         if(movableMass == 0f)
            return FlowDirections.None;

         float availableMass = movableMass;
         bool outFlowManagementIsDefined = crossingCmp.outputsFlowManagement != default;

         var flowPriorities = crossingCmp.flowPriorities;
         var outputPriorities = new List<(List<sbyte>, sbyte)>(flowPriorities.Count);
         foreach(var cluster in flowPriorities)
         {
            if(Utils.GetFlowDirection(crossingCmp, cluster.Item1[0]) == '2')
               outputPriorities.Add(cluster);
         }

         byte checkedDirectionsCount = outFlowManagementIsDefined ? crossingCmp.outputsFlowManagement.Item2 : (byte)0;

         byte[] flowOrder;
         if(outFlowManagementIsDefined)
         {
            flowOrder = crossingCmp.outputsFlowManagement.Item1;
            if(flowOrder == default || flowOrder.Length != outputPriorities.Count)
            {
               flowOrder = new byte[outputPriorities.Count];
            }
         }
         else
         {
            flowOrder = new byte[outputPriorities.Count];
         }

         FlowDirections resultDirection = FlowDirections.None;
         bool checkedEndpointDirection = false;
         for(int i = 0; i < 2; i++)
         {
            if(availableMass == 0f)
               break;

            int index = outputPriorities.Count - 1;
            int num = outputPriorities[index].Item1.Count;
            while(index > -1 && checkedDirectionsCount >= num)
            {
               index--;
               if(index > -1)
                  num += outputPriorities[index].Item1.Count;
            }
            if(index < 0)
               break;

            if(flowOrder[index] >= outputPriorities[index].Item1.Count)
               flowOrder[index] = 0;

            sbyte direction_outwards = outputPriorities[index].Item1[flowOrder[index]];
            if(direction_outwards == 4)// if isSink
            {
               checkedEndpointDirection = true;

               checkedDirectionsCount++;
               flowOrder[index]++;

               BufferStorageCmp bufferStorageCmp = Utils.CreateOrGetBufferStorage(crossingCmp, MaxMass);
               ConduitContents theseContents = grid_node.contents;
               ConduitContents bufferContents = bufferStorageCmp.bufferStorage[0];
               MoveConduitContents(ref theseContents, ref bufferContents, availableMass, MaxMass, out float moved_mass);
               grid_node.contents = theseContents;
               conduitFlow.grid[crossingCmp.crossingCell].contents = theseContents;
               Utils.StoreBufferContents(bufferStorageCmp, bufferContents, 0);

               availableMass -= moved_mass;
            }
            else if(i == 0 || checkedEndpointDirection)
            {
               checkedDirectionsCount++;
               flowOrder[index]++;

               ConduitContents otherContents = conduitFlow.grid[ConduitFlow.GetCellFromDirection(crossingCmp.crossingCell, (FlowDirections)(1 << direction_outwards))].contents;
               int num1 = otherContents.element == SimHashes.Vacuum ? 1 : (otherContents.element == grid_node.contents.element ? 1 : 0);
               float effectiveCapacity = otherContents.GetEffectiveCapacity(MaxMass);
               bool canMoveMass = num1 != 0 && (double)effectiveCapacity > 0.0;
               float moved_mass = Mathf.Min(availableMass, effectiveCapacity);

               if(canMoveMass && moved_mass > 0f)
               {
                  resultDirection = (FlowDirections)(1 << direction_outwards);
                  movableMass = moved_mass;

                  availableMass -= moved_mass;
               }
               else
               {
                  i--;// allowing one more direction check
               }
            }
            else
               break;
         }

         Utils.StoreInFlowMngmnt(crossingCmp, flowOrder, true);
         Utils.StoreInOutFlowMngmnt(crossingCmp, checkedDirectionsCount);
         return resultDirection;
      }
   }
}
