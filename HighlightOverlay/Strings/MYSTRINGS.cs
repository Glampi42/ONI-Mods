using STRINGS;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HighlightOverlay.Strings {
   public class MYSTRINGS : LocalizeStrings {
      public class UI {
         public class TOOLTIPS {
            public static LocString HIGHLIGHTOVERLAYSTRING = "Displays various things related to the selected object {Hotkey}";
         }
         public class OVERLAYS {
            public class HIGHLIGHTMODE {
               public static LocString NAME = "HIGHLIGHT OVERLAY";
               public static LocString BUTTON = "Highlight Overlay";

               public class HIGHLIGHTOPTIONS {
                  public static LocString HEADER = "Highlight Options:";

                  public class EXTRAOPTIONS {
                     public static LocString TRUECOLOR_LABEL = "True Color:";
                     public static LocString TRUECOLOR_TOOLTIP = "Highlight objects in their actual color instead of white";
                     public static LocString PRESERVEPREVIOUSOPTIONS_LABEL = "Keep Highlighting:";
                     public static LocString PRESERVEPREVIOUSOPTIONS_TOOLTIP = "Keep highlighting objects that match the options chosen previously";
                  }

                  public static LocString NOTPAUSED = "\n!--Game Not Paused--!\n";
                  public static LocString NOTPAUSED_TOOLTIP = "Pause the game in order to enable the Highlight Overlay";

                  public static LocString NOOBJECTSELECTED = "\n-No Object Selected-\n ";
                  public static LocString NOOBJECTSELECTED_TOOLTIP = "Select an object in order to highlight various things related to it";

                  public static LocString SELECTEDOBJECTTYPE_PREFIX = "Selected: ";

                  public static LocString CONSUMERS = "Consumers";
                  public static LocString PRODUCERS = "Producers";
                  public static LocString CONSUMABLES = "Consumables";
                  public static LocString PRODUCE = "Produce";
                  public static LocString BUILDINGMATERIAL = "Building Material";
                  public static LocString COPIES = "Copies";
                  public static LocString EXACTCOPIES = "Exact Copies";

                  public class ELEMENT {
                     public static LocString CONSIDEROPTION1 = "Consider Aggregate State";
                     public static LocString CONSIDEROPTION1_TOOLTIP = "Consider elements' aggregate state when highlighting their consumers, producers, produce & copies";
                     public static LocString CONSUMERS_TOOLTIP = "Highlight objects that can consume/store this element";
                     public static LocString PRODUCERS_TOOLTIP = "Highlight objects that can produce this element";
                     public static LocString PRODUCE_TOOLTIP = "Highlight elements that can be emitted by this element or be achieved via thermal transition from this element\n(f.e. Petroleum from Crude Oil)";
                     public static LocString COPIES_TOOLTIP = "Highlight objects consisting of this element/objects that contain this element in their storage room";
                  }
                  public class ITEM {
                     public static LocString CONSUMERS_TOOLTIP = "Highlight objects that can consume/store this item";
                     public static LocString PRODUCERS_TOOLTIP = "Highlight objects that can produce this item";
                     public static LocString COPIES_TOOLTIP = "Highlight copies of this item";
                  }
                  public class BUILDING {
                     public static LocString CONSIDEROPTION1 = "Consider Building's Settings";
                     public static LocString CONSIDEROPTION1_TOOLTIP = "Consider buildings' settings(storage filters, production queue) when highlighting their consumables, produce & building material";
                     public static LocString CONSUMABLES_TOOLTIP = "Highlight objects that can be consumed/stored/used by this building";
                     public static LocString PRODUCE_TOOLTIP = "Highlight objects that can be produced by this building";
                     public static LocString BUILDINGMATERIAL_TOOLTIP = "If \"Consider Building's Settings\" option is on:\nHighlight materials this specific building was built with\n\n" +
                        "If off:\nHighlight materials this building can be built with";
                     public static LocString COPIES_TOOLTIP = "Highlight copies of this building";
                     public static LocString EXACTCOPIES_TOOLTIP = "Highlight copies of this building that consist of the same element";
                  }
                  public class PLANTORSEED {
                     public static LocString CONSUMERS_TOOLTIP = "Highlight objects that can consume/store this plant's seed or consume this plant";
                     public static LocString CONSUMABLES_TOOLTIP = "Highlight objects that can be consumed by this plant";
                     public static LocString PRODUCE_TOOLTIP = "Highlight objects that can be produced by this plant";
                     public static LocString COPIES_TOOLTIP = "Highlight plants and seeds of this species";
                     public static LocString EXACTCOPIES_TOOLTIP = "Highlight plants and seeds of this species with the same mutation";
                  }
                  public class CRITTEROREGG {
                     public static LocString CONSIDEROPTION1 = "Consider Critter's Morph";
                     public static LocString CONSIDEROPTION1_TOOLTIP = "Consider critters' morph when highlighting their consumers, consumables, produce & copies";
                     public static LocString CONSUMERS_TOOLTIP = "Highlight objects that can consume/store this critter's egg or trap/release/interact with this critter";
                     public static LocString CONSUMABLES_TOOLTIP = "Highlight objects that can be consumed by this critter";
                     public static LocString PRODUCE_TOOLTIP = "Highlight objects that can be produced by this critter";
                     public static LocString COPIES_TOOLTIP = "Highlight critters and eggs of this species";
                  }
                  public class DUPLICANT {
                     public static LocString CONSUMERS_TOOLTIP = "Highlight buildings that this duplicant can work on/that are assigned to this duplicant";
                     public static LocString CONSUMABLES_TOOLTIP = "Highlight objects that can be consumed by this duplicant(considering their permitted food)";
                     public static LocString PRODUCE_TOOLTIP = "Highlight objects that can be produced by this duplicant(considering traits such as Flatulent)";
                     public static LocString COPIES_TOOLTIP = "Highlight all duplicants";
                  }
                  public class GEYSER {
                     public static LocString PRODUCE_TOOLTIP = "Highlight elements that can be emitted by this geyser";
                     public static LocString COPIES_TOOLTIP = "Highlight all geysers";
                     public static LocString EXACTCOPIES_TOOLTIP = "Highlight all geysers of this type";
                  }
                  public class DEFAULTSTRINGS {
                     public static LocString CONSUMERS_TOOLTIP = "Highlight objects that can consume this object";
                     public static LocString PRODUCERS_TOOLTIP = "Highlight objects that can produce this object";
                     public static LocString CONSUMABLES_TOOLTIP = "Highlight objects that can be consumed by this object";
                     public static LocString PRODUCE_TOOLTIP = "Highlight objects that can be produced by this object";
                     public static LocString COPIES_TOOLTIP = "Highlight all occurrences of this object";
                  }
               }
               public class HIGHLIGHTFILTERS {
                  public static LocString HEADER = "Highlight Filters:";


                  public static LocString ITEMS = "Items";
                  public static LocString ON_GROUND = "On the ground";
                  public static LocString STORED_ITEMS = "Stored";

                  public static LocString BUILDINGS = "Buildings";
                  public static LocString STANDARD_BUILDINGS = "Standard";
                  public static LocString LIQUID_PIPES = "Liquid pipes";
                  public static LocString GAS_PIPES = "Gas pipes";
                  public static LocString RAILS = "Conveyor rails";
                  public static LocString WIRES = "Wires";
                  public static LocString AUTOMATION = "Automation";
                  public static LocString BACKWALLS = "Backwalls";

                  public static LocString CONDUIT_CONTENTS = "Conduit Contents";
                  public static LocString LIQUID_CONTENTS = "Liquid pipes";
                  public static LocString GAS_CONTENTS = "Gas pipes";
                  public static LocString RAILS_CONTENTS = "Conveyor rails";

                  public static LocString CELLS = "Cells";
                  public static LocString NATURAL_TILES = "Natural tiles";
                  public static LocString LIQUIDS = "Liquids";
                  public static LocString GASES = "Gases";

                  public static LocString CREATURES = "Creatures";
                  public static LocString PLANTS = "Plants";
                  public static LocString CRITTERS = "Critters";
                  public static LocString DUPLICANTS = "Duplicants";
                  public static LocString ROBOTS = "Robots";

                  public static LocString MISCELLANEOUS = "Miscellaneous";
                  public static LocString GEYSERS = "Geysers";
                  public static LocString OTHER = "Other";
               }
            }
         }
         public class OBJECTTYPES {
            public static LocString NOTVALID = "Invalid";
            public static LocString ELEMENT = "Element";
            public static LocString ITEM = "Item";
            public static LocString BUILDING = "Building";
            public static LocString PLANTORSEED = "Plant/Seed";
            public static LocString CRITTEROREGG = "Critter/Egg";
            public static LocString DUPLICANT = "Duplicant";
            public static LocString GEYSER = "Geyser";
            public static LocString ROBOT = "Robot";
            public static LocString OILWELL = "Oil Reservoir";
            public static LocString SAPTREE = "Experiment 52B";
            public static LocString RADBOLT = "Radbolt";
         }
      }
   }
}