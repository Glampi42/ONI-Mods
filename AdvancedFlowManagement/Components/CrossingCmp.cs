using KSerialization;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using static AdvancedFlowManagement.STRINGS.BUILDING.STATUSITEMS;
using static ElementConsumer;

namespace AdvancedFlowManagement {
   [SerializationConfig(MemberSerialization.OptIn)]
   public class CrossingCmp : KMonoBehaviour {
      //-------Serializable fields-------DOWN
      [Serialize]
      public int crossingCell;
      [Serialize]
      public ConduitType conduitType;
      [Serialize]
      public string crossingID;
      [Serialize]
      public List<(List<sbyte>, sbyte)>/*List<(List<direction_outwards>, flow_priority)>*/ flowPriorities;
      [Serialize]
      public bool swappedBufferStorage;
      // when modifying which fields are serialized change the method CopySerializationFields() as well
      //-------Serializable fields-------UP
      public EndpointType endpointType;
      public bool isIllegal;
      public PipeEnding[] pipeEndings;
      public bool shouldManageInPriorities;
      public bool shouldManageOutPriorities;

      public (byte[], byte[], byte, bool)/*(flowOrder, local_flowOrder, occuredFlowsCount, endpointFlowOccured)*/ inputsFlowManagement;
      public (byte[], byte)/*(flowOrder, checkedDirectionsCount)*/ outputsFlowManagement;

      public GameObject crossingIcon;


      public void SetFieldsToDefault(int crossing_cell, ConduitType conduit_type) {
         crossingCell = crossing_cell;
         conduitType = conduit_type;
         crossingID = Utils.GetRotatedCrossingID(this);
         pipeEndings = Enumerable.Repeat(PipeEnding.Invalid, 4).ToArray();
         isIllegal = false;
         shouldManageInPriorities = false;
         shouldManageOutPriorities = false;
         flowPriorities = new List<(List<sbyte>, sbyte)>(2);
         endpointType = EndpointType.NOT_SET;
      }

      public CrossingCmp CloneSerializationFields(CrossingCmp otherCmp) {
         crossingCell = otherCmp.crossingCell;
         conduitType = otherCmp.conduitType;
         crossingID = otherCmp.crossingID;
         flowPriorities = otherCmp.flowPriorities.DeepClone();
         if(flowPriorities == default)
            flowPriorities = new List<(List<sbyte>, sbyte)>(2);
         swappedBufferStorage = otherCmp.swappedBufferStorage;
         return this;
      }


      public CrossingCmp() { }
      public CrossingCmp(int crossing_cell, ConduitType conduit_type) {
         crossingCell = crossing_cell;
         conduitType = conduit_type;
      }


      public enum EndpointType : byte {
         NOT_SET = 0,
         NO_ENDPOINT = 1,
         SOURCE = 2,
         SINK = 3
      }
   }
}
