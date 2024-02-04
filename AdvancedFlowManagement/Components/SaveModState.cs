using KSerialization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using UnityEngine;
using static ConduitFlow;

namespace AdvancedFlowManagement {
   [SerializationConfig(MemberSerialization.OptIn)]
   public class SaveModState : KMonoBehaviour {
      [Serialize]
      public List<CrossingCmp> savedCrossingCmps_liquid = new List<CrossingCmp>();
      [Serialize]
      public List<CrossingCmp> savedCrossingCmps_gas = new List<CrossingCmp>();

      [Serialize]
      public List<BufferStorageCmp> savedBufferStorageCmps_liquid = new List<BufferStorageCmp>();
      [Serialize]
      public List<BufferStorageCmp> savedBufferStorageCmps_gas = new List<BufferStorageCmp>();

      [OnSerializing]
      private void OnSerialize() {
         savedCrossingCmps_liquid.Clear();
         savedCrossingCmps_gas.Clear();
         savedBufferStorageCmps_liquid.Clear();
         savedBufferStorageCmps_gas.Clear();

         foreach(int crossing_cell in Utils.ConduitTypeToCrossingsSet(ConduitType.Liquid))
         {
            savedCrossingCmps_liquid.Add(new CrossingCmp().CloneSerializationFields(Utils.GetCrossingCmp(crossing_cell, ConduitType.Liquid)));
         }
         foreach(int crossing_cell in Utils.ConduitTypeToCrossingsSet(ConduitType.Gas))
         {
            savedCrossingCmps_gas.Add(new CrossingCmp().CloneSerializationFields(Utils.GetCrossingCmp(crossing_cell, ConduitType.Gas)));
         }

         foreach(int buffer_cell in Utils.ConduitTypeToBuffersSet(ConduitType.Liquid))
         {
            BufferStorageCmp bufferStorageCmp = Utils.GetBufferStorageCmp(buffer_cell, ConduitType.Liquid);
            if(bufferStorageCmp == null)
               continue;

            bool emptyBuffer = true;
            foreach(ConduitContents bufferContents in bufferStorageCmp.bufferStorage)
            {
               if(bufferContents.element != SimHashes.Vacuum)
               {
                  emptyBuffer = false;
                  break;
               }
            }
            if(emptyBuffer)
               continue;// no need to serialize an empty buffer

            savedBufferStorageCmps_liquid.Add(new BufferStorageCmp().CloneSerializationFields(bufferStorageCmp));
         }
         foreach(int buffer_cell in Utils.ConduitTypeToBuffersSet(ConduitType.Gas))
         {
            BufferStorageCmp bufferStorageCmp = Utils.GetBufferStorageCmp(buffer_cell, ConduitType.Gas);
            if(bufferStorageCmp == null)
               continue;

            bool emptyBuffer = true;
            foreach(ConduitContents bufferContents in bufferStorageCmp.bufferStorage)
            {
               if(bufferContents.element != SimHashes.Vacuum)
               {
                  emptyBuffer = false;
                  break;
               }
            }
            if(emptyBuffer)
               continue;// no need to serialize an empty buffer

            savedBufferStorageCmps_gas.Add(new BufferStorageCmp().CloneSerializationFields(bufferStorageCmp));
         }
      }


      public static SaveModState Instance { get; private set; }

      public SaveModState() {
         Instance = this;
      }
   }
}
