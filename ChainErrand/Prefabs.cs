using PeterHan.PLib.Core;
using PeterHan.PLib.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

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

      public static GameObject ArrowRightButtonPrefab = null;
      public static GameObject ArrowLeftButtonPrefab = null;
      public static void CreateArrowButtonsPrefab() {
         if(ArrowRightButtonPrefab != null)
            return;

         ArrowRightButtonPrefab = new GameObject("Button");
         var bgImage = ArrowRightButtonPrefab.AddComponent<Image>();
         bgImage.sprite = Assets.GetSprite("web_button");
         bgImage.type = Image.Type.Tiled;

         GameObject arrow = new GameObject("Arrow");
         arrow.SetParent(ArrowRightButtonPrefab);
         var arrowIcon = arrow.AddComponent<Image>();
         arrowIcon.sprite = PUITuning.Images.Arrow;
         arrowIcon.type = Image.Type.Simple;
         arrowIcon.preserveAspect = true;
         arrowIcon.SetNativeSize();

         StatePresentationSetting sps = new StatePresentationSetting() {
            sprite = PUITuning.Images.Arrow,
            color = PUITuning.Colors.ButtonPinkStyle.hoverColor,
            color_on_hover = PUITuning.Colors.ButtonPinkStyle.activeColor,
            use_color_on_hover = true,
            image_target = arrowIcon,
            name = "Arrow"
         };

         var mToggle = ArrowRightButtonPrefab.AddComponent<MultiToggle>();
         mToggle.toggle_image = bgImage;
         mToggle.play_sound_on_click = true;
         mToggle.states = [
            new ToggleState {
               sprite = Assets.GetSprite("web_button"),
               color = Main.whiteToggleSetting.inactiveColor,
               color_on_hover = Main.whiteToggleSetting.hoverColor,
               use_color_on_hover = true,
               additional_display_settings = [sps]
            }
         ];
         PCheckBox.SetCheckState(ArrowRightButtonPrefab, 0);

         GameObject disabled = new GameObject("DisabledIcon");
         disabled.SetParent(ArrowRightButtonPrefab);
         disabled.SetActive(true);

         Color disabledColor = new Color(0f, 0f, 0f, 0.4f);
         var disabledImage = disabled.AddComponent<Image>();
         disabledImage.color = disabledColor;

         var disabledToggle = disabled.AddComponent<MultiToggle>();
         disabledToggle.toggle_image = disabledImage;
         disabledToggle.play_sound_on_click = true;
         disabledToggle.states = [
            new ToggleState {
               color = disabledColor,
               use_color_on_hover = false,
               on_click_override_sound_path = "Negative",
               additional_display_settings = [
                  new StatePresentationSetting {// default value (to avoid crashes)
                     image_target = null,
                     color = Color.clear,
                     use_color_on_hover = false,
                  }
               ]
            }
         ];

         var layoutElem = ArrowRightButtonPrefab.AddOrGet<LayoutElement>();
         layoutElem.minHeight = 24f;
         layoutElem.minWidth = 24f;
         layoutElem.preferredHeight = 24f;
         layoutElem.preferredWidth = 24f;

         layoutElem = arrow.AddOrGet<LayoutElement>();// arrow icon's LayoutElement
         layoutElem.minHeight = 0f;
         layoutElem.minWidth = 0f;
         layoutElem.preferredHeight = 48f;
         layoutElem.preferredWidth = 48f;

         var rectTransform = arrow.rectTransform();
         if(rectTransform != null)
         {
            rectTransform.anchorMin = new Vector2(0.15f, 0.15f);
            rectTransform.anchorMax = new Vector2(0.85f, 0.85f);
            rectTransform.offsetMin = Vector2.zero;
            rectTransform.offsetMax = Vector2.zero;
         }

         rectTransform = disabled.rectTransform();
         if(rectTransform != null)
         {
            rectTransform.anchorMin = new Vector2(0f, 0f);
            rectTransform.anchorMax = new Vector2(1f, 1f);
            rectTransform.offsetMin = Vector2.zero;
            rectTransform.offsetMax = Vector2.zero;
         }


         ArrowLeftButtonPrefab = Util.KInstantiateUI(ArrowRightButtonPrefab);
         var scale = ArrowLeftButtonPrefab.transform.GetChild(0).localScale;
         scale.x = -1f;// flip the arrow on y-Axis
         ArrowLeftButtonPrefab.transform.GetChild(0).localScale = scale;

         TryRunCode(nameof(ArrowRightButtonPrefab));
         TryRunCode(nameof(ArrowLeftButtonPrefab));
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