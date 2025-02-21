using HighlightOverlay.Enums;
using HighlightOverlay;
using HighlightOverlay.Structs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static HighlightOverlay.Structs.ObjectProperties;
using UnityEngine;
using Klei.AI;
using System.Reflection;
using System.Linq.Expressions;

namespace HighlightOverlay {
   /// <summary>
   /// This class contains the methods that describe how to decide whether an object should be highlighted or not depending on its properties as well as the properties of the selected object.<br></br>
   /// Different "Cases" represent different selected objects and target objects' types.<br></br><br></br>
   /// 
   /// Cases naming structure: CASE_[ObjectType of source]_[HighlightOption]_[ObjectType(s) of target(s) (separated with "_") OR EVERYTHING]<br></br>
   /// Examples:<br></br><br></br>
   /// 
   /// CASE_ELEMENT_COPIES_EVERYTHING:<br></br>
   /// This method describes how EVERYTHING is highlighted when an ELEMENT's COPIES are selected<br></br><br></br>
   /// 
   /// CASE_BUILDING_CONSUMABLES_ELEMENT_ITEM_PLANTORSEED_CRITTEROREGG:<br></br>
   /// This method describes how ELEMENT, ITEM, PLANTORSEED, and CRITTEROREGG are highlighted when a BUILDING's CONSUMABLES are selected
   /// </summary>
   public static class ShouldHighlightCases {

      private static bool CASE_ELEMENT_PRODUCE_ELEMENT(ObjectProperties producer, ObjectProperties producee) {
         bool considerStateProducer = producer.objectType.ConsiderOption1();
         bool considerStateProducee = producee.objectType.ConsiderOption1();

         if(considerStateProducer)
         {
            if(considerStateProducee)
            {
               if(producee.element.id == Utils.GetElementsSublimationElement(producer.element))
                  return true;

               if(Utils.GetElementsTransitionElements(producer.element).Contains(producee.element.id))
                  return true;

               if(Utils.GetElementsTransitionOreElements(producer.element).Contains(producee.element.id))
                  return true;
            }
            else
            {
               List<SimHashes> produceeAggregateStates = Utils.OtherAggregateStatesIDs(producee.element);
               if(produceeAggregateStates.Contains(Utils.GetElementsSublimationElement(producer.element)))
                  return true;

               if(produceeAggregateStates.ContainsAnyFrom(Utils.GetElementsTransitionElements(producer.element)))
                  return true;

               if(produceeAggregateStates.ContainsAnyFrom(Utils.GetElementsTransitionOreElements(producer.element)))
                  return true;
            }
         }
         else
         {
            List<Element> producerAggregateStates = Utils.OtherAggregateStates(producer.element);
            if(considerStateProducee)
            {
               foreach(Element elem in producerAggregateStates)
               {
                  if(Utils.GetElementsSublimationElement(elem) == producee.element.id)
                     return true;

                  if(Utils.GetElementsTransitionElements(elem).Contains(producee.element.id))
                     return true;

                  if(Utils.GetElementsTransitionOreElements(elem).Contains(producee.element.id))
                     return true;
               }
            }
            else
            {
               List<SimHashes> produceeAggregateStates = Utils.OtherAggregateStatesIDs(producee.element);
               foreach(Element elem in producerAggregateStates)
               {
                  if(produceeAggregateStates.Contains(Utils.GetElementsSublimationElement(elem)))
                     return true;

                  if(produceeAggregateStates.ContainsAnyFrom(Utils.GetElementsTransitionElements(elem)))
                     return true;

                  if(produceeAggregateStates.ContainsAnyFrom(Utils.GetElementsTransitionOreElements(elem)))
                     return true;
               }
            }
         }
         return false;
      }

      private static bool CASE_ELEMENT_COPIES_EVERYTHING(ObjectProperties element, ObjectProperties obj) {
         if(ObjectType.ELEMENT.ConsiderOption1())
         {
            if(element.element.id == obj.element.id)
            {
               return true;
            }
         }
         else
         {
            if(Utils.OtherAggregateStatesIDs(element.element).Contains(obj.element.id))
            {
               return true;
            }
         }

         return false;
      }

      private static bool CASE_ITEM_COPIES_ITEM(ObjectProperties item, ObjectProperties obj) {
         return obj.objectType == ObjectType.ITEM && ((ItemInfo)item.info).itemID == ((ItemInfo)obj.info).itemID;
      }

      private static bool CASE_BUILDING_CONSUMABLES_ELEMENT_ITEM_PLANTORSEED_CRITTEROREGG(ObjectProperties building, ObjectProperties consumable) {
         BuildingInfo buildingInfo = (BuildingInfo)building.info;
         bool considerBuildingSettings = building.objectType.ConsiderOption1();

         HashSet<Tag> targetTags = CollectTags_ELEMENT_ITEM_PLANTORSEED_CRITTEROREGG(consumable);

         if(buildingInfo.buildingGO.TryGetComponent(out ComplexFabricator fabricator))
         {
            foreach(var recipe in fabricator.recipe_list)
            {
               if(!considerBuildingSettings || fabricator.IsRecipeQueued(recipe))
               {
                  if(recipe.ingredients.Select(ing => ing.material).ContainsAnyFrom(targetTags))
                     return true;
               }
            }
         }

         if(buildingInfo.buildingGO.TryGetComponent(out TreeFilterable treeFilterable))
         {
            if(considerBuildingSettings)
            {
               if(treeFilterable.acceptedTagSet.ContainsAnyFrom(targetTags))
                  return true;
            }
            else
            {
               if(treeFilterable.storage.storageFilters.ContainsAnyFrom(targetTags))
                  return true;
            }
         }

         if(buildingInfo.buildingGO.TryGetComponent(out EnergyGenerator generator))
         {
            if(generator.formula.inputs.Select(input => input.tag).ContainsAnyFrom(targetTags))
               return true;
         }

         if(buildingInfo.buildingGO.TryGetComponents(out ElementConverter[] elementConverters))
         {
            foreach(var elementConverter in elementConverters)
            {
               if(elementConverter.consumedElements.Select(elem => elem.Tag).ContainsAnyFrom(targetTags))
                  return true;
            }
         }

         if(buildingInfo.buildingGO.TryGetComponents(out ManualDeliveryKG[] deliveries))
         {
            foreach(var delivery in deliveries)
            {
               if(targetTags.Contains(delivery.requestedItemTag))
                  return true;
            }
         }

         if(buildingInfo.buildingGO.TryGetComponent(out SingleEntityReceptacle receptacle))
         {
            if(receptacle is PlantablePlot)
            {
               PlantablePlot plot = (PlantablePlot)receptacle;

               if(considerBuildingSettings)
               {
                  if(targetTags.Contains(plot.requestedEntityTag) || (plot.plant != null && targetTags.Contains(plot.plant.PrefabTag)))
                     return true;

                  if(plot.plant != null)
                  {
                     switch(consumable.objectType)
                     {
                        case ObjectType.ELEMENT:
                           if(CASE_PLANTORSEED_CONSUMABLES_ELEMENT(new ObjectProperties(plot.plant), consumable))
                              return true;
                           break;

                        case ObjectType.ITEM:
                           if(CASE_PLANTORSEED_CONSUMABLES_ITEM(new ObjectProperties(plot.plant), consumable))
                              return true;
                           break;
                     }
                  }
               }
               else
               {
                  if(plot.possibleDepositObjectTags.ContainsAnyFrom(targetTags))// possibleDepositObjectTags == tags of seeds that are allowed to be planted in the plot
                     return true;
               }
            }
            else
            {
               if(considerBuildingSettings)
               {
                  if(targetTags.Contains(receptacle.requestedEntityTag) || (receptacle.occupyingObject != null && targetTags.Contains(receptacle.occupyingObject.PrefabID())))
                     return true;
               }
               else
               {
                  if(receptacle.possibleDepositObjectTags.ContainsAnyFrom(targetTags))// possibleDepositObjectTags == tags of objects that are allowed to be put in this receptacle
                     return true;
               }
            }
         }

         if(buildingInfo.buildingGO.TryGetComponent(out Tinkerable tinkerable))
         {
            if(targetTags.Contains(tinkerable.tinkerMaterialTag))
               return true;
         }

         if(buildingInfo.buildingGO.TryGetComponent(out SuitLocker suitLocker))
         {
            if(suitLocker.OutfitTags.ContainsAnyFrom(targetTags))
               return true;
         }

         SpiceGrinder.StatesInstance spiceGrinder = buildingInfo.buildingGO.GetSMI<SpiceGrinder.StatesInstance>();
         if(spiceGrinder != null)
         {
            if(considerBuildingSettings)
            {
               if(spiceGrinder.currentSpice.Id != Tag.Invalid)
               {
                  foreach(var ingredient in SpiceGrinder.SettingOptions[spiceGrinder.currentSpice.Id].Spice.Ingredients)
                  {
                     if(ingredient.IngredientSet.ContainsAnyFrom(targetTags))
                        return true;
                  }
               }
            }
            else
            {
               foreach(var spice in SpiceGrinder.SettingOptions.Values)
               {
                  foreach(var ingredient in spice.Spice.Ingredients)
                  {
                     if(ingredient.IngredientSet.ContainsAnyFrom(targetTags))
                        return true;
                  }
               }
            }
         }

         if(buildingInfo.buildingGO.TryGetComponent(out TrapTrigger trap))
         {
            if(trap.trappableCreatures.ContainsAnyFrom(targetTags))
               return true;
         }

         RanchStation.Instance ranchStation = buildingInfo.buildingGO.GetSMI<RanchStation.Instance>();
         if(ranchStation != null && consumable.objectType == ObjectType.CRITTEROREGG)
         {
            if(buildingInfo.buildingGO.HasTag(Utils.RanchStation))
            {
               if(((CritterInfo)consumable.info).adultPrefab.GetDef<RanchableMonitor.Def>() != null)
                  return true;
            }
            else if(buildingInfo.buildingGO.HasTag(Utils.ShearingStation))
            {
               if(((CritterInfo)consumable.info).adultPrefab.GetSMI<IShearable>() != null)
                  return true;
            }
            else if(buildingInfo.buildingGO.HasTag(Utils.MilkingStation))
            {
               if((((CritterInfo)consumable.info).adultPrefab.GetComponent<CreatureBrain>()?.species ?? Tag.Invalid) == GameTags.Creatures.Species.MooSpecies)
                  return true;
            }
            else
            {
               if(ranchStation.def.IsCritterEligibleToBeRanchedCb(((CritterInfo)consumable.info).adultPrefab.gameObject, ranchStation))
                  return true;// this method is not 100% accurate, but better than nothing
            }
         }

         if(buildingInfo.buildingGO.TryGetComponent(out RocketEngine engine))
         {
            if(targetTags.Contains(engine.fuelTag))
               return true;

            if(engine.requireOxidizer && targetTags.Contains(GameTags.Oxidizer))
               return true;
         }
         if(buildingInfo.buildingGO.TryGetComponent(out RocketEngineCluster engineCluster))
         {
            if(targetTags.Contains(engineCluster.fuelTag))
               return true;

            if(engineCluster.requireOxidizer && targetTags.Contains(GameTags.Oxidizer))
               return true;
         }


         return false;
      }
      private static bool CASE_BUILDING_CONSUMABLES_DUPLICANT(ObjectProperties building, ObjectProperties duplicant) {
         BuildingInfo buildingInfo = (BuildingInfo)building.info;
         DuplicantInfo duplicantInfo = (DuplicantInfo)duplicant.info;
         bool considerBuildingSettings = building.objectType.ConsiderOption1();

         if(buildingInfo.buildingGO.GetComponent<BuildingComplete>()?.isManuallyOperated ?? false)
         {
            if(!considerBuildingSettings)
               return true;// highlight all duplicants

            List<string> requiredPerks = new List<string>(2);
            if(buildingInfo.buildingGO.TryGetComponents(out Workable[] workables))
            {
               foreach(var workable in workables)
               {
                  if(!string.IsNullOrEmpty(workable.requiredSkillPerk))
                     requiredPerks.Add(workable.requiredSkillPerk);
               }
            }

            if(requiredPerks.Count == 0)
               return true;

            foreach(string perk in requiredPerks)
            {
               if(duplicantInfo.duplicantInfo.HasPerk(perk))
                  return true;
            }
         }

         if(buildingInfo.buildingGO.TryGetComponent(out Ownable ownable))
         {
            if(!considerBuildingSettings)
               return true;// highlight all duplicants

            if(ownable.IsAssignedTo(duplicantInfo.duplicantInfo.GetIdentity))
               return true;
         }


         return false;
      }

      private static bool CASE_BUILDING_PRODUCE_ELEMENT_ITEM(ObjectProperties building, ObjectProperties produce) {
         BuildingInfo buildingInfo = (BuildingInfo)building.info;
         bool considerBuildingSettings = building.objectType.ConsiderOption1();

         HashSet<Tag> targetTags = CollectTags_ELEMENT_ITEM_PLANTORSEED_CRITTEROREGG(produce);

         if(buildingInfo.buildingGO.TryGetComponent(out ComplexFabricator fabricator))
         {
            foreach(var recipe in fabricator.recipe_list)
            {
               if(!considerBuildingSettings || fabricator.IsRecipeQueued(recipe))
               {
                  if(recipe.results.Select(ing => ing.material).ContainsAnyFrom(targetTags))
                     return true;
               }
            }
         }

         if(buildingInfo.buildingGO.TryGetComponents(out ElementConverter[] elementConverters))
         {
            foreach(var elementConverter in elementConverters)
            {
               if(elementConverter.outputElements.Select(elem => elem.elementHash.CreateTag()).ContainsAnyFrom(targetTags))
                  return true;
            }
         }

         if(buildingInfo.buildingGO.TryGetComponent(out EnergyGenerator generator))
         {
            if(generator.formula.outputs != null && generator.formula.outputs.Select(output => output.element.CreateTag()).ContainsAnyFrom(targetTags))
               return true;
         }

         if(buildingInfo.buildingGO.TryGetComponent(out BuildingElementEmitter elementEmitter))
         {
            if(targetTags.Contains(elementEmitter.Element.CreateTag()))
               return true;
         }

         if(buildingInfo.buildingGO.TryGetComponent(out TinkerStation tinkerStation))
         {
            if(targetTags.Contains(tinkerStation.outputPrefab))
               return true;
         }

         if(buildingInfo.buildingGO.TryGetComponent(out OilWellCap wellCap))
         {
            if(targetTags.Contains(wellCap.gasElement.CreateTag()))
               return true;
         }

         if(buildingInfo.buildingGO.TryGetComponent(out Toilet toilet))
         {
            if(targetTags.Contains(toilet.solidWastePerUse.elementID.CreateTag()) || targetTags.Contains(toilet.gasWasteWhenFull.elementID.CreateTag()))
               return true;
         }

         if(buildingInfo.buildingGO.TryGetComponent(out FlushToilet _))
         {
            if(targetTags.Contains(Utils.PollutedWater))
               return true;
         }

         if(buildingInfo.buildingGO.TryGetComponent(out RocketEngine engine))
         {
            if(targetTags.Contains(engine.exhaustElement.CreateTag()))
               return true;
         }
         if(buildingInfo.buildingGO.TryGetComponent(out RocketEngineCluster engineCluster))
         {
            if(targetTags.Contains(engineCluster.exhaustElement.CreateTag()))
               return true;
         }


         return false;
      }
      private static HashSet<Tag> CollectTags_ELEMENT_ITEM_PLANTORSEED_CRITTEROREGG(ObjectProperties obj) {
         bool considerOption1 = obj.objectType.ConsiderOption1();

         ItemInfo itemInfo = default;
         if(obj.objectType == ObjectType.ITEM)
            itemInfo = (ItemInfo)obj.info;

         PlantInfo plantInfo = default;
         if(obj.objectType == ObjectType.PLANTORSEED)
            plantInfo = (PlantInfo)obj.info;

         CritterInfo critterInfo = default;
         if(obj.objectType == ObjectType.CRITTEROREGG)
            critterInfo = (CritterInfo)obj.info;

         HashSet<Tag> targetTags = new HashSet<Tag>();
         if(obj.objectType == ObjectType.ELEMENT)
         {
            if(considerOption1)// if considerAggregateState
            {
               targetTags.Add(obj.element.tag);
            }
            else
            {
               foreach(var elem in Utils.OtherAggregateStates(obj.element))
               {
                  targetTags.Add(elem.tag);
               }
            }
         }
         else if(obj.objectType == ObjectType.ITEM)
         {
            targetTags.Add(itemInfo.itemID);
         }
         else if(obj.objectType == ObjectType.PLANTORSEED)
         {
            targetTags.AddAll(plantInfo.plantID, plantInfo.seedID.ToTag());
         }
         else if(obj.objectType == ObjectType.CRITTEROREGG)
         {
            if(considerOption1)// if considerCritterMorph
            {
               targetTags.AddAll(critterInfo.critterID, critterInfo.eggID);
            }
            else
            {
               targetTags = Utils.CollectMorphTagsOfSpecies(critterInfo.species, true, true, true);
            }
         }
         //-----------Expanding targetTags-----------DOWN
         HashSet<Tag> tags = new HashSet<Tag>();
         tags.AddAll(targetTags);
         foreach(var tag in tags)
         {
            if(tag == Utils.Vacuum || tag == default)
               continue;

            var prefabID = Assets.PrefabsByTag.GetOrDefault(tag);
            if(prefabID != null)
            {
               targetTags.AddAll(prefabID.Tags);
            }
         }
         //-----------Expanding targetTags-----------UP

         return targetTags;
      }

      private static bool CASE_BUILDING_PRODUCE_ROBOT(ObjectProperties building, ObjectProperties robot) {
         BuildingInfo buildingInfo = (BuildingInfo)building.info;

         if(robot.prefabID == GameTags.Robots.Models.SweepBot)
         {
            return buildingInfo.buildingID == SweepBotStationConfig.ID;
         }
         else if(robot.prefabID == GameTags.Robots.Models.ScoutRover)
         {
            return buildingInfo.buildingID == ScoutLanderConfig.ID || buildingInfo.buildingID == ScoutModuleConfig.ID;
         }
         else if(robot.prefabID == GameTags.Robots.Models.MorbRover)
         {
            return buildingInfo.buildingID == MorbRoverMakerConfig.ID;
         }

         return false;
      }

      private static bool CASE_BUILDING_BUILDINGMATERIAL_ELEMENT_ITEM(ObjectProperties building, ObjectProperties elemoritem) {
         BuildingInfo buildingInfo = (BuildingInfo)building.info;
         bool considerBuildingSettings = building.objectType.ConsiderOption1();
         bool considerOption1 = elemoritem.objectType.ConsiderOption1();

         bool isElement = elemoritem.objectType == ObjectType.ELEMENT;

         List<Tag> targetTags;
         if(isElement)
         {
            if(considerOption1)// if considerAggregateState
            {
               targetTags = new List<Tag> { elemoritem.element.id.CreateTag() };
            }
            else
            {
               targetTags = Utils.OtherAggregateStatesTags(elemoritem.element);
            }
         }
         else
         {
            targetTags = new List<Tag> { ((ItemInfo)elemoritem.info).itemID };
         }

         if(considerBuildingSettings)
         {
            Deconstructable deconstructable = buildingInfo.buildingGO.GetComponent<Deconstructable>();

            if(deconstructable?.constructionElements != null && deconstructable.constructionElements.Length > 0)
            {
               if(deconstructable.constructionElements.ContainsAnyFrom(targetTags))
                  return true;
            }
            else if(isElement)
            {
               if(targetTags.Contains(building.element.id.CreateTag()))
                  return true;
            }
         }
         else
         {
            BuildingDef def = buildingInfo.buildingGO.GetComponent<BuildingComplete>()?.Def;

            if(def?.CraftRecipe?.Ingredients != null)
            {
               foreach(var ingredient in def.CraftRecipe.Ingredients)
               {
                  if(isElement)
                  {
                     if(ingredient.GetElementOptions().Select(e => e.id.CreateTag()).ContainsAnyFrom(targetTags))
                        return true;
                  }
                  else
                  {
                     if(((ItemInfo)elemoritem.info).itemGO.HasTag(ingredient.tag))
                        return true;
                  }
               }
            }
            else if(isElement)
            {
               if(targetTags.Contains(building.element.id.CreateTag()))
                  return true;
            }
         }

         return false;
      }

      private static bool CASE_BUILDING_COPIES_BUILDING(ObjectProperties building, ObjectProperties obj) {
         return obj.objectType == ObjectType.BUILDING && ((BuildingInfo)building.info).buildingID == ((BuildingInfo)obj.info).buildingID;
      }

      private static bool CASE_BUILDING_EXACTCOPIES_BUILDING(ObjectProperties building, ObjectProperties obj) {
         return obj.objectType == ObjectType.BUILDING && ((BuildingInfo)building.info).buildingID == ((BuildingInfo)obj.info).buildingID &&
                  building.element.id == obj.element.id;
      }

      private static bool CASE_UNDERCONSTRUCTION_COPIES_UNDERCONSTRUCTION(ObjectProperties building, ObjectProperties obj) {
         return true;
      }

      private static bool CASE_PLANTORSEED_CONSUMABLES_ELEMENT(ObjectProperties plant, ObjectProperties element) {
         PlantInfo plantInfo = (PlantInfo)plant.info;
         bool considerState = element.objectType.ConsiderOption1();

         List<Tag> targetElements;
         if(considerState)
         {
            targetElements = new List<Tag>() { element.element.id.CreateTag() };
         }
         else
         {
            targetElements = Utils.OtherAggregateStatesTags(element.element);
         }

         var irrigationMonitor = plantInfo.plantPrefab.GetDef<IrrigationMonitor.Def>();
         if(irrigationMonitor != null)
         {
            foreach(var consumedElement in irrigationMonitor.consumedElements)
            {
               if(targetElements.Contains(consumedElement.tag))
                  return true;
            }
         }

         var fertilizationMonitor = plantInfo.plantPrefab.GetDef<FertilizationMonitor.Def>();
         if(fertilizationMonitor != null)
         {
            foreach(var consumedElement in fertilizationMonitor.consumedElements)
            {
               if(targetElements.Contains(consumedElement.tag))
                  return true;
            }
         }

         if(plantInfo.plantPrefab.TryGetComponents(out ElementConverter[] elementConverters))
         {
            foreach(var elementConverter in elementConverters)
            {
               if(elementConverter.consumedElements.Select(elem => elem.Tag).ContainsAnyFrom(targetElements))
                  return true;
            }
         }

         return false;
      }
      private static bool CASE_PLANTORSEED_CONSUMABLES_ITEM(ObjectProperties plant, ObjectProperties item) {
         PlantInfo plantInfo = (PlantInfo)plant.info;
         ItemInfo itemInfo = (ItemInfo)item.info;

         if(plantInfo.plantPrefab.TryGetComponent(out Tinkerable tinkerable))
         {
            if(tinkerable.tinkerMaterialTag == itemInfo.itemID)
               return true;
         }

         return false;
      }
      private static bool CASE_PLANTORSEED_CONSUMABLES_CRITTEROREGG(ObjectProperties plant, ObjectProperties critter) {
         PlantInfo plantInfo = (PlantInfo)plant.info;
         CritterInfo critterInfo = (CritterInfo)critter.info;

         if(plantInfo.plantPrefab.TryGetComponent(out CritterTrapPlant _))
         {
            if(plantInfo.plantPrefab.TryGetComponent(out TrapTrigger trap))
            {
               if(critterInfo.adultPrefab.HasAnyTags(trap.trappableCreatures))
                  return true;
            }
         }

         return false;
      }
      private static bool CASE_PLANTORSEED_PRODUCE_ELEMENT(ObjectProperties plant, ObjectProperties element) {
         PlantInfo plantInfo = (PlantInfo)plant.info;
         bool considerState = element.objectType.ConsiderOption1();

         List<SimHashes> targetElements;
         if(considerState)
         {
            targetElements = new List<SimHashes>() { element.element.id };
         }
         else
         {
            targetElements = Utils.OtherAggregateStatesIDs(element.element);
         }

         if(plantInfo.plantPrefab.TryGetComponent(out ElementConverter elementConverter))
         {
            foreach(var producedElement in elementConverter.outputElements)
            {
               if(targetElements.Contains(producedElement.elementHash))
                  return true;
            }
         }

         if(plantInfo.plantPrefab.TryGetComponent(out CritterTrapPlant saturnTrap))
         {
            if(targetElements.Contains(saturnTrap.outputElement))
               return true;
         }

         return false;
      }
      private static bool CASE_PLANTORSEED_PRODUCE_ITEM(ObjectProperties plant, ObjectProperties item) {
         PlantInfo plantInfo = (PlantInfo)plant.info;
         ItemInfo itemInfo = (ItemInfo)item.info;

         if(plantInfo.plantPrefab.TryGetComponent(out Crop crop))
         {
            if(crop.cropId == itemInfo.itemID.ToString())
               return true;
         }

         return false;
      }

      private static bool CASE_PLANTORSEED_COPIES_PLANTORSEED(ObjectProperties plant, ObjectProperties obj) {
         return obj.objectType == ObjectType.PLANTORSEED && ((PlantInfo)plant.info).plantID == ((PlantInfo)obj.info).plantID;
      }

      private static bool CASE_PLANTORSEED_EXACTCOPIES_PLANTORSEED(ObjectProperties plant, ObjectProperties obj) {
         if(obj.objectType == ObjectType.PLANTORSEED)
         {
            PlantInfo selectedPlant = (PlantInfo)plant.info;
            PlantInfo targetPlant = (PlantInfo)obj.info;
            if(selectedPlant.plantID == targetPlant.plantID)
            {
               if((selectedPlant.plantPrefab.GetComponent<MutantPlant>()?.SubSpeciesID ?? default) == (targetPlant.plantPrefab.GetComponent<MutantPlant>()?.SubSpeciesID ?? default))
                  return true;
            }
         }

         return false;
      }

      private static bool CASE_GEYSER_PRODUCE_ELEMENT(ObjectProperties geyser, ObjectProperties element) {
         bool considerState = element.objectType.ConsiderOption1();

         List<SimHashes> targetElements;
         if(considerState)
         {
            targetElements = new List<SimHashes>() { element.element.id };
         }
         else
         {
            targetElements = Utils.OtherAggregateStatesIDs(element.element);
         }

         return targetElements.Contains(((GeyserInfo)geyser.info).geyser.emitter.outputElement.elementHash);
      }

      private static bool CASE_GEYSER_COPIES_GEYSER(ObjectProperties geyser, ObjectProperties obj) {
         return true;
      }

      private static bool CASE_GEYSER_EXACTCOPIES_GEYSER(ObjectProperties geyser, ObjectProperties obj) {
         return geyser.prefabID == obj.prefabID;
      }

      private static bool CASE_CRITTEROREGG_CONSUMABLES_ELEMENT(ObjectProperties critter, ObjectProperties element) {
         CritterInfo critterInfo = (CritterInfo)critter.info;
         bool considerMorph = critter.objectType.ConsiderOption1();
         bool considerState = element.objectType.ConsiderOption1();

         List<Tag> targetElements;
         if(considerState)
         {
            targetElements = new List<Tag>() { element.element.id.CreateTag() };
         }
         else
         {
            targetElements = Utils.OtherAggregateStatesIDs(element.element).Select(elemID => elemID.CreateTag()).ToList();
         }

         if(considerMorph)
         {
            if(CheckOneMorph(critterInfo.adultPrefab))
               return true;
         }
         else
         {
            foreach(KPrefabID morph in Main.speciesMorphs[critterInfo.species])
            {
               if(CheckOneMorph(morph))
                  return true;
            }
         }

         return false;


         bool CheckOneMorph(KPrefabID morph) {
            Diet diet = morph.GetDef<CreatureCalorieMonitor.Def>()?.diet;
            if(diet != null)
            {
               if(diet.consumedTags.Select(pair => pair.Key).ContainsAnyFrom(targetElements))
                  return true;
            }

            DrinkMilkMonitor.Def milkMonitor = morph.GetDef<DrinkMilkMonitor.Def>();
            if(milkMonitor != null)
            {
               if(targetElements.Contains(Utils.Milk))
                  return true;
            }

            return false;
         }
      }
      private static bool CASE_CRITTEROREGG_CONSUMABLES_PLANTORSEED(ObjectProperties critter, ObjectProperties plant) {
         CritterInfo critterInfo = (CritterInfo)critter.info;
         PlantInfo plantInfo = (PlantInfo)plant.info;
         bool considerMorph = critter.objectType.ConsiderOption1();

         if(considerMorph)
         {
            if(CheckOneMorph(critterInfo.adultPrefab))
               return true;
         }
         else
         {
            foreach(KPrefabID morph in Main.speciesMorphs[critterInfo.species])
            {
               if(CheckOneMorph(morph))
                  return true;
            }
         }

         return false;


         bool CheckOneMorph(KPrefabID morph) {
            Diet diet = morph.GetDef<CreatureCalorieMonitor.Def>()?.diet;
            if(diet != null)
            {
               if((diet.CanEatAnyPlantDirectly && diet.directlyEatenPlantInfos.Select(pair => pair.consumedTags).SelectMany(c => c).Contains(plantInfo.plantID))
                  || (plantInfo.seedID != Tag.Invalid && diet.consumedTags.Select(pair => pair.Key).Contains(plantInfo.seedID)))
                  return true;
            }

            return false;
         }
      }
      private static bool CASE_CRITTEROREGG_CONSUMABLES_ITEM(ObjectProperties critter, ObjectProperties item) {
         CritterInfo critterInfo = (CritterInfo)critter.info;
         ItemInfo itemInfo = (ItemInfo)item.info;
         bool considerMorph = critter.objectType.ConsiderOption1();

         if(considerMorph)
         {
            if(CheckOneMorph(critterInfo.adultPrefab))
               return true;
         }
         else
         {
            foreach(KPrefabID morph in Main.speciesMorphs[critterInfo.species])
            {
               if(CheckOneMorph(morph))
                  return true;
            }
         }

         return false;


         bool CheckOneMorph(KPrefabID morph) {
            Diet diet = morph.GetDef<CreatureCalorieMonitor.Def>()?.diet;
            if(diet != null)
            {
               IEnumerable<Tag> consumables = diet.consumedTags.Select(pair => pair.Key);
               if(consumables.Contains(itemInfo.itemID))
                  return true;
            }

            return false;
         }
      }
      private static bool CASE_CRITTEROREGG_PRODUCE_ELEMENT(ObjectProperties critter, ObjectProperties element) {
         CritterInfo critterInfo = (CritterInfo)critter.info;
         bool considerMorph = critter.objectType.ConsiderOption1();
         bool considerState = element.objectType.ConsiderOption1();

         List<Tag> elementTags;
         if(considerState)
         {
            elementTags = new List<Tag> { element.element.id.CreateTag() };
         }
         else
         {
            elementTags = Utils.OtherAggregateStatesTags(element.element);
         }

         if(considerMorph)
         {
            if(CheckOneMorph(critterInfo.adultPrefab))
               return true;
         }
         else
         {
            foreach(KPrefabID morph in Main.speciesMorphs[critterInfo.species])
            {
               if(CheckOneMorph(morph))
                  return true;
            }
         }

         return false;


         bool CheckOneMorph(KPrefabID morph) {
            Diet diet = morph.GetDef<CreatureCalorieMonitor.Def>()?.diet;
            if(diet != null)
            {
               IEnumerable<Tag> produce = diet.producedTags.Select(pair => pair.Key);
               if(produce.ContainsAnyFrom(elementTags))
                  return true;
            }

            return false;
         }
      }
      private static bool CASE_CRITTEROREGG_PRODUCE_ITEM(ObjectProperties critter, ObjectProperties item) {
         CritterInfo critterInfo = (CritterInfo)critter.info;
         ItemInfo itemInfo = (ItemInfo)item.info;
         bool considerMorph = critter.objectType.ConsiderOption1();

         if(considerMorph)
         {
            if(CheckOneMorph(critterInfo.adultPrefab))
               return true;
         }
         else
         {
            foreach(KPrefabID morph in Main.speciesMorphs[critterInfo.species])
            {
               if(CheckOneMorph(morph))
                  return true;
            }
         }

         return false;


         bool CheckOneMorph(KPrefabID morph) {
            ScaleGrowthMonitor.Def scaleMonitor = morph.GetDef<ScaleGrowthMonitor.Def>();
            if(scaleMonitor != null)
            {
               if(scaleMonitor.itemDroppedOnShear == itemInfo.itemID)
                  return true;
            }

            return false;
         }
      }

      private static bool CASE_CRITTEROREGG_COPIES_CRITTEROREGG(ObjectProperties critter, ObjectProperties obj) {
         if(ObjectType.CRITTEROREGG.ConsiderOption1())
         {
            return obj.objectType == ObjectType.CRITTEROREGG && ((CritterInfo)critter.info).critterID == ((CritterInfo)obj.info).critterID;
         }
         else
         {
            return obj.objectType == ObjectType.CRITTEROREGG && ((CritterInfo)critter.info).species == ((CritterInfo)obj.info).species;
         }
      }

      private static bool CASE_DUPLICANT_CONSUMABLES_ELEMENT(ObjectProperties duplicant, ObjectProperties element) {
         bool considerState = element.objectType.ConsiderOption1();

         List<Tag> elementTags;
         if(considerState)
         {
            elementTags = new List<Tag> { element.element.id.CreateTag() };
         }
         else
         {
            elementTags = Utils.OtherAggregateStatesTags(element.element);
         }

         if(elementTags.Contains(Utils.Oxygen) || elementTags.Contains(Utils.PollutedOxygen))
            return true;


         return false;
      }
      private static bool CASE_DUPLICANT_CONSUMABLES_ITEM(ObjectProperties duplicant, ObjectProperties item) {
         DuplicantInfo duplicantInfo = (DuplicantInfo)duplicant.info;
         ItemInfo itemInfo = (ItemInfo)item.info;

         if(itemInfo.itemGO.HasTag(GameTags.Edible) || itemInfo.itemGO.HasTag(GameTags.Medicine))
         {
            if(duplicantInfo.duplicantInfo.TryGetComponent(out ConsumableConsumer consumer))
            {
               return !consumer.forbiddenTagSet.Contains(itemInfo.itemID);
            }
         }

         if(itemInfo.itemGO.TryGetComponent(out Equippable _))
         {
            return true;// any duplicant may wear whatever he/she wants(we live in a free world!)
         }


         return false;
      }

      private static bool CASE_DUPLICANT_PRODUCE_ELEMENT(ObjectProperties duplicant, ObjectProperties element) {
         DuplicantInfo duplicantInfo = (DuplicantInfo)duplicant.info;
         bool considerState = element.objectType.ConsiderOption1();

         List<Tag> elementTags;
         if(considerState)
         {
            elementTags = new List<Tag> { element.element.id.CreateTag() };
         }
         else
         {
            elementTags = Utils.OtherAggregateStatesTags(element.element);
         }

         if(elementTags.Contains(Utils.CO2) || elementTags.Contains(Utils.PollutedWater)/*pissing/throwing up*/)
            return true;

         if(duplicantInfo.duplicantInfo.TryGetComponent(out Traits traits))
         {
            if(traits.HasTrait("Flatulence"))
            {
               if(elementTags.Contains(Utils.Methane))
                  return true;
            }
         }


         return false;
      }

      private static bool CASE_DUPLICANT_COPIES_DUPLICANT(ObjectProperties duplicant, ObjectProperties obj) {
         return true;
      }

      private static bool CASE_ROBOT_COPIES_ROBOT(ObjectProperties robot, ObjectProperties obj) {
         return obj.objectType == ObjectType.ROBOT && robot.prefabID == obj.prefabID;
      }

      private static bool CASE_OILWELL_CONSUMABLES_ELEMENT(ObjectProperties well, ObjectProperties element) {
         return CASE_BUILDING_CONSUMABLES_ELEMENT_ITEM_PLANTORSEED_CRITTEROREGG(new ObjectProperties(Assets.PrefabsByTag.GetOrDefault(OilWellCapConfig.ID)), element);
      }

      private static bool CASE_OILWELL_PRODUCE_ELEMENT(ObjectProperties well, ObjectProperties element) {
         return CASE_BUILDING_PRODUCE_ELEMENT_ITEM(new ObjectProperties(Assets.PrefabsByTag.GetOrDefault(OilWellCapConfig.ID)), element);
      }

      private static bool CASE_OILWELL_COPIES_OILWELL(ObjectProperties well, ObjectProperties obj) {
         return true;
      }

      private static bool CASE_SAPTREE_CONSUMABLES_ITEM(ObjectProperties saptree, ObjectProperties item) {
         ItemInfo itemInfo = (ItemInfo)item.info;
         return itemInfo.itemGO.HasTag(GameTags.Edible);
      }

      private static bool CASE_SAPTREE_PRODUCE_ELEMENT(ObjectProperties saptree, ObjectProperties element) {
         bool considerState = ObjectType.ELEMENT.ConsiderOption1();
         return (considerState && element.element.id == SimHashes.Resin) || (!considerState && Utils.OtherAggregateStatesIDs(SimHashes.Resin).Contains(element.element.id));
      }


      public static Dictionary<int, Func<ObjectProperties, ObjectProperties, bool>> caseMethods = new Dictionary<int, Func<ObjectProperties, ObjectProperties, bool>>();


      public static class CasesUtils {
         public static void RegisterCases() {
            //Debug.Log(Main.debugPrefix + "Registering highlight cases...");
            foreach(ObjectType objType in Enum.GetValues(typeof(ObjectType)))
            {
               if(objType == ObjectType.NOTVALID)
                  continue;

               foreach(HighlightOptions highlightOption in Enum.GetValues(typeof(HighlightOptions)))
               {
                  if(highlightOption == HighlightOptions.NONE || highlightOption == HighlightOptions.CONSIDEROPTION1)
                     continue;

                  foreach(ObjectType objType2 in Enum.GetValues(typeof(ObjectType)))
                  {
                     if(objType2 == ObjectType.NOTVALID)
                        continue;

                     bool isReversed = highlightOption == HighlightOptions.CONSUMERS || highlightOption == HighlightOptions.PRODUCERS;// these are reversed to CONSUMABLES & PRODUCE respectively

                     MethodInfo correspondingCase;
                     if(isReversed)
                     {
                        correspondingCase = FindCorrespondingCase(objType2.ToString(), highlightOption.Reverse().ToString(), objType.ToString());
                     }
                     else
                     {
                        correspondingCase = FindCorrespondingCase(objType.ToString(), highlightOption.ToString(), objType2.ToString());
                     }

                     if(correspondingCase == null)
                        continue;

                     var param1 = ParameterExpression.Parameter(typeof(ObjectProperties));
                     var param2 = ParameterExpression.Parameter(typeof(ObjectProperties));

                     MethodCallExpression callExpr;
                     if(isReversed)
                     {
                        callExpr = MethodCallExpression.Call(correspondingCase, param2, param1);
                     }
                     else
                     {
                        callExpr = MethodCallExpression.Call(correspondingCase, param1, param2);
                     }

                     Func<ObjectProperties, ObjectProperties, bool> caseFunc = LambdaExpression.Lambda<Func<ObjectProperties, ObjectProperties, bool>>(callExpr, param1, param2).Compile();

                     int dictKey = CasesUtils.CalculateCaseKey(objType, highlightOption, objType2);
                     caseMethods.Add(dictKey, caseFunc);

                     //Debug.Log($"Registered case CASE_{objType}_{highlightOption}_{objType2}");
                  }
               }
            }
         }
         private static MethodInfo FindCorrespondingCase(string segment0, string segment1, string segmentN) {
            foreach(var cas3 in AllCases())
            {
               List<string> stringSegments = SplitCaseIntoSegments(cas3);
               if(stringSegments[0] == segment0)
               {
                  if(stringSegments[1] == segment1)
                  {
                     for(int segment = 2; segment < stringSegments.Count; segment++)
                     {
                        if(stringSegments[segment] == "EVERYTHING" || stringSegments[segment] == segmentN)
                        {
                           return cas3;
                        }
                     }
                  }
               }
            }

            return null;
         }

         public static void ValidateCasesMethods() {
            foreach(var cas3 in AllCases())
            {
               List<string> stringSegments = SplitCaseIntoSegments(cas3);

               if(stringSegments.Count < 3)
                  throw new Exception(Main.debugPrefix + $"Case {cas3.Name} does not have enough segments");

               bool isFirstSegmentCorrect = false;
               foreach(ObjectType objType in Enum.GetValues(typeof(ObjectType)))
               {
                  if(objType.ToString() == stringSegments[0])
                  {
                     isFirstSegmentCorrect = true;
                     break;
                  }
               }
               if(!isFirstSegmentCorrect)
                  throw new Exception(Main.debugPrefix + $"Case {cas3.Name}'s first segment does not represent an existing {nameof(ObjectType)}");

               bool isSecondSegmentCorrect = false;
               foreach(HighlightOptions highlightOption in Enum.GetValues(typeof(HighlightOptions)))
               {
                  if(highlightOption.ToString() == stringSegments[1])
                  {
                     isSecondSegmentCorrect = true;
                     break;
                  }
               }
               if(!isSecondSegmentCorrect)
                  throw new Exception(Main.debugPrefix + $"Case {cas3.Name}'s second segment does not represent an existing {nameof(HighlightOptions)}");

               for(int segment = 2; segment < stringSegments.Count; segment++)
               {
                  bool isSegmentCorrect = false;

                  if(stringSegments[segment] == "EVERYTHING")
                  {
                     isSegmentCorrect = true;
                  }
                  else
                  {
                     foreach(ObjectType objType in Enum.GetValues(typeof(ObjectType)))
                     {
                        if(objType.ToString() == stringSegments[segment])
                        {
                           isSegmentCorrect = true;
                           break;
                        }
                     }
                  }
                  if(!isSegmentCorrect)
                     throw new Exception(Main.debugPrefix + $"Case {cas3.Name}'s {segment + 1}th segment does not represent an existing {nameof(ObjectType)}");
               }


               IEnumerable<Type> @params = cas3.GetParameters().Select(param => param.ParameterType);
               if(@params.Count() != 2 || @params.Where(param => param != typeof(ObjectProperties)).Count() > 0)
                  throw new Exception(Main.debugPrefix + $"Case {cas3.Name}'s parameters are not correct");

               if(cas3.ReturnType != typeof(bool))
                  throw new Exception(Main.debugPrefix + $"Case {cas3.Name}'s return type is not a bool(come on man)");
            }
         }

         private static MethodInfo[] AllCases() {
            return typeof(ShouldHighlightCases).GetMethods(BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic).Where(cas3 => cas3.Name.StartsWith("CASE_")).ToArray();
         }

         public static List<string> SplitCaseIntoSegments(MethodInfo cas3) {
            List<string> stringSegments = cas3.Name.Split('_').ToList();
            stringSegments.RemoveAt(0);// removing CASE segment

            return stringSegments;
         }

         public static int CalculateCaseKey(ObjectType objType1, HighlightOptions highlightOption, ObjectType objType2) {
            return ((int)objType1 << 20) | (Utils.CountTrailingZeros((int)highlightOption) << 10) | (int)objType2;// every enum has 10 bits = 1024 possible combinations(their bits won't overlap = the result is unique for every case)
         }
      }
   }
}
