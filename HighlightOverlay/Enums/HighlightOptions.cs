using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HighlightOverlay.Enums {
   [Flags]
   public enum HighlightOptions {
      NONE = 0x0,
      CONSIDEROPTION1 = 0x1,
      CONSUMERS = 0x2,
      PRODUCERS = 0x4,
      CONSUMABLES = 0x8,
      PRODUCE = 0x10,
      BUILDINGMATERIAL = 0x20,
      COPIES = 0x40,
      EXACTCOPIES = 0x80
   }
}
