using HighlightOverlay.Attributes;
using HighlightOverlay.Enums;
using HighlightOverlay.Structs;
using HighlightOverlay;
using System;
using System.Collections.Generic;
using System.Diagnostics.Tracing;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;
using VoronoiTree;
using PeterHan.PLib.Core;
using PeterHan.PLib.UI;
using static STRINGS.UI.TOOLS;
using KSerialization;
using static HighlightOverlay.Strings.MYSTRINGS.UI.OVERLAYS.HIGHLIGHTMODE;

namespace HighlightOverlay {
   public class HighlightOverlayDiagram : MonoBehaviour {
      public static GameObject diagramPrefab = null;

      public KButton trueColorCheckbox;
      public Image trueColorCheckmark;

      public KButton preservePreviousOptionsCheckbox;
      public Image preservePreviousOptionsCheckmark;


      public GameObject gameNotPausedContainer;
      public GameObject gameNotPausedLabel;

      public GameObject noObjectSelectedContainer;
      public GameObject noObjectSelectedLabel;

      public LocText selectedObjectTypeLabel;

      [HighlightOption(HighlightOptions.CONSIDEROPTION1)]
      public GameObject considerOption1;
      [HighlightOptionLabel(HighlightOptions.CONSIDEROPTION1)]
      public GameObject considerOption1_Label;
      public MultiToggle considerOption1_Toggle;

      [HighlightOption(HighlightOptions.CONSUMERS)]
      public GameObject consumers;
      [HighlightOptionLabel(HighlightOptions.CONSUMERS)]
      public GameObject consumers_Label;
      public MultiToggle consumers_Toggle;

      [HighlightOption(HighlightOptions.PRODUCERS)]
      public GameObject producers;
      [HighlightOptionLabel(HighlightOptions.PRODUCERS)]
      public GameObject producers_Label;
      public MultiToggle producers_Toggle;

      [HighlightOption(HighlightOptions.CONSUMABLES)]
      public GameObject consumables;
      [HighlightOptionLabel(HighlightOptions.CONSUMABLES)]
      public GameObject consumables_Label;
      public MultiToggle consumables_Toggle;

      [HighlightOption(HighlightOptions.PRODUCE)]
      public GameObject produce;
      [HighlightOptionLabel(HighlightOptions.PRODUCE)]
      public GameObject produce_Label;
      public MultiToggle produce_Toggle;

      [HighlightOption(HighlightOptions.BUILDINGMATERIAL)]
      public GameObject buildingMaterial;
      [HighlightOptionLabel(HighlightOptions.BUILDINGMATERIAL)]
      public GameObject buildingMaterial_Label;
      public MultiToggle buildingMaterial_Toggle;

      [HighlightOption(HighlightOptions.COPIES)]
      public GameObject copies;
      [HighlightOptionLabel(HighlightOptions.COPIES)]
      public GameObject copies_Label;
      public MultiToggle copies_Toggle;

      [HighlightOption(HighlightOptions.EXACTCOPIES)]
      public GameObject exactCopies;
      [HighlightOptionLabel(HighlightOptions.EXACTCOPIES)]
      public GameObject exactCopies_Label;
      public MultiToggle exactCopies_Toggle;

      public GameObject filtersContainer;


      public static void InitPrefab() {
         diagramPrefab = new GameObject(nameof(HighlightOverlayDiagram));
         diagramPrefab.SetActive(true);
         diagramPrefab.AddOrGet<RectTransform>();
         //UnityEngine.Object.DontDestroyOnLoad(prefab);

         VerticalLayoutGroup layoutGroup = diagramPrefab.AddComponent<VerticalLayoutGroup>();
         layoutGroup.childControlHeight = true;
         layoutGroup.childControlWidth = true;
         layoutGroup.spacing = 12f;

         RectTransform rectTransform = diagramPrefab.rectTransform();
         rectTransform.anchorMin = new Vector2(0.0f, 0.5f);
         rectTransform.anchorMax = new Vector2(1f, 0.5f);
         rectTransform.offsetMax = new Vector2();
         rectTransform.offsetMin = new Vector2();

         HighlightOverlayDiagram diagram = diagramPrefab.AddComponent<HighlightOverlayDiagram>();
         //----------------------------Highlight Options----------------------------DOWN
         GameObject optionsHeaderContainer = new GameObject(nameof(optionsHeaderContainer));
         optionsHeaderContainer.transform.SetParent(diagramPrefab.transform);
         optionsHeaderContainer.SetActive(true);
         HorizontalLayoutGroup layoutGrouph = optionsHeaderContainer.AddComponent<HorizontalLayoutGroup>();
         layoutGrouph.childControlHeight = false;
         layoutGrouph.childControlWidth = false;
         layoutGrouph.childAlignment = TextAnchor.MiddleLeft;

         GameObject optionsHeader = Util.KInstantiateUI(Prefabs.LabelPrefab, optionsHeaderContainer, true);
         LocText headerLabel = optionsHeader.GetComponent<LocText>();
         headerLabel.text = Strings.MYSTRINGS.UI.OVERLAYS.HIGHLIGHTMODE.HIGHLIGHTOPTIONS.HEADER;
         headerLabel.fontSize *= 1.2f;
         //----------------------------Extra Options----------------------------DOWN
         GameObject extraOptionsContainer = new GameObject(nameof(extraOptionsContainer));
         extraOptionsContainer.transform.SetParent(diagramPrefab.transform);
         extraOptionsContainer.SetActive(true);
         layoutGrouph = extraOptionsContainer.AddComponent<HorizontalLayoutGroup>();
         layoutGrouph.childControlHeight = true;
         layoutGrouph.childControlWidth = true;
         layoutGrouph.childForceExpandHeight = true;
         layoutGrouph.childForceExpandWidth = true;
         layoutGrouph.spacing = 20f;
         layoutGrouph.childAlignment = TextAnchor.MiddleCenter;

         GameObject trueColorContainer = new GameObject(nameof(trueColorContainer));
         trueColorContainer.transform.SetParent(extraOptionsContainer.transform);
         trueColorContainer.SetActive(true);
         layoutGrouph = trueColorContainer.AddComponent<HorizontalLayoutGroup>();
         layoutGrouph.childControlHeight = false;
         layoutGrouph.childControlWidth = false;
         layoutGrouph.childForceExpandHeight = false;
         layoutGrouph.childForceExpandWidth = false;
         layoutGrouph.spacing = 5f;
         layoutGrouph.childAlignment = TextAnchor.MiddleCenter;

         GameObject trueColorLabel = Util.KInstantiateUI(Prefabs.LabelPrefab, trueColorContainer, true);
         trueColorLabel.name = nameof(trueColorLabel);
         trueColorLabel.GetComponent<LocText>().text = Strings.MYSTRINGS.UI.OVERLAYS.HIGHLIGHTMODE.HIGHLIGHTOPTIONS.EXTRAOPTIONS.TRUECOLOR_LABEL;

         GameObject trueColorCheckbox_go = Util.KInstantiateUI(Prefabs.CheckboxPrefab, trueColorContainer, true);
         trueColorCheckbox_go.name = nameof(trueColorCheckbox_go);
         diagram.trueColorCheckmark = trueColorCheckbox_go.transform.GetChild(0).gameObject.GetComponent<Image>();
         diagram.trueColorCheckbox = trueColorCheckbox_go.GetComponent<KButton>();

         GameObject preserveOptionsContainer = new GameObject(nameof(preserveOptionsContainer));
         preserveOptionsContainer.transform.SetParent(extraOptionsContainer.transform);
         preserveOptionsContainer.SetActive(true);
         layoutGrouph = preserveOptionsContainer.AddComponent<HorizontalLayoutGroup>();
         layoutGrouph.childControlHeight = false;
         layoutGrouph.childControlWidth = false;
         layoutGrouph.childForceExpandHeight = false;
         layoutGrouph.childForceExpandWidth = false;
         layoutGrouph.spacing = 5f;
         layoutGrouph.childAlignment = TextAnchor.MiddleCenter;

         GameObject preservePreviousOptionsLabel = Util.KInstantiateUI(Prefabs.LabelPrefab, preserveOptionsContainer, true);
         preservePreviousOptionsLabel.name = nameof(preservePreviousOptionsLabel);
         preservePreviousOptionsLabel.GetComponent<LocText>().text = Strings.MYSTRINGS.UI.OVERLAYS.HIGHLIGHTMODE.HIGHLIGHTOPTIONS.EXTRAOPTIONS.PRESERVEPREVIOUSOPTIONS_LABEL;

         GameObject preservePreviousOptionsCheckbox_go = Util.KInstantiateUI(Prefabs.CheckboxPrefab, preserveOptionsContainer, true);
         preservePreviousOptionsCheckbox_go.name = nameof(preservePreviousOptionsCheckbox_go);
         diagram.preservePreviousOptionsCheckmark = preservePreviousOptionsCheckbox_go.transform.GetChild(0).gameObject.GetComponent<Image>();
         diagram.preservePreviousOptionsCheckbox = preservePreviousOptionsCheckbox_go.GetComponent<KButton>();
         //----------------------------Extra Options----------------------------UP
         //----------------------------Options----------------------------DOWN
         diagram.gameNotPausedContainer = new GameObject(nameof(gameNotPausedContainer));
         diagram.gameNotPausedContainer.transform.SetParent(diagramPrefab.transform);
         diagram.gameNotPausedContainer.SetActive(true);
         layoutGrouph = diagram.gameNotPausedContainer.AddComponent<HorizontalLayoutGroup>();
         layoutGrouph.childControlHeight = true;
         layoutGrouph.childControlWidth = true;
         layoutGrouph.childAlignment = TextAnchor.MiddleCenter;

         diagram.gameNotPausedLabel = Util.KInstantiateUI(Prefabs.LabelPrefab, diagram.gameNotPausedContainer, true);
         LocText notPausedLabel = diagram.gameNotPausedLabel.GetComponent<LocText>();
         notPausedLabel.text = Strings.MYSTRINGS.UI.OVERLAYS.HIGHLIGHTMODE.HIGHLIGHTOPTIONS.NOTPAUSED;
         notPausedLabel.alignment = TMPro.TextAlignmentOptions.Center;
         Color textColor = notPausedLabel.color;
         textColor.a = 0.8f;
         notPausedLabel.color = textColor;

         diagram.noObjectSelectedContainer = new GameObject(nameof(noObjectSelectedContainer));
         diagram.noObjectSelectedContainer.transform.SetParent(diagramPrefab.transform);
         diagram.noObjectSelectedContainer.SetActive(true);
         layoutGrouph = diagram.noObjectSelectedContainer.AddComponent<HorizontalLayoutGroup>();
         layoutGrouph.childControlHeight = true;
         layoutGrouph.childControlWidth = true;
         layoutGrouph.childAlignment = TextAnchor.MiddleCenter;

         diagram.noObjectSelectedLabel = Util.KInstantiateUI(Prefabs.LabelPrefab, diagram.noObjectSelectedContainer, true);
         LocText noObjectLabel = diagram.noObjectSelectedLabel.GetComponent<LocText>();
         noObjectLabel.text = Strings.MYSTRINGS.UI.OVERLAYS.HIGHLIGHTMODE.HIGHLIGHTOPTIONS.NOOBJECTSELECTED;
         noObjectLabel.alignment = TMPro.TextAlignmentOptions.Center;
         textColor = noObjectLabel.color;
         textColor.a = 0.8f;
         noObjectLabel.color = textColor;

         diagram.selectedObjectTypeLabel = Util.KInstantiateUI(Prefabs.LabelPrefab, diagramPrefab, true).GetComponent<LocText>();
         diagram.selectedObjectTypeLabel.gameObject.name = nameof(selectedObjectTypeLabel);


         GameObject highlightOptionsContainer = new GameObject(nameof(highlightOptionsContainer));
         highlightOptionsContainer.transform.SetParent(diagramPrefab.transform);
         highlightOptionsContainer.SetActive(true);
         layoutGroup = highlightOptionsContainer.AddComponent<VerticalLayoutGroup>();
         layoutGroup.childControlHeight = true;
         layoutGroup.childControlWidth = true;
         layoutGroup.spacing = 0f;
         layoutGroup.childAlignment = TextAnchor.UpperLeft;

         diagram.considerOption1 = Util.KInstantiateUI(Prefabs.FilterToggleReversedPrefab, highlightOptionsContainer, true);
         diagram.considerOption1_Label = diagram.considerOption1.GetComponentInChildren<LocText>().gameObject;
         diagram.considerOption1_Toggle = diagram.considerOption1.GetComponentInChildren<MultiToggle>();
         diagram.considerOption1_Toggle.ChangeState(0);

         diagram.consumers = Util.KInstantiateUI(Prefabs.FilterTogglePrefab, highlightOptionsContainer, true);
         LocText optionName = diagram.consumers.GetComponentInChildren<LocText>();
         optionName.text = Strings.MYSTRINGS.UI.OVERLAYS.HIGHLIGHTMODE.HIGHLIGHTOPTIONS.CONSUMERS;
         diagram.consumers_Label = optionName.gameObject;
         diagram.consumers_Toggle = diagram.consumers.GetComponentInChildren<MultiToggle>();
         diagram.consumers_Toggle.ChangeState(0);

         diagram.producers = Util.KInstantiateUI(Prefabs.FilterTogglePrefab, highlightOptionsContainer, true);
         optionName = diagram.producers.GetComponentInChildren<LocText>();
         optionName.text = Strings.MYSTRINGS.UI.OVERLAYS.HIGHLIGHTMODE.HIGHLIGHTOPTIONS.PRODUCERS;
         diagram.producers_Label = optionName.gameObject;
         diagram.producers_Toggle = diagram.producers.GetComponentInChildren<MultiToggle>();
         diagram.producers_Toggle.ChangeState(0);

         diagram.consumables = Util.KInstantiateUI(Prefabs.FilterTogglePrefab, highlightOptionsContainer, true);
         optionName = diagram.consumables.GetComponentInChildren<LocText>();
         optionName.text = Strings.MYSTRINGS.UI.OVERLAYS.HIGHLIGHTMODE.HIGHLIGHTOPTIONS.CONSUMABLES;
         diagram.consumables_Label = optionName.gameObject;
         diagram.consumables_Toggle = diagram.consumables.GetComponentInChildren<MultiToggle>();
         diagram.consumables_Toggle.ChangeState(0);

         diagram.produce = Util.KInstantiateUI(Prefabs.FilterTogglePrefab, highlightOptionsContainer, true);
         optionName = diagram.produce.GetComponentInChildren<LocText>();
         optionName.text = Strings.MYSTRINGS.UI.OVERLAYS.HIGHLIGHTMODE.HIGHLIGHTOPTIONS.PRODUCE;
         diagram.produce_Label = optionName.gameObject;
         diagram.produce_Toggle = diagram.produce.GetComponentInChildren<MultiToggle>();
         diagram.produce_Toggle.ChangeState(0);

         diagram.buildingMaterial = Util.KInstantiateUI(Prefabs.FilterTogglePrefab, highlightOptionsContainer, true);
         optionName = diagram.buildingMaterial.GetComponentInChildren<LocText>();
         optionName.text = Strings.MYSTRINGS.UI.OVERLAYS.HIGHLIGHTMODE.HIGHLIGHTOPTIONS.BUILDINGMATERIAL;
         diagram.buildingMaterial_Label = optionName.gameObject;
         diagram.buildingMaterial_Toggle = diagram.buildingMaterial.GetComponentInChildren<MultiToggle>();
         diagram.buildingMaterial_Toggle.ChangeState(0);

         diagram.copies = Util.KInstantiateUI(Prefabs.FilterTogglePrefab, highlightOptionsContainer, true);
         optionName = diagram.copies.GetComponentInChildren<LocText>();
         optionName.text = Strings.MYSTRINGS.UI.OVERLAYS.HIGHLIGHTMODE.HIGHLIGHTOPTIONS.COPIES;
         diagram.copies_Label = optionName.gameObject;
         diagram.copies_Toggle = diagram.copies.GetComponentInChildren<MultiToggle>();
         diagram.copies_Toggle.ChangeState(0);

         diagram.exactCopies = Util.KInstantiateUI(Prefabs.FilterTogglePrefab, highlightOptionsContainer, true);
         optionName = diagram.exactCopies.GetComponentInChildren<LocText>();
         optionName.text = Strings.MYSTRINGS.UI.OVERLAYS.HIGHLIGHTMODE.HIGHLIGHTOPTIONS.EXACTCOPIES;
         diagram.exactCopies_Label = optionName.gameObject;
         diagram.exactCopies_Toggle = diagram.exactCopies.GetComponentInChildren<MultiToggle>();
         diagram.exactCopies_Toggle.ChangeState(0);

         for(int i = 0; i < highlightOptionsContainer.transform.childCount; i++)
         {
            highlightOptionsContainer.transform.GetChild(i).gameObject.SetActive(false);
         }
         //----------------------------Options----------------------------UP
         //----------------------------Highlight Options----------------------------UP
         //----------------------------Highlight Filters----------------------------DOWN
         GameObject filtersHeaderContainer = new GameObject(nameof(filtersHeaderContainer));
         filtersHeaderContainer.transform.SetParent(diagramPrefab.transform);
         filtersHeaderContainer.SetActive(true);
         layoutGrouph = filtersHeaderContainer.AddComponent<HorizontalLayoutGroup>();
         layoutGrouph.childControlHeight = false;
         layoutGrouph.childControlWidth = false;
         layoutGrouph.childAlignment = TextAnchor.MiddleLeft;

         GameObject filtersHeader = Util.KInstantiateUI(Prefabs.LabelPrefab, filtersHeaderContainer, true);
         headerLabel = filtersHeader.GetComponent<LocText>();
         headerLabel.text = Strings.MYSTRINGS.UI.OVERLAYS.HIGHLIGHTMODE.HIGHLIGHTFILTERS.HEADER;
         headerLabel.fontSize *= 1.2f;


         diagram.filtersContainer = new GameObject(nameof(filtersContainer));
         diagram.filtersContainer.transform.SetParent(diagramPrefab.transform);
         diagram.filtersContainer.SetActive(true);
         layoutGroup = diagram.filtersContainer.AddComponent<VerticalLayoutGroup>();
         layoutGroup.childControlHeight = true;
         layoutGroup.childControlWidth = true;
         layoutGroup.childAlignment = TextAnchor.MiddleLeft;
         //----------------------------Highlight Filters----------------------------UP
      }

      public void ConfigureDiagramExceptOptions() {
         trueColorCheckbox.transform.parent.gameObject.AddSimpleTooltip(Strings.MYSTRINGS.UI.OVERLAYS.HIGHLIGHTMODE.HIGHLIGHTOPTIONS.EXTRAOPTIONS.TRUECOLOR_TOOLTIP);
         trueColorCheckmark.enabled = Main.highlightInTrueColor;
         trueColorCheckbox.onClick += () => {
            Main.highlightInTrueColor = !Main.highlightInTrueColor;
            trueColorCheckmark.enabled = Main.highlightInTrueColor;

            Utils.UpdateHighlightColor();
         };

         preservePreviousOptionsCheckbox.transform.parent.gameObject.AddSimpleTooltip(Strings.MYSTRINGS.UI.OVERLAYS.HIGHLIGHTMODE.HIGHLIGHTOPTIONS.EXTRAOPTIONS.PRESERVEPREVIOUSOPTIONS_TOOLTIP);
         preservePreviousOptionsCheckmark.enabled = Main.preservePreviousHighlightOptions;
         preservePreviousOptionsCheckbox.onClick += () => {
            Main.preservePreviousHighlightOptions = !Main.preservePreviousHighlightOptions;
            preservePreviousOptionsCheckmark.enabled = Main.preservePreviousHighlightOptions;

            if(!Main.preservePreviousHighlightOptions)
               Utils.UpdateHighlightMode();
         };

         gameNotPausedLabel.AddSimpleTooltip(Strings.MYSTRINGS.UI.OVERLAYS.HIGHLIGHTMODE.HIGHLIGHTOPTIONS.NOTPAUSED_TOOLTIP);
         noObjectSelectedLabel.AddSimpleTooltip(Strings.MYSTRINGS.UI.OVERLAYS.HIGHLIGHTMODE.HIGHLIGHTOPTIONS.NOOBJECTSELECTED_TOOLTIP);


         considerOption1_Toggle.onClick += () => {
            Main.considerOption1[Main.selectedObjProperties.objectType] = !Main.considerOption1[Main.selectedObjProperties.objectType];
            UpdateOptionsTogglesState();

            OnOptionsChangedListener();
         };
         considerOption1_Toggle.onClick += ConsiderOption1ToggleListener;

         consumers_Toggle.onClick += () => {
            Main.highlightOption ^= HighlightOptions.CONSUMERS;
            Main.highlightOption &= HighlightOptions.CONSUMERS;
            SaveLastHighlightOption();
            UpdateOptionsTogglesState();

            OnOptionsChangedListener();
         };
         producers_Toggle.onClick += () => {
            Main.highlightOption ^= HighlightOptions.PRODUCERS;
            Main.highlightOption &= HighlightOptions.PRODUCERS;
            SaveLastHighlightOption();
            UpdateOptionsTogglesState();

            OnOptionsChangedListener();
         };
         consumables_Toggle.onClick += () => {
            Main.highlightOption ^= HighlightOptions.CONSUMABLES;
            Main.highlightOption &= HighlightOptions.CONSUMABLES;
            SaveLastHighlightOption();
            UpdateOptionsTogglesState();

            OnOptionsChangedListener();
         };
         produce_Toggle.onClick += () => {
            Main.highlightOption ^= HighlightOptions.PRODUCE;
            Main.highlightOption &= HighlightOptions.PRODUCE;
            SaveLastHighlightOption();
            UpdateOptionsTogglesState();

            OnOptionsChangedListener();
         };
         buildingMaterial_Toggle.onClick += () => {
            Main.highlightOption ^= HighlightOptions.BUILDINGMATERIAL;
            Main.highlightOption &= HighlightOptions.BUILDINGMATERIAL;
            SaveLastHighlightOption();
            UpdateOptionsTogglesState();

            OnOptionsChangedListener();
         };
         copies_Toggle.onClick += () => {
            Main.highlightOption ^= HighlightOptions.COPIES;
            Main.highlightOption &= HighlightOptions.COPIES;
            SaveLastHighlightOption();
            UpdateOptionsTogglesState();

            OnOptionsChangedListener();
         };
         exactCopies_Toggle.onClick += () => {
            Main.highlightOption ^= HighlightOptions.EXACTCOPIES;
            Main.highlightOption &= HighlightOptions.EXACTCOPIES;
            SaveLastHighlightOption();
            UpdateOptionsTogglesState();

            OnOptionsChangedListener();
         };


         Main.highlightFilters = new HighlightFiltersTree();
         Main.highlightFilters.InitializeToggles();

         GameObject rootPanel = Main.highlightFilters.RootPanel;
         rootPanel.transform.SetParent(filtersContainer.transform);
         rootPanel.SetActive(true);
      }

      private void OnOptionsChangedListener() {
         Utils.UpdateHighlightMode();
      }

      private void ConsiderOption1ToggleListener() {
         if(Main.selectedObjProperties.TryUpdateHighlightOptionsForConsiderOptionToggle())
         {
            ConfigureDiagramOptions();
         }
      }


      public void ConfigureDiagramOptions() {
         if(!ModConfig.Instance.AllowNotPaused && !Game.Instance.IsPaused)
         {
            noObjectSelectedContainer.SetActive(false);
            selectedObjectTypeLabel.gameObject.SetActive(false);
            SetAllOptionsNotActive();

            gameNotPausedContainer.SetActive(true);
            return;
         }
         gameNotPausedContainer.SetActive(false);

         ObjectProperties selectedObjProperties = Main.selectedObjProperties;
         ObjectType objectType = selectedObjProperties.objectType;

         if(objectType == ObjectType.NOTVALID)
         {
            noObjectSelectedContainer.SetActive(true);
            selectedObjectTypeLabel.gameObject.SetActive(false);
            SetAllOptionsNotActive();
            return;
         }
         else
         {
            noObjectSelectedContainer.SetActive(false);
            selectedObjectTypeLabel.gameObject.SetActive(true);

            selectedObjectTypeLabel.text = Strings.MYSTRINGS.UI.OVERLAYS.HIGHLIGHTMODE.HIGHLIGHTOPTIONS.SELECTEDOBJECTTYPE_PREFIX + selectedObjProperties.StringRepresentation();


            Type highlightOptionsStrings = typeof(Strings.MYSTRINGS.UI.OVERLAYS.HIGHLIGHTMODE.HIGHLIGHTOPTIONS);

            FieldInfo[] fields = typeof(HighlightOverlayDiagram).GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
            var highlightFields = fields.Where(f => f.GetCustomAttribute(typeof(HighlightOptionAttribute)) != default);
            var highlightLabelFields = fields.Where(f => f.GetCustomAttribute(typeof(HighlightOptionLabelAttribute)) != default);
            foreach(HighlightOptions highlightOption in Enum.GetValues(typeof(HighlightOptions)))
            {
               if(highlightOption == HighlightOptions.NONE)
                  continue;

               if((selectedObjProperties.highlightOptions & highlightOption) != 0)
               {
                  ((GameObject)highlightFields.First(f => ((HighlightOptionAttribute)f.GetCustomAttribute(typeof(HighlightOptionAttribute))).highlightOption == highlightOption)
                     ?.GetValue(this)).SetActive(true);

                  GameObject highlightLabel = (GameObject)highlightLabelFields.First(f => ((HighlightOptionLabelAttribute)f.GetCustomAttribute(typeof(HighlightOptionLabelAttribute))).highlightOption == highlightOption)
                     ?.GetValue(this);

                  if(highlightLabel != null)
                  {
                     if(highlightOption == HighlightOptions.CONSIDEROPTION1)
                     {
                        highlightLabel.GetComponent<LocText>().text = Utils.GetMyString(highlightOptionsStrings, highlightOption.ToString(), objectType.ToString());
                     }

                     LocString labelTooltip;
                     if(!Utils.TryGetMyString(highlightOptionsStrings, highlightOption.ToString() + "_TOOLTIP", out labelTooltip, objectType.ToString()))
                     {
                        labelTooltip = Utils.GetMyString(highlightOptionsStrings, highlightOption.ToString() + "_TOOLTIP", "DEFAULTSTRINGS");
                     }
                     highlightLabel.AddSimpleTooltip(labelTooltip);
                  }
               }
               else
               {
                  ((GameObject)highlightFields.First(f => ((HighlightOptionAttribute)f.GetCustomAttribute(typeof(HighlightOptionAttribute))).highlightOption == highlightOption)
                     ?.GetValue(this)).SetActive(false);
               }
            }

            Main.highlightOption = !Main.preservePreviousHighlightOptions &&// preservePreviousHighlightOptions forces the highlight option to switch to NONE when other object is selected
               ((selectedObjProperties.highlightOptions & Main.lastHighlightOption[objectType]) != 0) ? Main.lastHighlightOption[objectType] : HighlightOptions.NONE;

            UpdateOptionsTogglesState();
         }



         void SetAllOptionsNotActive() {
            GameObject highlightOptionsContainer = considerOption1.transform.parent.gameObject;
            for(int i = 0; i < highlightOptionsContainer.transform.childCount; i++)
            {
               highlightOptionsContainer.transform.GetChild(i).gameObject.SetActive(false);
            }
         }
      }

      private void UpdateOptionsTogglesState() {
         considerOption1_Toggle.ChangeState(Main.considerOption1[Main.selectedObjProperties.objectType] ? 1 : 0);

         consumers_Toggle.ChangeState((Main.highlightOption & HighlightOptions.CONSUMERS) != 0 ? 1 : 0);
         producers_Toggle.ChangeState((Main.highlightOption & HighlightOptions.PRODUCERS) != 0 ? 1 : 0);
         consumables_Toggle.ChangeState((Main.highlightOption & HighlightOptions.CONSUMABLES) != 0 ? 1 : 0);
         produce_Toggle.ChangeState((Main.highlightOption & HighlightOptions.PRODUCE) != 0 ? 1 : 0);
         buildingMaterial_Toggle.ChangeState((Main.highlightOption & HighlightOptions.BUILDINGMATERIAL) != 0 ? 1 : 0);
         copies_Toggle.ChangeState((Main.highlightOption & HighlightOptions.COPIES) != 0 ? 1 : 0);
         exactCopies_Toggle.ChangeState((Main.highlightOption & HighlightOptions.EXACTCOPIES) != 0 ? 1 : 0);
      }

      private void SaveLastHighlightOption() {
         Main.lastHighlightOption[Main.selectedObjProperties.objectType] = Main.highlightOption;
      }
   }
}
