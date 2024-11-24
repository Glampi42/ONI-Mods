using HighlightOverlay.Enums;
using HighlightOverlay.Strings;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace HighlightOverlay.Structs {
   public struct ObjectProperties {
      public ObjectType objectType;
      public HighlightOptions highlightOptions;

      public Element element;
      public Tag prefabID;

      public AdditionalInfo info;


      public ObjectProperties(Element element) {
         objectType = ObjectType.ELEMENT;

         highlightOptions = HighlightOptions.NONE;
         this.element = element;
         prefabID = Tag.Invalid;

         info = null;

         CASE_ELEMENT(element.id.CreateTag());

         if(prefabID == Tag.Invalid)
            throw new Exception(Main.debugPrefix + $"The {nameof(prefabID)} was not given a value in ObjectType {objectType}'s case");
      }
      public ObjectProperties(KPrefabID obj, ObjectType objType = ObjectType.NOTVALID) {
         objectType = objType == ObjectType.NOTVALID ? GetObjectType(obj) : objType;

         highlightOptions = HighlightOptions.NONE;
         element = obj.GetComponent<PrimaryElement>().Element;
         prefabID = Tag.Invalid;

         info = null;

         //---------------Generating HighlightOptions, AdditionalInfo & prefabID---------------DOWN
         switch(objectType)
         {
            case ObjectType.ELEMENT:
               CASE_ELEMENT(obj.PrefabTag);
               break;

            case ObjectType.ITEM:
               CASE_ITEM(obj);
               break;

            case ObjectType.BUILDING:
               CASE_BUILDING(obj);
               break;

            case ObjectType.PLANTORSEED:
               CASE_PLANTORSEED(obj);
               break;

            case ObjectType.CRITTEROREGG:
               CASE_CRITTEROREGG(obj);
               break;

            case ObjectType.DUPLICANT:
               CASE_DUPLICANT(obj);
               break;

            case ObjectType.GEYSER:
               CASE_GEYSER(obj);
               break;

            case ObjectType.ROBOT:
               CASE_ROBOT(obj);
               break;

            case ObjectType.OILWELL:
               CASE_OILWELL(obj);
               break;

            case ObjectType.SAPTREE:
               CASE_SAPTREE(obj);
               break;

            case ObjectType.RADBOLT:
               CASE_RADBOLT(obj);
               break;

            default:
               throw new Exception(Main.debugPrefix + $"Missing case for ObjectType {objectType}");
         }
         //---------------Generating HighlightOptions, AdditionalInfo & prefabID---------------UP

         if(prefabID == Tag.Invalid)
            throw new Exception(Main.debugPrefix + $"The {nameof(prefabID)} was not given a value in ObjectType {objectType}'s case");
      }



      private void CASE_ELEMENT(Tag prefabID) {
         CaseElement_HighlightOptions();

         this.prefabID = prefabID;
      }
      private void CaseElement_HighlightOptions() {
         if(ObjectType.ELEMENT.ConsiderOption1())// if considerAggregateState
         {
            if(Utils.ElementSublimates(element) || Utils.ElementTransitsIntoOther(element))
            {
               highlightOptions |= HighlightOptions.PRODUCE;
            }
         }
         else
         {
            foreach(Element elem in Utils.OtherAggregateStates(element))
            {
               if(Utils.ElementSublimates(elem) || Utils.ElementTransitsIntoOther(elem))
               {
                  highlightOptions |= HighlightOptions.PRODUCE;
                  break;
               }
            }
         }
         highlightOptions |= HighlightOptions.CONSIDEROPTION1 | HighlightOptions.CONSUMERS | HighlightOptions.PRODUCERS | HighlightOptions.COPIES;
      }

      private void CASE_ITEM(KPrefabID obj) {
         ItemInfo itemInfo = new ItemInfo();

         KPrefabID itemPrefab;
         if(obj.HasTag(GameTags.Plant))
         {
            // pseudo-plants:
            if(obj.TryGetComponent(out SeedProducer seedProducer))
            {
               itemPrefab = Assets.PrefabsByTag.GetOrDefault(seedProducer.seedInfo.seedId);
            }
            else
            {
               itemPrefab = obj;
            }
         }
         else
         {
            itemPrefab = obj;
         }

         itemInfo.itemID = itemPrefab?.PrefabTag ?? Tag.Invalid;
         itemInfo.itemGO = itemPrefab;

         info = itemInfo;

         CaseItem_HighlightOptions();

         prefabID = itemInfo.itemID;
      }
      private void CaseItem_HighlightOptions() {
         highlightOptions |= HighlightOptions.CONSUMERS | HighlightOptions.PRODUCERS | HighlightOptions.COPIES;
      }

      private void CASE_BUILDING(KPrefabID obj) {
         BuildingInfo buildingInfo = new BuildingInfo();

         buildingInfo.buildingID = obj.PrefabTag;
         buildingInfo.buildingGO = obj;

         info = buildingInfo;

         CaseBuilding_HighlightOptions();

         prefabID = buildingInfo.buildingID;
      }
      private void CaseBuilding_HighlightOptions() {
         BuildingInfo buildingInfo = (BuildingInfo)info;

         if(!Main.buildingsCachedOptions.ContainsKey(buildingInfo.buildingID))
            throw new Exception(Main.debugPrefix + $"Building {buildingInfo.buildingID} was not found in {nameof(Main.buildingsCachedOptions)} dictionary");

         highlightOptions |= Main.buildingsCachedOptions[buildingInfo.buildingID];
         highlightOptions |= HighlightOptions.CONSIDEROPTION1 | HighlightOptions.COPIES | HighlightOptions.EXACTCOPIES;
      }
      public static bool BuildingHasConsumables(KPrefabID building) {
         return building.GetComponent<ComplexFabricator>() != null || IsStorageBuilding(building) ||
            (building.GetComponent<EnergyGenerator>()?.formula.inputs.Length ?? 0) > 0 ||
            building.GetComponent<ElementConverter>() != null || building.GetComponent<ManualDeliveryKG>() != null ||

            (building.GetComponent<BuildingComplete>()?.isManuallyOperated ?? false) || building.GetComponent<Ownable>() != null ||

            building.GetComponent<SingleEntityReceptacle>() != null || building.GetComponent<Tinkerable>() != null ||
            building.GetComponent<SuitLocker>() != null || building.GetSMI<SpiceGrinder.StatesInstance>() != null ||
            building.GetComponent<TrapTrigger>() != null || building.GetDef<RanchStation.Def>() != null ||
            building.GetComponent<RocketEngine>() != null || building.GetComponent<RocketEngineCluster>() != null ||

            IsRadboltConsumer(building);
      }
      public static bool IsStorageBuilding(KPrefabID building) {// all buildings that have the TreeFilterable component(their prefab may not have it however)
         return building.TryGetComponent(out TreeFilterable _) || building.TryGetComponent(out StorageLocker _) || building.GetComponent<TinkerStation>() != null;
      }
      public static bool IsRadboltConsumer(KPrefabID building) {
         return building.TryGetComponent(out HighEnergyParticlePort port) && port.particleInputEnabled && !building.TryGetComponent(out HighEnergyParticleRedirector _);
      }
      public static bool BuildingHasProduce(KPrefabID building) {
         return building.GetComponent<ComplexFabricator>() != null || building.GetComponent<ElementConverter>() != null ||
            ((building.GetComponent<EnergyGenerator>()?.formula.outputs?.Length ?? 0) > 0) || building.GetComponent<BuildingElementEmitter>() != null ||
            building.GetComponent<TinkerStation>() != null || building.GetComponent<OilWellCap>() != null ||
            building.GetComponent<Toilet>() != null || building.GetComponent<FlushToilet>() != null ||
            building.GetComponent<RocketEngine>() != null || building.GetComponent<RocketEngineCluster>() != null ||

            building.PrefabTag == SweepBotStationConfig.ID || building.PrefabTag == ScoutLanderConfig.ID || building.PrefabTag == ScoutModuleConfig.ID || building.PrefabTag == MorbRoverMakerConfig.ID ||

            IsRadboltProducer(building);
      }
      public static bool IsRadboltProducer(KPrefabID building) {
         return building.TryGetComponent(out HighEnergyParticlePort port) && port.particleOutputEnabled && !building.TryGetComponent(out HighEnergyParticleRedirector _);
      }
      public static bool BuildingHasBuildingMaterial(GameObject building) {
         return building.GetComponent<BuildingComplete>()?.Def.ShowInBuildMenu ?? false;// otherwise this option is irrelevant
      }

      private void CASE_PLANTORSEED(KPrefabID obj) {
         PlantInfo plantInfo = new PlantInfo();

         string seedID = "";
         if(!obj.TryGetComponent(out PlantableSeed seed))
         {
            if(obj.TryGetComponent(out TreeBud _))
            {
               seedID = ForestTreeConfig.SEED_ID;
            }
            else
            {
               seedID = obj.GetComponent<SeedProducer>()?.seedInfo.seedId ?? "";
            }

            if(seedID != "")
               seed = Assets.GetPrefab(seedID).GetComponent<PlantableSeed>();
         }
         plantInfo.plantID = seed != default ? seed.PlantID : obj.PrefabTag;
         plantInfo.plantPrefab = Assets.PrefabsByTag.GetOrDefault(plantInfo.plantID);
         plantInfo.seedID = seedID;

         info = plantInfo;

         CasePlant_HighlightOptions();

         prefabID = plantInfo.plantID;
      }
      private void CasePlant_HighlightOptions() {
         PlantInfo plantInfo = (PlantInfo)info;

         if(!Main.plantsCachedOptions.ContainsKey(plantInfo.plantID))
            throw new Exception(Main.debugPrefix + $"Plant {plantInfo.plantID} was not found in {nameof(Main.plantsCachedOptions)} dictionary");

         highlightOptions |= Main.plantsCachedOptions[plantInfo.plantID];
         highlightOptions |= HighlightOptions.CONSUMERS | HighlightOptions.COPIES;
      }
      public static bool PlantHasConsumables(GameObject plant) {
         return plant.GetDef<IrrigationMonitor.Def>() != null || plant.GetDef<FertilizationMonitor.Def>() != null ||
         plant.GetComponent<ElementConverter>() != null || plant.GetComponent<Tinkerable>() != null ||
            plant.GetComponent<CritterTrapPlant>() != null;
      }
      public static bool PlantHasProduce(GameObject plant) {
         return plant.GetComponent<Crop>() != null || plant.GetComponent<ElementConverter>() != null ||
            plant.GetComponent<CritterTrapPlant>() != null;
      }
      public static bool PlantHasExactCopies(GameObject plant) {
         return plant.GetComponent<MutantPlant>() != null;
      }

      private void CASE_CRITTEROREGG(KPrefabID obj) {
         CritterInfo critterInfo = new CritterInfo();

         KPrefabID critterPrefab;
         BabyMonitor.Def monitorDef;
         if(obj.HasTag(GameTags.Egg))
         {
            KPrefabID babyGO = Assets.PrefabsByTag.GetOrDefault(obj.GetDef<IncubationMonitor.Def>()?.spawnedCreature ?? Tag.Invalid);

            if((monitorDef = babyGO.GetDef<BabyMonitor.Def>()) != null)
               critterPrefab = Assets.PrefabsByTag.GetOrDefault(monitorDef.adultPrefab);
            else
               critterPrefab = babyGO;
         }
         else if((monitorDef = obj.GetDef<BabyMonitor.Def>()) != null)
         {
            critterPrefab = Assets.PrefabsByTag.GetOrDefault(monitorDef.adultPrefab);
         }
         else
         {
            critterPrefab = obj;
         }
         critterInfo.critterID = critterPrefab?.PrefabTag ?? Tag.Invalid;
         critterInfo.eggID = critterPrefab.GetDef<FertilityMonitor.Def>()?.eggPrefab ?? default;
         critterInfo.species = critterPrefab.GetComponent<CreatureBrain>()?.species ?? Utils.DefaultSpecies;
         critterInfo.adultPrefab = critterPrefab;

         info = critterInfo;

         CaseCritter_HighlightOptions();

         prefabID = critterInfo.critterID;
      }
      private void CaseCritter_HighlightOptions() {
         CritterInfo critterInfo = (CritterInfo)info;

         if(!Main.crittersCachedOptions.ContainsKey(critterInfo.critterID))
            throw new Exception(Main.debugPrefix + $"Critter {critterInfo.critterID} was not found in {nameof(Main.crittersCachedOptions)} dictionary");

         if(ObjectType.CRITTEROREGG.ConsiderOption1())// if considerCritterMorph
         {
            highlightOptions |= Main.crittersCachedOptions[critterInfo.critterID];
         }
         else if(Main.speciesMorphs.ContainsKey(critterInfo.species))
         {
            foreach(KPrefabID morph in Main.speciesMorphs[critterInfo.species])
            {
               highlightOptions |= Main.crittersCachedOptions[morph.PrefabTag];
            }
         }
         highlightOptions |= HighlightOptions.CONSUMERS | HighlightOptions.COPIES;
      }
      public static bool CritterHasConsiderOption1(GameObject critter) {
         Tag species = critter.GetComponent<CreatureBrain>()?.species ?? Tag.Invalid;
         return species != Tag.Invalid && Main.speciesMorphs.ContainsKey(species) && Main.speciesMorphs[species].Count > 1;
      }
      public static bool CritterHasConsumables(GameObject critter) {
         return ((critter.GetDef<CreatureCalorieMonitor.Def>()?.diet?.consumedTags?.Count ?? 0) > 0) || (critter.GetDef<DrinkMilkMonitor.Def>()?.consumesMilk ?? false);
      }
      public static bool CritterHasProduce(GameObject critter) {
         return ((critter.GetDef<CreatureCalorieMonitor.Def>()?.diet?.producedTags?.Count ?? 0) > 0) || critter.GetDef<ScaleGrowthMonitor.Def>() != null;
      }

      private void CASE_DUPLICANT(KPrefabID obj) {
         DuplicantInfo duplicantInfo = new DuplicantInfo();

         duplicantInfo.duplicantInfo = obj.GetComponent<MinionResume>();

         info = duplicantInfo;

         CaseDuplicant_HighlightOptions();

         prefabID = obj.PrefabTag;
      }
      private void CaseDuplicant_HighlightOptions() {
         highlightOptions |= HighlightOptions.CONSUMERS | HighlightOptions.CONSUMABLES | HighlightOptions.PRODUCE | HighlightOptions.COPIES;
      }

      private void CASE_GEYSER(KPrefabID obj) {
         GeyserInfo geyserInfo = new GeyserInfo();

         geyserInfo.geyser = obj.GetComponent<Geyser>();

         info = geyserInfo;

         CaseGeyser_HighlightOptions();

         prefabID = obj.PrefabTag;
      }
      private void CaseGeyser_HighlightOptions() {
         highlightOptions |= HighlightOptions.PRODUCE | HighlightOptions.COPIES | HighlightOptions.EXACTCOPIES;
      }

      private void CASE_ROBOT(KPrefabID obj) {
         CaseRobot_HighlightOptions();

         prefabID = obj.PrefabTag;
      }
      private void CaseRobot_HighlightOptions() {
         highlightOptions |= HighlightOptions.PRODUCERS | HighlightOptions.COPIES;
      }

      private void CASE_OILWELL(KPrefabID obj) {
         CaseOilwell_HighlightOptions();

         prefabID = obj.PrefabTag;
      }
      private void CaseOilwell_HighlightOptions() {
         highlightOptions |= HighlightOptions.CONSUMABLES | HighlightOptions.PRODUCE | HighlightOptions.COPIES;
      }

      private void CASE_SAPTREE(KPrefabID obj) {
         CaseSaptree_HighlightOptions();

         prefabID = obj.PrefabTag;
      }
      private void CaseSaptree_HighlightOptions() {
         highlightOptions |= HighlightOptions.CONSUMABLES | HighlightOptions.PRODUCE;
      }

      private void CASE_RADBOLT(KPrefabID obj) {
         CaseRadbolt_HighlightOptions();

         prefabID = obj.PrefabTag;
      }
      private void CaseRadbolt_HighlightOptions() {
         highlightOptions |= HighlightOptions.CONSUMERS | HighlightOptions.PRODUCERS | HighlightOptions.COPIES;
      }




      public bool TryUpdateHighlightOptionsForConsiderOptionToggle() {
         if(objectType == ObjectType.ELEMENT)
         {
            highlightOptions = HighlightOptions.NONE;
            CaseElement_HighlightOptions();
            return true;
         }
         if(objectType == ObjectType.CRITTEROREGG)
         {
            highlightOptions = HighlightOptions.NONE;
            CaseCritter_HighlightOptions();
            return true;
         }

         return false;
      }


      public static ObjectType GetObjectType(KPrefabID obj) {
         if(obj.TryGetComponent(out BuildingComplete _) ||
            (obj.TryGetComponent(out Demolishable _) || obj.TryGetComponent(out LoreBearer _))/*<- Gravitas building*/)
         {
            return ObjectType.BUILDING;
         }
         if(obj.GetComponent<ElementChunk>() != null)
         {
            return ObjectType.ELEMENT;
         }
         if(obj.HasTag(GameTags.Seed) || obj.HasTag(GameTags.CropSeed))// Sleet Wheat Grain doesn't have seed tag, but has CropSeed tag
         {
            return ObjectType.PLANTORSEED;
         }
         if(obj.HasTag(GameTags.Plant) || obj.TryGetComponent(out Uprootable _))// Wheezewort's prefab doesn't have the GameTags.Plant tag
         {
            //----------Pseudo-plants----------DOWN
            if(obj.TryGetComponent(out SeedProducer seedProducer))
            {
               if(Assets.GetPrefab(seedProducer.seedInfo.seedId)?.GetComponent<PlantableSeed>() == null)
                  return ObjectType.ITEM;// f.e. muckroot
            }
            //----------Pseudo-plants----------UP
            return ObjectType.PLANTORSEED;
         }
         if(obj.HasTag(GameTags.Robot))
         {
            return ObjectType.ROBOT;
         }
         if(obj.HasTag(GameTags.Creature) || obj.HasTag(GameTags.Egg))
         {
            return ObjectType.CRITTEROREGG;
         }
         if(obj.HasTag(GameTags.BaseMinion))
         {
            return ObjectType.DUPLICANT;
         }
         if(obj.HasTag(GameTags.Pickupable))
         {
            return ObjectType.ITEM;// things above have Pickupable tag too
         }
         if(obj.HasTag(GameTags.GeyserFeature))
         {
            return ObjectType.GEYSER;
         }
         if(obj.HasTag(GameTags.OilWell))
         {
            return ObjectType.OILWELL;
         }
         if(obj.HasTag(SapTreeConfig.ID))
         {
            return ObjectType.SAPTREE;
         }
         if(obj.HasTag(GameTags.HighEnergyParticle))
         {
            return ObjectType.RADBOLT;
         }
         return ObjectType.ELEMENT;// assuming that the object has the PrimaryElement component(which it should)
      }

      /// <summary>
      /// Calculates an object that unifies different objs into groups that will have the same shouldHighlight result. When multiple objs have the same objectForHighlight, then the highlight
      /// will be computed for the first of them; the other objs will inherit the result calculated for the first one. For example, if shouldHighlight was calculated for one sandstone debris
      /// on the map, then there is no need to calculate it again for all other sandstone debris, because the result will be the same.
      /// </summary>
      /// <param name="objectType"></param>
      /// <param name="obj"></param>
      /// <param name="element"></param>
      /// <returns>Returns the objectForHighlight</returns>
      public static object ObjectForShouldHighlight(ObjectType objectType, KPrefabID obj, Element element) {
         if(obj == null)
         {
            if(element != null)
            {
               return element.id;
            }

            return null;
         }

         if(!Main.cachedObjectIDs.ContainsKey(obj.PrefabTag))
            throw new Exception(Main.debugPrefix + $"No cached objectID found for {obj.PrefabTag} inside of {nameof(Main.cachedObjectIDs)}");

         Tag objectID = Main.cachedObjectIDs[obj.PrefabTag];// objectID != prefab tag of the object itself (f.e. objectID for seeds is the prefab tag of their plant)

         switch(objectType)
         {
            case ObjectType.BUILDING:
               if((((Main.buildingsCachedOptions[objectID] & HighlightOptions.CONSUMABLES) == 0) || Main.highlightOption != HighlightOptions.CONSUMABLES.Reverse()) &&
                  (((Main.buildingsCachedOptions[objectID] & HighlightOptions.PRODUCE) == 0) || Main.highlightOption != HighlightOptions.PRODUCE.Reverse()))
               {
                  return objectID.hash + ((long)obj.GetComponent<PrimaryElement>().Element.id << 32);
               }
               else
               {
                  return null;// force to calculate shouldHighlight for this building
               }

            case ObjectType.DUPLICANT:
               return null;// calculating shouldHighlight for each duplicant individually

            default:
               return objectID.hash;
         }
      }

      public string StringRepresentation() {
         return Utils.GetMyString(typeof(MYSTRINGS.UI.OBJECTTYPE), objectType.ToString());
      }


      public interface AdditionalInfo { }
      public struct ItemInfo : AdditionalInfo {
         public Tag itemID;
         public KPrefabID itemGO;
      }
      public struct BuildingInfo : AdditionalInfo {
         public Tag buildingID;
         public KPrefabID buildingGO;
      }
      public struct PlantInfo : AdditionalInfo {
         public Tag plantID;
         public string seedID;
         public KPrefabID plantPrefab;
      }
      public struct GeyserInfo : AdditionalInfo {
         public Geyser geyser;
      }
      public struct CritterInfo : AdditionalInfo {
         public Tag critterID;
         public Tag eggID;
         public Tag species;
         public KPrefabID adultPrefab;
      }
      public struct DuplicantInfo : AdditionalInfo {
         public MinionResume duplicantInfo;
      }
   }
}
