using ErrandNotifier.Components;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace ErrandNotifier {
   public static class Utils {
      public static HashSet<GameObject> CollectPrioritizableObjects(Extents extents) {
         HashSet<GameObject> collectedGOs = new();

         List<ScenePartitionerEntry> gatheredObjects = new List<ScenePartitionerEntry>();
         GameScenePartitioner.Instance.GatherEntries(extents, GameScenePartitioner.Instance.prioritizableObjects, gatheredObjects);
         foreach(ScenePartitionerEntry gatheredObject in gatheredObjects)
         {
            GameObject obj = ((Component)gatheredObject?.obj)?.gameObject;

            if(obj != null && Grid.IsValidCell(Grid.PosToCell(obj)) && Grid.IsVisible(Grid.PosToCell(obj)))
            {
               collectedGOs.Add(obj);
            }
         }

         // double-checking the buildings layer (GatherEntries() works with GameObjects' center points; a building might be partially in the extents):
         for(int x = extents.x; x < extents.x + extents.width; x++)
         {
            for(int y = extents.y; y < extents.y + extents.height; y++)
            {
               int cell = Grid.XYToCell(x, y);
               if(Grid.IsValidCell(cell) && Grid.IsVisible(cell))
               {
                  var building = Grid.Objects[cell, (int)ObjectLayer.Building];
                  if(building != null)
                  {
                     collectedGOs.Add(building);
                  }
               }
            }
         }

         return collectedGOs;
      }

      /// <summary>
      /// Tries to retrieve the ChainedErrand component attached to the same GameObject that is related to the specified errand.
      /// </summary>
      /// <param name="errand">The errand</param>
      /// <param name="notifiableErrand">The retrieved ChainedErrand</param>
      /// <param name="allowDisabled">If true, the ChainedErrand component may be disabled</param>
      /// <returns>True if such ChainedErrand was found; false otherwise.</returns>
      public static bool TryGetCorrespondingNotifiableErrand(this Workable errand, out NotifiableErrand notifiableErrand, bool allowDisabled = false) {
         notifiableErrand = null;

         if(errand.IsNullOrDestroyed())
            return false;

         Type notifiableErrandType = ChainedErrandPackRegistry.GetChainedErrandPack(errand).GetChainedErrandType();
         if(errand.TryGetComponent(notifiableErrandType, out Component ce) && (allowDisabled || ((KMonoBehaviour)ce).enabled))
         {
            notifiableErrand = ce as NotifiableErrand;
         }

         return notifiableErrand != null;
      }
      /// <summary>
      /// Tries to retrieve the ChainedErrand component that is related to the specified chore.
      /// </summary>
      /// <param name="chore">The chore</param>
      /// <param name="go">The GameObject that potentially has the ChainedErrand component</param>
      /// <param name="chainedErrand">The retrieved ChainedErrand</param>
      /// <returns>True if such ChainedErrand was found; false otherwise.</returns>
      public static bool TryGetCorrespondingNotifiableErrand(this Chore chore, GameObject go, out ChainedErrand chainedErrand) {
         chainedErrand = null;

         if(go.TryGetComponents(out ChainedErrand[] cEs))
         {
            chainedErrand = cEs.FirstOrDefault(ce => ce.enabled && ce.chore == chore);
         }

         return chainedErrand != null;
      }

      /// <summary>
      /// Clamps the given vector to the interior of the currently active world.
      /// </summary>
      /// <param name="vector">The vector</param>
      public static void ClampToActiveWorldBounds(ref Vector2I vector) {
         WorldContainer activeWorld = ClusterManager.Instance.activeWorld;
         vector.x = Math.Max(vector.x, (int)activeWorld.minimumBounds.x);
         vector.x = Math.Min(vector.x, (int)activeWorld.maximumBounds.x);
         vector.y = Math.Max(vector.y, (int)activeWorld.minimumBounds.y);
         vector.y = Math.Min(vector.y, (int)activeWorld.maximumBounds.y);
      }

      public static bool IsTile(GameObject obj, out SimCellOccupier cellOccupier) {
         cellOccupier = null;
         return obj != null && obj.TryGetComponent(out cellOccupier);
      }

      //---------------------Vectors etc.---------------------DOWN
      public static Vector3 InverseLocalScale(this RectTransform rectTransform) {
         Vector3 localScale = rectTransform.localScale;
         return new Vector3(1f / localScale.x, 1f / localScale.y, 1f / localScale.z);
      }
      //---------------------Vectors etc.---------------------UP
      //---------------------Extensions---------------------DOWN
      /// <returns>The transform child of the specified index or null if such child doesn't exist</returns>
      public static GameObject GetChildSafe(this GameObject parent, int index) {
         if(parent.transform.childCount <= index)
            return null;

         return parent.transform.GetChild(index).gameObject;
      }
      /// <returns>The transform child of the specified index or null if such child doesn't exist</returns>
      public static Transform GetChildSafe(this Transform parent, int index) {
         if(parent.childCount <= index)
            return null;

         return parent.GetChild(index);
      }

      public static bool TryGetComponents<T>(this GameObject go, out T[] cmps) where T : Component {
         cmps = go.GetComponents<T>();

         return cmps.Length > 0;
      }
      public static bool TryGetComponents<T>(this MonoBehaviour mb, out T[] cmps) where T : Component {
         return TryGetComponents(mb.gameObject, out cmps);
      }

      public static Transform FindRecursively(this Transform parent, string childName) {
         if(parent.childCount == 0)
            return null;

         for(int i = 0; i < parent.childCount; i++)
         {
            var child = parent.GetChild(i);
            if(child.name == childName)
               return child;

            if((child = FindRecursively(child, childName)) != null)
               return child;
         }

         return null;
      }

      public static void AddRange<T>(this HashSet<T> set, IEnumerable<T> another) {
         if(another == null)
            return;

         foreach(T item in another)
            set.Add(item);
      }
      public static void Add<T>(this HashSet<T> set, params T[] items) {
         foreach(var item in items)
         {
            set.Add(item);
         }
      }

      public static T GetOrDefault<K, T>(this Dictionary<K, T> dict, K key) {
         if(dict.ContainsKey(key))
            return dict[key];

         return default;
      }
      //---------------------Extensions---------------------UP
   }
}
