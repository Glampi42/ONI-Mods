using AdvancedFlowManagement;
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
   class Main {
      public const string debugPrefix = "[AdvancedFlowManagement] > ";
      public static readonly ConduitType[] allConduitTypes = { ConduitType.Liquid, ConduitType.Gas };
      public static readonly FlowDirections[] allFlowDirections = { FlowDirections.Down, FlowDirections.Left, FlowDirections.Up, FlowDirections.Right };

      public static readonly object lockBuffersHashSet = new object();
      public static readonly object lockCrossingsHashSet = new object();

      public static Dictionary<string, (string, byte)>/*rotatedID, (unrotatedID, rotation)*/ allCrossingIDs = new Dictionary<string, (string, byte)>();
      public static StatusItem bufferContentsSI;

      public static HashSet<int> crossings_liquid = new HashSet<int>();
      public static HashSet<int> crossings_gas = new HashSet<int>();

      public static HashSet<int> buffers_liquid = new HashSet<int>();
      public static HashSet<int> buffers_gas = new HashSet<int>();

      public static HashSet<int> customFlowConduits_liquid = new HashSet<int>();
      public static HashSet<int> customFlowConduits_gas = new HashSet<int>();

      public static Dictionary<int, List<int>>/*network_id, List<crossing_cell>*/ crossingsNetworks_liquid = new Dictionary<int, List<int>>();
      public static Dictionary<int, List<int>>/*network_id, List<crossing_cell>*/ crossingsNetworks_gas = new Dictionary<int, List<int>>();

      public static bool showCrossings_Liquid = true;
      public static bool showCrossings_Gas = true;

      public static bool firstUpdate_Liquid = true;
      public static bool firstUpdate_Gas = true;


      public static void DeserializeAll() {
         //------------------------Re-instantiating serialized components------------------------DOWN
         foreach(CrossingCmp crossingCmp in SaveModState.Instance.savedCrossingCmps_liquid)
         {
            if(Utils.TryGetConduitGO(crossingCmp.crossingCell, crossingCmp.conduitType, out GameObject conduitGO))
            {
               CrossingCmp newCrossingCmp = conduitGO.AddComponent<CrossingCmp>();
               newCrossingCmp.CloneSerializationFields(crossingCmp);

               Main.crossings_liquid.Add(newCrossingCmp.crossingCell);
            }
         }
         foreach(CrossingCmp crossingCmp in SaveModState.Instance.savedCrossingCmps_gas)
         {
            if(Utils.TryGetConduitGO(crossingCmp.crossingCell, crossingCmp.conduitType, out GameObject conduitGO))
            {
               CrossingCmp newCrossingCmp = conduitGO.AddComponent<CrossingCmp>();
               newCrossingCmp.CloneSerializationFields(crossingCmp);

               Main.crossings_gas.Add(newCrossingCmp.crossingCell);
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
         foreach(BufferStorageCmp bufferStorageCmp in SaveModState.Instance.savedBufferStorageCmps_gas)
         {
            if(Utils.TryGetConduitGO(bufferStorageCmp.conduitCell, bufferStorageCmp.conduitType, out GameObject conduitGO))
            {
               BufferStorageCmp newBufferStorageCmp = conduitGO.AddComponent<BufferStorageCmp>();
               newBufferStorageCmp.CloneSerializationFields(bufferStorageCmp);

               Main.buffers_gas.Add(newBufferStorageCmp.conduitCell);
            }
         }

         SaveModState.Instance.savedCrossingCmps_liquid.Clear();
         SaveModState.Instance.savedCrossingCmps_gas.Clear();
         SaveModState.Instance.savedBufferStorageCmps_liquid.Clear();
         SaveModState.Instance.savedBufferStorageCmps_gas.Clear();
         //------------------------Re-instantiating serialized components------------------------UP
      }
   }
}
