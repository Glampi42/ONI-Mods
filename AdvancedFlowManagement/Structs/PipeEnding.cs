using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AdvancedFlowManagement {
   public struct PipeEnding {
      public Type type;
      public int endingCell;
      public int pipeLength;// if PipeEnding.Type == CROSSING then the other crossing cell is not included; the starting crossing is not included either
      public ConduitFlow.FlowDirections backwardsDirection;

      public enum Type {
         NOT_SET,
         DEAD_END,
         SINK,
         SOURCE,
         CROSSING
      }

      public static readonly PipeEnding Invalid =
          new PipeEnding { endingCell = -1 };
   }
}
