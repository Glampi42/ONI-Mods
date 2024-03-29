﻿using HighlightOverlay.Enums;
using HighlightOverlay;
using HighlightOverlay.Structs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HighlightOverlay {
   public class ComputedShouldHighlightValues {
      private Dictionary<ObjectType, Dictionary<(object, HighlightFilters), bool>> values = new Dictionary<ObjectType, Dictionary<(object, HighlightFilters), bool>>(Enum.GetValues(typeof(ObjectType)).Length);

      public void StoreValue(ObjectType objectType, KPrefabID obj, Element element, HighlightFilters highlightFilter, bool shouldHighlight) {
         object objForShouldHighlight = ObjectProperties.ObjectForShouldHighlight(objectType, obj, element);
         if(objForShouldHighlight != null)
            values[objectType].Add((objForShouldHighlight, highlightFilter), shouldHighlight);// there may be one object with multiple different highlight filters applied to it(f.e. element: cell, stored_item, on_ground)
      }

      public bool TryGetValue(ObjectType objectType, KPrefabID obj, Element element, HighlightFilters highlightFilter, out bool shouldHighlight) {
         shouldHighlight = default;

         object objForShouldHighlight = ObjectProperties.ObjectForShouldHighlight(objectType, obj, element);
         if(objForShouldHighlight != null && values[objectType].ContainsKey((objForShouldHighlight, highlightFilter)))
         {
            shouldHighlight = values[objectType][(objForShouldHighlight, highlightFilter)];
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
            values.Add(objType, new Dictionary<(object, HighlightFilters), bool>(32));
         }
      }
   }
}
