using KSerialization;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

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
      // when modifying which fields are serialized consider the method CloneField()
      //-------Serializable fields-------UP
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
      }

      public CrossingCmp CloneSerializationFields(CrossingCmp otherCmp) {
         FieldInfo[] serializedFields = this.GetType().GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
         serializedFields = serializedFields.Where(field => field.GetCustomAttribute(typeof(Serialize)) != default).ToArray();

         foreach(var serializedField in serializedFields)
         {
            CloneField(serializedField, otherCmp);
         }

         return this;
      }
      private void CloneField(FieldInfo field, CrossingCmp otherCmp) {
         if(field.Name == nameof(flowPriorities))
         {
            flowPriorities = ((List<(List<sbyte>, sbyte)>)field.GetValue(otherCmp)).DeepClone();
            if(flowPriorities == default)
               flowPriorities = new List<(List<sbyte>, sbyte)>(2);
         }
         else
         {
            field.SetValue(this, field.GetValue(otherCmp));
         }
      }


      public CrossingCmp() { }
      public CrossingCmp(int crossing_cell, ConduitType conduit_type) {
         crossingCell = crossing_cell;
         conduitType = conduit_type;
      }
   }
}
