using HighlightOverlay.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HighlightOverlay.Attributes {
   public class HighlightOptionAttribute : Attribute {
      public HighlightOptions highlightOption;

      public HighlightOptionAttribute(HighlightOptions highlightOption) {
         this.highlightOption = highlightOption;
      }
   }
}
