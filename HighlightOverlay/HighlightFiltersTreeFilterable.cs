using PeterHan.PLib.Core;
using PeterHan.PLib.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine.UI;
using UnityEngine;
using HighlightOverlay.Enums;
using HighlightOverlay.Strings;
using static STRINGS.BUILDING.STATUSITEMS.ACCESS_CONTROL;

namespace HighlightOverlay {
   public sealed class HighlightFiltersTreeFilterable {
      private static readonly RectOffset ELEMENT_MARGIN = new RectOffset(2, 2, 2, 2);

      /// <summary>
      /// The indent of the categories, and the items in each category.
      /// </summary>
      internal const int INDENT = 36;

      /// <summary>
      /// The size of the panel (UI sizes are hard coded in prefabs).
      /// </summary>
      internal static readonly Vector2 PANEL_SIZE = new Vector2(260.0f, 320.0f);

      /// <summary>
      /// The size of checkboxes and images in this control.
      /// </summary>
      internal static readonly Vector2 ROW_SIZE = new Vector2(24.0f, 24.0f);

      /// <summary>
      /// The spacing between each row.
      /// </summary>
      internal const int ROW_SPACING = 2;

      /// <summary>
      /// Updates the all check box state from the children.
      /// </summary>
      /// <param name="allItems">The "all" or "none" check box.</param>
      /// <param name="children">The child check boxes.</param>
      internal static void UpdateAllItems<T>(GameObject allItems,
            IEnumerable<T> children) where T : IHasCheckBox {
         if(allItems != null)
         {
            bool all = true, none = true;
            foreach(var child in children)
               if(PCheckBox.GetCheckState(child.CheckBox) == PCheckBox.STATE_CHECKED)
                  none = false;
               else
                  // Partially checked or unchecked
                  all = false;
            PCheckBox.SetCheckState(allItems, none ? PCheckBox.STATE_UNCHECKED : (all ?
               PCheckBox.STATE_CHECKED : PCheckBox.STATE_PARTIAL));
         }
      }

      /// <summary>
      /// Returns true if all items are selected to sweep.
      /// </summary>
      public bool IsAllSelected {
         get {
            return PCheckBox.GetCheckState(allItems) == PCheckBox.STATE_CHECKED;
         }
      }

      /// <summary>
      /// The root panel of the whole control.
      /// </summary>
      public GameObject RootPanel { get; }

      /// <summary>
      /// The "all items" checkbox.
      /// </summary>
      private GameObject allItems;

      /// <summary>
      /// The child panel where all categories are added.
      /// </summary>
      public GameObject childPanel;

      /// <summary>
      /// The child categories.
      /// </summary>
      private readonly Dictionary<HighlightFilters, TypeSelectCategory> children;

      public HighlightFiltersTreeFilterable() {
         // Select/deselect all types
         RootPanel = new PPanel("HighlightFiltersPanel") {
            Direction = PanelDirection.Vertical,
            Alignment = TextAnchor.UpperLeft,
            Spacing = ROW_SPACING,
            Margin = ELEMENT_MARGIN,
            FlexSize = Vector2.one,
            DynamicSize = true
         }
         .AddChild(new PCheckBox("SelectAll") {
            Text = STRINGS.UI.UISIDESCREENS.TREEFILTERABLESIDESCREEN.ALLBUTTON,
            CheckSize = ROW_SIZE,
            InitialState = PCheckBox.STATE_CHECKED,
            OnChecked = OnCheck,
            TextStyle = PUITuning.Fonts.TextLightStyle
         }.AddOnRealize((obj) => allItems = obj))
         
         .AddOnRealize((obj) => {
            childPanel = obj;
            childPanel.AddComponent<Canvas>();
            childPanel.AddComponent<GraphicRaycaster>();
            var sizeFitter = childPanel.AddComponent<ContentSizeFitter>();
            sizeFitter.horizontalFit = ContentSizeFitter.FitMode.MinSize;
            sizeFitter.verticalFit = ContentSizeFitter.FitMode.MinSize;
         }).Build();

         RootPanel.SetMinUISize(PANEL_SIZE);

         children = new Dictionary<HighlightFilters, TypeSelectCategory>(16);

         RootPanel.SetActive(false);


         InitializeToggles();
      }

      /// <summary>
      /// Selects all items.
      /// </summary>
      public void CheckAll() {
         PCheckBox.SetCheckState(allItems, PCheckBox.STATE_CHECKED);
         foreach(var child in children)
            child.Value.CheckAll();
      }

      /// <summary>
      /// Deselects all items.
      /// </summary>
      public void ClearAll() {
         PCheckBox.SetCheckState(allItems, PCheckBox.STATE_UNCHECKED);
         foreach(var child in children)
            child.Value.ClearAll();
      }

      private void OnCheck(GameObject source, int state) {
         if(state == PCheckBox.STATE_UNCHECKED)
            // Clicked when unchecked, check all
            CheckAll();
         else
            // Clicked when checked or partial, clear all
            ClearAll();

         Utils.UpdateHighlightMode();// update highlight when highlight filters change
      }

      /// <summary>
      /// Creates the toggles for all categories.
      /// </summary>
      private void InitializeToggles() {
         if(children.Count == 0)
         {
            foreach(HighlightFilters category in Enum.GetValues(typeof(HighlightFilters)))
            {
               if(Utils.HasMoreThanOneBitSet((int)category) && category != HighlightFilters.ALL)
                  InitializeCategory(category);
            }

            UpdateAllItems(allItems, children.Values);
         }
      }

      /// <summary>
      /// Creates all elements in the specified category.
      /// </summary>
      /// <param name="category">The category to search.</param>
      private void InitializeCategory(HighlightFilters category) {
         // Attempt to add to type select control
         if(!children.TryGetValue(category, out TypeSelectCategory current))
         {
            current = new TypeSelectCategory(this, category);
            children.Add(category, current);

            int index = 1 + ((children.Count - 1) << 1);
            GameObject header = current.Header, panel = current.ChildPanel;
            // Header goes in even indexes, panel goes in odds
            header.SetParent(childPanel);
            PUIElements.SetAnchors(header, PUIAnchoring.Stretch, PUIAnchoring.Stretch);
            header.transform.SetSiblingIndex(index);
            panel.SetParent(childPanel);
            PUIElements.SetAnchors(panel, PUIAnchoring.Stretch, PUIAnchoring.Stretch);
            panel.transform.SetSiblingIndex(index + 1);
         }

         for(int filter = 1; filter <= (1 << (Utils.GetBinaryLength((int)category) - 1)); filter <<= 1)
         {
            if(((int)category & filter) != 0)
            {
               current.TryAddType((HighlightFilters)filter);
            }
         }

         UpdateAllItems(current.CheckBox, current.children.Values);
      }

      /// <summary>
      /// Updates the parent check box state from the children.
      /// </summary>
      internal void UpdateFromChildren() {
         UpdateAllItems(allItems, children.Values);

         Utils.UpdateHighlightMode();// update highlight when highlight filters change
      }

      /// <summary>
      /// A category used in type select controls.
      /// </summary>
      private sealed class TypeSelectCategory : IHasCheckBox {
         /// <summary>
         /// The margins around a checkbox for a category.
         /// </summary>
         private static readonly RectOffset HEADER_MARGIN = new RectOffset(5, 0, 0, 0);

         /// <summary>
         /// The HighlightFilters category this object controls.
         /// </summary>
         public HighlightFilters Category { get; }

         /// <summary>
         /// The check box for selecting or deselecting children.
         /// </summary>
         public GameObject CheckBox { get; private set; }

         /// <summary>
         /// The panel holding all children.
         /// </summary>
         public GameObject ChildPanel { get; }

         /// <summary>
         /// The parent control.
         /// </summary>
         public HighlightFiltersTreeFilterable Control { get; }

         /// <summary>
         /// The header for this category.
         /// </summary>
         public GameObject Header { get; }

         /// <summary>
         /// The child elements.
         /// </summary>
         internal readonly Dictionary<HighlightFilters, TypeSelectFilter> children;

         internal TypeSelectCategory(HighlightFiltersTreeFilterable parent, HighlightFilters category) {
            Control = parent ?? throw new ArgumentNullException("parent");
            Category = category;
            var selectBox = new PCheckBox("SelectCategory") {
               Text = Utils.GetMyString(typeof(MYSTRINGS.UI.OVERLAYS.HIGHLIGHTMODE.HIGHLIGHTFILTERS), category.ToString()),
               OnChecked = OnCheck,
               CheckSize = ROW_SIZE,
               InitialState =
               PCheckBox.STATE_CHECKED,
               TextStyle = PUITuning.Fonts.TextLightStyle
            };
            selectBox.OnRealize += (obj) => { CheckBox = obj; };
            var showHide = new PToggle("ShowHide") {
               OnStateChanged = OnToggle,
               Size = new Vector2(ROW_SIZE.x * 0.5f,
               ROW_SIZE.y * 0.5f),
               Color = PUITuning.Colors.ComponentDarkStyle
            };
            Header = new PRelativePanel("TypeSelectCategory") { DynamicSize = false }.
               AddChild(showHide).AddChild(selectBox).SetLeftEdge(showHide,
               fraction: 0.0f).SetRightEdge(selectBox, fraction: 1.0f).SetLeftEdge(
               selectBox, toRight: showHide).SetMargin(selectBox, HEADER_MARGIN).
               AnchorYAxis(showHide, anchor: 0.5f).Build();

            children = new Dictionary<HighlightFilters, TypeSelectFilter>(16);

            ChildPanel = new PPanel("Children") {
               Direction = PanelDirection.Vertical,
               Alignment = TextAnchor.UpperLeft,
               Spacing = ROW_SPACING,
               Margin = new RectOffset(INDENT, 0, 0, 0)
            }.Build();
            ChildPanel.transform.localScale = Vector3.zero;
         }

         /// <summary>
         /// Selects all items in this category.
         /// </summary>
         public void CheckAll() {
            PCheckBox.SetCheckState(CheckBox, PCheckBox.STATE_CHECKED);
            foreach(var child in children)
               child.Value.SetSelected(true, false);
         }

         /// <summary>
         /// Deselects all items in this category.
         /// </summary>
         public void ClearAll() {
            PCheckBox.SetCheckState(CheckBox, PCheckBox.STATE_UNCHECKED);
            foreach(var child in children)
               child.Value.SetSelected(false, false);
         }

         private void OnCheck(GameObject source, int state) {
            if(state == PCheckBox.STATE_UNCHECKED)
               // Clicked when unchecked, check all
               CheckAll();
            else
               // Clicked when checked or partial, clear all
               ClearAll();

            Control.UpdateFromChildren();
         }

         private void OnToggle(GameObject source, bool open) {
            var obj = ChildPanel;
            if(obj != null)
            {
               // Scale to 0x0 if not visible
               var rt = obj.rectTransform();
               rt.localScale = open ? Vector3.one : Vector3.zero;
               LayoutRebuilder.MarkLayoutForRebuild(rt);
            }
         }

         /// <summary>
         /// Attempts to add a type to this category.
         /// </summary>
         /// <param name="filter">The type to add.</param>
         /// <returns>true if it was added, or false if it already exists.</returns>
         public bool TryAddType(HighlightFilters filter) {
            bool add = !children.ContainsKey(filter);
            if(add)
            {
               var child = new TypeSelectFilter(this, filter);
               var cb = child.CheckBox;
               // Add the element to the list, then get its index and move it in the panel
               children.Add(filter, child);

               cb.SetParent(ChildPanel);

               cb.transform.SetSiblingIndex(children.Count - 1);
            }
            return add;
         }

         /// <summary>
         /// Updates the parent check box state from the children.
         /// </summary>
         internal void UpdateFromChildren() {
            UpdateAllItems(CheckBox, children.Values);
            Control.UpdateFromChildren();
         }
      }

      /// <summary>
      /// An individual filter choice used in type select controls.
      /// </summary>
      private sealed class TypeSelectFilter : IHasCheckBox {
         /// <summary>
         /// The selection checkbox.
         /// </summary>
         public GameObject CheckBox { get; }

         /// <summary>
         /// The HighlightFilter this object controls.
         /// </summary>
         public HighlightFilters Filter { get; }

         /// <summary>
         /// The parent category.
         /// </summary>
         private readonly TypeSelectCategory parent;

         internal TypeSelectFilter(TypeSelectCategory parent, HighlightFilters filter) {
            this.parent = parent ?? throw new ArgumentNullException("parent");
            var tint = Color.white;
            Filter = filter;
            CheckBox = new PCheckBox("Select") {
               CheckSize = ROW_SIZE,
               SpriteSize = ROW_SIZE,
               OnChecked = OnCheck,
               Text = Utils.GetMyString(typeof(MYSTRINGS.UI.OVERLAYS.HIGHLIGHTMODE.HIGHLIGHTFILTERS), filter.ToString()),
               InitialState = (Main.highlightFilters & filter) != 0 ? PCheckBox.STATE_CHECKED : PCheckBox.STATE_UNCHECKED,
               Sprite = null,
               SpriteTint = tint,
               TextStyle = PUITuning.Fonts.TextLightStyle
            }.Build();
         }

         private void OnCheck(GameObject source, int state) {
            SetSelected(state == PCheckBox.STATE_UNCHECKED, true);
         }

         /// <summary>
         /// Sets the selected state of this type.
         /// </summary>
         /// <param name="selected">true to select this type, or false otherwise.</param>
         public void SetSelected(bool selected, bool updateParent) {
            if(selected)
               // Clicked when unchecked, check and possibly check all
               PCheckBox.SetCheckState(CheckBox, PCheckBox.STATE_CHECKED);
            else
               // Clicked when checked, clear and possibly uncheck
               PCheckBox.SetCheckState(CheckBox, PCheckBox.STATE_UNCHECKED);

            if(selected)
            {
               Main.highlightFilters |= this.Filter;
            }
            else
            {
               Main.highlightFilters &= ~this.Filter;
            }

            if(updateParent)
               parent.UpdateFromChildren();
         }
      }

      /// <summary>
      /// Applied to categories and elements with a single summary checkbox.
      /// </summary>
      internal interface IHasCheckBox {
         /// <summary>
         /// Checkbox!
         /// </summary>
         GameObject CheckBox { get; }
      }
   }
}
