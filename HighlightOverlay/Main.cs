using HighlightOverlay.Enums;
using HighlightOverlay;
using HighlightOverlay.Structs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using HighlightOverlay.Strings;
using UnityEngine.UI;
using TemplateClasses;

namespace HighlightOverlay {
   public static class Main {
      public const string debugPrefix = "[HighlightOverlay] > ";

      public static readonly Color whiteHighlightColor = new Color(0.90f, 0.90f, 0.90f, 0f);
      public static readonly Color whiteBackgroundColor = new Color(0.95f, 0.95f, 0.95f, 1f);
      public static readonly Color blackBackgroundColor = new Color(0.15114269f, 0.15114269f, 0.15114269f, 1f);// should be unique because of technical reasons

      public static readonly Color checkboxHoverColor = new Color(0.5f, 0.5f, 0.5f, 1f);

      public static bool highlightInTrueColor = false;
      public static bool preservePreviousHighlightOptions = false;


      public static Dictionary<Tag, Tag> cachedObjectIDs = new Dictionary<Tag, Tag>();

      public static Dictionary<Tag, HighlightFilters> cachedHighlightFilters = new Dictionary<Tag, HighlightFilters>();
      public static Dictionary<SimHashes, HighlightFilters> cachedHighlightFiltersCells = new Dictionary<SimHashes, HighlightFilters>();

      public static Dictionary<Tag, List<KPrefabID>> speciesMorphs = new Dictionary<Tag, List<KPrefabID>>();
      public static Dictionary<Tag, List<KPrefabID>> speciesMorphsBabies = new Dictionary<Tag, List<KPrefabID>>();

      public static Dictionary<SimHashes, List<Element>> otherAggregateStates;
      public static Dictionary<SimHashes, SimHashes> sublimationElement;
      public static Dictionary<SimHashes, List<SimHashes>> transitionElements;
      public static Dictionary<SimHashes, List<SimHashes>> transitionOreElements;

      public static Dictionary<Tag, HighlightOptions> buildingsCachedOptions;
      public static Dictionary<Tag, HighlightOptions> plantsCachedOptions;
      public static Dictionary<Tag, HighlightOptions> crittersCachedOptions;


      public static HighlightMode highlightMode = default;

      public static GameObject selectedObj = null;
      public static int selectedCell = -1;
      public static (GameObject, int)/*(tile_go, cell)*/ selectedTile = default;
      public static ObjectProperties selectedObjProperties = default;
      public static Color selectedCellHighlightColor = Color.clear;

      public static Dictionary<ObjectType, bool> considerOption1 = new Dictionary<ObjectType, bool>();
      public static HighlightOptions highlightOption = HighlightOptions.NONE;

      public static Dictionary<ObjectType, HighlightOptions> lastHighlightOption = new Dictionary<ObjectType, HighlightOptions>();

      public static HighlightFilters highlightFilters = HighlightFilters.ALL;


      public static Color[] cellColors;
      public static Color[] tileColors;



      static Main() {
         foreach(ObjectType objectType in typeof(ObjectType).GetEnumValues())
         {
            considerOption1.Add(objectType, objectType.DefaultConsiderOption1());

            lastHighlightOption.Add(objectType, HighlightOptions.NONE);
         }
      }


      public static void CacheObjectIDs() {
         foreach(var prefab in Assets.Prefabs)
         {
            if(prefab == null || prefab.gameObject == null)
               continue;

            if(Utils.IsObjectValidForHighlight(prefab, out ObjectType objectType))
            {
               ObjectProperties properties = new ObjectProperties(prefab, objectType);
               cachedObjectIDs.Add(prefab.PrefabTag, properties.prefabID);
            }
         }
      }

      public static void CacheHighlightFilters() {
         foreach(var prefab in Assets.Prefabs)
         {
            if(prefab == null || prefab.gameObject == null)
               continue;

            if(Utils.IsObjectValidForHighlight(prefab, out _))
            {
               cachedHighlightFilters.Add(prefab.PrefabTag, Utils.GetCorrespondingHighlightFilter(prefab));
            }
         }
         foreach(var element in ElementLoader.elements)
         {
            if(element == null)
               continue;

            cachedHighlightFiltersCells.Add(element.id, Utils.GetCorrespondingHighlightFilterCell(element));
         }
      }

      public static void CacheCrittersMorphs() {
         foreach(var prefab in Assets.Prefabs)
         {
            if(prefab == null || prefab.gameObject == null)
               continue;

            if(prefab.TryGetComponent(out CreatureBrain brain))
            {
               if(prefab.GetDef<BabyMonitor.Def>() == null)
               {
                  if(!speciesMorphs.ContainsKey(brain.species))
                     speciesMorphs.Add(brain.species, new List<KPrefabID>(4));

                  speciesMorphs[brain.species].Add(prefab);
               }
               else
               {
                  if(!speciesMorphsBabies.ContainsKey(brain.species))
                     speciesMorphsBabies.Add(brain.species, new List<KPrefabID>(4));

                  speciesMorphsBabies[brain.species].Add(prefab);
               }
            }
         }
      }

      public static void CacheElementsAggregateStates() {
         otherAggregateStates = new Dictionary<SimHashes, List<Element>>(ElementLoader.elements.Count);

         foreach(Element element in ElementLoader.elements)
         {
            if(element == null)
               continue;

            List<Element> otherStates = new List<Element>(3);
            otherStates.Add(element);

            if(element.id == SimHashes.Vacuum)
            {
               otherAggregateStates.Add(element.id, otherStates);
               continue;
            }

            if(element.IsSolid)
            {
               Element highTrans = element.highTempTransition;
               if(highTrans != null && highTrans.lowTempTransitionTarget == element.id)
               {
                  otherStates.Add(highTrans);

                  if(highTrans.highTempTransition != null && highTrans.highTempTransition.lowTempTransitionTarget == highTrans.id)
                  {
                     otherStates.Add(highTrans.highTempTransition);
                  }
               }
            }
            else if(element.IsLiquid)
            {
               if(element.lowTempTransition != null && element.lowTempTransition.highTempTransitionTarget == element.id)
               {
                  otherStates.Add(element.lowTempTransition);
               }
               if(element.highTempTransition != null && element.highTempTransition.lowTempTransitionTarget == element.id)
               {
                  otherStates.Add(element.highTempTransition);
               }
            }
            else if(element.IsGas)
            {
               Element lowTrans = element.lowTempTransition;
               if(lowTrans != null && lowTrans.highTempTransitionTarget == element.id)
               {
                  otherStates.Add(lowTrans);

                  if(lowTrans.lowTempTransition != null && lowTrans.lowTempTransition.highTempTransitionTarget == lowTrans.id)
                  {
                     otherStates.Add(lowTrans.lowTempTransition);
                  }
               }
            }

            otherAggregateStates.Add(element.id, otherStates);
         }
      }

      public static void CacheElementsSublimationElement() {
         sublimationElement = new Dictionary<SimHashes, SimHashes>(ElementLoader.elements.Count);

         foreach(Element element in ElementLoader.elements)
         {
            if(element == null)
               continue;

            if(element.id == SimHashes.Vacuum || element.id == SimHashes.Void)
            {
               sublimationElement.Add(element.id, default);
               continue;
            }

            if(element.sublimateId != default)
            {
               sublimationElement.Add(element.id, element.sublimateId);
               continue;
            }

            Sublimates sublimates = Assets.GetPrefab(element.id.CreateTag())?.GetComponent<Sublimates>();
            if(sublimates != null)
            {
               sublimationElement.Add(element.id, sublimates.info.sublimatedElement);
               continue;
            }

            sublimationElement.Add(element.id, default);
         }
      }

      public static void CacheElementsTransitionElements() {
         transitionElements = new Dictionary<SimHashes, List<SimHashes>>(ElementLoader.elements.Count);

         foreach(Element element in ElementLoader.elements)
         {
            if(element == null)
               continue;

            List<SimHashes> elements = new List<SimHashes>(2);

            if(element.id == SimHashes.Vacuum)
            {
               transitionElements.Add(element.id, elements);
               continue;
            }

            if(element.highTempTransition != null && element.highTempTransitionTarget != element.id && element.highTempTransition.lowTempTransitionTarget != element.id)
               elements.Add(element.highTempTransitionTarget);

            if(element.lowTempTransition != null && element.lowTempTransitionTarget != element.id && element.lowTempTransition.highTempTransitionTarget != element.id)
               elements.Add(element.lowTempTransitionTarget);

            transitionElements.Add(element.id, elements);
         }
      }

      public static void CacheElementsTransitionOreElements() {
         transitionOreElements = new Dictionary<SimHashes, List<SimHashes>>(ElementLoader.elements.Count);

         foreach(Element element in ElementLoader.elements)
         {
            if(element == null)
               continue;

            List<SimHashes> elements = new List<SimHashes>(2);

            if(element.id == SimHashes.Vacuum)
            {
               transitionOreElements.Add(element.id, elements);
               continue;
            }

            if(element.highTempTransitionOreID != default && element.highTempTransitionOreID != SimHashes.Vacuum && element.highTempTransitionOreID != element.id)
               elements.Add(element.highTempTransitionOreID);

            if(element.lowTempTransitionOreID != default && element.lowTempTransitionOreID != SimHashes.Vacuum && element.lowTempTransitionOreID != element.id)
               elements.Add(element.lowTempTransitionOreID);

            transitionOreElements.Add(element.id, elements);
         }
      }

      public static void CacheBuildingsHighlightOptions() {
         buildingsCachedOptions = new Dictionary<Tag, HighlightOptions>(Assets.BuildingDefs.Count);

         foreach(var buildingID in Assets.Prefabs)
         {
            if(buildingID == null || buildingID.gameObject == null)
               continue;

            if(ObjectProperties.GetObjectType(buildingID) != ObjectType.BUILDING)
               continue;

            HighlightOptions options = HighlightOptions.NONE;

            if(ObjectProperties.BuildingHasConsumables(buildingID))
               options |= HighlightOptions.CONSUMABLES;

            if(ObjectProperties.BuildingHasProduce(buildingID))
               options |= HighlightOptions.PRODUCE;

            if(ObjectProperties.BuildingHasBuildingMaterial(buildingID.gameObject))
               options |= HighlightOptions.BUILDINGMATERIAL;

            buildingsCachedOptions.Add(buildingID.PrefabTag, options);
         }
      }

      public static void CachePlantsHighlightOptions() {
         plantsCachedOptions = new Dictionary<Tag, HighlightOptions>();

         foreach(var plantID in Assets.Prefabs)
         {
            if(plantID == null || plantID.gameObject == null)
               continue;

            if((!(plantID.HasTag(GameTags.Plant) || plantID.TryGetComponent(out Uprootable _)/*Wheezewort's prefab doesn't have the GameTags.Plant tag*/)) ||
               ObjectProperties.GetObjectType(plantID) != ObjectType.PLANTORSEED)
               continue;

            HighlightOptions options = HighlightOptions.NONE;

            if(ObjectProperties.PlantHasConsumables(plantID.gameObject))
               options |= HighlightOptions.CONSUMABLES;

            if(ObjectProperties.PlantHasProduce(plantID.gameObject))
               options |= HighlightOptions.PRODUCE;

            if(ObjectProperties.PlantHasExactCopies(plantID.gameObject))
               options |= HighlightOptions.EXACTCOPIES;

            plantsCachedOptions.Add(plantID.PrefabTag, options);
         }
      }

      public static void CacheCrittersHighlightOptions() {
         crittersCachedOptions = new Dictionary<Tag, HighlightOptions>();

         foreach(var critterID in Assets.Prefabs)
         {
            if(critterID == null || critterID.gameObject == null)
               continue;

            if(!critterID.HasTag(GameTags.Creature) || ObjectProperties.GetObjectType(critterID) != ObjectType.CRITTEROREGG)
               continue;

            HighlightOptions options = HighlightOptions.NONE;

            if(ObjectProperties.CritterHasConsiderOption1(critterID.gameObject))
            {
               options |= HighlightOptions.CONSIDEROPTION1;
            }

            if(ObjectProperties.CritterHasConsumables(critterID.gameObject))
            {
               options |= HighlightOptions.CONSUMABLES;
            }

            if(ObjectProperties.CritterHasProduce(critterID.gameObject))
            {
               options |= HighlightOptions.PRODUCE;
            }

            crittersCachedOptions.Add(critterID.PrefabTag, options);
         }
      }
   }
}
