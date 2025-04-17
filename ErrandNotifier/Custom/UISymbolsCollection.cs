using ErrandNotifier.Structs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace ErrandNotifier.Custom {
   /// <summary>
   /// This class manages, creates and deletes the symbols that represent the notification type and are displayed
   /// in the NotifierOverlay above the errands assigned to the notification.
   /// </summary>
   public class UISymbolsCollection {
      private Dictionary<GameObject, HashSet<UISymbol>> uiSymbols;

      public UISymbolsCollection() {
         uiSymbols = new();
      }

      public void Add(GameObject go, UISymbol symbol) {
         if(uiSymbols.ContainsKey(go))
         {
            uiSymbols[go].Add(symbol);
         }
         else
         {
            uiSymbols.Add(go, new HashSet<UISymbol> { symbol });
         }
      }

      public void RemoveAttached(GameObject go) {
         uiSymbols.Remove(go);
      }

      public void Remove(GameObject go, UISymbol symbol) {
         if(uiSymbols.ContainsKey(go))
         {
            uiSymbols[go].Remove(symbol);
         }
      }
      public void Remove(GameObject go, Workable relatedErrand) {
         if(uiSymbols.ContainsKey(go))
         {
            uiSymbols[go].RemoveWhere(symbol => symbol.GetRelatedErrand() == relatedErrand);
         }
      }

      public HashSet<UISymbol> GetAttachedUISymbols(GameObject go) {
         HashSet<UISymbol> result = new();
         if(uiSymbols.ContainsKey(go))
            result = uiSymbols[go];

         return result;
      }

      public bool TryGetUISymbol(GameObject parentGO, Workable relatedErrand, out UISymbol uiSymbol) {
         uiSymbol = default;

         if(uiSymbols.ContainsKey(parentGO))
         {
            uiSymbol = uiSymbols[parentGO].FirstOrDefault(symbol => symbol.GetRelatedErrand() == relatedErrand);
            return uiSymbol.GetRelatedErrand() != default;
         }

         return false;
      }

      public Dictionary<GameObject, HashSet<UISymbol>> GetAllUISymbols() {
         return uiSymbols;
      }
      public HashSet<UISymbol> GetAllUISymbolsFlattened() {
         return new HashSet<UISymbol>(uiSymbols.Values.SelectMany(x => x));
      }

      public void Clear() {
         uiSymbols.Clear();
      }
   }
}
