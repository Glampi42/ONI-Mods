using AdvancedFlowManagement;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using HarmonyLib;
using UnityEngine.UI;
using PeterHan.PLib.Core;

namespace AdvancedFlowManagement {
   public class CrossingSprite {
      public static readonly Color normalColor = new Color(0.2f, 0.75f, 0.2f, 1f);
      public static readonly Color dimColor = new Color(0.12f, 0.45f, 0.12f, 1f);
      public static readonly Color highlightedColor = new Color(0.95f, 0.29f, 0.28f, 1f);
      public static readonly Color dimHighlightedColor = new Color(0.57f, 0.174f, 0.168f, 1f);

      public static void Create(CrossingCmp crossingCmp, bool forceFirstSibling) {
         GameObject crossingIcon_go = Util.KInstantiateUI(crossingIconPrefab, GameScreenManager.Instance.worldSpaceCanvas, true);
         crossingCmp.crossingIcon = crossingIcon_go;
         crossingIcon_go.name = "JunctionIcon";

         if(!forceFirstSibling && Utils.TryGetVisualEndpoint(crossingCmp, out _))
         {
            if(Utils.TryGetEndpointVisualizerObj(crossingCmp, false, out GameObject endpoint_go, out _))
            {
               crossingIcon_go.transform.SetSiblingIndex(endpoint_go.transform.GetSiblingIndex() + 1);// so that the icon is displayed above the endpoint-icon
            }
            // else the crossingIcon will be displayed above all(is the case for modded buildings' endpoints)
         }
         else
         {
            crossingIcon_go.transform.SetAsFirstSibling();// needed so that the icon is displayed underneath the SelectMarker
         }

         crossingIcon_go.transform.position = Grid.CellToPosCCC(crossingCmp.crossingCell, Grid.SceneLayer.SceneMAX);

         Update(crossingCmp);
      }

      public static void Update(CrossingCmp crossingCmp) {
         GameObject crossingIcon_go = crossingCmp.crossingIcon;
         Image crossingIcon = crossingIcon_go.GetComponentInChildren<Image>();
         Sprite newSprite = Utils.GetSpriteForCrossing(crossingCmp, out Quaternion rotation);
         bool hasIconChanged = crossingIcon.sprite != null && (!crossingIcon.sprite.Equals(newSprite) || crossingIcon_go.transform.rotation != rotation);

         if(hasIconChanged)
         {
            GameObject pulsing_go = crossingIcon_go.transform.GetChild(0).gameObject;
            if(pulsing_go.TryGetComponent(out SizePulse currentSizePulse))
            {
               currentSizePulse.state = 0;
               currentSizePulse.cur = currentSizePulse.from;// resetting the SizePulse
            }
            else
            {
               SizePulse sizePulse = pulsing_go.AddComponent<SizePulse>();
               sizePulse.speed = 13.3f;
               sizePulse.multiplier = 0.75f;
               sizePulse.updateWhenPaused = true;
               sizePulse.onComplete += () => UnityEngine.Object.Destroy(sizePulse);
            }
         }

         bool isIllegal;
         crossingIcon.sprite = newSprite;
         crossingIcon.color = (isIllegal = !IsCrossingConfigurationLegal(crossingCmp)) ? highlightedColor : normalColor;
         crossingIcon_go.transform.rotation = rotation;
         crossingIcon.enabled = Utils.ConduitTypeToOverlayModeID(crossingCmp.conduitType).Equals(OverlayScreen.Instance.GetMode()) &&
            Utils.ConduitTypeToShowCrossingsBool(crossingCmp.conduitType);

         crossingCmp.isIllegal = isIllegal;
      }

      public static void UpdateVisibility(CrossingCmp crossingCmp) {
         GameObject crossingIcon_go = crossingCmp.crossingIcon;
         Image crossingIcon = crossingIcon_go.GetComponentInChildren<Image>();

         crossingIcon.enabled = Utils.ConduitTypeToOverlayModeID(crossingCmp.conduitType).Equals(OverlayScreen.Instance.GetMode()) &&
            Utils.ConduitTypeToShowCrossingsBool(crossingCmp.conduitType);
      }
      public static void Hide(CrossingCmp crossingCmp) {
         GameObject crossingIcon_go = crossingCmp.crossingIcon;
         Image crossingIcon = crossingIcon_go.GetComponentInChildren<Image>();

         crossingIcon.enabled = false;
      }

      public static void UpdateIsIllegal(CrossingCmp crossingCmp) {
         GameObject crossingIcon_go = crossingCmp.crossingIcon;
         Image crossingIcon = crossingIcon_go.GetComponentInChildren<Image>();

         bool isIllegal;
         crossingIcon.color = (isIllegal = !IsCrossingConfigurationLegal(crossingCmp)) ? highlightedColor : normalColor;

         crossingCmp.isIllegal = isIllegal;
      }

      private static bool IsCrossingConfigurationLegal(CrossingCmp crossingCmp) {
         int inputscount = 0;
         int outputscount = 0;

         if(Utils.TryGetRealEndpointType(crossingCmp, out Endpoint endpoint_type))
         {
            if(endpoint_type == Endpoint.Sink)
               outputscount++;
            else
               inputscount++;
         }
         string crossingID = crossingCmp.crossingID;
         inputscount += crossingID.Count(ch4r => ch4r == '1');
         outputscount += crossingID.Count(ch4r => ch4r == '2');
         return inputscount > 0 && outputscount > 0;
      }

      public static void Remove(CrossingCmp crossingCmp) {
         GameObject crossingIcon_go = crossingCmp.crossingIcon;
         UnityEngine.Object.Destroy(crossingIcon_go);
      }

      public static GameObject crossingIconPrefab = null;
      public static void CreateCrossingIconPrefab() {
         crossingIconPrefab = new GameObject("JunctionIconPrefab");
         crossingIconPrefab.transform.localScale = new Vector3(0.0117f, 0.0117f, 1f);
         crossingIconPrefab.SetActive(false);

         GameObject pulsing_go = new GameObject("SizePulsingGO");// Needed so that the localScale is a reasonable number(not 0.0119)
         pulsing_go.transform.SetParent(crossingIconPrefab.transform, false);

         Image image = pulsing_go.AddComponent<Image>();
         image.type = Image.Type.Simple;
         image.raycastTarget = false;
         image.color = normalColor;
      }
   }
}
