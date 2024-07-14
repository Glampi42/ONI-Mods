using HighlightOverlay.Components;
using HighlightOverlay.Enums;
using HighlightOverlay.Structs;
using HighlightOverlay;
using Klei.AI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using static Components;
using static ElementConverter;
using static Game;
using static HighlightOverlay.Structs.ObjectProperties;
using static UnityEngine.GraphicsBuffer;

namespace HighlightOverlay {
   public class HighlightMode : OverlayModes.Mode {
      public static HashedString ID = (HashedString)"glampi_HighlightMode";
      public bool isEnabled = false;

      private HashSet<GameObject> highlightedObjects = new HashSet<GameObject>();

      public bool dataIsClear = true;
      private bool defaultBackgroundColor = true;

      private ComputedShouldHighlightValues shouldHighlightObjects = new ComputedShouldHighlightValues();

      private readonly int targetLayer;
      private readonly int cameraLayerMask;

      public HighlightMode() {
         targetLayer = LayerMask.NameToLayer("MaskedOverlay");
         cameraLayerMask = LayerMask.GetMask("MaskedOverlay", "MaskedOverlayBG");
      }

      public override void Enable() {
         base.Enable();

         Camera.main.cullingMask |= cameraLayerMask;
         int mask = LayerMask.GetMask("MaskedOverlay");
         SelectTool.Instance.SetLayerMask(SelectTool.Instance.GetDefaultLayerMask() | mask);

         if(!dataIsClear)
         {
            foreach(var obj in highlightedObjects)
            {
               UpdateObjectHighlight(obj);
            }
         }

         isEnabled = true;
         UpdateObjectHighlight(Main.selectedObj);
         UpdateCellHighlight(Main.selectedCell, true);
         UpdateTileHighlight(Main.selectedTile, true);
      }

      public override void Update() {
         if(!ModConfig.Instance.AllowNotPaused && !Game.Instance.IsPaused)
         {
            if(!dataIsClear || !defaultBackgroundColor)
            {
               ClearAllData(true, true);
            }
         }
         else
         {
            if(dataIsClear)
            {
               if(Main.selectedObjProperties.objectType == ObjectType.NOTVALID || Main.highlightOption == HighlightOptions.NONE)
                  ClearAllData(!Main.preservePreviousHighlightOptions, false);

               dataIsClear = false;
               defaultBackgroundColor = false;

               if(Main.selectedObjProperties.objectType == ObjectType.NOTVALID || Main.highlightOption == HighlightOptions.NONE)
                  return;


               WorldContainer activeWorld = ClusterManager.Instance.activeWorld;
               Vector2I min = new Vector2I((int)activeWorld.minimumBounds.x, (int)activeWorld.minimumBounds.y);
               Vector2I max = new Vector2I((int)activeWorld.maximumBounds.x, (int)activeWorld.maximumBounds.y);
               Extents extents = new Extents(min.x, min.y, max.x - min.x, max.y - min.y);

               List<ScenePartitionerEntry> visibleObjects = new List<ScenePartitionerEntry>();

               GameScenePartitioner.Instance.GatherEntries(extents, GameScenePartitioner.Instance.pickupablesLayer, visibleObjects);// debris, duplicants, critters
               foreach(ScenePartitionerEntry visibleObject in visibleObjects)
               {
                  if(((Component)visibleObject.obj).TryGetComponent(out PrimaryElement _))
                  {
                     TryAddObjectToHighlightedObjects(((Component)visibleObject.obj).gameObject);
                  }
               }

               visibleObjects.Clear();

               if((Main.highlightFilters & HighlightFilters.BUILDINGS) != 0)// not necessary; for optimization
               {
                  GameScenePartitioner.Instance.GatherEntries(extents, GameScenePartitioner.Instance.completeBuildings, visibleObjects);
                  foreach(ScenePartitionerEntry visibleObject in visibleObjects)
                  {
                     BuildingComplete buildingComplete = (BuildingComplete)visibleObject.obj;

                     if(buildingComplete.gameObject.layer != 0)
                        continue;

                     if(Utils.IsTile(buildingComplete.gameObject, out _))
                        continue;// tiles are highlighted via the cells system

                     TryAddObjectToHighlightedObjects(buildingComplete.gameObject);
                  }

                  visibleObjects.Clear();
               }

               if((Main.highlightFilters & HighlightFilters.PLANTS) != 0)// not necessary; for optimization
               {
                  GameScenePartitioner.Instance.GatherEntries(extents, GameScenePartitioner.Instance.plants, visibleObjects);
                  foreach(ScenePartitionerEntry visibleObject in visibleObjects)
                  {
                     TryAddObjectToHighlightedObjects(((Component)visibleObject.obj).gameObject);
                  }

                  visibleObjects.Clear();
               }

               GatherSpecialObjectsOnBuildingsLayer(out HashSet<GameObject> buildings);// geysers, gravitas buildings
               foreach(GameObject building in buildings)
               {
                  TryAddObjectToHighlightedObjects(building);
               }

               GameScenePartitioner.Instance.GatherEntries(extents, GameScenePartitioner.Instance.collisionLayer, visibleObjects);// checking leftover things (radbolts)
               foreach(ScenePartitionerEntry visibleObject in visibleObjects)
               {
                  KPrefabID prefabID = ((Component)visibleObject.obj).GetComponent<KPrefabID>();
                  if(prefabID == null)
                     continue;

                  if(prefabID.HasTag(GameTags.HighEnergyParticle))
                  {
                     TryAddObjectToHighlightedObjects(prefabID.gameObject);
                  }
               }

               TryAddObjectToHighlightedObjects(Main.selectedObj);// it is not guaranteed that the selected obj will get checked in the methods above

               //----------------------Updating cells color----------------------DOWN
               for(int cell = 0; cell < Main.cellColors.Length; cell++)
               {
                  bool isTile = Utils.IsTile(cell, out SimCellOccupier tile);
                  if(isTile && tile.TryGetComponent(out KPrefabID prefabID))
                  {
                     if((Main.highlightFilters & HighlightFilters.TILES) != 0 && ComputeShouldHighlight(prefabID))
                     {
                        UpdateTileHighlight((tile.gameObject, cell));
                        continue;// don't have to calculate whether the cell should be highlighted because the tile is
                     }
                     else
                     {
                        if(!Main.preservePreviousHighlightOptions)
                           RemoveTileHighlight((tile.gameObject, cell));
                     }
                  }

                  if(!isTile || !tile.doReplaceElement)
                  {
                     if((Main.highlightFilters & HighlightFilters.CELLS) != 0 && ComputeShouldHighlight(null, Grid.Element[cell]))
                     {
                        UpdateCellHighlight(cell);
                     }
                     else
                     {
                        if(!Main.preservePreviousHighlightOptions)
                           RemoveCellHighlight(cell);
                     }
                  }
               }
               //----------------------Updating cells color----------------------UP
            }
         }
      }
      private static void GatherSpecialObjectsOnBuildingsLayer(out HashSet<GameObject> buildings) {
         buildings = new HashSet<GameObject>();

         for(int cell = 0; cell < Grid.CellCount; cell++)
         {
            if(Grid.IsVisible(cell))
            {
               GameObject building = Grid.Objects[cell, (int)ObjectLayer.Building];
               if(building != null && !building.TryGetComponent(out Building _))// geysers, gravitas buildings
                  buildings.Add(building);
            }
         }
      }
      private void TryAddObjectToHighlightedObjects(GameObject targetObject) {
         if(targetObject == null)
            return;

         if(highlightedObjects.Contains(targetObject))
            return;

         if(!targetObject.TryGetComponent(out KPrefabID targetID))
            return;

         if(targetID.HasTag(GameTags.GeyserFeature))
         {
            if(!ModConfig.Instance.HighlightBuriedGeysers && Utils.IsGeyserBuried(targetObject))
               return;
         }

         Vector2 min = targetID.PosMin();
         Vector2 max = targetID.PosMax();
         for(int x = (int)min.x; x <= max.x; x++)
         {
            for(int y = (int)min.y; y <= max.y; y++)
            {
               int cell = Grid.XYToCell(x, y);
               if(Grid.IsValidCell(cell) && !Grid.IsVisible(cell))
                  return;
            }
         }

         if(ComputeShouldHighlight(targetID))
         {
            highlightedObjects.Add(targetObject);
            UpdateObjectHighlight(targetObject);
         }
      }


      public void UpdateHighlightColor() {
         foreach(var target in highlightedObjects)
         {
            UpdateObjectHighlight(target);
         }

         for(int cell = 0; cell < Main.cellColors.Length; cell++)
         {
            if(Main.tileColors[cell] != Main.blackBackgroundColor)
            {
               if(Utils.IsTile(cell, out SimCellOccupier tile))
                  UpdateTileHighlight((tile.gameObject, cell));
            }
            else if(Main.cellColors[cell] != Main.blackBackgroundColor)
            {
               UpdateCellHighlight(cell);
            }
         }

         UpdateObjectHighlight(Main.selectedObj);
         UpdateCellHighlight(Main.selectedCell, true);
         UpdateTileHighlight(Main.selectedTile, true);
      }

      public void UpdateSelectedObjHighlight(GameObject oldSelected, int oldSelectedCell, (GameObject, int) oldSelectedTile) {
         if(!isEnabled)
            return;

         if(Main.selectedObj != oldSelected)
         {
            RemoveObjectHighlight(oldSelected, true);
            UpdateObjectHighlight(Main.selectedObj);
         }

         if(Main.selectedCell != oldSelectedCell)
         {
            RemoveCellHighlight(oldSelectedCell, true);
            UpdateCellHighlight(Main.selectedCell, true);
         }

         if(Main.selectedTile != oldSelectedTile)
         {
            RemoveTileHighlight(oldSelectedTile, true);
            UpdateTileHighlight(Main.selectedTile, true);
         }
      }

      private void UpdateObjectHighlight(GameObject obj) {
         if(obj == null)
            return;

         if(obj.TryGetComponent(out TintManagerCmp tintManager))
         {
            tintManager.SetTintColor(Main.highlightInTrueColor ? tintManager.actualTintColor : Main.whiteHighlightColor);
            tintManager.animController.SetLayer(targetLayer);
         }

         foreach(Storage storage in obj.GetComponents<Storage>())
         {
            if(!storage.defaultStoredItemModifers.Contains(Storage.StoredItemModifier.Hide))
            {
               foreach(GameObject storedItem in storage.items)
               {
                  if(storedItem != null && storedItem.TryGetComponent(out TintManagerCmp tintManager2))
                  {
                     tintManager2.SetTintColor(Main.highlightInTrueColor ? tintManager2.actualTintColor : Main.whiteHighlightColor);
                  }
               }
            }
         }
      }
      private void RemoveObjectHighlight(GameObject obj, bool removeSelectedHighlight = false) {
         if(obj == null)
            return;

         if((removeSelectedHighlight && highlightedObjects.Contains(obj)) ||
            (!removeSelectedHighlight && obj == Main.selectedObj))
            return;

         if(obj.TryGetComponent(out TintManagerCmp tintManager))
         {
            tintManager.ResetTintColor();
            tintManager.animController.SetLayer(obj.GetComponent<KPrefabID>().defaultLayer);
         }

         foreach(Storage storage in obj.GetComponents<Storage>())
         {
            if(!storage.defaultStoredItemModifers.Contains(Storage.StoredItemModifier.Hide))
            {
               foreach(GameObject storedItem in storage.items)
               {
                  if(storedItem != null && storedItem.TryGetComponent(out TintManagerCmp tintManager2))
                  {
                     tintManager2.ResetTintColor();
                  }
               }
            }
         }
      }

      private void UpdateCellHighlight(int cell, bool updateSelectedHighlight = false) {
         if(cell < 0)
            return;

         Color highlightColor = Main.highlightInTrueColor ? (Color)Grid.Element[cell].substance.uiColour : Main.whiteBackgroundColor;
         if(updateSelectedHighlight || cell == Main.selectedCell)
         {
            Main.selectedCellHighlightColor = highlightColor;
         }
         if(!updateSelectedHighlight)
         {
            Main.cellColors[cell] = highlightColor;
         }
      }
      private void RemoveCellHighlight(int cell, bool removeSelectedHighlight = false) {
         if(cell < 0)
            return;

         Color highlightColor = Main.blackBackgroundColor;
         if(removeSelectedHighlight)
         {
            if(cell == Main.selectedCell)
               Main.selectedCellHighlightColor = highlightColor;
         }
         else
         {
            Main.cellColors[cell] = highlightColor;
         }
      }

      private void UpdateTileHighlight((GameObject, int) tile, bool updateSelectedHighlight = false) {
         if(tile == default)
            return;

         Color highlightColor = Main.highlightInTrueColor ? (Color)tile.Item1.GetComponent<PrimaryElement>().Element.substance.uiColour : Main.whiteBackgroundColor;
         if(updateSelectedHighlight || tile == Main.selectedTile)
         {
            Main.selectedCellHighlightColor = highlightColor;
         }
         if(!updateSelectedHighlight)
         {
            Main.tileColors[tile.Item2] = highlightColor;
         }
      }
      private void RemoveTileHighlight((GameObject, int) tile, bool removeSelectedHighlight = false) {
         if(tile == default)
            return;

         Color highlightColor = Main.blackBackgroundColor;
         if(removeSelectedHighlight)
         {
            if(tile == Main.selectedTile)
               Main.selectedCellHighlightColor = highlightColor;
         }
         else
         {
            Main.tileColors[tile.Item2] = highlightColor;
         }
      }

      public void ClearAllData(bool setDefaultCellColors, bool forceRemoveHighlightedObjects) {
         if(forceRemoveHighlightedObjects || !Main.preservePreviousHighlightOptions)
            RemoveAllHighlightedObjects(true);

         shouldHighlightObjects.Clear();

         if(setDefaultCellColors)
         {
            Utils.SetDefaultCellColors();
            defaultBackgroundColor = true;
         }

         dataIsClear = true;
      }

      private void RemoveAllHighlightedObjects(bool clearSet) {
         foreach(var target in highlightedObjects)
         {
            RemoveHighlightedObject(target, false);
         }
         
         if(clearSet)
            highlightedObjects.Clear();
      }

      private void RemoveHighlightedObject(GameObject target, bool removeFromSet = true) {
         RemoveObjectHighlight(target);

         if(removeFromSet)
            highlightedObjects.Remove(target);
      }



      public bool ComputeShouldHighlight(KPrefabID targetObject, Element element = null, HighlightFilters givenFilter = HighlightFilters.NONE) {
         bool isObject = targetObject != null;

         if(!isObject && element == null)
            return false;

         if((Main.highlightFilters & HighlightFilters.STORED_ITEMS) != 0)
         {
            if(isObject)
            {
               //----------------------Stored items----------------------DOWN
               foreach(Storage storage in targetObject.GetComponents<Storage>())
               {
                  if(storage.showInUI)
                  {
                     foreach(GameObject storedItem in storage.items)
                     {
                        if(storedItem != null && storedItem.TryGetComponent(out KPrefabID storedID))
                        {
                           if(ComputeShouldHighlight(storedID, givenFilter: HighlightFilters.STORED_ITEMS))
                              return true;
                        }
                     }
                  }
               }
               //----------------------Stored items----------------------UP
               //----------------------Equipped items(clothing, suits)----------------------DOWN
               if(targetObject.TryGetComponent(out MinionIdentity minionIdentity))
               {
                  foreach(AssignableSlotInstance slot in minionIdentity.GetEquipment().Slots)
                  {
                     Equippable equippable = slot.assignable as Equippable;
                     if(equippable != null && equippable.isEquipped && equippable.TryGetComponent(out KPrefabID equippedID))
                     {
                        if(ComputeShouldHighlight(equippedID, givenFilter: HighlightFilters.STORED_ITEMS))
                           return true;
                     }
                  }
               }
               //----------------------Equipped items(clothing, suits)----------------------UP
            }
         }
         if((Main.highlightFilters & HighlightFilters.CONDUIT_CONTENTS) != 0)
         {
            //----------------------Conduit Contents----------------------DOWN
            if(isObject)
            {
               if(targetObject.TryGetComponent(out Conduit conduit))
               {
                  ConduitFlow conduitFlow = null;
                  if(conduit.ConduitType == ConduitType.Liquid && (Main.highlightFilters & HighlightFilters.LIQUID_CONTENTS) != 0)
                  {
                     conduitFlow = Game.Instance.liquidConduitFlow;
                  }
                  else if(conduit.ConduitType == ConduitType.Gas && (Main.highlightFilters & HighlightFilters.GAS_CONTENTS) != 0)
                  {
                     conduitFlow = Game.Instance.gasConduitFlow;
                  }

                  if(conduitFlow != null)
                  {
                     var contents = conduitFlow.GetContents(conduit.Cell);
                     if(contents.element != SimHashes.Vacuum && contents.mass > 0f)
                     {
                        if(ComputeShouldHighlight(null, ElementLoader.FindElementByHash(contents.element), givenFilter: HighlightFilters.CONDUIT_CONTENTS))
                           return true;
                     }
                  }
               }
               else if((Main.highlightFilters & HighlightFilters.RAILS_CONTENTS) != 0 && targetObject.TryGetComponent(out SolidConduit solidConduit))
               {
                  var contents = Game.Instance.solidConduitFlow.GetContents(Grid.PosToCell(solidConduit.Position));
                  if(contents.pickupableHandle != null && contents.pickupableHandle.IsValid())
                  {
                     var pickupable = Game.Instance.solidConduitFlow.GetPickupable(contents.pickupableHandle);
                     if(pickupable != null && pickupable.TryGetComponent(out KPrefabID pickupableID))
                     {
                        if(ComputeShouldHighlight(pickupableID, givenFilter: HighlightFilters.CONDUIT_CONTENTS))
                           return true;
                     }
                  }
               }
            }
            //----------------------Conduit Contents----------------------UP
         }

         ObjectProperties selectedProperties = Main.selectedObjProperties;

         ObjectType targetType;
         if(isObject)
         {
            targetType = ObjectProperties.GetObjectType(targetObject);
         }
         else
         {
            targetType = ObjectType.ELEMENT;
         }

         int dictKey = ShouldHighlightCases.CasesUtils.CalculateCaseKey(selectedProperties.objectType, Main.highlightOption, targetType);
         if(!ShouldHighlightCases.caseMethods.ContainsKey(dictKey))
            return false;

         if(givenFilter == HighlightFilters.NONE)// if filter is given it means it was already checked whether the obj applies to the highlight filters
         {
            if(!ApplyHighlightFilters(targetObject, element, isObject, out givenFilter))
               return false;
         }

         bool shouldHighlight;

         if(WasShouldHighlightAlreadyComputed(targetType, targetObject, element, givenFilter, out shouldHighlight))
         {
            return shouldHighlight;
         }

         ObjectProperties targetProperties;
         if(isObject)
         {
            targetProperties = new ObjectProperties(targetObject, targetType);
         }
         else
         {
            targetProperties = new ObjectProperties(element);
         }

         if((selectedProperties.highlightOptions & Main.highlightOption) == 0 ||
            (Main.highlightOption.Reverse() != HighlightOptions.NONE && ((targetProperties.highlightOptions & Main.highlightOption.Reverse()) == 0)))
         {
            shouldHighlight = false;
         }
         else
         {
            shouldHighlight = ShouldHighlightCases.caseMethods[dictKey](selectedProperties, targetProperties);
         }

         StoreShouldHighlight(targetType, targetObject, element, givenFilter, shouldHighlight);
         return shouldHighlight;
      }



      private bool WasShouldHighlightAlreadyComputed(ObjectType objectType, KPrefabID obj, Element element, HighlightFilters highlightFilter, out bool shouldHighlight) {
         if(shouldHighlightObjects.TryGetValue(objectType, obj, element, highlightFilter, out shouldHighlight))
            return true;

         return false;
      }
      private void StoreShouldHighlight(ObjectType objectType, KPrefabID obj, Element element, HighlightFilters highlightFilter, bool shouldHighlight) {
         shouldHighlightObjects.StoreValue(objectType, obj, element, highlightFilter, shouldHighlight);
      }

      /// <summary>
      /// Filters out objects that shouldn't be highlighted because of active highlight filters.
      /// </summary>
      /// <returns>True if the object passes all filters; false otherwise</returns>
      private bool ApplyHighlightFilters(KPrefabID targetObject, Element element, bool isObject, out HighlightFilters highlightFilter) {
         highlightFilter = HighlightFilters.NONE;

         if(Main.highlightFilters == HighlightFilters.ALL)
            return true;

         if(isObject)
         {
            if(!Main.cachedHighlightFilters.ContainsKey(targetObject.PrefabTag))
               throw new Exception(Main.debugPrefix + $"No value found for {targetObject.PrefabTag} inside of {nameof(Main.cachedHighlightFilters)} dictionary");

            highlightFilter = Main.cachedHighlightFilters[targetObject.PrefabTag];
         }
         else
         {
            if(!Main.cachedHighlightFiltersCells.ContainsKey(element.id))
               throw new Exception(Main.debugPrefix + $"No value found for {element.id} inside of {nameof(Main.cachedHighlightFiltersCells)} dictionary");

            highlightFilter = Main.cachedHighlightFiltersCells[element.id];
         }

         return highlightFilter == HighlightFilters.NONE || (Main.highlightFilters & highlightFilter) != 0;
      }



      public override void Disable() {
         base.Disable();
         Camera.main.cullingMask &= ~cameraLayerMask;
         SelectTool.Instance.ClearLayerMask();

         if(!Main.preservePreviousHighlightOptions)
         {
            ClearAllData(true, true);
         }
         else// removing highlight but keeping objects saved in the HashSet; doing nothing to cells
         {
            RemoveAllHighlightedObjects(false);

            dataIsClear = false;
         }

         RemoveObjectHighlight(Main.selectedObj, true);
         RemoveCellHighlight(Main.selectedCell, true);
         RemoveTileHighlight(Main.selectedTile, true);

         isEnabled = false;
      }

      public override HashedString ViewMode() => ID;
      public override string GetSoundName() => "SuitRequired";
   }
}
