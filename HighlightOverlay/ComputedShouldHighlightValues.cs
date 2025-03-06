using HighlightOverlay.Enums;
using HighlightOverlay;
using HighlightOverlay.Structs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HighlightOverlay {
   public class ComputedShouldHighlightValues {
      private Dictionary<ObjectType, Dictionary<object, bool>> values = new Dictionary<ObjectType, Dictionary<object, bool>>(Enum.GetValues(typeof(ObjectType)).Length);

      public void StoreValue(ObjectType objectType, KPrefabID obj, Element element, bool shouldHighlight) {
         object objForShouldHighlight = ObjectProperties.ObjectForShouldHighlight(objectType, obj, element);
         if(objForShouldHighlight != null)
            values[objectType].Add(objForShouldHighlight, shouldHighlight);
      }

      public bool TryGetValue(ObjectType objectType, KPrefabID obj, Element element, out bool shouldHighlight) {
         shouldHighlight = default;

         object objForShouldHighlight = ObjectProperties.ObjectForShouldHighlight(objectType, obj, element);
         if(objForShouldHighlight != null && values[objectType].ContainsKey(objForShouldHighlight))
         {
            shouldHighlight = values[objectType][objForShouldHighlight];
            return true;
         }

         return false;
      }

      public void Clear() {
         foreach(ObjectType objType in Enum.GetValues(typeof(ObjectType)))
         {
            values[objType].Clear();
         }
      }

      public ComputedShouldHighlightValues() {
         foreach(ObjectType objType in Enum.GetValues(typeof(ObjectType)))
         {
            values.Add(objType, new Dictionary<object, bool>(32));
         }
      }
   }
}
