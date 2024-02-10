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

      public void StoreValue(ObjectType objectType, PrimaryElement obj, int cell, bool shouldHighlight) {
         if(!values.ContainsKey(objectType))
            values.Add(objectType, new Dictionary<object, bool>());

         object objForShouldHighlight = ObjectProperties.ObjectForShouldHighlight(objectType, obj, cell);
         if(objForShouldHighlight != null)
            values[objectType].Add(objForShouldHighlight, shouldHighlight);
      }

      public bool TryGetValue(ObjectType objectType, PrimaryElement obj, int cell, out bool shouldHighlight) {
         shouldHighlight = default;

         if(values.ContainsKey(objectType))
         {
            object objForShouldHighlight = ObjectProperties.ObjectForShouldHighlight(objectType, obj, cell);
            if(objForShouldHighlight != null && values[objectType].ContainsKey(objForShouldHighlight))
            {
               shouldHighlight = values[objectType][objForShouldHighlight];
               return true;
            }
         }
         return false;
      }

      public void Clear() {
         values.Clear();
      }
   }
}
