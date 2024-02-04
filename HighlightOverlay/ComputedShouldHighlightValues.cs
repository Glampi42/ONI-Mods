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

      public void StoreValue(ObjectProperties obj, bool shouldHighlight) {
         if(!values.ContainsKey(obj.objectType))
            values.Add(obj.objectType, new Dictionary<object, bool>());

         if(obj.ObjectForShouldHighlight() != null)
            values[obj.objectType].Add(obj.ObjectForShouldHighlight(), shouldHighlight);
      }

      public bool TryGetValue(ObjectProperties obj, out bool shouldHighlight) {
         shouldHighlight = default;

         if(values.ContainsKey(obj.objectType))
         {
            object objForShouldHighlight = obj.ObjectForShouldHighlight();
            if(objForShouldHighlight != null && values[obj.objectType].ContainsKey(objForShouldHighlight))
            {
               shouldHighlight = values[obj.objectType][objForShouldHighlight];
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
