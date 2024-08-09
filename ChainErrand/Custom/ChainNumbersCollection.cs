using ChainErrand.ChainHierarchy;
using ChainErrand.Structs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace ChainErrand.Custom {
   public class ChainNumbersCollection {
      private Dictionary<GameObject, HashSet<ChainNumber>> chainNumbers;

      public ChainNumbersCollection() {
         chainNumbers = new();
      }

      public void Add(GameObject go, ChainNumber number) {
         if(chainNumbers.ContainsKey(go))
         {
            chainNumbers[go].Add(number);
         }
         else
         {
            chainNumbers.Add(go, new HashSet<ChainNumber> { number });
         }
      }

      public void RemoveAttached(GameObject go) {
         chainNumbers.Remove(go);
      }

      public void Remove(GameObject go, ChainNumber chainNumber) {
         if(chainNumbers.ContainsKey(go))
         {
            chainNumbers[go].Remove(chainNumber);
         }
      }
      public void Remove(GameObject go, Workable relatedErrand) {
         if(chainNumbers.ContainsKey(go))
         {
            chainNumbers[go].RemoveWhere(number => number.GetRelatedErrand() == relatedErrand);
         }
      }

      public HashSet<ChainNumber> GetAttachedChainNumbers(GameObject go) {
         HashSet<ChainNumber> result = new();
         if(chainNumbers.ContainsKey(go))
            result = chainNumbers[go];

         return result;
      }

      public bool TryGetChainNumber(GameObject parentGO, Workable relatedErrand, out ChainNumber chainNumber) {
         chainNumber = default;

         if(chainNumbers.ContainsKey(parentGO))
         {
            chainNumber = chainNumbers[parentGO].FirstOrDefault(number => number.GetRelatedErrand() == relatedErrand);
            return chainNumber.GetRelatedErrand() != default;
         }

         return false;
      }

      public Dictionary<GameObject, HashSet<ChainNumber>> GetAllChainNumbers() {
         return chainNumbers;
      }
      public HashSet<ChainNumber> GetAllChainNumbersFlattened() {
         return new HashSet<ChainNumber>(chainNumbers.Values.SelectMany(x => x));
      }

      public void Clear() {
         chainNumbers.Clear();
      }
   }
}
