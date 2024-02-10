using HighlightOverlay.Enums;
using HighlightOverlay;
using HighlightOverlay.Structs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using HighlightOverlay.Strings;
using UnityEngine.UI;

namespace HighlightOverlay {
   public static class Main {
      public const string debugPrefix = "[HighlightOverlay] > ";

      public static readonly Color whiteHighlightColor = new Color(0.87f, 0.87f, 0.87f, 0f);
      public static readonly Color whiteBackgroundColor = new Color(0.95f, 0.95f, 0.95f, 1f);
      public static readonly Color blackBackgroundColor = new Color(0.15114269f, 0.15114269f, 0.15114269f, 1f);// should be unique because of technical reasons

      public static readonly Color checkboxHoverColor = new Color(0.5f, 0.5f, 0.5f, 1f);

      public static bool highlightInTrueColor = false;
      public static bool preservePreviousHighlightOptions = false;


      public static Dictionary<Tag, Tag> cachedPrefabIDs = new Dictionary<Tag, Tag>();

      public static Dictionary<Tag, List<GameObject>> speciesMorphs = new Dictionary<Tag, List<GameObject>>();
      public static Dictionary<Tag, List<GameObject>> speciesMorphsBabies = new Dictionary<Tag, List<GameObject>>();


      public static HighlightMode highlightMode = default;

      public static GameObject selectedObj = null;
      public static int selectedCell = -1;
      public static GameObject selectedTile = null;
      public static ObjectProperties selectedObjProperties = default;
      public static Color selectedCellHighlightColor = Color.clear;

      public static Dictionary<ObjectType, bool> considerOption1 = new Dictionary<ObjectType, bool>();
      public static HighlightOptions highlightOption = HighlightOptions.NONE;

      public static Dictionary<ObjectType, HighlightOptions> lastHighlightOption = new Dictionary<ObjectType, HighlightOptions>();

      public static Color[] cellColors;
      public static Color[] tileColors;


      public static HighlightFiltersTree highlightFilters = null;
      public static TreeNode highlightFiltersStructure;
      public static void GenerateHighlightFilters() {
         highlightFiltersStructure = new TreeNode("root");

         highlightFiltersStructure.AddChild(new TreeNode(MYSTRINGS.UI.OVERLAYS.HIGHLIGHTMODE.HIGHLIGHTFILTERS.ITEMS), true)
         .AddChild(new TreeNode(MYSTRINGS.UI.OVERLAYS.HIGHLIGHTMODE.HIGHLIGHTFILTERS.ON_GROUND))
         .AddChild(new TreeNode(MYSTRINGS.UI.OVERLAYS.HIGHLIGHTMODE.HIGHLIGHTFILTERS.STORED_ITEMS));

         highlightFiltersStructure.AddChild(new TreeNode(MYSTRINGS.UI.OVERLAYS.HIGHLIGHTMODE.HIGHLIGHTFILTERS.BUILDINGS), true)
         .AddChild(new TreeNode(MYSTRINGS.UI.OVERLAYS.HIGHLIGHTMODE.HIGHLIGHTFILTERS.STANDARD_BUILDINGS))
         .AddChild(new TreeNode(MYSTRINGS.UI.OVERLAYS.HIGHLIGHTMODE.HIGHLIGHTFILTERS.LIQUID_PIPES))
         .AddChild(new TreeNode(MYSTRINGS.UI.OVERLAYS.HIGHLIGHTMODE.HIGHLIGHTFILTERS.GAS_PIPES))
         .AddChild(new TreeNode(MYSTRINGS.UI.OVERLAYS.HIGHLIGHTMODE.HIGHLIGHTFILTERS.RAILS))
         .AddChild(new TreeNode(MYSTRINGS.UI.OVERLAYS.HIGHLIGHTMODE.HIGHLIGHTFILTERS.WIRES))
         .AddChild(new TreeNode(MYSTRINGS.UI.OVERLAYS.HIGHLIGHTMODE.HIGHLIGHTFILTERS.AUTOMATION))
         .AddChild(new TreeNode(MYSTRINGS.UI.OVERLAYS.HIGHLIGHTMODE.HIGHLIGHTFILTERS.BACKWALLS));

         highlightFiltersStructure.AddChild(new TreeNode(MYSTRINGS.UI.OVERLAYS.HIGHLIGHTMODE.HIGHLIGHTFILTERS.CONDUIT_CONTENTS), true)
         .AddChild(new TreeNode(MYSTRINGS.UI.OVERLAYS.HIGHLIGHTMODE.HIGHLIGHTFILTERS.LIQUID_CONTENTS))
         .AddChild(new TreeNode(MYSTRINGS.UI.OVERLAYS.HIGHLIGHTMODE.HIGHLIGHTFILTERS.GAS_CONTENTS))
         .AddChild(new TreeNode(MYSTRINGS.UI.OVERLAYS.HIGHLIGHTMODE.HIGHLIGHTFILTERS.RAILS_CONTENTS));

         highlightFiltersStructure.AddChild(new TreeNode(MYSTRINGS.UI.OVERLAYS.HIGHLIGHTMODE.HIGHLIGHTFILTERS.CELLS), true)
         .AddChild(new TreeNode(MYSTRINGS.UI.OVERLAYS.HIGHLIGHTMODE.HIGHLIGHTFILTERS.NATURAL_TILES))
         .AddChild(new TreeNode(MYSTRINGS.UI.OVERLAYS.HIGHLIGHTMODE.HIGHLIGHTFILTERS.LIQUIDS))
         .AddChild(new TreeNode(MYSTRINGS.UI.OVERLAYS.HIGHLIGHTMODE.HIGHLIGHTFILTERS.GASES));

         highlightFiltersStructure.AddChild(new TreeNode(MYSTRINGS.UI.OVERLAYS.HIGHLIGHTMODE.HIGHLIGHTFILTERS.CREATURES), true)
         .AddChild(new TreeNode(MYSTRINGS.UI.OVERLAYS.HIGHLIGHTMODE.HIGHLIGHTFILTERS.PLANTS))
         .AddChild(new TreeNode(MYSTRINGS.UI.OVERLAYS.HIGHLIGHTMODE.HIGHLIGHTFILTERS.CRITTERS))
         .AddChild(new TreeNode(MYSTRINGS.UI.OVERLAYS.HIGHLIGHTMODE.HIGHLIGHTFILTERS.DUPLICANTS))
         .AddChild(new TreeNode(MYSTRINGS.UI.OVERLAYS.HIGHLIGHTMODE.HIGHLIGHTFILTERS.ROBOTS));

         highlightFiltersStructure.AddChild(new TreeNode(MYSTRINGS.UI.OVERLAYS.HIGHLIGHTMODE.HIGHLIGHTFILTERS.MISCELLANEOUS), true)
         .AddChild(new TreeNode(MYSTRINGS.UI.OVERLAYS.HIGHLIGHTMODE.HIGHLIGHTFILTERS.GEYSERS))
         .AddChild(new TreeNode(MYSTRINGS.UI.OVERLAYS.HIGHLIGHTMODE.HIGHLIGHTFILTERS.OTHER));
      }



      static Main() {
         foreach(ObjectType objectType in typeof(ObjectType).GetEnumValues())
         {
            considerOption1.Add(objectType, objectType.DefaultConsiderOption1());

            lastHighlightOption.Add(objectType, HighlightOptions.NONE);
         }
      }

      public static void CacheCrittersMorphs() {
         foreach(var prefab in Assets.Prefabs)
         {
            if(prefab == null || prefab.gameObject == null)
               continue;

            if(prefab.TryGetComponent(out CreatureBrain brain))
            {
               if(prefab.GetDef<BabyMonitor.Def>() == null)
               {
                  if(!speciesMorphs.ContainsKey(brain.species))
                     speciesMorphs.Add(brain.species, new List<GameObject>(4));

                  speciesMorphs[brain.species].Add(prefab.gameObject);
               }
               else
               {
                  if(!speciesMorphsBabies.ContainsKey(brain.species))
                     speciesMorphsBabies.Add(brain.species, new List<GameObject>(4));

                  speciesMorphsBabies[brain.species].Add(prefab.gameObject);
               }
            }
         }
      }

      public static void CacheObjectsPrefabIDs() {
         foreach(var prefab in Assets.Prefabs)
         {
            if(prefab == null || prefab.gameObject == null)
               continue;

            if(Utils.IsObjectValidForHighlight(prefab.gameObject, out PrimaryElement primaryElement, out ObjectType objectType))
            {
               ObjectProperties properties = new ObjectProperties(primaryElement, objectType);
               cachedPrefabIDs.Add(prefab.PrefabTag, properties.prefabID);
            }
         }
      }
   }
}
