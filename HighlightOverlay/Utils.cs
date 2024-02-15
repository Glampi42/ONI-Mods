using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using static STRINGS.BUILDINGS.PREFABS.EXTERIORWALL.FACADES;
using UnityEngine;
using System.IO;
using System.Linq.Expressions;
using System.Data.Common;
using HighlightOverlay.Enums;
using static HighlightOverlay.Strings.MYSTRINGS.UI.OVERLAYS;
using HighlightOverlay;
using static HighlightOverlay.Strings.MYSTRINGS.UI.OVERLAYS.HIGHLIGHTMODE;
using static HighlightOverlay.Structs.ObjectProperties;
using HighlightOverlay.Structs;
using ProcGen.Noise;
using static AttackProperties;

namespace HighlightOverlay {
   public static class Utils {
      public static readonly Tag Milk = SimHashes.Milk.CreateTag();
      public static readonly Tag Vacuum = SimHashes.Vacuum.CreateTag();
      public static readonly Tag PollutedWater = SimHashes.DirtyWater.CreateTag();
      public static readonly Tag RanchStation = RanchStationConfig.ID.ToTag();
      public static readonly Tag ShearingStation = ShearingStationConfig.ID.ToTag();
      public static readonly Tag MilkingStation = MilkingStationConfig.ID.ToTag();
      public static readonly Tag DefaultSpecies = nameof(DefaultSpecies).ToTag();
      public static readonly Tag Oxygen = SimHashes.Oxygen.CreateTag();
      public static readonly Tag PollutedOxygen = SimHashes.ContaminatedOxygen.CreateTag();
      public static readonly Tag CO2 = SimHashes.CarbonDioxide.CreateTag();
      public static readonly Tag Methane = SimHashes.Methane.CreateTag();

      public static void SaveSpriteToAssets(string sprite_name, string additional_path = null) {
         Texture2D texture = LoadTexture(sprite_name, additional_path);
         Sprite sprite = Sprite.Create(texture, new Rect(0.0f, 0.0f, (float)texture.width, (float)texture.height), new Vector2((float)texture.width / 2f, (float)texture.height / 2f));
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

      public static LocString GetMyString(Type stringRoot, string fieldName, params string[] additionalPath) {
         if(stringRoot == null || (stringRoot != typeof(Strings.MYSTRINGS) && !IsNestedWithin(stringRoot, typeof(Strings.MYSTRINGS))))
            throw new ArgumentException(Main.debugPrefix + $"Type {stringRoot} is not a nested type of {typeof(Strings.MYSTRINGS).FullName}");

         Type fieldContainer = stringRoot;
         foreach(string path in additionalPath)
         {
            fieldContainer = fieldContainer.GetNestedType(path);

            if(fieldContainer == null)
               ThrowException();
         }

         FieldInfo field = fieldContainer.GetField(fieldName, BindingFlags.Static | BindingFlags.Public);
         if(field?.GetValue(null) as LocString == null)
            ThrowException();

         return (LocString)field.GetValue(null);


         void ThrowException() {
            string fullName = stringRoot.FullName;
            foreach(string className in additionalPath)
            {
               fullName += "+" + className;
            }
            fullName += "." + fieldName;

            throw new Exception(Main.debugPrefix + "Missing string " + fullName);
         }
      }
      public static bool TryGetMyString(Type stringRoot, string fieldName, out LocString str1ng, params string[] additionalPath) {
         if(stringRoot == null || (stringRoot != typeof(Strings.MYSTRINGS) && !IsNestedWithin(stringRoot, typeof(Strings.MYSTRINGS))))
            throw new ArgumentException(Main.debugPrefix + $"Type {stringRoot} is not a nested type of {typeof(Strings.MYSTRINGS).FullName}");

         str1ng = "";

         Type fieldContainer = stringRoot;
         foreach(string path in additionalPath)
         {
            fieldContainer = fieldContainer.GetNestedType(path);

            if(fieldContainer == null)
               return false;
         }

         FieldInfo field = fieldContainer.GetField(fieldName, BindingFlags.Static | BindingFlags.Public);
         if(field?.GetValue(null) as LocString == null)
            return false;

         str1ng = (LocString)field.GetValue(null);
         return true;
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

      public static HighlightOverlayDiagram GetHighlightOverlayDiagram() {
         return OverlayLegend.Instance?.activeDiagrams?.FirstOrDefault(diagram_go => diagram_go.name == nameof(HighlightOverlayDiagram))?.GetComponent<HighlightOverlayDiagram>();
      }

      public static void UpdateHighlightDiagramOptions() {
         HighlightOverlayDiagram diagram = Utils.GetHighlightOverlayDiagram();
         if(diagram != null)
         {
            diagram.ConfigureDiagramOptions();
         }
      }
      public static void UpdateHighlightMode(bool fullUpdate = false) {
         HighlightMode highlightMode = Main.highlightMode;
         if(highlightMode != default && highlightMode.isEnabled)
         {
            highlightMode.ClearAllData(fullUpdate, fullUpdate);
         }
      }
      public static void UpdateHighlightOfSelectedObject(GameObject oldSelected, int oldSelectedCell, GameObject oldSelectedTile) {
         HighlightMode highlightMode = Main.highlightMode;
         if(highlightMode != default)
         {
            highlightMode.UpdateSelectedObjHighlight(oldSelected, oldSelectedCell, oldSelectedTile);
         }
      }
      public static void UpdateHighlightColor() {
         HighlightMode highlightMode = Main.highlightMode;
         if(highlightMode != default)
         {
            highlightMode.UpdateHighlightColor();
         }
      }

      public static bool IsTile(int cell, out SimCellOccupier cellOccupier) {
         cellOccupier = null;
         GameObject tile_go = Grid.Objects[cell, (int)ObjectLayer.Building];

         return tile_go != null && tile_go.TryGetComponent(out cellOccupier);
      }
      public static bool IsTile(GameObject obj, out SimCellOccupier cellOccupier) {
         cellOccupier = null;
         return obj != null && obj.TryGetComponent(out cellOccupier);
      }

      public static HashSet<Tag> CollectMorphTagsOfSpecies(Tag species, bool collectCritters, bool collectEggs, bool collectBabies) {
         HashSet<Tag> result = new HashSet<Tag>();

         foreach(var morph in Main.speciesMorphs[species])
         {
            if(collectCritters)
               result.Add(morph.PrefabTag);

            if(collectEggs && morph.GetDef<FertilityMonitor.Def>() != null)
               result.Add(morph.GetDef<FertilityMonitor.Def>().eggPrefab);
         }
         if(collectBabies && Main.speciesMorphsBabies.ContainsKey(species))
         {
            foreach(var baby in Main.speciesMorphsBabies[species])
            {
               result.Add(baby.PrefabTag);
            }
         }

         return result;
      }

      public static bool IsGeyserBurried(GameObject geyser) {
         if(geyser == null)
            return default;

         KSelectable selectable = geyser.GetComponent<KSelectable>();
         return selectable == null || !selectable.IsSelectable;
      }

      public static bool IsObjectValidForHighlight(KPrefabID go, out ObjectType objectType) {
         PrimaryElement primaryElement;
         objectType = default;
         return go != null && go.TryGetComponent(out primaryElement) && ((objectType = ObjectProperties.GetObjectType(go)) != ObjectType.ELEMENT || primaryElement.Element != null) &&
            !go.HasTag(GameTags.UnderConstruction);
      }

      //-------------------------------------Elements stuff-------------------------------------DOWN
      public static List<Element> OtherAggregateStates(Element element) {
         if(!Main.otherAggregateStates.ContainsKey(element.id))
            throw new Exception(Main.debugPrefix + $"Element {element.id} was not found in {nameof(Main.otherAggregateStates)} dictionary");

         return Main.otherAggregateStates[element.id];
      }
      public static List<Element> OtherAggregateStates(SimHashes element) {
         return OtherAggregateStates(ElementLoader.GetElement(element.CreateTag()));
      }
      public static List<SimHashes> OtherAggregateStatesIDs(Element element) {
         return OtherAggregateStates(element).Select(elem => elem.id).ToList();
      }
      public static List<Tag> OtherAggregateStatesTags(Element element) {
         return OtherAggregateStates(element).Select(elem => elem.id.CreateTag()).ToList();
      }
      public static List<SimHashes> OtherAggregateStatesIDs(SimHashes element) {
         return OtherAggregateStatesIDs(ElementLoader.GetElement(element.CreateTag()));
      }

      public static SimHashes GetElementsSublimationElement(Element element) {
         if(!Main.sublimationElement.ContainsKey(element.id))
            throw new Exception(Main.debugPrefix + $"Element {element.id} was not found in {nameof(Main.sublimationElement)} dictionary");

         return Main.sublimationElement[element.id];
      }
      public static bool ElementSublimates(Element element) {
         return GetElementsSublimationElement(element) != default;
      }

      public static List<SimHashes> GetElementsTransitionElements(Element element) {
         if(!Main.transitionElements.ContainsKey(element.id))
            throw new Exception(Main.debugPrefix + $"Element {element.id} was not found in {nameof(Main.transitionElements)} dictionary");

         return Main.transitionElements[element.id];
      }
      public static bool ElementTransitsIntoOther(Element element) {
         return GetElementsTransitionElements(element).Count > 0;
      }
      public static List<SimHashes> GetElementsTransitionOreElements(Element element) {
         if(!Main.transitionOreElements.ContainsKey(element.id))
            throw new Exception(Main.debugPrefix + $"Element {element.id} was not found in {nameof(Main.transitionOreElements)} dictionary");

         return Main.transitionOreElements[element.id];
      }
      //-------------------------------------Elements stuff-------------------------------------UP

      public static HighlightFilters GetCorrespondingHighlightFilter(KPrefabID objID) {
         ObjectType objectType = ObjectProperties.GetObjectType(objID);
         if(objectType == ObjectType.ELEMENT || objectType == ObjectType.ITEM)
         {
            return HighlightFilters.ON_GROUND;
         }
         else if(objectType == ObjectType.BUILDING)
         {
            if(objID.TryGetComponent(out Building building))
            {
               var objLayer = building.Def.ObjectLayer;

               if(objLayer == ObjectLayer.Building || objLayer == ObjectLayer.Gantry)
               {
                  if(Utils.IsTile(objID.gameObject, out _))
                  {
                     return HighlightFilters.TILES;
                  }
                  else
                  {
                     return HighlightFilters.STANDARD_BUILDINGS;
                  }
               }
               else if(objLayer == ObjectLayer.LiquidConduit || objLayer == ObjectLayer.LiquidConduitConnection)
               {
                  return HighlightFilters.LIQUID_PIPES;
               }
               else if(objLayer == ObjectLayer.GasConduit || objLayer == ObjectLayer.GasConduitConnection)
               {
                  return HighlightFilters.GAS_PIPES;
               }
               else if(objLayer == ObjectLayer.SolidConduit || objLayer == ObjectLayer.SolidConduitConnection)
               {
                  return HighlightFilters.RAILS;
               }
               else if(objLayer == ObjectLayer.Wire || objLayer == ObjectLayer.WireConnectors)
               {
                  return HighlightFilters.WIRES;
               }
               else if(objLayer == ObjectLayer.LogicWire || objLayer == ObjectLayer.LogicGate)
               {
                  return HighlightFilters.AUTOMATION;
               }
               else if(objLayer == ObjectLayer.Backwall)
               {
                  return HighlightFilters.BACKWALLS;
               }
            }

            return HighlightFilters.STANDARD_BUILDINGS;
         }
         else if(objectType == ObjectType.PLANTORSEED)
         {
            if(objID.HasTag(GameTags.Seed))
            {
               return HighlightFilters.ON_GROUND;
            }
            else
            {
               return HighlightFilters.PLANTS;
            }
         }
         else if(objectType == ObjectType.CRITTEROREGG)
         {
            if(objID.HasTag(GameTags.Egg))
            {
               return HighlightFilters.ON_GROUND;
            }
            else
            {
               return HighlightFilters.CRITTERS;
            }
         }
         else if(objectType == ObjectType.DUPLICANT)
         {
            return HighlightFilters.DUPLICANTS;
         }
         else if(objectType == ObjectType.ROBOT)
         {
            return HighlightFilters.ROBOTS;
         }
         else if(objectType == ObjectType.GEYSER)
         {
            return HighlightFilters.GEYSERS;
         }
         else if(objectType == ObjectType.SAPTREE)
         {
            return HighlightFilters.PLANTS;
         }
         else
         {
            return HighlightFilters.OTHER;
         }
      }
      public static HighlightFilters GetCorrespondingHighlightFilterCell(Element element) {
         if(element.IsSolid)
         {
            return HighlightFilters.NATURAL_TILES;
         }
         else if(element.IsLiquid)
         {
            return HighlightFilters.LIQUIDS;
         }
         else if(element.IsGas)
         {
            return HighlightFilters.GASES;
         }

         return HighlightFilters.NONE;
      }

      public static void ClampExtentsToActiveWorldBounds(ref Extents extents) {
         WorldContainer activeWorld = ClusterManager.Instance.activeWorld;
         int max_x = extents.x + extents.width;
         int max_y = extents.y + extents.height;

         extents.x = Math.Max(extents.x, (int)activeWorld.minimumBounds.x);
         extents.y = Math.Max(extents.y, (int)activeWorld.minimumBounds.y);
         max_x = Math.Min(max_x, (int)activeWorld.maximumBounds.x);
         max_y = Math.Min(max_y, (int)activeWorld.maximumBounds.y);
         extents.width = Math.Max(max_x - extents.x, 0);
         extents.height = Math.Max(max_y - extents.y, 0);
      }

      public static int PosToCell(GameObject go) {
         if(go == null)
            return -1;

         return Grid.PosToCell(go);
      }

      //--------------------------------Cells highlighting utils--------------------------------DOWN
      public static void SetDefaultCellColors() {
         for(int cell = 0; cell < Main.cellColors.Length; cell++)
         {
            Main.cellColors[cell] = Main.blackBackgroundColor;
            Main.tileColors[cell] = Main.blackBackgroundColor;
         }
      }
      //--------------------------------Cells highlighting utils--------------------------------UP
      //-----------------------------------Extentions-----------------------------------DOWN
      public static bool HasSkill(this MinionResume minionResume, string skillID) {
         return minionResume.HasMasteredSkill(skillID) || minionResume.HasBeenGrantedSkill(skillID);
      }

      public static bool TryGetComponents<T>(this GameObject go, out T[] cmps) where T : Component {
         cmps = go.GetComponents<T>();

         return cmps.Length > 0;
      }
      public static bool TryGetComponents<T>(this KPrefabID go, out T[] cmps) where T : Component {
         return TryGetComponents(go.gameObject, out cmps);
      }

      public static T GetOrDefault<K, T>(this Dictionary<K, T> dict, K key) {
         if(dict.ContainsKey(key))
            return dict[key];

         return default;
      }

      public static bool ContainsAnyFrom<T>(this IEnumerable<T> collection, IEnumerable<T> other) {
         return other.Any(x => collection.Contains(x));
      }

      public static string AsString<T>(this T[] array) {
         string str = "[";
         for(int i = 0; i < array.Length; i++)
         {
            str += array[i].ToString();

            if(i != array.Length - 1)
               str += ", ";
         }
         str += "]";

         return str;
      }

      public static void AddAll<T>(this HashSet<T> set, params T[] other) {
         foreach(var item in other)
            set.Add(item);
      }
      public static void AddAll<T>(this HashSet<T> set, IEnumerable<T> other) {
         foreach(var item in other)
            set.Add(item);
      }
      //-----------------------------------Extentions-----------------------------------UP

      public static bool IsNestedWithin(Type nestedType, Type rootType) {
         Type declaringType = nestedType.DeclaringType;
         while(declaringType != null)
         {
            if(declaringType == rootType)
            {
               return true;
            }
            declaringType = declaringType.DeclaringType;
         }
         return false;
      }

      public static int CountTrailingZeros(int num) {
         int mask = 1;
         for(int i = 0; i < 32; i++, mask <<= 1)
            if((num & mask) != 0)
               return i;

         return 32;
      }

      public static bool HasMoreThanOneBitSet(int bitMask) {
         int count = 0;
         while(bitMask > 0)
         {
            if((bitMask & 1) == 1)
               count++;

            bitMask >>= 1;
         }
         return count > 1;
      }

      public static int GetBinaryLength(int number) {
         int length = 0;
         while(number > 0)
         {
            number >>= 1;
            length++;
         }
         return length;
      }
   }
}
