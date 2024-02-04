using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HighlightOverlay.Enums {
   public static class HighlightOptionsExtentions {
      public static HighlightOptions Reverse(this HighlightOptions highlightOption) {
         switch(highlightOption)
         {
            case HighlightOptions.CONSUMERS:
               return HighlightOptions.CONSUMABLES;

            case HighlightOptions.PRODUCERS:
               return HighlightOptions.PRODUCE;

            case HighlightOptions.CONSUMABLES:
               return HighlightOptions.CONSUMERS;

            case HighlightOptions.PRODUCE:
               return HighlightOptions.PRODUCERS;

            default:
               return HighlightOptions.NONE;
         }
      }
   }
}
