﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using TemplateClasses;
using UnityEngine;
using UnityEngine.UI;

namespace HighlightOverlay {
   public static class Prefabs {
      private static Dictionary<System.Action, List<string>> requiredPrefabs = new Dictionary<System.Action, List<string>>();

      public static void RunAfterPrefabsInit(System.Action codeToRun, params string[] prefabNames) {
         requiredPrefabs.Add(codeToRun, new List<string>());

         FieldInfo[] infos = typeof(Prefabs).GetFields(BindingFlags.Static | BindingFlags.Public);

         foreach(string prefabName in prefabNames)
         {
            FieldInfo prefabInfo = infos.FirstOrDefault(field => field.Name == prefabName);
            if (prefabInfo != null)
            {
               if(prefabInfo.GetValue(null) == null)
               {
                  requiredPrefabs[codeToRun].Add(prefabName);
               }
            }
         }

         if(requiredPrefabs[codeToRun].Count == 0)
         {
            codeToRun();
            requiredPrefabs.Remove(codeToRun);
         }
      }

      public static GameObject FilterTogglePrefab = null;
      public static GameObject FilterToggleReversedPrefab = null;
      public static void CreateFilterTogglePrefabs() {
         if(FilterTogglePrefab != null)
            return;

         FilterTogglePrefab = Util.KInstantiateUI(OverlayLegend.Instance.toolParameterMenuPrefab.GetComponent<ToolParameterMenu>()?.widgetPrefab);

         if(FilterTogglePrefab?.GetComponentInChildren<MultiToggle>() == null || FilterTogglePrefab?.GetComponentInChildren<LocText>() == null)
            throw new Exception(Main.debugPrefix + $"Could not create {nameof(FilterTogglePrefab)}");

         MultiToggle toggle = FilterTogglePrefab.GetComponentInChildren<MultiToggle>();
         toggle.states[1].additional_display_settings = toggle.states[0].additional_display_settings;// grey out the toggle on hover also if it is active


         FilterToggleReversedPrefab = Util.KInstantiateUI(FilterTogglePrefab);

         // switching the order of the label and the toggle:
         FilterToggleReversedPrefab.transform.localScale = new Vector3(-FilterToggleReversedPrefab.transform.localScale.x,
                                                                        FilterToggleReversedPrefab.transform.localScale.y,
                                                                        FilterToggleReversedPrefab.transform.localScale.z);
         FilterToggleReversedPrefab.transform.ForEachChild(child => {
            var scale = child.rectTransform().localScale;
            child.rectTransform().localScale = new Vector3(-scale.x, scale.y, scale.z);
         }, true);

         // increasing the spacing between the toggle and the label:
         FilterToggleReversedPrefab.GetComponent<HorizontalLayoutGroup>().spacing *= 2;

         TryRunCode(nameof(FilterTogglePrefab));
         TryRunCode(nameof(FilterToggleReversedPrefab));
      }

      public static GameObject LabelPrefab = null;
      public static void CreateLabelPrefab() {
         if(LabelPrefab != null)
            return;

         LabelPrefab = Util.KInstantiateUI(OverlayLegend.Instance.toolParameterMenuPrefab.GetComponent<ToolParameterMenu>()?.widgetPrefab?.GetComponentInChildren<LocText>()?.gameObject);

         if(LabelPrefab?.GetComponent<LocText>() == null)
            throw new Exception(Main.debugPrefix + $"Could not create {nameof(LabelPrefab)}");

         LabelPrefab.GetComponent<LayoutElement>().minWidth = -1f;

         ContentSizeFitter csf = LabelPrefab.AddComponent<ContentSizeFitter>();
         csf.horizontalFit = ContentSizeFitter.FitMode.PreferredSize;
         csf.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

         TryRunCode(nameof(LabelPrefab));
      }

      public static GameObject CheckboxPrefab = null;
      public static void CreateCheckboxPrefab() {
         if(CheckboxPrefab != null)
            return;

         var options = Util.KInstantiateUI<OptionsMenuScreen>(ScreenPrefabs.Instance.OptionsScreen?.gameObject);

         CheckboxPrefab = Util.KInstantiateUI(options.gameOptionsScreenPrefab.defaultToCloudSaveToggle);
         UnityEngine.Object.DontDestroyOnLoad(CheckboxPrefab);

         UnityEngine.Object.Destroy(options.gameObject);

         if(CheckboxPrefab?.GetComponent<KButton>() == null)
            throw new Exception(Main.debugPrefix + $"Could not create {nameof(CheckboxPrefab)}");

         if(CheckboxPrefab.TryGetComponent(out ToolTip tt))
            UnityEngine.Object.Destroy(tt);

         TryRunCode(nameof(CheckboxPrefab));
      }

      private static void TryRunCode(string createdPrefabName) {
         List<System.Action> keys = requiredPrefabs.Keys.ToList();
         foreach(var code in keys)
         {
            if(requiredPrefabs[code].Contains(createdPrefabName))
            {
               requiredPrefabs[code].Remove(createdPrefabName);

               if(requiredPrefabs[code].Count == 0)
               {
                  code();

                  requiredPrefabs.Remove(code);
               }
            }
         }
      }
   }
}
