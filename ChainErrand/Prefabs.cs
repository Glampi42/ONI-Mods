using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace ChainErrand {
   public static class Prefabs {
      private static Dictionary<System.Action, List<string>> requiredPrefabs = new Dictionary<System.Action, List<string>>();

      public static void RunAfterPrefabsInit(System.Action codeToRun, params string[] prefabNames) {
         requiredPrefabs.Add(codeToRun, new List<string>());

         FieldInfo[] infos = typeof(Prefabs).GetFields(BindingFlags.Static | BindingFlags.Public);

         foreach(string prefabName in prefabNames)
         {
            FieldInfo prefabInfo = infos.FirstOrDefault(field => field.Name == prefabName);
            if(prefabInfo != null)
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

         GameObject label = FilterToggleReversedPrefab.GetComponentInChildren<LocText>().gameObject;
         label.transform.SetAsLastSibling();// switching label and toggle places

         // flipping toggle's collider horizontally:
         RectTransformUtility.FlipLayoutOnAxis(FilterToggleReversedPrefab.GetComponentInChildren<MultiToggle>().transform.Find("collider").rectTransform(), 0, false, false);

         TryRunCode(nameof(FilterTogglePrefab));
         TryRunCode(nameof(FilterToggleReversedPrefab));
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

      public static void DestroyPrefabs() {
         foreach(var prefab in typeof(Prefabs).GetFields(BindingFlags.Public | BindingFlags.Static))
         {
            if(prefab.FieldType == typeof(GameObject))
            {
               UnityEngine.Object.Destroy((GameObject)prefab.GetValue(null));
               prefab.SetValue(null, null);
            }
         }
      }
   }
}