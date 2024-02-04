using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HighlightOverlay.Enums {
   public static class ObjectTypeExtentions {
      public static bool DefaultConsiderOption1(this ObjectType objectType) {
         switch(objectType)
         {
            case ObjectType.ELEMENT:
               return false;// shouldn't consider aggregate state

            case ObjectType.BUILDING:
               return true;// should consider building's options

            case ObjectType.CRITTEROREGG:
               return true;// should consider critter's morph

            default:
               return default;
         }
      }

      public static bool ConsiderOption1(this ObjectType objectType) {
         return Main.considerOption1[objectType];
      }
   }
}
