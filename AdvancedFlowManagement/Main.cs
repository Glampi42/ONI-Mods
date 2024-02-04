using AdvancedFlowManagement;
using HarmonyLib;
using KMod;
using PeterHan.PLib.Core;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;
using static ConduitFlow;

namespace AdvancedFlowManagement {
   class Main : UserMod2 {
      public const string debugPrefix = "[AdvancedFlowManagement] > ";
      public static readonly ConduitType[] allConduitTypes = { ConduitType.Liquid, ConduitType.Gas };
      public static readonly FlowDirections[] allFlowDirections = { FlowDirections.Down, FlowDirections.Left, FlowDirections.Up, FlowDirections.Right };

      public static Dictionary<string, (string, byte)>/*rotatedID, (unrotatedID, rotation)*/ allCrossingIDs = new Dictionary<string, (string, byte)>();
      public static StatusItem bufferContentsSI;

      public static HashSet<int> crossings_liquid = new HashSet<int>();
      public static readonly object lockCrossings_liquid = new object();
      public static HashSet<int> crossings_gas = new HashSet<int>();
      public static readonly object lockCrossings_gas = new object();

      public static HashSet<int> buffers_liquid = new HashSet<int>();
      public static readonly object lockBuffers_liquid = new object();
      public static HashSet<int> buffers_gas = new HashSet<int>();
      public static readonly object lockBuffers_gas = new object();

      public static HashSet<int> customFlowConduits_liquid = new HashSet<int>();
      public static HashSet<int> customFlowConduits_gas = new HashSet<int>();

      public static Dictionary<int, Endpoint> endpoints_liquid = new Dictionary<int, Endpoint>();
      public static Dictionary<int, Endpoint> endpoints_gas = new Dictionary<int, Endpoint>();

      public static Dictionary<int, List<int>>/*network_id, List<crossing_cell>*/ crossingsNetworks_liquid = new Dictionary<int, List<int>>();
      public static Dictionary<int, List<int>>/*network_id, List<crossing_cell>*/ crossingsNetworks_gas = new Dictionary<int, List<int>>();

      public static bool showCrossings_Liquid = true;
      public static bool showCrossings_Gas = true;

      public static bool deserializedCmps_liquid = false;
      public static bool deserializedCmps_gas = false;


      public override void OnLoad(Harmony harmony) => base.OnLoad(harmony);


      public static void DeserializeCmpsIfNeeded(ConduitType conduit_type, out bool deserialized) {
         deserialized = false;
         //------------------------Re-instantiating serialized components------------------------DOWN
         if(conduit_type == ConduitType.Liquid)
         {
            if(deserializedCmps_liquid)
               return;

            foreach(CrossingCmp crossingCmp in SaveModState.Instance.savedCrossingCmps_liquid)
            {
               if(Utils.TryGetConduitGO(crossingCmp.crossingCell, crossingCmp.conduitType, out GameObject conduitGO))
               {
                  CrossingCmp newCrossingCmp = conduitGO.AddComponent<CrossingCmp>();
                  newCrossingCmp.CloneSerializationFields(crossingCmp);

                  Main.crossings_liquid.Add(newCrossingCmp.crossingCell);
               }
            }

            foreach(BufferStorageCmp bufferStorageCmp in SaveModState.Instance.savedBufferStorageCmps_liquid)
            {
               if(Utils.TryGetConduitGO(bufferStorageCmp.conduitCell, bufferStorageCmp.conduitType, out GameObject conduitGO))
               {
                  BufferStorageCmp newBufferStorageCmp = conduitGO.AddComponent<BufferStorageCmp>();
                  newBufferStorageCmp.CloneSerializationFields(bufferStorageCmp);

                  Main.buffers_liquid.Add(newBufferStorageCmp.conduitCell);
               }
            }

            SaveModState.Instance.savedCrossingCmps_liquid.Clear();
            SaveModState.Instance.savedBufferStorageCmps_liquid.Clear();

            deserializedCmps_liquid = true;
            deserialized = true;
         }
         else
         {
            if(deserializedCmps_gas)
               return;

            foreach(CrossingCmp crossingCmp in SaveModState.Instance.savedCrossingCmps_gas)
            {
               if(Utils.TryGetConduitGO(crossingCmp.crossingCell, crossingCmp.conduitType, out GameObject conduitGO))
               {
                  CrossingCmp newCrossingCmp = conduitGO.AddComponent<CrossingCmp>();
                  newCrossingCmp.CloneSerializationFields(crossingCmp);

                  Main.crossings_gas.Add(newCrossingCmp.crossingCell);
               }
            }

            foreach(BufferStorageCmp bufferStorageCmp in SaveModState.Instance.savedBufferStorageCmps_gas)
            {
               if(Utils.TryGetConduitGO(bufferStorageCmp.conduitCell, bufferStorageCmp.conduitType, out GameObject conduitGO))
               {
                  BufferStorageCmp newBufferStorageCmp = conduitGO.AddComponent<BufferStorageCmp>();
                  newBufferStorageCmp.CloneSerializationFields(bufferStorageCmp);

                  Main.buffers_gas.Add(newBufferStorageCmp.conduitCell);
               }
            }

            SaveModState.Instance.savedCrossingCmps_gas.Clear();
            SaveModState.Instance.savedBufferStorageCmps_gas.Clear();

            deserializedCmps_gas = true;
            deserialized = true;
         }
         //------------------------Re-instantiating serialized components------------------------UP
      }
   }
}
