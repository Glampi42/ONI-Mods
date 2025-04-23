using ErrandNotifier.NotifiableErrandPacks;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using System.Reflection;
using ErrandNotifier.NotificationsHierarchy;
using System.IO;
using ErrandNotifier.Enums;

namespace ErrandNotifier {
   public static class Utils {
      public static readonly BindingFlags GeneralBindingFlags = BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic;

      public static HashSet<ObjectLayer> ObjectLayersFromNotifierToolFilter(NotifierToolFilter filter) {
         HashSet<ObjectLayer> layers = new();

         switch(filter)
         {
            case NotifierToolFilter.STANDARD_BUILDINGS:
               layers.Add(ObjectLayer.Building, ObjectLayer.Gantry, ObjectLayer.FoundationTile, ObjectLayer.ReplacementTile, ObjectLayer.ReplacementLadder);
               break;

            case NotifierToolFilter.LIQUID_PIPES:
               layers.Add(ObjectLayer.LiquidConduit, ObjectLayer.LiquidConduitConnection, ObjectLayer.ReplacementLiquidConduit);
               break;

            case NotifierToolFilter.GAS_PIPES:
               layers.Add(ObjectLayer.GasConduit, ObjectLayer.GasConduitConnection, ObjectLayer.ReplacementGasConduit);
               break;

            case NotifierToolFilter.CONVEYOR_RAILS:
               layers.Add(ObjectLayer.SolidConduit, ObjectLayer.SolidConduitConnection, ObjectLayer.ReplacementSolidConduit);
               break;

            case NotifierToolFilter.WIRES:
               layers.Add(ObjectLayer.Wire, ObjectLayer.WireConnectors, ObjectLayer.ReplacementWire);
               break;

            case NotifierToolFilter.AUTOMATION:
               layers.Add(ObjectLayer.LogicGate, ObjectLayer.LogicWire, ObjectLayer.ReplacementLogicWire);
               break;

            case NotifierToolFilter.BACKWALLS:
               layers.Add(ObjectLayer.Backwall, ObjectLayer.ReplacementBackwall);
               break;
         }

         return layers;
      }

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
      /// Tries to retrieve the NotifiableErrand component attached to the same GameObject that is related to the specified errand.
      /// </summary>
      /// <param name="errand">The errand</param>
      /// <param name="notifiableErrand">The retrieved NotifiableErrand</param>
      /// <param name="allowDisabled">If true, the NotifiableErrand component may be disabled</param>
      /// <returns>True if such NotifiableErrand was found; false otherwise.</returns>
      public static bool TryGetCorrespondingNotifiableErrand(this Workable errand, out NotifiableErrand notifiableErrand, bool allowDisabled = false) {
         notifiableErrand = null;

         if(errand.IsNullOrDestroyed())
            return false;

         Type notifiableErrandType = NotifiableErrandPackRegistry.GetNotifiableErrandPack(errand).GetNotifiableErrandType();
         if(errand.TryGetComponent(notifiableErrandType, out Component ce) && (allowDisabled || ((KMonoBehaviour)ce).enabled))
         {
            notifiableErrand = ce as NotifiableErrand;
         }

         return notifiableErrand != null;
      }
      /// <summary>
      /// Tries to retrieve the NotifiableErrand component that is related to the specified chore.
      /// </summary>
      /// <param name="chore">The chore</param>
      /// <param name="go">The GameObject that potentially has the NotifiableErrand component</param>
      /// <param name="notifiableErrand">The retrieved NotifiableErrand</param>
      /// <returns>True if such NotifiableErrand was found; false otherwise.</returns>
      public static bool TryGetCorrespondingNotifiableErrand(this Chore chore, GameObject go, out NotifiableErrand notifiableErrand) {
         notifiableErrand = null;

         if(go.TryGetComponents(out NotifiableErrand[] cEs))
         {
            notifiableErrand = cEs.FirstOrDefault(ce => ce.enabled && ce.chore == chore);
         }

         return notifiableErrand != null;
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

      public static ToolTip AddSimpleTooltip(this GameObject go, string tooltip, bool alignCenter = true, float wrapWidth = 0, bool onBottom = true) {
         if(go == null)
            return null;

         var tooltipCmp = go.AddOrGet<ToolTip>();
         tooltipCmp.UseFixedStringKey = false;
         tooltipCmp.enabled = true;
         tooltipCmp.tooltipPivot = alignCenter ? new Vector2(0.5f, onBottom ? 1f : 0f) : new Vector2(1f, onBottom ? 1f : 0f);
         tooltipCmp.tooltipPositionOffset = onBottom ? new Vector2(0f, -20f) : new Vector2(0f, 20f);
         tooltipCmp.parentPositionAnchor = new Vector2(0.5f, 0.5f);
         if(wrapWidth > 0)
         {
            tooltipCmp.WrapWidth = wrapWidth;
            tooltipCmp.SizingSetting = ToolTip.ToolTipSizeSetting.MaxWidthWrapContent;
         }
         //ToolTipScreen.Instance.SetToolTip(tooltipCmp);
         tooltipCmp.SetSimpleTooltip(tooltip);
         return tooltipCmp;
      }
      /// <summary>
      /// Adds a ToolTip to the specified GameObject that mimics the style of tooltips in tool filter menu: the tooltip is located to the left of the panel and stays at the same location
      /// even for different objects.
      /// </summary>
      /// <param name="go">The GameObject</param>
      /// <param name="tooltip">The text that will be displayed in the tooltip</param>
      /// <param name="parentOverride">The object that overrides the positioning of the tooltip (it will be positioned relative to it instead of the GameObject that is the owner of the tooltip)</param>
      /// <param name="spacingToLeft">Distance between the tooltip and the parent object</param>
      /// <param name="wrapWidth">The wrap width of the tooltip; if 0, the width will be dynamic to fit all contents without wrapping</param>
      /// <returns>The created ToolTip component.</returns>
      public static ToolTip AddFilterMenuToolTip(this GameObject go, string tooltip, RectTransform parentOverride, float spacingToLeft = 4f, float wrapWidth = 0f) {
         if(go == null)
            return null;

         var tooltipCmp = go.AddOrGet<ToolTip>();
         tooltipCmp.UseFixedStringKey = false;
         tooltipCmp.enabled = true;
         tooltipCmp.toolTipPosition = ToolTip.TooltipPosition.Custom;
         tooltipCmp.overrideParentObject = parentOverride;
         tooltipCmp.tooltipPivot = new Vector2(1f, 1f);
         tooltipCmp.tooltipPositionOffset = new Vector2(-spacingToLeft, 0f);
         tooltipCmp.parentPositionAnchor = new Vector2(0f, 1f);
         if(wrapWidth > 0f)
         {
            tooltipCmp.WrapWidth = wrapWidth;
            tooltipCmp.SizingSetting = ToolTip.ToolTipSizeSetting.MaxWidthWrapContent;
         }
         else
         {
            tooltipCmp.SizingSetting = ToolTip.ToolTipSizeSetting.DynamicWidthNoWrap;
         }
         //ToolTipScreen.Instance.SetToolTip(tooltipCmp);
         tooltipCmp.SetSimpleTooltip(tooltip);
         return tooltipCmp;
      }

      public static void SaveSpriteToAssets(string sprite_name, string additional_path = null) {
         Texture2D texture = LoadTexture(sprite_name, additional_path);
         Sprite sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), Vector3.zero);
         sprite.name = sprite_name;
         Assets.Sprites.Add(sprite_name, sprite);
      }
      private static Texture2D LoadTexture(string name, string additional_path) {
         Texture2D texture = null;
         string path = Path.Combine(Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "assets" + (additional_path ?? "")), name + ".png");
         try
         {
            byte[] data = File.ReadAllBytes(path);
            texture = new Texture2D(1, 1);
            texture.LoadImage(data);
         }
         catch(Exception ex)
         {
            Debug.LogError((object)(Main.debugPrefix + "Could not load texture at " + path));
            Debug.LogException(ex);
         }
         return texture;
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
