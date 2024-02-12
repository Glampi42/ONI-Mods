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

         CASE_ELEMENT();

         if(prefabID == Tag.Invalid)
            throw new Exception(Main.debugPrefix + $"The {nameof(prefabID)} was not given a value in ObjectType {objectType}'s case");
      }
      public ObjectProperties(PrimaryElement obj, ObjectType objType = ObjectType.NOTVALID) {
         objectType = objType == ObjectType.NOTVALID ? GetObjectType(obj.gameObject) : objType;

         highlightOptions = HighlightOptions.NONE;
         element = obj.Element;
         prefabID = Tag.Invalid;

         info = null;

         //---------------Generating HighlightOptions, AdditionalInfo & prefabID---------------DOWN
         switch(objectType)
         {
            case ObjectType.ELEMENT:
               CASE_ELEMENT();
               break;

            case ObjectType.ITEM:
               CASE_ITEM(obj.gameObject);
               break;

            case ObjectType.BUILDING:
               CASE_BUILDING(obj.gameObject);
               break;

            case ObjectType.PLANTORSEED:
               CASE_PLANTORSEED(obj.gameObject);
               break;

            case ObjectType.CRITTEROREGG:
               CASE_CRITTEROREGG(obj.gameObject);
               break;

            case ObjectType.DUPLICANT:
               CASE_DUPLICANT(obj.gameObject);
               break;

            case ObjectType.GEYSER:
               CASE_GEYSER(obj.gameObject);
               break;

            case ObjectType.ROBOT:
               CASE_ROBOT(obj.gameObject);
               break;

            case ObjectType.OILWELL:
               CASE_OILWELL(obj.gameObject);
               break;

            case ObjectType.SAPTREE:
               CASE_SAPTREE(obj.gameObject);
               break;

            case ObjectType.RADBOLT:
               CASE_RADBOLT(obj.gameObject);
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
            building.GetComponent<RocketEngine>() != null || building.GetComponent<RocketEngineCluster>() != null ||

            IsRadboltConsumer(building);
      }
      public static bool IsRadboltConsumer(GameObject building) {
         return building.TryGetComponent(out HighEnergyParticlePort port) && port.particleInputEnabled && !building.TryGetComponent(out HighEnergyParticleRedirector _);
      }
      public static bool BuildingHasProduce(GameObject building) {
         return building.GetComponent<ComplexFabricator>() != null || building.GetComponent<ElementConverter>() != null ||
            ((building.GetComponent<EnergyGenerator>()?.formula.outputs?.Length ?? 0) > 0) || building.GetComponent<BuildingElementEmitter>() != null ||
            building.GetComponent<TinkerStation>() != null || building.GetComponent<OilWellCap>() != null ||
            building.GetComponent<Toilet>() != null || building.GetComponent<FlushToilet>() != null ||
            building.GetComponent<RocketEngine>() != null || building.GetComponent<RocketEngineCluster>() != null ||

            building.PrefabID() == SweepBotStationConfig.ID || building.PrefabID() == ScoutLanderConfig.ID || building.PrefabID() == ScoutModuleConfig.ID || building.PrefabID() == MorbRoverMakerConfig.ID ||

            IsRadboltProducer(building);
      }
      public static bool IsRadboltProducer(GameObject building) {
         return building.TryGetComponent(out HighEnergyParticlePort port) && port.particleOutputEnabled && !building.TryGetComponent(out HighEnergyParticleRedirector _);
      }
      public static bool BuildingHasBuildingMaterial(GameObject building) {
         return building.GetComponent<BuildingComplete>()?.Def.ShowInBuildMenu ?? false;// otherwise this option is irrelevant
      }

      private void CASE_PLANTORSEED(GameObject obj) {
         PlantInfo plantInfo = new PlantInfo();

         if(!obj.TryGetComponent(out PlantableSeed seed))
         {
            string seedID;
            if(obj.TryGetComponent(out TreeBud _))
            {
               seedID = ForestTreeConfig.SEED_ID;
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

         if(ObjectType.CRITTEROREGG.ConsiderOption1())// if considerCritterMorph
         {
            highlightOptions |= Utils.crittersCachedOptions[critterInfo.critterID];
         }
         else if(Main.speciesMorphs.ContainsKey(critterInfo.species))
         {
            foreach(GameObject morph in Main.speciesMorphs[critterInfo.species])
            {
               highlightOptions |= Utils.crittersCachedOptions[morph.PrefabID()];
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

      private void CASE_DUPLICANT(GameObject obj) {
         DuplicantInfo duplicantInfo = new DuplicantInfo();

         duplicantInfo.duplicantInfo = obj.GetComponent<MinionResume>();

         info = duplicantInfo;

         CaseDuplicant_HighlightOptions();

         prefabID = obj.PrefabID();
      }
      private void CaseDuplicant_HighlightOptions() {
         highlightOptions |= HighlightOptions.CONSUMERS | HighlightOptions.CONSUMABLES | HighlightOptions.PRODUCE | HighlightOptions.COPIES;
      }

      private void CASE_GEYSER(GameObject obj) {
         GeyserInfo geyserInfo = new GeyserInfo();

         geyserInfo.geyser = obj.GetComponent<Geyser>();

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

      private void CASE_SAPTREE(GameObject obj) {
         CaseSaptree_HighlightOptions();

         prefabID = obj.PrefabID();
      }
      private void CaseSaptree_HighlightOptions() {
         highlightOptions |= HighlightOptions.CONSUMABLES | HighlightOptions.PRODUCE;
      }

      private void CASE_RADBOLT(GameObject obj) {
         CaseRadbolt_HighlightOptions();

         prefabID = obj.PrefabID();
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
         if(prefabID.HasTag(SapTreeConfig.ID))
         {
            return ObjectType.SAPTREE;
         }
         if(prefabID.HasTag(GameTags.HighEnergyParticle))
         {
            return ObjectType.RADBOLT;
         }
         return ObjectType.ELEMENT;// assuming that the object has the PrimaryElement component(which it should)
      }

      public static object ObjectForShouldHighlight(ObjectType objectType, PrimaryElement obj, Element element) {
         if(obj == null)
         {
            if(element != null)
               return element.id;

            return null;
         }

         KPrefabID objID = obj.GetComponent<KPrefabID>();
         if(!Main.cachedPrefabIDs.ContainsKey(objID.PrefabTag))
            throw new Exception(Main.debugPrefix + $"No cached prefabID found for {objID.PrefabTag} inside of {nameof(Main.cachedPrefabIDs)}");

         Tag prefabID = Main.cachedPrefabIDs[objID.PrefabTag];// prefabID != prefab tag of the object itself (f.e. prefabID for seeds is the prefab tag of their plant)

         switch(objectType)
         {
            case ObjectType.BUILDING:
               if((((Utils.buildingsCachedOptions[prefabID] & HighlightOptions.CONSUMABLES) == 0) || Main.highlightOption != HighlightOptions.CONSUMABLES.Reverse()) &&
                  (((Utils.buildingsCachedOptions[prefabID] & HighlightOptions.PRODUCE) == 0) || Main.highlightOption != HighlightOptions.PRODUCE.Reverse()))
               {
                  return prefabID.hash + ((long)obj.Element.id << 32);
               }
               else
               {
                  return null;// force to calculate shouldHighlight for this building
               }

            case ObjectType.DUPLICANT:
               return null;// calculating shouldHighlight for each duplicant individually

            default:
               return prefabID.hash;
         }
      }

      public string StringRepresentation() {
         return Utils.GetMyString(typeof(MYSTRINGS.UI.OBJECTTYPE), objectType.ToString());
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
         public Geyser geyser;
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
