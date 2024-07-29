using ChainErrand.Strings;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YamlDotNet.Core.Tokens;

namespace ChainErrand {
   public enum ChainToolFilter {
      ALL,
      CONSTRUCTION,
      DIG,
      MOP,
      EMPTY_PIPE,
      MOVE_TO,

      STANDARD_BUILDINGS,
      WIRES,
      LIQUID_PIPES,
      GAS_PIPES,
      CONVEYOR_RAILS,
      AUTOMATION,
      BACKWALLS
   }

   public static class ChainToolFilterExtensions {
      /// <summary>
      /// Checks to see if the filter is applied.
      /// </summary>
      /// <returns>True if the filter is applied, false otherwise.</returns>
      public static bool IsOn(this ChainToolFilter filter) {
         if(ChainToolMenu.Instance == null)
            return false;

         return ChainToolMenu.Instance.GetToggleState(filter) == ToolParameterMenu.ToggleState.On;
      }

      /// <summary>
      /// Returns the string representation of the filter.
      /// </summary>
      /// <param name="filter">The filter</param>
      /// <returns>The string.</returns>
      public static string Name(this ChainToolFilter filter) {
         switch(filter)
         {
         case ChainToolFilter.ALL:
               return STRINGS.UI.TOOLS.FILTERLAYERS.ALL.NAME;
            case ChainToolFilter.CONSTRUCTION:
               return STRINGS.UI.TOOLS.FILTERLAYERS.CONSTRUCTION.NAME;
            case ChainToolFilter.DIG:
               return STRINGS.UI.TOOLS.FILTERLAYERS.DIG.NAME;
            case ChainToolFilter.MOP:
               return MYSTRINGS.UI.TOOLS.FILTERLAYERS.MOP.NAME;
            case ChainToolFilter.EMPTY_PIPE:
               return STRINGS.UI.TOOLS.EMPTY_PIPE.NAME;
            case ChainToolFilter.MOVE_TO:
               return STRINGS.UI.USERMENUACTIONS.PICKUPABLEMOVE.NAME;

            case ChainToolFilter.STANDARD_BUILDINGS:
               return STRINGS.UI.TOOLS.FILTERLAYERS.BUILDINGS.NAME;
            case ChainToolFilter.WIRES:
               return STRINGS.UI.TOOLS.FILTERLAYERS.WIRES.NAME;
            case ChainToolFilter.LIQUID_PIPES:
               return STRINGS.UI.TOOLS.FILTERLAYERS.LIQUIDPIPES.NAME;
            case ChainToolFilter.GAS_PIPES:
               return STRINGS.UI.TOOLS.FILTERLAYERS.GASPIPES.NAME;
            case ChainToolFilter.CONVEYOR_RAILS:
               return STRINGS.UI.TOOLS.FILTERLAYERS.SOLIDCONDUITS.NAME;
            case ChainToolFilter.AUTOMATION:
               return STRINGS.UI.TOOLS.FILTERLAYERS.LOGIC.NAME;
            case ChainToolFilter.BACKWALLS:
               return STRINGS.UI.TOOLS.FILTERLAYERS.BACKWALL.NAME;

            default:
               return "MISSING.NAME.FOR.FILTER." + filter;
         }
      }

      /// <summary>
      /// Returns the tooltip that is displayed when hovering over the filter.
      /// </summary>
      /// <param name="filter">The filter</param>
      /// <returns>The tooltip.</returns>
      public static string Tooltip(this ChainToolFilter filter) {
         switch(filter)
         {
            case ChainToolFilter.ALL:
               return STRINGS.UI.TOOLS.FILTERLAYERS.ALL.TOOLTIP;
            case ChainToolFilter.CONSTRUCTION:
               return STRINGS.UI.TOOLS.FILTERLAYERS.CONSTRUCTION.TOOLTIP;
            case ChainToolFilter.DIG:
               return STRINGS.UI.TOOLS.FILTERLAYERS.DIG.TOOLTIP;
            case ChainToolFilter.MOP:
               return MYSTRINGS.UI.TOOLS.FILTERLAYERS.MOP.TOOLTIP;
            case ChainToolFilter.EMPTY_PIPE:
               return MYSTRINGS.UI.TOOLS.FILTERLAYERS.EMPTYPIPE.TOOLTIP;
            case ChainToolFilter.MOVE_TO:
               return MYSTRINGS.UI.TOOLS.FILTERLAYERS.MOVETO.TOOLTIP;

            case ChainToolFilter.STANDARD_BUILDINGS:
               return STRINGS.UI.TOOLS.FILTERLAYERS.BUILDINGS.TOOLTIP;
            case ChainToolFilter.WIRES:
               return STRINGS.UI.TOOLS.FILTERLAYERS.WIRES.TOOLTIP;
            case ChainToolFilter.LIQUID_PIPES:
               return STRINGS.UI.TOOLS.FILTERLAYERS.LIQUIDPIPES.TOOLTIP;
            case ChainToolFilter.GAS_PIPES:
               return STRINGS.UI.TOOLS.FILTERLAYERS.GASPIPES.TOOLTIP;
            case ChainToolFilter.CONVEYOR_RAILS:
               return STRINGS.UI.TOOLS.FILTERLAYERS.SOLIDCONDUITS.TOOLTIP;
            case ChainToolFilter.AUTOMATION:
               return STRINGS.UI.TOOLS.FILTERLAYERS.LOGIC.TOOLTIP;
            case ChainToolFilter.BACKWALLS:
               return STRINGS.UI.TOOLS.FILTERLAYERS.BACKWALL.TOOLTIP;

            default:
               return "MISSING.TOOLTIP.FOR.FILTER." + filter;
         }
      }
   }
}