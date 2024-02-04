using HighlightOverlay.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HighlightOverlay.Attributes {
   public class HighlightOptionLabelAttribute : Attribute {
      public HighlightOptions highlightOption;

      public HighlightOptionLabelAttribute(HighlightOptions highlightOption) {
         this.highlightOption = highlightOption;
      }
   }
}
