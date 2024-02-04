using KSerialization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using static AdvancedFlowManagement.CrossingCmp;
using static ConduitFlow;

namespace AdvancedFlowManagement {
   [SerializationConfig(MemberSerialization.OptIn)]
   public class BufferStorageCmp : KMonoBehaviour {
      [Serialize]
      public int conduitCell;
      [Serialize]
      public ConduitType conduitType;

      [Serialize]
      public float bufferMaxMass;

      public ConduitContents[] bufferStorage;
      [Serialize]
      private AllPublicConduitContents[] serialized_bufferStorage;


      [OnSerializing]
      private void SerializeBufferStorages() {
         serialized_bufferStorage = ConvertToSerializable(bufferStorage);
      }
      [OnDeserialized]
      private void DeserializeBufferStorages() {
         bufferStorage = ConvertFromSerializable(serialized_bufferStorage);
         serialized_bufferStorage = null;
      }

      public BufferStorageCmp() { }

      public BufferStorageCmp CloneSerializationFields(BufferStorageCmp otherCmp) {
         conduitCell = otherCmp.conduitCell;
         conduitType = otherCmp.conduitType;
         bufferMaxMass = otherCmp.bufferMaxMass;
         bufferStorage = (ConduitContents[])otherCmp.bufferStorage.Clone();
         return this;
      }


      public struct AllPublicConduitContents {
         public SimHashes element;
         public float initial_mass;
         public float added_mass;
         public float removed_mass;
         public float temperature;
         public byte diseaseIdx;
         public int diseaseCount;

         public static implicit operator ConduitContents(AllPublicConduitContents contents) => new ConduitContents {
            added_mass = contents.added_mass,
            diseaseCount = contents.diseaseCount,
            diseaseIdx = contents.diseaseIdx,
            temperature = contents.temperature,
            element = contents.element,
            initial_mass = contents.initial_mass,
            removed_mass = contents.removed_mass
         };
         public static implicit operator AllPublicConduitContents(ConduitContents contents) => new AllPublicConduitContents {
            added_mass = contents.added_mass,
            diseaseCount = contents.diseaseCount,
            diseaseIdx = contents.diseaseIdx,
            temperature = contents.temperature,
            element = contents.element,
            initial_mass = contents.initial_mass,
            removed_mass = contents.removed_mass
         };
      }

      public static AllPublicConduitContents[] ConvertToSerializable(ConduitContents[] sourceBuffer) {
         AllPublicConduitContents[] array = new AllPublicConduitContents[sourceBuffer.Length];
         for(int i = 0; i < array.Length; i++)
            array[i] = sourceBuffer[i];
         return array;
      }
      public static ConduitContents[] ConvertFromSerializable(AllPublicConduitContents[] sourceBuffer) {
         ConduitContents[] array = new ConduitContents[sourceBuffer.Length];
         for(int i = 0; i < array.Length; i++)
            array[i] = sourceBuffer[i];
         return array;
      }
   }
}
