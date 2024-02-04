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


      public ObjectProperties(int cell) : this(Grid.Element[cell]) { }
      public ObjectProperties(Element cellElement) {
         objectType = ObjectType.ELEMENT;

         highlightOptions = HighlightOptions.NONE;
         element = cellElement;
         prefabID = Tag.Invalid;

         info = null;

         CASE_ELEMENT();

         if(prefabID == Tag.Invalid)
            throw new Exception(Main.debugPrefix + $"The {nameof(prefabID)} was not given a value in ObjectType {objectType}'s case");
      }
      public ObjectProperties(GameObject obj) {
         objectType = GetObjectType(obj);

         if(objectType == ObjectType.NOTVALID)
            throw new ArgumentException(Main.debugPrefix + $"GameObject {obj.name} is not a valid object for generating its {nameof(ObjectProperties)}");

         highlightOptions = HighlightOptions.NONE;
         element = obj.GetComponent<PrimaryElement>().Element;
         prefabID = Tag.Invalid;

         info = null;

         //---------------Generating HighlightOptions, AdditionalInfo & prefabID---------------DOWN
         switch(objectType)
         {
            case ObjectType.ELEMENT:
               CASE_ELEMENT();
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

            case ObjectType.OILWELL:
               CASE_OILWELL(obj);
               break;

            default:
               throw new Exception(Main.debugPrefix + $"Missing case for ObjectType {objectType}");
         }
         //---------------Generating HighlightOptions, AdditionalInfo & prefabID---------------UP

         if(prefabID == Tag.Invalid)
            throw new Exception(Main.debugPrefix + $"The {nameof(prefabID)} was not given a value in ObjectType {objectType}'s case");
      }



      private void CASE_ELEMENT() {
         CaseElement_HighlightOptions();

         prefabID = element.id.CreateTag();
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

      private void CASE_ITEM(GameObject obj) {
         ItemInfo itemInfo = new ItemInfo();

         GameObject itemPrefab;
         if(obj.HasTag(GameTags.Plant))
         {
            // pseudo-plants:
            if(obj.TryGetComponent(out SeedProducer seedProducer))
            {
               itemPrefab = Assets.GetPrefab(seedProducer.seedInfo.seedId);
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

         itemInfo.itemID = itemPrefab.GetComponent<KPrefabID>()?.PrefabTag ?? Tag.Invalid;
         itemInfo.itemGO = itemPrefab;

         info = itemInfo;

         CaseItem_HighlightOptions();

         prefabID = itemInfo.itemID;
      }
      private void CaseItem_HighlightOptions() {
         highlightOptions |= HighlightOptions.CONSUMERS | HighlightOptions.PRODUCERS | HighlightOptions.COPIES;
      }

      private void CASE_BUILDING(GameObject obj) {
         BuildingInfo buildingInfo = new BuildingInfo();

         buildingInfo.buildingID = obj.GetComponent<KPrefabID>()?.PrefabTag ?? Tag.Invalid;
         buildingInfo.buildingGO = obj;

         info = buildingInfo;

         CaseBuilding_HighlightOptions();

         prefabID = buildingInfo.buildingID;
      }
      private void CaseBuilding_HighlightOptions() {
         BuildingInfo buildingInfo = (BuildingInfo)info;

         if(!Utils.buildingsCachedOptions.ContainsKey(buildingInfo.buildingID))
            throw new Exception(Main.debugPrefix + $"Building {buildingInfo.buildingID} was not found in {nameof(Utils.buildingsCachedOptions)} dictionary");

         highlightOptions |= Utils.buildingsCachedOptions[buildingInfo.buildingID];
         highlightOptions |= HighlightOptions.CONSIDEROPTION1 | HighlightOptions.COPIES | HighlightOptions.EXACTCOPIES;
      }
      public static bool BuildingHasConsumables(GameObject building) {
         return building.GetComponent<ComplexFabricator>() != null || building.GetComponent<TreeFilterable>() != null ||
            (building.GetComponent<EnergyGenerator>()?.formula.inputs.Length ?? 0) > 0 ||
            building.GetComponent<ElementConverter>() != null || building.GetComponent<ManualDeliveryKG>() != null ||

            (building.GetComponent<BuildingComplete>()?.isManuallyOperated ?? false) || building.GetComponent<Ownable>() != null ||

            building.GetComponent<SingleEntityReceptacle>() != null || building.GetComponent<Tinkerable>() != null ||
            building.GetComponent<SuitLocker>() != null || building.GetSMI<SpiceGrinder.StatesInstance>() != null ||
            building.GetComponent<TrapTrigger>() != null || building.GetDef<RanchStation.Def>() != null ||
            building.GetComponent<RocketEngine>() != null || building.GetComponent<RocketEngineCluster>() != null;
      }
      public static bool BuildingHasProduce(GameObject building) {
         return building.GetComponent<ComplexFabricator>() != null || building.GetComponent<ElementConverter>() != null ||
            ((building.GetComponent<EnergyGenerator>()?.formula.outputs?.Length ?? 0) > 0) || building.GetComponent<BuildingElementEmitter>() != null ||
            building.GetComponent<TinkerStation>() != null || building.GetComponent<OilWellCap>() != null ||
            building.GetComponent<Toilet>() != null || building.GetComponent<FlushToilet>() != null ||
            building.GetComponent<RocketEngine>() != null || building.GetComponent<RocketEngineCluster>() != null;
      }
      public static bool BuildingHasBuildingMaterial(GameObject building) {
         return building.GetComponent<BuildingComplete>()?.Def.ShowInBuildMenu ?? false;// otherwise this option is irrelevant
      }

      private void CASE_PLANTORSEED(GameObject obj) {
         PlantInfo plantInfo = new PlantInfo();

         if(!obj.TryGetComponent(out PlantableSeed seed))
         {
            string seedID;
            if(obj.TryGetComponent(out TreeBud arborTreeBranch))
            {
               seedID = arborTreeBranch.buddingTrunk.Get().GetComponent<SeedProducer>().seedInfo.seedId;
            }
            else
            {
               seedID = obj.GetComponent<SeedProducer>().seedInfo.seedId;
            }

            seed = Assets.GetPrefab(seedID).GetComponent<PlantableSeed>();
         }
         plantInfo.plantID = seed.PlantID;
         plantInfo.plantPrefab = Assets.GetPrefab(plantInfo.plantID);
         plantInfo.seedID = plantInfo.plantPrefab.GetComponent<SeedProducer>().seedInfo.seedId;

         info = plantInfo;

         CasePlant_HighlightOptions();

         prefabID = plantInfo.plantID;
      }
      private void CasePlant_HighlightOptions() {
         PlantInfo plantInfo = (PlantInfo)info;

         if(!Utils.plantsCachedOptions.ContainsKey(plantInfo.plantID))
            throw new Exception(Main.debugPrefix + $"Plant {plantInfo.plantID} was not found in {nameof(Utils.plantsCachedOptions)} dictionary");

         highlightOptions |= Utils.plantsCachedOptions[plantInfo.plantID];
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

      private void CASE_CRITTEROREGG(GameObject obj) {
         CritterInfo critterInfo = new CritterInfo();

         GameObject critterPrefab;
         if(obj.HasTag(GameTags.Egg))
         {
            GameObject babyGO = Assets.GetPrefab(obj.GetDef<IncubationMonitor.Def>().spawnedCreature);

            if(babyGO.GetDef<BabyMonitor.Def>() != null)
               critterPrefab = Assets.GetPrefab(babyGO.GetDef<BabyMonitor.Def>().adultPrefab);
            else
               critterPrefab = babyGO;
         }
         else if(obj.GetDef<BabyMonitor.Def>() != null)
         {
            critterPrefab = Assets.GetPrefab(obj.GetDef<BabyMonitor.Def>().adultPrefab);
         }
         else
         {
            critterPrefab = obj;
         }
         critterInfo.critterID = critterPrefab.GetComponent<KPrefabID>()?.PrefabTag ?? Tag.Invalid;
         critterInfo.eggID = critterPrefab.GetDef<FertilityMonitor.Def>()?.eggPrefab ?? default;
         critterInfo.species = critterPrefab.GetComponent<CreatureBrain>()?.species ?? Utils.DefaultSpecies;
         critterInfo.adultPrefab = critterPrefab;

         info = critterInfo;

         CaseCritter_HighlightOptions();

         prefabID = critterInfo.critterID;
      }
      private void CaseCritter_HighlightOptions() {
         CritterInfo critterInfo = (CritterInfo)info;

         if(!Utils.crittersCachedOptions.ContainsKey(critterInfo.critterID))
            throw new Exception(Main.debugPrefix + $"Critter {critterInfo.critterID} was not found in {nameof(Utils.crittersCachedOptions)} dictionary");

         highlightOptions |= Utils.crittersCachedOptions[critterInfo.critterID];
         highlightOptions |= HighlightOptions.CONSUMERS | HighlightOptions.COPIES;
      }
      public static bool CritterHasConsiderOption1(GameObject critter) {
         Tag species = critter.GetComponent<CreatureBrain>()?.species ?? Tag.Invalid;
         return species != Tag.Invalid && Main.speciesMorphs.ContainsKey(species) && Main.speciesMorphs[species].Count > 1;
      }
      public static bool CritterHasConsumables(GameObject critter) {
         if(ObjectType.CRITTEROREGG.ConsiderOption1())// if considerCritterMorph
         {
            if(CheckMorph(critter))
               return true;
         }
         else
         {
            Tag species = critter.GetComponent<CreatureBrain>()?.species ?? Tag.Invalid;
            if(Main.speciesMorphs.ContainsKey(species))
            {
               foreach(GameObject morph in Main.speciesMorphs[species])
               {
                  if(CheckMorph(morph))
                     return true;
               }
            }
         }

         return false;

         bool CheckMorph(GameObject morph) {
            if(((morph.GetDef<CreatureCalorieMonitor.Def>()?.diet?.consumedTags?.Count ?? 0) > 0) || (morph.GetDef<DrinkMilkMonitor.Def>()?.consumesMilk ?? false))
            {
               return true;
            }

            return false;
         }
      }
      public static bool CritterHasProduce(GameObject critter) {
         if(ObjectType.CRITTEROREGG.ConsiderOption1())// if considerCritterMorph
         {
            if(CheckMorph(critter))
               return true;
         }
         else
         {
            Tag species = critter.GetComponent<CreatureBrain>()?.species ?? Tag.Invalid;
            if(Main.speciesMorphs.ContainsKey(species))
            {
               foreach(GameObject morph in Main.speciesMorphs[species])
               {
                  if(CheckMorph(morph))
                     return true;
               }
            }
         }

         return false;

         bool CheckMorph(GameObject morph) {
            if(((morph.GetDef<CreatureCalorieMonitor.Def>()?.diet?.producedTags?.Count ?? 0) > 0) || morph.GetDef<ScaleGrowthMonitor.Def>() != null)
            {
               return true;
            }

            return false;
         }
      }

      private void CASE_DUPLICANT(GameObject obj) {
         DuplicantInfo duplicantInfo = new DuplicantInfo();

         duplicantInfo.duplicantInfo = obj.GetComponent<MinionResume>();

         info = duplicantInfo;

         CaseDuplicant_HighlightOptions();

         prefabID = duplicantInfo.duplicantInfo.PrefabID();
      }
      private void CaseDuplicant_HighlightOptions() {
         highlightOptions |= HighlightOptions.CONSUMERS | HighlightOptions.CONSUMABLES | HighlightOptions.PRODUCE | HighlightOptions.COPIES;
      }

      private void CASE_GEYSER(GameObject obj) {
         GeyserInfo geyserInfo = new GeyserInfo();

         geyserInfo.outputElement = obj.GetComponent<Geyser>().emitter.outputElement.elementHash;

         info = geyserInfo;

         CaseGeyser_HighlightOptions();

         prefabID = obj.PrefabID();
      }
      private void CaseGeyser_HighlightOptions() {
         highlightOptions |= HighlightOptions.PRODUCE | HighlightOptions.COPIES | HighlightOptions.EXACTCOPIES;
      }

      private void CASE_ROBOT(GameObject obj) {
         CaseRobot_HighlightOptions();

         prefabID = obj.PrefabID();
      }
      private void CaseRobot_HighlightOptions() {
         highlightOptions |= HighlightOptions.PRODUCERS | HighlightOptions.COPIES;
      }

      private void CASE_OILWELL(GameObject obj) {
         CaseOilwell_HighlightOptions();

         prefabID = obj.PrefabID();
      }
      private void CaseOilwell_HighlightOptions() {
         highlightOptions |= HighlightOptions.CONSUMABLES | HighlightOptions.PRODUCE | HighlightOptions.COPIES;
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


      public static ObjectType GetObjectType(GameObject obj) {
         KPrefabID prefabID = obj.GetComponent<KPrefabID>();

         if(prefabID.TryGetComponent(out BuildingComplete _) ||
            (prefabID.TryGetComponent(out Demolishable _) || prefabID.TryGetComponent(out LoreBearer _))/*<- Gravitas building*/)
         {
            return ObjectType.BUILDING;
         }
         if(prefabID.GetComponent<ElementChunk>() != null)
         {
            return ObjectType.ELEMENT;
         }
         if(prefabID.HasTag(GameTags.Seed))
         {
            return ObjectType.PLANTORSEED;
         }
         if(prefabID.HasTag(GameTags.Plant) || prefabID.TryGetComponent(out Uprootable _))// Wheezewort's prefab doesn't have the GameTags.Plant tag
         {
            //----------Pseudo-plants----------DOWN
            if(prefabID.TryGetComponent(out SeedProducer seedProducer))
            {
               if(Assets.GetPrefab(seedProducer.seedInfo.seedId)?.GetComponent<PlantableSeed>() == null)
                  return ObjectType.ITEM;// f.e. muckroot
            }
            //----------Pseudo-plants----------UP
            return ObjectType.PLANTORSEED;
         }
         if(prefabID.HasTag(GameTags.Robot))
         {
            return ObjectType.ROBOT;
         }
         if(prefabID.HasTag(GameTags.Creature) || prefabID.HasTag(GameTags.Egg))
         {
            return ObjectType.CRITTEROREGG;
         }
         if(prefabID.HasTag(GameTags.Minion))
         {
            return ObjectType.DUPLICANT;
         }
         if(prefabID.HasTag(GameTags.Pickupable))
         {
            return ObjectType.ITEM;// things above have Pickupable tag too
         }
         if(prefabID.HasTag(GameTags.GeyserFeature))
         {
            return ObjectType.GEYSER;
         }
         if(prefabID.HasTag(GameTags.OilWell))
         {
            return ObjectType.OILWELL;
         }
         return ObjectType.ELEMENT;// assuming that the object has the PrimaryElement component(which it should)
      }

      public object ObjectForShouldHighlight() {
         switch(objectType)
         {
            case ObjectType.BUILDING:
               if((highlightOptions & (HighlightOptions.CONSUMABLES | HighlightOptions.PRODUCE)) == 0 ||
                  (((highlightOptions & HighlightOptions.CONSUMABLES) == 0) || Main.highlightOption != HighlightOptions.CONSUMABLES.Reverse()) &&
                  (((highlightOptions & HighlightOptions.PRODUCE) == 0) || Main.highlightOption != HighlightOptions.PRODUCE.Reverse()))
               {
                  return prefabID.hash + ((long)element.id << 32);
               }
               else
               {
                  return null;// force to calculate shouldHighlight for this building
               }

            case ObjectType.DUPLICANT:
               return null;// calculating shouldHighlight for each duplicant individually

            case ObjectType.GEYSER:
               return ((GeyserInfo)info).outputElement;

            default:
               return prefabID.hash;
         }
      }

      public string StringRepresentation() {
         return Utils.GetMyString(typeof(MYSTRINGS.UI.OBJECTTYPES), objectType.ToString());
      }


      public interface AdditionalInfo { }
      public struct ItemInfo : AdditionalInfo {
         public Tag itemID;
         public GameObject itemGO;
      }
      public struct BuildingInfo : AdditionalInfo {
         public Tag buildingID;
         public GameObject buildingGO;
      }
      public struct PlantInfo : AdditionalInfo {
         public Tag plantID;
         public string seedID;
         public GameObject plantPrefab;
      }
      public struct GeyserInfo : AdditionalInfo {
         public SimHashes outputElement;
      }
      public struct CritterInfo : AdditionalInfo {
         public Tag critterID;
         public Tag eggID;
         public Tag species;
         public GameObject adultPrefab;
      }
      public struct DuplicantInfo : AdditionalInfo {
         public MinionResume duplicantInfo;
      }
   }
}
