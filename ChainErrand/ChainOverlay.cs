using ChainErrand.Structs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using static STRINGS.DUPLICANTS.PERSONALITIES;

namespace ChainErrand {
   public class ChainOverlay : OverlayModes.Mode {
      public static readonly HashedString ID = "glampi_ChainOverlay";

      public override HashedString ViewMode() => ID;

      public override string GetSoundName() => nameof(OverlayModes.Priorities);

      private int layersCount;

      private readonly int targetLayer;
      private readonly int targetUILayer;
      private readonly int cameraLayerMask;

      private UIPool<LocText> uiGOPool;
      private HashSet<Workable> visibleErrands = new HashSet<Workable>();
      private HashSet<GameObject> visibleErrands_GOs = new HashSet<GameObject>();
      public Dictionary<GameObject, ChainNumber> chainNumbers = new Dictionary<GameObject, ChainNumber>();


      public ChainOverlay() {
         layersCount = (int)ObjectLayer.NumLayers;// Peter Han told it's determined at runtime, so yeah

         targetLayer = LayerMask.NameToLayer("MaskedOverlay");
         targetUILayer = LayerMask.NameToLayer("UI");
         cameraLayerMask = LayerMask.GetMask("MaskedOverlay", "MaskedOverlayBG");
      }

      public override void Enable() {
         base.Enable();

         Camera.main.cullingMask |= cameraLayerMask;
         int mask = LayerMask.GetMask("MaskedOverlay");
         SelectTool.Instance.SetLayerMask(SelectTool.Instance.GetDefaultLayerMask() | mask);

         uiGOPool = new UIPool<LocText>(ChainNumberPrefab.GetChainNumberPrefab());
      }

      public override void Disable() {
         base.Disable();

         Camera.main.cullingMask &= ~cameraLayerMask;
         SelectTool.Instance.ClearLayerMask();

         visibleErrands.Clear();
         foreach(var errand_go in visibleErrands_GOs)
         {
            if(errand_go.TryGetComponent(out KBatchedAnimController animController))
            {
               ResetDisplayValues(animController);
            }
         }
         visibleErrands_GOs.Clear();

         chainNumbers.Clear();
         uiGOPool.ClearAll();
      }

      public override void Update() {
         Grid.GetVisibleExtents(out Vector2I min, out Vector2I max);
         Extents extents = new Extents(min.x, min.y, max.x - min.x, max.y - min.y);

         OverlayModes.Mode.RemoveOffscreenTargets(this.visibleErrands, min, max, errand => {
            visibleErrands_GOs.Remove(errand.gameObject);
         });

         List<ScenePartitionerEntry> visibleObjects = new List<ScenePartitionerEntry>();

         GameScenePartitioner.Instance.GatherEntries(extents, GameScenePartitioner.Instance.prioritizableObjects, visibleObjects);
         foreach(ScenePartitionerEntry visibleObject in visibleObjects)
         {
            if(visibleErrands_GOs.Contains(((Component)visibleObject.obj).gameObject))
               return;

            if(AppliesToToolFilters(((Component)visibleObject.obj).gameObject, out Workable errand, out bool isUI))
            {
               AddToVisible(errand, isUI);
            }
         }
      }
      private bool AppliesToToolFilters(GameObject errand_go, out Workable errand, out bool isUI) {
         errand = default;

         isUI = false;

         if(ChainToolFilters.All.IsOn())
         {
            if(CheckConstruction(out errand))
               return true;

            if(CheckDigging(out errand, ref isUI))
               return true;


         }

         return false;


         bool CheckConstruction(out Workable workable) {
            workable = default;

            if(errand_go.TryGetComponent(out Constructable constructable))
            {
               workable = constructable;
               return true;
            }
            if(errand_go.TryGetComponent(out Deconstructable deconstructable) &&
               deconstructable.IsMarkedForDeconstruction())
            {
               workable = deconstructable;
               return true;
            }

            return false;
         }

         bool CheckDigging(out Workable workable, ref bool isUI_inner) {
            workable = default;

            if(errand_go.TryGetComponent(out Diggable diggable))
            {
               workable = diggable;
               isUI_inner = true;
               return true;
            }

            return false;
         }

         bool CheckSweeping(out Workable workable) {
            workable = default;

            if(errand_go.TryGetComponent(out Pickupable pickupable))
            {
               workable = pickupable;
               return true;
            }

            return false;
         }
      }
      private void AddToVisible(Workable errand, bool isUI) {
         visibleErrands.Add(errand);
         visibleErrands_GOs.Add(errand.gameObject);

         if(errand.gameObject.TryGetComponent(out KBatchedAnimController animController))
         {
            animController.SetLayer(isUI ? targetUILayer : targetLayer);
         }
      }


      public void CreateChainNumber(GameObject parentGO) {
         if(parentGO == null)
            return;

         ChainNumber chainNumber = new ChainNumber(uiGOPool.GetFreeElement(GameScreenManager.Instance.worldSpaceCanvas), parentGO, Utils.RandomColor(0.6, 0.8, 0.6, 0.9, Main.random), 1);

         chainNumbers.Add(parentGO, chainNumber);
      }
   }
}